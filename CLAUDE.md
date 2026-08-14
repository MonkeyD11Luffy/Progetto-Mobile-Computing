# Progetto — Cyberpunk Dungeon Crawler 2D

Progetto d'esame. Unity 6 (2D, URP), C#, sviluppatore singolo.
Dungeon crawler top-down a stanze fisse generate proceduralmente, ispirato a
The Binding of Isaac.

## Architettura

- `EnemyBase` — classe astratta con vita, morte, danno da contatto, riferimento al player. Fornisce anche le utilità condivise: `DirectionToPlayer()`, `MoveTowardsPlayer(speed)`, `SpawnProjectile(...)`, `FireRadialBurst(...)`, `SpawnEnemiesAroundSelf(...)`. **Tutti** i nemici ereditano da qui.
- `BossBase : EnemyBase` — struttura comune a Boss e Miniboss: macchina a stati (movimento → telegrafo → azione), carica, fase di rabbia, barra della vita, morte con ricompensa. Le sottoclassi implementano solo `ChooseNextAction()`, `ExecuteAction()` e `UpdateEnrageVisual()`.
- `DungeonGenerator` — genera il grafo delle celle su griglia, istanzia le stanze dai template, collega le porte, popola i nemici e sceglie stanza boss, miniboss e segreta. Gira in `Awake()`, sullo stesso GameObject del `RoomManager`
- `Pathfinder` — classe statica con A* a 8 direzioni sulla griglia. Come `VectorUtils` non è un componente: non va messa su nessun GameObject
- `VectorUtils` — classe statica (`Rotate`, `FromAngle`, `ToAngle`). Non è un componente: non va messa su nessun GameObject.

## Convenzioni da rispettare

**Nuovi nemici**: eredita da `EnemyBase`, implementa solo il comportamento specifico. Non riscrivere vita, morte o danno da contatto. Per un nemico con fasi e barra della vita, eredita invece da `BossBase`.

**Codice condiviso**: prima di scrivere inseguimento del player, rotazione di un vettore, raffica radiale o spawn di nemici, controlla `EnemyBase` e `VectorUtils`: esistono già. Sono stati estratti proprio perché erano duplicati in cinque script.

**Danno**: il danno ai nemici è applicato **solo** da `ProjectileController` / `BombController` via `SendMessage("TakeDamage", ...)`. Non aggiungere `OnTriggerEnter2D` sui nemici per intercettare i proiettili: causa doppio danno e doppio conteggio delle morti.

**Morte**: `EnemyBase` usa il flag `isDead` per garantire che `Die()` venga eseguito una volta sola. `Destroy()` in Unity agisce a fine frame, quindi senza il flag un nemico colpito da più proiettili nello stesso frame (arma spread) muore più volte.

**Stanze**: sono template in `Assets/Prefabs/Rooms/`, non oggetti di scena. Il generatore le istanzia tutte a posizione `{0, 0, 0}`: le celle della griglia servono solo a costruire il grafo dei collegamenti, e `ActivateOnly` ne mostra comunque una sola alla volta.

**Porte nei template**: ogni stanza deve avere tutte e quattro le porte, anche sui lati che sembrano inutili — il generatore distrugge quelle che restano senza collegamento. Vanno distrutte con `DestroyImmediate` sull'intero GameObject e non disattivate, perché `RoomManager.SetDoorsActive` cerca per tag `"Door"` e le riattiverebbe alla liberazione della stanza.

**Campi di `DoorTrigger`**: `direction` e `arrivalPosition` sono dati di progettazione e vivono nel prefab; `targetRoom` e `playerSpawnPosition` sono dati di collegamento e li scrive `Connect()` a runtime. Nei prefab restano rispettivamente *None* e `{0, 0}`: in Play, un valore diverso da zero significa "porta collegata".

**Direzioni delle porte**: duplicare una porta senza cambiarne il campo `direction` produce un dungeon spezzato senza alcun errore, perché `FindDoor` restituisce sempre la prima e l'altra resta scollegata. Il generatore valida i template all'avvio proprio per questo.

**Centro della stanza**: va tenuto libero da ostacoli. È il punto in cui `RoomManager` fa cadere il pickup di fine stanza.

