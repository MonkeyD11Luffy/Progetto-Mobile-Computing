# Progetto — Cyberpunk Dungeon Crawler 2D

Progetto d'esame. Unity 6 (2D, URP), C#, sviluppatore singolo.
Dungeon crawler top-down a stanze fisse, ispirato a The Binding of Isaac.

## Struttura

- `Assets/Scripts/` — tutti gli script di gioco
- `Assets/Prefabs/` — nemici, proiettili, pickup, bombe
- `Assets/Scenes/` — `MainMenu` (indice 0) e `SampleScene` (il gioco)
- `Assets/Sprites/`, `Assets/audio/` — asset

## Architettura

- `EnemyBase` — classe astratta con vita, morte, danno da contatto, riferimento al player. Fornisce anche le utilità condivise: `DirectionToPlayer()`, `MoveTowardsPlayer(speed)`, `SpawnProjectile(...)`, `FireRadialBurst(...)`, `SpawnEnemiesAroundSelf(...)`. **Tutti** i nemici ereditano da qui.
- `BossBase : EnemyBase` — struttura comune a Boss e Miniboss: macchina a stati (movimento → telegrafo → azione), carica, fase di rabbia, barra della vita, morte con ricompensa. Le sottoclassi implementano solo `ChooseNextAction()`, `ExecuteAction()` e `UpdateEnrageVisual()`.
- `PlayerController` — input, 4 modalità di fuoco, bombe, vita, potenziamenti
- `PlayerVisuals` — sprite direzionali del player e arma orbitante; espone `MuzzlePosition`, cioè il punto da cui `PlayerController.Fire()` fa partire i proiettili
- `RoomManager` — attiva una stanza alla volta, conta i nemici, blocca le porte, gestisce drop e condizione di vittoria
- `DoorTrigger` / `SecretWall` — transizioni tra stanze
- `GameManager` — schermate di fine partita, ricarica scena
- `AudioManager` — riproduce tutti gli SFX da una sorgente centrale
- `VectorUtils` — classe statica (`Rotate`, `FromAngle`, `ToAngle`). Non è un componente: non va messa su nessun GameObject.

## Convenzioni da rispettare

**Nuovi nemici**: eredita da `EnemyBase`, implementa solo il comportamento specifico. Non riscrivere vita, morte o danno da contatto. Per un nemico con fasi e barra della vita, eredita invece da `BossBase`.

**Codice condiviso**: prima di scrivere inseguimento del player, rotazione di un vettore, raffica radiale o spawn di nemici, controlla `EnemyBase` e `VectorUtils`: esistono già. Sono stati estratti proprio perché erano duplicati in cinque script.

**Danno**: il danno ai nemici è applicato **solo** da `ProjectileController` / `BombController` via `SendMessage("TakeDamage", ...)`. Non aggiungere `OnTriggerEnter2D` sui nemici per intercettare i proiettili: causa doppio danno e doppio conteggio delle morti.

**Morte**: `EnemyBase` usa il flag `isDead` per garantire che `Die()` venga eseguito una volta sola. `Destroy()` in Unity agisce a fine frame, quindi senza il flag un nemico colpito da più proiettili nello stesso frame (arma spread) muore più volte.

**Nemici generati a runtime** (Splitter che si divide, Boss che evoca): vanno creati con `SpawnEnemiesAroundSelf(...)`, che chiama già `RoomManager.Instance.RegisterEnemySpawn(n)`. Se li istanzi a mano devi registrarli tu, altrimenti le porte si sbloccano in anticipo — ma non fare entrambe le cose: una doppia registrazione blocca le porte per sempre.

**Conteggio stanza**: `RoomManager` tiene `enemiesRemaining` più il flag `roomCleared`, che garantisce che sblocco porte, drop del pickup e vittoria avvengano una volta sola per stanza. Non aggirare `RegisterEnemyDeath` / `RegisterEnemySpawn` toccando il contatore da fuori.

**Fine partita**: `GameManager` mette `Time.timeScale = 0`, che ferma la fisica ma **non** `Update()`. Ogni script che legge l'input deve uscire subito quando il gioco è fermo (vedi la guardia in cima a `PlayerController.Update()`).

**Valori di bilanciamento**: esposti con `[SerializeField]` e tarati nell'Inspector, non fissati nel codice. Modificare i default nello script non ha effetto sugli oggetti già in scena.

**Riferimenti a oggetti di scena**: mai per nome nel codice. Si dichiara un campo `[SerializeField]` e si assegna trascinando nell'Inspector.

**Testo UI**: TextMeshPro (`TextMeshProUGUI`), non il vecchio componente Text.

**Input**: sistema legacy (`Input.GetKey`). Il progetto ha *Active Input Handling* impostato su "Both".

## Comandi di gioco

WASD movimento · frecce sparo · Q cambia arma · E bomba

## Limiti

Le operazioni sull'editor (assegnare riferimenti nell'Inspector, creare GameObject, configurare i Prefab, impostare i Tag) vanno fatte a mano in Unity: da riga di comando si modificano solo gli script.
