# Progetto — Cyberpunk Dungeon Crawler 2D

Progetto d'esame. Unity 6 (2D, URP), C#, sviluppatore singolo.
Dungeon crawler top-down a stanze fisse, ispirato a The Binding of Isaac.

## Struttura

- `Assets/Scripts/` — tutti gli script di gioco
- `Assets/Prefabs/` — nemici, proiettili, pickup, bombe
- `Assets/Scenes/` — `MainMenu` (indice 0) e `SampleScene` (il gioco)
- `Assets/Sprites/`, `Assets/audio/` — asset

## Architettura

- `EnemyBase` — classe astratta con vita, morte, danno da contatto, riferimento al player, spawn proiettili. **Tutti** i nemici ereditano da qui.
- `PlayerController` — input, 4 modalità di fuoco, bombe, vita, potenziamenti
- `RoomManager` — attiva una stanza alla volta, conta i nemici, blocca le porte, gestisce drop e condizione di vittoria
- `DoorTrigger` / `SecretWall` — transizioni tra stanze
- `GameManager` — schermate di fine partita, ricarica scena
- `AudioManager` — riproduce tutti gli SFX da una sorgente centrale

## Convenzioni da rispettare

**Nuovi nemici**: eredita da `EnemyBase`, implementa solo il comportamento specifico. Non riscrivere vita, morte o danno da contatto.

**Danno**: il danno ai nemici è applicato **solo** da `ProjectileController` / `BombController` via `SendMessage("TakeDamage", ...)`. Non aggiungere `OnTriggerEnter2D` sui nemici per intercettare i proiettili: causa doppio danno e doppio conteggio delle morti.

**Morte**: `EnemyBase` usa il flag `isDead` per garantire che `Die()` venga eseguito una volta sola. `Destroy()` in Unity agisce a fine frame, quindi senza il flag un nemico colpito da più proiettili nello stesso frame (arma spread) muore più volte.

**Nemici generati a runtime** (Splitter che si divide, Boss che evoca): devono chiamare `RoomManager.Instance.RegisterEnemySpawn(n)`, altrimenti le porte si sbloccano in anticipo.

**Valori di bilanciamento**: esposti con `[SerializeField]` e tarati nell'Inspector, non fissati nel codice. Modificare i default nello script non ha effetto sugli oggetti già in scena.

**Riferimenti a oggetti di scena**: mai per nome nel codice. Si dichiara un campo `[SerializeField]` e si assegna trascinando nell'Inspector.

**Testo UI**: TextMeshPro (`TextMeshProUGUI`), non il vecchio componente Text.

**Input**: sistema legacy (`Input.GetKey`). Il progetto ha *Active Input Handling* impostato su "Both".

## Comandi di gioco

WASD movimento · frecce sparo · Q cambia arma · E bomba

## Limiti

Le operazioni sull'editor (assegnare riferimenti nell'Inspector, creare GameObject, configurare i Prefab, impostare i Tag) vanno fatte a mano in Unity: da riga di comando si modificano solo gli script.