**Passaggi nei template**: niente più stretto di 1,5 unità, altrimenti sparisce dalla griglia di navigazione per via del margine d'agente e i nemici non possono attraversarlo.

**Nemici piazzati dal generatore**: non vanno registrati con `RegisterEnemySpawn`, perché `CountEnemiesInRoom` li conta già all'ingresso nella stanza. Registrarli li conterebbe due volte e le porte resterebbero bloccate per sempre.

**Nemici generati a runtime** (Splitter che si divide, Boss che evoca): vale la regola opposta. Vanno creati con `SpawnEnemiesAroundSelf(...)`, che chiama già `RoomManager.Instance.RegisterEnemySpawn(n)`. Se li istanzi a mano devi registrarli tu, altrimenti le porte si sbloccano in anticipo — ma non fare entrambe le cose: una doppia registrazione blocca le porte per sempre.

**Conteggio stanza**: `RoomManager` tiene `enemiesRemaining` più il flag `roomCleared`, che garantisce che sblocco porte, drop del pickup e vittoria avvengano una volta sola per stanza. Non aggirare `RegisterEnemyDeath` / `RegisterEnemySpawn` toccando il contatore da fuori.

**Movimento dei nemici**: `MoveTowardsPlayer` usa `rb.MovePosition`, non `rb.linearVelocity`. Spingendo con la velocità contro lo spigolo di un ostacolo il solver azzera la spinta e il nemico resta incastrato, mentre `MovePosition` risolve la collisione scivolando lungo il collider.

**Sprite direzionali dei nemici**: `EnemyVisuals` legge `EnemyBase.MoveDirection` e non `rb.linearVelocity`, che nel ramo con percorso è sempre zero.

**Griglia di navigazione**: `RoomNavGrid` marca le celle occupate gonfiando il test con `agentRadius`. Senza quel margine i percorsi passano a filo degli spigoli e i nemici ci si incastrano contro.

**Linea di vista**: i cast usano la semi-diagonale del collider come raggio, non il raggio inscritto. Un cast più sottile del corpo dichiara libera una strada in cui il nemico non passa.

**Fine partita**: `GameManager` mette `Time.timeScale = 0`, che ferma la fisica ma **non** `Update()`. Ogni script che legge l'input deve uscire subito quando il gioco è fermo (vedi la guardia in cima a `PlayerController.Update()`). Fa eccezione `GameManager.Update()`, che deve continuare a leggere ESC per togliere la pausa: distingue i due casi con il flag `isGameOver`.

**Valori di bilanciamento**: esposti con `[SerializeField]` e tarati nell'Inspector, non fissati nel codice. Modificare i default nello script non ha effetto sugli oggetti già in scena.

**Riferimenti a oggetti di scena**: mai per nome nel codice. Si dichiara un campo `[SerializeField]` e si assegna trascinando nell'Inspector.

**Testo UI**: TextMeshPro (`TextMeshProUGUI`), non il vecchio componente Text.

**Input**: sistema legacy (`Input.GetKey`). Il progetto ha *Active Input Handling* impostato su "Both".

## Eccezioni note

**Punti di spawn cercati per nome**: il generatore trova i segnaposto dei nemici come figli chiamati `Spawn_*`. È un'eccezione consapevole alla regola "mai riferimenti per nome nel codice": un array di `Transform` per stanza andrebbe riassegnato a mano su ogni template.

**Doppio uso di `RoomBounds`**: la stessa area serve sia a limitare il teletrasporto del Teleporter sia a definire l'estensione della griglia di navigazione, e le due cose hanno esigenze opposte — il primo vuole un margine dal muro, la seconda vuole l'area piena. Conflitto ancora aperto.

## Comandi di gioco

WASD movimento · frecce sparo · Q cambia arma · E bomba · Tab mostra e nasconde la minimappa · ESC pausa

## Limiti

Le operazioni sull'editor (assegnare riferimenti nell'Inspector, creare GameObject, configurare i Prefab, impostare i Tag) vanno fatte a mano in Unity: da riga di comando si modificano solo gli script.
