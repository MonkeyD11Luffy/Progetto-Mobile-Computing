# SETUP — stato attuale della scena e degli asset

Documento ricavato leggendo direttamente `Assets/Scenes/SampleScene.unity`,
`ProjectSettings/TagManager.asset`, tutti i `.prefab` sotto `Assets/Prefabs/`
(inclusa la sottocartella `Rooms/`) e tutti gli script in `Assets/Scripts/`.

Descrive **solo lo stato attuale**: nessuna proposta di modifica.

`SampleSceneOld.unity` non è considerato: è la vecchia scena con il dungeon
costruito a mano, non più usata. La scena di gioco è `SampleScene`, che **non
contiene nessuna stanza**: le stanze vengono istanziate a runtime da
`DungeonGenerator` a partire dai template in `Assets/Prefabs/Rooms/`.

---

## 1. Gerarchia di `SampleScene`

Otto oggetti radice:

```
Main Camera
Player
Global Light 2D
AudioManager
EventSystem
RoomManager
Canvas
GameManager
```

### Main Camera

Tag `MainCamera`. Componenti:

- `Camera`
- `CameraShake`
- `AudioListener`
- `UniversalAdditionalCameraData` (URP)
- `PixelPerfectCamera` — `Assets PPU 16`, `Reference Resolution 240 × 135`

È l'oggetto puntato da `RoomManager.pixelPerfectCamera`.

### Player

Tag `Player`. Componenti: `SpriteRenderer`, `Rigidbody2D`, `BoxCollider2D`,
`PlayerController`, `PlayerVisuals`, `PlayerAbilities`.

Figli: `WeaponPivot` (`SpriteRenderer`), `MeleeVisual` (disattivato, `SpriteRenderer`).

`PlayerController`:

| Campo | Valore | | Campo | Valore |
|---|---|---|---|---|
| `moveSpeed` | 5 | | `maxHealth` | 6 |
| `projectilePrefab` | `Projectile` | | `invulnerabilityDuration` | 0.8 |
| `projectileSpeed` | 10 | | `healthRegenInterval` | 5 |
| `fireCooldown` | 0.3 | | `healthRegenChance` | 0 |
| `projectileDamage` | 1 | | `contactDamageReduction` | 0 |
| `firePointDistance` | 0.5 | | `projectileBounces` | 0 |
| `spreadAngle` | 20 | | `hitFlashDuration` | 0.1 |
| `spreadCooldownMult` | 1.6 | | `hitShakeDuration` | 0.15 |
| `pierceCooldownMult` | 1.3 | | `hitShakeMagnitude` | 0.1 |
| `spreadLifetime` | 0.35 | | `upgradePopupDuration` | 1.5 |
| `meleeRange` | 1.1 | | `meleeVisualDuration` | 0.1 |
| `meleeArc` | 0.3 | | `bombPrefab` | `Bomb` |
| `meleeDamageMult` | 2 | | `maxBombs` | 3 |
| `meleeCooldownMult` | 1.4 | | `bombCooldown` | 1 |

Riferimenti UI assegnati: `healthText` → `Canvas/HealthText`, `bombText` →
`BombText`, `creditText` → `CreditText`, `weaponText` → `WeaponText`,
`upgradePopupText` → `UpgradePopUpText`, `meleeVisual` → figlio `MeleeVisual`,
`playerVisuals` → il componente sullo stesso GameObject.

`PlayerVisuals`: `frameRate 8`, `idleFrame 1`, `weaponOrbitRadius 0.28`,
`weaponSortingOffset 1`, `muzzleUp 0.19`, `weaponHideDelay 0.15`, array
`walkDown` / `walkUp` / `walkSide` (3 sprite ciascuno da `neo_zero_char_01`),
`weaponSprites` (4 da `neo_zero_armi_20x16`), `muzzleForward`
`[0.5625, 0.5, 0.6875, 0.75]`, `weaponPivot` e `weaponRenderer` → figlio
`WeaponPivot`.

`PlayerAbilities`: vedi [sezione 10](#10-abilità-attive).

### Global Light 2D

Solo `Light2D` (URP 2D), tipo *Global*.

### AudioManager

Componente `AudioManager` con due sorgenti figlie e otto clip assegnate.

| Campo | Valore |
|---|---|
| `sfxSource` | figlio `SFX_source` (`AudioSource`) |
| `musicSource` | figlio `Music_source` (`AudioSource`) |
| `shootSingleClip` | `Single` |
| `shootSpreadClip` | `Spread` |
| `shootPierceClip` | `Penetrativo` |
| `meleeClip` | `melee` |
| `playerHurtClip` | `PlayerHit` |
| `enemyDeathClip` | `EnemyDeath` |
| `explosionClip` | `Explosion` |
| `pickupClip` | `Pickup` |
| `pitchVariation` | 0.2 |

La musica non è assegnata qui: la porta ogni stanza con il proprio `RoomMusic`,
e `RoomManager` la passa a `AudioManager.PlayMusic` con dissolvenza.

### EventSystem

`EventSystem` + `InputSystemUIInputModule` (nuovo Input System, usato **solo**
per l'interazione con la UI; il gameplay legge l'input legacy `Input.GetKey`).

### RoomManager

Un solo GameObject che porta **due** componenti: `RoomManager` e
`DungeonGenerator`. Le stanze generate a runtime diventano suoi figli.

`RoomManager`:

| Campo | Valore |
|---|---|
| `startingRoom` | *None* — scritto a runtime da `DungeonGenerator.SetStartingRoom` |
| `finalRoom` | *None* — scritto a runtime da `DungeonGenerator.SetFinalRoom` |
| `pickupPrefabs` | `Pickup_Heal`, `Pickup_Speed`, `Pickup_Bomb` |
| `pickupDropChance` | 0.35 |
| `pickupRadius` | 0.3 |
| `placementStep` | 0.5 |
| `maxPlacementAttempts` | 12 |
| `creditPrefab` | `Pickup_Credit` |
| `creditScatter` | 0.35 |
| `permanentUpgradePrefabs` | `Pickup_MaxHealthUp`, `Pickup_DamageUp`, `Pickup_SpeedUp`, `Pickup_FireRateUp`, `HealthRegen`, `MeleeArcUp`, `PickUp_ArmorUp`, `ProjectileBounceUp` |
| `minimap` | `Canvas` (componente `MinimapController`) |
| `pixelPerfectCamera` | `Main Camera` |
| `defaultReferenceResolution` | `240 × 135` |
| `doorIgnoreDelay` | 0.25 |

`permanentUpgradePrefabs` è letto anche da `DungeonGenerator` per riempire i
piedistalli della stanza tesoro e il bancone del negozio: la lista è una sola.

`DungeonGenerator`: vedi [sezione 5](#5-dungeongenerator).

### GameManager

| Campo | Valore |
|---|---|
| `gameOverPanel` | `Canvas/GameOverPanel` |
| `victoryPanel` | `Canvas/VictoryPanel` |
| `pausePanel` | `Canvas/PausePanel` |

---

## 2. Struttura del Canvas

`Canvas` in **Screen Space – Overlay**, con `CanvasScaler` (modo *Constant Pixel
Size*, `scaleFactor 1`, `referencePixelsPerUnit 100`), `GraphicRaycaster` e il
componente `MinimapController`.

```
Canvas
├── CreditText            TextMeshProUGUI
├── BombText              TextMeshProUGUI
├── HealthText            TextMeshProUGUI
├── PausePanel            (disattivato) Image
│   ├── PauseText         TextMeshProUGUI
│   ├── RestartButton     Image + Button → GameManager.RestartGame
│   │   └── Text (TMP)
│   ├── ResumeButton      Image + Button → GameManager.ResumeGame
│   │   └── Text (TMP)
│   └── MenuButton        Image + Button → GameManager.GoToMainMenu
│       └── Text (TMP)
├── UpgradePopUpText      (disattivato) TextMeshProUGUI
├── SlowtimeOverlay       (disattivato) Image
├── AbilityText           TextMeshProUGUI
├── WeaponText            TextMeshProUGUI
├── GameOverPanel         (disattivato) Image
│   ├── MenuButton        Image + Button → GameManager.GoToMainMenu
│   │   └── Text (TMP)
│   ├── RestartButton     Image + Button → GameManager.RestartGame
│   │   └── Text (TMP)
│   └── GameOverText      TextMeshProUGUI
├── MinimapPanel          solo RectTransform (contenitore delle celle)
└── VictoryPanel          (disattivato) TextMeshProUGUI
    ├── Sfondo            Image
    ├── MainMenuButton    Image + Button → GameManager.GoToMainMenu
    │   └── Text (TMP)
    └── PlayAgainButton (1)  Image + Button → GameManager.RestartGame
        └── Text (TMP)
```

Le tre righe di testo dell'HUD sono ancorate in alto a sinistra e incolonnate:
`CreditText` a `y −153`, `AbilityText` a `y −191`, entrambe larghe `350 × 50`.

`MinimapPanel` è ancorato **in alto a destra** (`anchor 1,1`, `pivot 1,1`),
`420 × 420`, `anchoredPosition (250, 300)`. Non ha `Image`: è solo il
contenitore in cui `MinimapController` istanzia le celle.

`SlowtimeOverlay` è un `Image` a schermo pieno (anchor `0,0`–`1,1`, `sizeDelta 0`)
con lo sprite `vfx_sandevistan_96` e **Raycast Target spento**, così non
intercetta i click dei pannelli. Nasce disattivato: lo accende e lo spegne
`PlayerAbilities` con l'effetto Rallenta Tempo.

---

## 3. Tag e Layer

Tag personalizzati definiti in `TagManager.asset` — cinque:

| Tag | Usato da |
|---|---|
| `EnemyProjectile` | `EnemyProjectile`; cercato da `PlayerAbilities.Shockwave`, che li distrugge nel raggio |
| `Enemy` | tutti i prefab nemico; conteggio stanza di `RoomManager`, danno di bombe, proiettili, scatto e onda d'urto, `TeleporterController.IsFree` |
| `PlayerProjectile` | `Projectile` |
| `Door` | oggetti porta nei template; `RoomManager.SetDoorsActive` li cerca per tag |
| `Wall` | muri e ostacoli nei template; `RoomNavGrid`, `Pathfinder`, `EnemyBase`, `RoomManager` e `DungeonGenerator` li cercano per tag |

Il tag `Player`, usato da `PlayerController`, `EnemyBase`, `ShopRoom` e
`RoomManager`, è quello predefinito di Unity.

**Layer**: nessun layer personalizzato, solo i predefiniti (`Default`,
`TransparentFX`, `Ignore Raycast`, `Water`, `UI`); tutti gli oggetti di gioco
stanno su `Default`. Un solo Sorting Layer, `Default`.

> Nota: essendoci un solo Sorting Layer, il `sortingOrder` è l'unico ordine di
> disegno disponibile. È il motivo per cui `PlayerVisuals.weaponSortingOffset`
> lavora in `sortingOrder` e non in layer.

---

## 4. Template di stanza (`Assets/Prefabs/Rooms/`)

Dieci prefab: cinque stanze comuni pescate a caso, e cinque con un ruolo
dedicato (boss, miniboss, tesoro, negozio, segreta).

### Struttura comune

Ogni template ha sulla radice:

- **`RoomBounds`** — `halfWidth` / `halfHeight`, le semi-dimensioni interne
  misurate dal centro della stanza. Letto da `TeleporterController.Start()` e
  richiesto da `RoomNavGrid` (`[RequireComponent]`).
- **`RoomNavGrid`** — `cellSize` e `agentRadius`. Ora serializzati in **tutti**
  i template (`agentRadius 0.7` ovunque).
- **`RoomMusic`** — `combatClip` e `clearedClip`; una clip vuota significa
  silenzio in quello stato.
- **`RoomCameraSettings`** — presente **solo** su `Boss_Room`.

E come figli:

- **4 muri perimetrali** `Wall_Top` / `Wall_Left` / `Wall_Bottom` / `Wall_Right`,
  tag `Wall`, con `SpriteRenderer` e `BoxCollider2D`.
- **`Grid` → `Floor`**, tilemap del pavimento (`Grid`, `Tilemap`, `TilemapRenderer`).
- **4 porte** `Door_N` / `Door_S` / `Door_E` / `Door_W`, tag `Door`, con
  `SpriteRenderer`, `BoxCollider2D` (trigger) e `DoorTrigger`.
- **punti di spawn** `Spawn_0`, `Spawn_1`, … dove previsti; nelle stanze speciali
  al loro posto ci sono `Pedestal_*` (tesoro) o `Slot_*` (negozio).

### `DoorTrigger` nei template

Il campo `direction` è l'enum `DoorTrigger.Direction` serializzato come intero:

| Valore | Direzione | Porta |
|---|---|---|
| `0` | North | `Door_N` |
| `1` | South | `Door_S` |
| `2` | East | `Door_E` |
| `3` | West | `Door_W` |

`arrivalPosition` è il punto in cui viene messo il player quando entra **in
quella stanza** attraversando **quella porta**:

| Porta | `direction` | 8×8 | `Boss_Room` | `MiniBoss_Room` |
|---|---|---|---|---|
| `Door_N` | 0 | `(0, 2.8)` | `(0, 3.8)` | `(0, 2.8)` |
| `Door_S` | 1 | `(0, −2.8)` | `(0, −3.8)` | `(0, −2.8)` |
| `Door_E` | 2 | `(2.8, 0)` | `(7.8, 0)` | `(4.8, 0)` |
| `Door_W` | 3 | `(−2.8, 0)` | `(−7.8, 0)` | `(−4.8, 0)` |

In **tutti** i template `targetRoom` è *None*: viene assegnato a runtime da
`DungeonGenerator.ConnectDoors` tramite `DoorTrigger.Connect`. Anche
`playerSpawnPosition` è sovrascritto a runtime, quindi i valori salvati nei
prefab non hanno effetto.

> Nota: `targetRoom` a *None* è anche il criterio con cui
> `RemoveUnconnectedDoors` riconosce le porte da distruggere. In Play, una porta
> con `targetRoom` diverso da *None* è una porta collegata.

### Stanze comuni (array `roomPrefabs`)

Tutte con `RoomBounds 4 × 4`, `RoomNavGrid cellSize 0.5`, muri a ±4, porte a ±4,
`RoomMusic` `battleUx` / `spaceWhales`.

| Prefab | Punti di spawn | Ostacoli (figli di `Obstacle`, tag `Wall`) |
|---|---|---|
| `Room_8x8` | `Spawn_0` (0,0), `Spawn_1` (−3,−3), `Spawn_2` (3,−3), `Spawn_3` (3,3), `Spawn_4` (−3,3), `Spawn_5` (1,1), `Spawn_6` (0,−1) | nessuno |
| `Room_8x8 debris` | `Spawn_1` (−3,−3), `Spawn_2` (3,−3), `Spawn_3` (3,3), `Spawn_4` (−3,3), `Spawn_5` (1,2), `Spawn_6` (−1,2) | `Obj` (0,0) — un blocco centrale |
| `Room_8x8 obstacle` | `Spawn_0` (0,0), `Spawn_1` (−2,−3), `Spawn_2` (3,−3), `Spawn_3` (3,3) | `debris` (0, 1.5), `debris (1)` (0, −1.7), `debris (2)` (−2, 0.9), `debris (3)` (2.4, 2) |
| `Room_8x8 path` | `Spawn_0` (0,0), `Spawn_1` (−2,−3), `Spawn_2` (3,−3), `Spawn_3` (3,3) | `Path` ×6: (0, ±1.5), (1, ±1.5), (−1, ±1.5) — due barriere orizzontali |
| `Room_8x8 pillars` | `Spawn_0` (0,0), `Spawn_1` (−2,−3), `Spawn_2` (3,−3), `Spawn_3` (3,3) | `Pillar` ×4 a (±2, ±2) |

> Nota: i contenuti di `Room_8x8 debris` e `Room_8x8 obstacle` sono invertiti
> rispetto ai nomi dei file — il prefab *debris* contiene un oggetto chiamato
> `Obj`, quello *obstacle* contiene quattro oggetti chiamati `debris`.

I punti di spawn di `Room_8x8 debris` hanno anche uno `SpriteRenderer`; negli
altri template sono GameObject vuoti.

> Nota: con `spawnDoorClearance` a 2.5 (vedi sezione 5) i segnaposto d'angolo a
> ±3 restano validi (3.16 unità dalle porte vicine) e così quelli centrali, a 4
> unità da ogni porta. Gli unici scartati sono `Spawn_5` (1,2) e `Spawn_6` (−1,2)
> di `Room_8x8 debris`, a 2.24 unità da `Door_N`, e solo quando quella porta è
> collegata: l'insieme valido dipende da quali porte il generatore ha collegato
> in quella stanza.

### `Room_8x8 treasure`

| | |
|---|---|
| `RoomBounds` | `4 × 4` |
| `RoomNavGrid` | `cellSize 0.5`, `agentRadius 0.7` |
| `RoomMusic` | `combatClip` *None*, `clearedClip` `intruding` |
| `TreasureRoom.pedestals` | `Pedestal_1` (2,2), `Pedestal_2` (0,0), `Pedestal_3` (−2,2) |
| Punti di spawn | nessuno |

Nessun nemico: `PopulateRoom` non trova nessun figlio `Spawn_*` ed esce subito,
quindi la stanza risulta già liberata all'ingresso.

### `Room_8x8 shop`

| | |
|---|---|
| `RoomBounds` | `4 × 4` |
| `RoomNavGrid` | `cellSize 0.5`, `agentRadius 0.7` |
| `RoomMusic` | `combatClip` *None*, `clearedClip` `intruding` |
| Punti di spawn | nessuno |

`ShopRoom`:

| Campo | Valore |
|---|---|
| `slots` | `Slot_0` (−3,2), `Slot_1` (−1,1), `Slot_2` (1,1), `Slot_3` (3,2) |
| `consumablePrefabs` | `Pickup_Heal`, `Pickup_Bomb` |
| `consumablePrice` | 2 |
| `upgradePrice` | 7 |
| `abilityPrice` | 10 |
| `abilityPickupPrefab` | `AbilityPickup` |
| `abilitySprites` | 4 sprite da `neo_zero_abilita_128x32` |
| `priceLabelPrefab` | `PriceLabel` |

Vedi [sezione 9](#9-negozio-e-stanza-tesoro) per come gli slot vengono riempiti.

### `Boss_Room`

| | |
|---|---|
| `RoomCameraSettings.referenceResolution` | `320 × 180` |
| `RoomBounds` | `halfWidth 8`, `halfHeight 4` |
| `RoomNavGrid` | `cellSize 0.25`, `agentRadius 0.7` |
| `RoomMusic` | `streetFight` / `spaceWhales` |
| Muri | `Wall_Left/Right` a x = ∓9, `Wall_Top/Bottom` a y = ±5 |
| Porte | `Door_W` (−9,0) · `Door_N` (0,5) · `Door_E` (9,0) · `Door_S` (0,−5) |
| Punti di spawn | nessuno |
| Contenuto | un'istanza del prefab `Boss` in `(0, 0)` |

### `MiniBoss_Room`

| | |
|---|---|
| `RoomBounds` | `halfWidth 5`, `halfHeight 4` |
| `RoomNavGrid` | `cellSize 0.25`, `agentRadius 0.7` |
| `RoomMusic` | `filature` / `spaceWhales` |
| Muri | `Wall_Left/Right` a x = ∓6, `Wall_Top/Bottom` a y = ±4 |
| Porte | `Door_W` (−6,0) · `Door_N` (0,4) · `Door_E` (6,0) · `Door_S` (0,−4) |
| Punti di spawn | nessuno |
| Contenuto | un'istanza del prefab `MiniBoss` in `(0, 0)` |

Il miniboss arriva quindi dal prefab della stanza, non dal codice: il campo
`DungeonGenerator.minibossPrefab` è **non assegnato**, e `SpawnMiniboss` esce
senza fare nulla.

### `Room_Secret`

`RoomBounds 4 × 4`, `RoomNavGrid cellSize 0.5`, `RoomMusic` *None* / `intruding`,
quattro muri, quattro porte complete di `DoorTrigger`, nessun punto di spawn,
nessun nemico.

Non contiene nessun componente `SecretWall`: è `DungeonGenerator` ad aggiungerlo
a runtime al muro della stanza ospite, tingendolo con `secretWallTint`.

---

## 5. `DungeonGenerator`

Sul GameObject `RoomManager`, accanto al componente omonimo. Gira in `Awake()`.

| Campo | Valore in scena |
|---|---|
| `roomPrefabs` | `Room_8x8`, `Room_8x8 debris`, `Room_8x8 obstacle`, `Room_8x8 path`, `Room_8x8 pillars` |
| `bossRoomPrefab` | `Boss_Room` |
| `minibossRoomPrefab` | `MiniBoss_Room` |
| `secretRoomPrefab` | `Room_Secret` |
| `treasureRoomPrefab` | `Room_8x8 treasure` |
| `shopRoomPrefab` | `Room_8x8 shop` |
| `secretWallTint` | `0.85, 0.85, 0.9, 1` |
| `roomCount` | 16 |
| `gridSize` | 9 |
| `enemyPrefabs` | `Dasher`, `Shielded`, `Teleporter`, `Turret`, `Walker`, `Splitter` |
| `minibossPrefab` | *None* |
| `minEnemiesPerRoom` | 1 |
| `maxEnemiesPerRoom` | 3 |
| `spawnDoorClearance` | non serializzato → default `2.5` |
| `minibossDistanceBias` | 1 |
| `minimap` | `Canvas` (`MinimapController`) |
| `randomSeed` | 0 (seed casuale a ogni partita) |

Il primo elemento di `roomPrefabs` (`Room_8x8`) è il template usato per la
stanza iniziale; gli altri vengono pescati a caso per le stanze comuni.

> Nota: `gridSize 9` con `roomCount 16` significa che le 16 celle devono stare
> in una griglia 9 × 9 partendo dal centro; è il limite entro cui l'espansione a
> coda si muove, non il numero di stanze.

Sequenza eseguita in `Awake()`:

1. `ValidateRoomPrefabs()` — controlla che ogni prefab stanza abbia esattamente
   quattro `DoorTrigger`, uno per direzione, e stampa `Debug.LogError` sui
   prefab incompleti o con direzioni duplicate.
2. `GenerateLayout()` — espansione a coda su griglia `gridSize × gridSize` dalla
   cella centrale fino a `roomCount` celle.
3. Cella boss = distanza di Manhattan massima dalla partenza.
4. `CellsReachableWithoutBoss()` — visita in ampiezza dalla partenza che tratta
   la cella boss come un muro. Serve a tesoro e negozio: entrare nella stanza
   boss chiude la partita, quindi una stanza raggiungibile solo passando di lì
   non sarebbe mai visitabile.
5. Cella miniboss (roulette wheel pesata su `distanza ^ minibossDistanceBias`,
   escluse partenza, cella boss e celle adiacenti alla partenza).
6. Cella tesoro: preferite le celle con **un solo vicino** (vicoli ciechi), e in
   ogni caso solo fra quelle raggiungibili senza il boss; se non ne resta
   nessuna si ripiega su una cella qualsiasi dello stesso insieme.
7. Cella negozio: stesso criterio del tesoro, esclusa la cella del tesoro.
8. `InstantiateRooms()` — un prefab per cella, figlio del `RoomManager`,
   disattivato.
9. `ConnectDoors()` — collega le porte delle celle adiacenti e registra le
   adiacenze reali per la minimappa.
10. `AttachSecretRoom()` — istanzia `Room_Secret` fuori dal grafo e la collega a
    una stanza ospite tramite un `SecretWall` aggiunto a runtime.
11. `RemoveUnconnectedDoors()` — distrugge il GameObject delle porte rimaste
    senza `targetRoom`.
12. `PopulateRooms()` — nemici sui figli `Spawn_*`, esclusa la stanza iniziale,
    quella boss e quella del miniboss.
13. `SetStartingRoom` / `SetFinalRoom` sul `RoomManager`, poi
    `PopulateTreasureRoom` e `PopulateShopRoom` con
    `RoomManager.PermanentUpgradePrefabs`.
14. `MinimapController.Build(...)` e `VerifyConnectivity` (visita in ampiezza sui
    collegamenti reali, `Debug.LogError` sulle stanze irraggiungibili).

### Piazzamento dei nemici

`PopulateRoom` sceglie sempre da `minEnemiesPerRoom` a `maxEnemiesPerRoom`
nemici, indipendentemente da quanti segnaposto ci sono:

1. raccoglie i figli `Spawn_*` della stanza;
2. scarta quelli a meno di `spawnDoorClearance` da una porta **collegata**
   (distanza in 2D); se non ne resta nessuno, li tiene tutti;
3. mescola i validi e li assegna in ordine; finito il giro ricomincia da capo
   riusando gli stessi segnaposto, con uno scarto casuale di `0.4` unità
   (costante privata `SpawnReuseScatter`) perché due nemici non nascano
   sovrapposti.

> Nota: i nemici piazzati qui **non** vengono registrati con
> `RegisterEnemySpawn`. Li conta `RoomManager.CountEnemiesInRoom` all'ingresso
> nella stanza; registrarli li conterebbe due volte.

---

## 6. Prefab dei nemici

Tutti hanno tag `Enemy`, `SpriteRenderer`, `Rigidbody2D` e un `Collider2D`.
I primi campi di ogni componente sono quelli ereditati da `EnemyBase`.

| Prefab | Script | Vita | Danno contatto | Altro |
|---|---|---|---|---|
| `Walker` | `EnemyController` | 4 | 1 | `moveSpeed 2`, `EnemyVisuals` |
| `Splitter` | `SplitterController` | 8 | 1 | `moveSpeed 1.2`, `splitPrefab` → `Splitterling 1`, `splitCount 2`, `splitSpread 0.6`, `EnemyVisuals` |
| `Splitterling 1` | `SplitterController` | 1 | 1 | `moveSpeed 3.5`, `splitPrefab` *None* (non si divide oltre), `EnemyVisuals` |
| `Shielded` | `ShieldedController` | 5 | 1 | `moveSpeed 1.3`, `shieldArc 0.3`, `shieldRotationSpeed 30`, `shieldVisual` → figlio `ShieldPivot`, `EnemyVisuals` |
| `Dasher` | `DasherController` | 4 | 2 | `idleDuration 1.5`, `telegraphDuration 0.6`, `dashDuration 0.4`, `dashSpeed 12`, `idleMoveSpeed 1`, `telegraphColor` viola scuro, `EnemyVisuals` |
| `Teleporter` | `TeleporterController` | 2 | 1 | `idleDuration 1.8`, `vanishDuration 0.4`, distanza dal player 2.5–4, `roomHalfWidth/Height 3` (sovrascritti da `RoomBounds`), `obstacleMargin 0.25`, `maxPlacementAttempts 12`, `arrivalIndicatorPrefab` → `TeleporterIndicator`, `projectilePrefab` → `EnemyProjectile`, `projectileSpeed 7` |
| `Turret` | `TurretController` | 4 | 1 | `projectilePrefab` → `EnemyProjectile`, `projectileSpeed 6`, `fireInterval 2`, `directionSprites` (8 sprite) |
| `MiniBoss` | `MinibossController` | 25 | 2 | `creditDrop 5`, `creditDropChance 1` |
| `Boss` | `BossController` | 60 | 2 | `creditDrop 10`, `creditDropChance 1` |

`hitFlashDuration` è `0.15` sul `Walker` e `0.08` su tutti gli altri;
`deathEffectDuration` è `0.2` ovunque.

### `MiniBoss`

`moveSpeed 1.5`, `moveDuration 1.5`, `telegraphDuration 0.4`, `chargeSpeed 7`,
`chargeDuration 0.4`, `enrageHealthRatio 0.4`, `enrageSpeedMultiplier 1.4`,
`enrageColor (1, 0.3, 0.1)`, `deathShakeDuration 0.25`,
`deathShakeMagnitude 0.12`, `projectilesPerBurst 5`, `aimedBurstCount 3`,
`aimedSpread 12`, `projectilePrefab` → `EnemyProjectile`, `projectileSpeed 5`.

Figlio `HealthBar` (Canvas in world space) con `Background` e `Fill`, collegati a
`healthBarRoot` e `healthBarFill`.

### `Boss`

`moveSpeed 1.2`, `moveDuration 2`, `telegraphDuration 0.5`, `chargeSpeed 9`,
`chargeDuration 0.5`, `enrageHealthRatio 0.15`, `enrageSpeedMultiplier 1.3`,
`enrageColor` rosso pieno, `deathShakeDuration 0.3`, `deathShakeMagnitude 0.15`.

Azioni: `radialCount 8`, `volleySpread 15`, `spiralWaveCount 3`,
`spiralBulletsPerWave 5`, `spiralWaveInterval 0.3`, `spiralRotationStep 20`,
`spiralProjectileSpeed 3.5`, `maxChargeChain 2`, `chargeChainChance 0.5`,
`minionPrefab` → `Dasher`, `minionCount 2`, `minionSpawnRadius 1.2`.

Colori di fase: `enragePulseSpeed 6`, `phase2Color (1, 0.6, 0.2)`,
`phase3Color (1, 0.2, 0.2)`.

Figlio `HealthBar` come il MiniBoss.

---

## 7. Prefab di proiettili, bombe ed effetti

| Prefab | Tag | Componenti | Valori |
|---|---|---|---|
| `Projectile` | `PlayerProjectile` | `SpriteRenderer`, `Rigidbody2D`, `CircleCollider2D`, `ProjectileController` | `lifetime 3` (danno, perforazione e rimbalzi li imposta `PlayerController` a runtime) |
| `EnemyProjectile` | `EnemyProjectile` | `SpriteRenderer`, `Rigidbody2D`, `CircleCollider2D`, `EnemyProjectileController` | `lifetime 4`, `damage 1` |
| `Bomb` | — | `SpriteRenderer`, `BombController` | `fuseTime 2`, `explosionRadius 1.5`, `explosionDamage 3`, `explosionEffectPrefab` → `BombExplosion`, `fuseSprites` (4 da `neo_zero_bomba_64x16`) |
| `BombExplosion` | — | `SpriteRenderer`, `ExplosionEffect` | `duration 0.3`, `maxScale 3` |
| `TeleporterIndicator` | — | `SpriteRenderer` | nessun collider |
| `MinimapCell` | — | `RectTransform`, `CanvasRenderer`, `Image` | `16 × 16` |
| `PriceLabel` | — | `Canvas` (World Space), `CanvasScaler`, `GraphicRaycaster`, figlio `Text` (TMP) | `localScale 0.0125`, `sizeDelta 250 × 50` |

> Nota: `PriceLabel` è un Canvas in world space appeso alla merce. La sua scala
> minuscola è voluta — `ShopRoom.CompensateScale` la ricalcola dividendo per la
> scala del pickup che lo ospita, così tutti i prezzi si leggono della stessa
> dimensione anche su pickup di scale diverse.

---

## 8. Pickup e crediti

Tutti i pickup hanno `SpriteRenderer`, `CircleCollider2D` (trigger) e
`PickupController`. Il campo `pickupType` è l'enum `PickupController.PickupType`
serializzato come intero.

| Prefab | `pickupType` | Effetto | Gruppo |
|---|---|---|---|
| `Pickup_Heal` | 0 `Heal` | `healAmount 2` | drop di stanza, negozio |
| `Pickup_Speed` | 1 `SpeedBoost` | `speedMultiplier 1.5` per `boostDuration 5` | drop di stanza |
| `Pickup_Bomb` | 2 `Bomb` | `bombAmount 1` | drop di stanza, negozio |
| `Pickup_MaxHealthUp` | 3 `MaxHealthUp` | `maxHealthIncrease 2` | permanente |
| `Pickup_DamageUp` | 4 `DamageUp` | `damageIncrease 1` | permanente |
| `Pickup_SpeedUp` | 5 `SpeedUp` | `speedIncrease 1` | permanente |
| `Pickup_FireRateUp` | 6 `FireRateUp` | `fireRateIncrease 0.05` | permanente |
| `HealthRegen` | 7 `HealthRegenUp` | `healthRegenChanceIncrease 0.15` | permanente |
| `PickUp_ArmorUp` | 8 `ArmorUp` | `contactDamageReductionIncrease 1` | permanente |
| `MeleeArcUp` | 9 `MeleeArcUp` | `meleeArcIncrease 0.15` | permanente |
| `ProjectileBounceUp` | 10 `ProjectileBounceUp` | `projectileBounceIncrease 1` | permanente |
| `Pickup_Credit` | 11 `Credit` | `creditAmount 1` | crediti |

Ogni prefab porta comunque **tutti** i campi di `PickupController` con gli stessi
valori; conta solo quello corrispondente al proprio `pickupType`.

I tre pickup di stanza sono in `RoomManager.pickupPrefabs`, gli otto permanenti
in `RoomManager.permanentUpgradePrefabs`, `Pickup_Credit` in
`RoomManager.creditPrefab`.

### Catena dei crediti

`EnemyBase.Die()` chiama `RoomManager.SpawnCredits` con `creditDrop` monete, ma
solo se `Random.value <= creditDropChance`. `SpawnCredits` le sparpaglia entro
`creditScatter 0.35` e le appende alla stanza corrente. Raccogliendole,
`PickupController.ApplyEffect` chiama `PlayerController.AddCredits`, che
aggiorna `creditText` (`"Crediti: N"`).

Valori di `EnemyBase` sui prefab: `creditDrop`/`creditDropChance` sono
serializzati solo su `Boss` (10, 1) e `MiniBoss` (5, 1); su tutti gli altri
valgono i default dello script, cioè **1 moneta con probabilità 0.5**.

`PickupController` ha due membri che riguardano il negozio:

- `IsForSale` — proprietà pubblica **non serializzata**. Se vera,
  `OnTriggerEnter2D` esce subito: la merce esposta non si raccoglie camminandoci
  sopra.
- `ApplyEffect(PlayerController)` — applica l'effetto senza distruggere
  l'oggetto né avvisare la `TreasureRoom`. È il punto unico da cui l'effetto
  passa, chiamato sia dal trigger sia da `ShopItem` a pagamento riuscito.

---

## 9. Negozio e stanza tesoro

### Stanza tesoro

`TreasureRoom.Populate` riceve `permanentUpgradePrefabs`, li mescola e ne mette
uno per piedistallo, tutti diversi. Il player ne prende **uno solo**: alla
raccolta, `PickupController` avvisa `TreasureRoom.OnUpgradeTaken` e gli altri
spariscono.

> Nota: `Pedestal_2` sta in `(0, 0)`, cioè al centro della stanza. È il punto in
> cui `RoomManager` fa cadere il pickup di fine stanza, ma qui non c'è
> conflitto: la stanza tesoro non ha nemici, quindi non c'è nessun drop.

### Negozio

`ShopRoom.Populate` riempie i quattro slot leggendoli da fuori verso dentro:

| Slot | Merce | Prezzo |
|---|---|---|
| `Slot_0` | consumabile scelto a caso fra `consumablePrefabs` (può ripetersi) | `consumablePrice 2` |
| `Slot_1` | potenziamento permanente, mai due volte lo stesso | `upgradePrice 7` |
| `Slot_2`, `Slot_3` | un'abilità ciascuno, diverse fra loro, scelte fra quelle **non ancora sbloccate** dal player | `abilityPrice 10` |

Le costanti che decidono la ripartizione sono `ConsumableSlots = 1` e
`AbilitySlots = 2`: il primo slot ai consumabili, gli ultimi due alle abilità,
quelli in mezzo ai potenziamenti.

Se le abilità disponibili finiscono — il player ne possiede già tre o quattro,
oppure `abilityPickupPrefab` non è assegnato — lo slot torna a vendere un
potenziamento permanente.

> Nota: il negozio viene popolato durante `DungeonGenerator.Awake`, cioè a inizio
> partita. Le abilità in vendita sono quindi scelte fra quelle non possedute **in
> quel momento**, non all'ingresso nella stanza.

Componenti coinvolti:

- **`ShopItem`** — aggiunto a runtime a ogni pezzo di merce. Tiene `price` e il
  riferimento al cartellino; al contatto col player chiama `SpendCredits` e, se
  il pagamento riesce, fa consegnare la merce e distrugge oggetto e cartellino.
  Se i crediti non bastano mostra "Crediti insufficienti" e non tocca niente.
  In `Awake` cerca sia un `PickupController` sia uno `ShopAbility`: consegna
  chiamando `ApplyEffect` sul primo o `Grant` sul secondo, e stampa un
  `Debug.LogWarning` se non trova nessuno dei due.
- **`ShopAbility`** — il corrispettivo di `PickupController` per le abilità.
  Porta un `PlayerAbilities.AbilityType`, scritto da `ShopRoom` con `Setup`, e
  in `Grant` chiama `PlayerAbilities.Unlock` più il popup `+<nome abilità>`.
- **`AbilityPickup`** — il prefab della merce-abilità: solo `SpriteRenderer` e
  `CircleCollider2D` (trigger, raggio 0.5), nessuno script. Sprite e componenti
  glieli mette `ShopRoom` a runtime.

> Nota: `ShopItem` **non** ha `[RequireComponent(typeof(PickupController))]`. Con
> l'attributo, l'`AddComponent<ShopItem>()` su un oggetto-abilità aggiungerebbe
> d'ufficio un `PickupController` con `pickupType` a zero (`Heal`), e comprando
> l'abilità il player si curerebbe invece di sbloccarla.

---

## 10. Abilità attive

`PlayerAbilities` sul GameObject `Player`, accanto a `PlayerController`. Quattro
abilità (`AbilityType { Dash, Shockwave, SlowTime, Shield }`), selezione ciclica
come le armi che salta quelle non possedute, un cooldown indipendente per
ciascuna.

| Gruppo | Campo | Valore in scena |
|---|---|---|
| Comandi | `switchKey` | `102` = `KeyCode.F` |
| | `useKey` | `114` = `KeyCode.R` |
| Cooldown | `dashCooldown` | 3 |
| | `shockwaveCooldown` | 8 |
| | `slowTimeCooldown` | 15 |
| | `shieldCooldown` | 12 |
| Scatto | `dashSpeed` | 14 |
| | `dashDuration` | 0.25 |
| | `dashDamage` | 3 |
| | `dashRadius` | 0.7 |
| | `dashTrailPrefab` | `DashTrail` |
| Onda d'urto | `shockwaveRadius` | 2.5 |
| | `shockwaveDamage` | 4 |
| | `shockwaveEffectPrefab` | `ShockwaveEffect` |
| Rallenta Tempo | `slowFactor` | 0.35 |
| | `slowDuration` | 4 |
| | `slowTimeOverlay` | `Canvas/SlowtimeOverlay` |
| Scudo | `shieldDuration` | 3 |
| | `shieldVisualPrefab` | `ShieldVisual` |
| UI | `abilityText` | `Canvas/AbilityText` |
| Test | `unlockAllForTesting` | `false` |

`abilityText` mostra `Abilità: <nome> (pronta)` oppure i secondi mancanti, e
`Abilità: nessuna` finché non se ne possiede una. I nomi visualizzati sono
Scatto, Onda d'urto, Rallenta Tempo, Scudo.

Cosa fa ciascuna:

- **Scatto** — muove il player nella direzione corrente (o in
  `PlayerController.LastMoveDirection` se è fermo), lo rende immune e danneggia
  i nemici attraversati con `Physics2D.OverlapCircleAll` a ogni `FixedUpdate`.
  Un `HashSet` garantisce un colpo solo per nemico per scatto. All'inizio
  istanzia `dashTrailPrefab` come figlio della stanza corrente, ruotato con
  `VectorUtils.ToAngle` verso la direzione dello scatto.
- **Onda d'urto** — istanzia `shockwaveEffectPrefab` sul player, danneggia i
  nemici nel raggio e distrugge i proiettili con tag `EnemyProjectile`.
- **Rallenta Tempo** — porta `Time.timeScale` a `slowFactor`, scala
  `Time.fixedDeltaTime` in proporzione, compensa la velocità del player con
  `1/slowFactor` e accende l'overlay.
- **Scudo** — immunità per `shieldDuration` secondi e `shieldVisualPrefab`
  istanziato come figlio del player.

Prefab degli effetti (tutti `SpriteRenderer` + `SpriteAnimator`, nessun collider):

| Prefab | `frames` | `frameRate` | `loop` | `destroyOnEnd` |
|---|---|---|---|---|
| `DashTrail` | 4 da `vfx_mantis_128x32` | 16 | no | sì |
| `ShockwaveEffect` | 4 da `shockwave_80_1` | 12 | no | sì |
| `ShieldVisual` | 4 da `shield_32` | 8 | sì | no |

> Nota: `ShieldVisual` è l'unico ciclico e con `destroyOnEnd` spento, perché non
> deve sparire da solo: lo distrugge `PlayerAbilities.EndShield` alla scadenza
> dell'immunità.

Ganci in `PlayerController` usati dalle abilità:

| Membro | A cosa serve |
|---|---|
| `SetMovementOverride` / `ClearMovementOverride` | lo scatto scrive la velocità al posto dell'input |
| `SetAbilityImmunity(bool)` / `IsImmune` | immunità a **contatore**, distinta da `isInvulnerable` e dal suo lampeggio; scatto e scudo possono sovrapporsi senza annullarsi a vicenda |
| `SetAbilitySpeedMultiplier` / `ClearAbilitySpeedMultiplier` | compensazione della velocità sotto Rallenta Tempo |
| `IsDead`, `MoveInput`, `LastMoveDirection` | guardia di `Update` e direzione dello scatto |

Chiusura degli effetti — tutti passano da `EndDash` / `EndSlowTime` /
`EndShield`, idempotenti, richiamati da:

- la scadenza normale;
- `OnDisable` (morte del player, ricarica della scena);
- `GameManager.IsPaused` / `IsGameOver`, controllati a ogni frame da Rallenta
  Tempo, che in quel caso **non** rimette `Time.timeScale` a 1: quello zero è
  della pausa;
- l'evento `RoomManager.RoomEntered`, a cui `PlayerAbilities` si iscrive in
  `OnEnable` e da cui si disiscrive in `OnDisable`, che chiude Rallenta Tempo a
  ogni cambio stanza.

> Nota: `Time.fixedDeltaTime` sopravvive al cambio di scena. È il motivo per cui
> il valore di partenza viene letto una volta in `Awake` e ripristinato in
> `OnDisable`: senza, una partita ricominciata durante l'effetto girerebbe con
> la fisica al 35%.

---

## 11. Minimappa

`MinimapController` sta sulla radice del `Canvas`; disegna le celle dentro
`MinimapPanel` a partire dal dizionario di stanze e adiacenze che riceve da
`DungeonGenerator.Build(...)`.

| Campo | Valore |
|---|---|
| `minimapPanel` | `MinimapPanel` |
| `cellPrefab` | `MinimapCell` |
| `cellSize` | 40 |
| `cellSpacing` | 5 |
| `toggleKey` | `9` = `KeyCode.Tab` |
| `visitedColor` | `0.8, 0.8, 0.85, 1` |
| `currentColor` | `1, 1, 1, 1` |
| `adjacentColor` | `0.35, 0.35, 0.4, 0.6` |
| `bossColor` | `0.9, 0.2, 0.2, 1` |
| `minibossColor` | `0.9, 0.5, 0.15, 1` |
| `treasureColor` | `0.95, 0.85, 0.3, 1` |
| `shopColor` | `0, 1, 0.067, 1` |
| `unvisitedTint` | 0.5 |

Le quattro stanze speciali (boss, miniboss, tesoro, negozio) usano il proprio
colore pieno se già visitate, e mescolato verso `adjacentColor` con
`unvisitedTint` se conosciute ma non ancora visitate. `SetCurrentRoom`, chiamato
da `RoomManager.EnterRoom`, sposta l'evidenziazione.

---

## 12. Pathfinding

Tre pezzi:

- **`RoomNavGrid`** (sulla radice di ogni template) costruisce in `Start()` una
  griglia di celle percorribili grande quanto il `RoomBounds`, marcando occupate
  le celle che intersecano un collider con tag `Wall`. Il test è gonfiato di
  `agentRadius` per lato: senza quel margine i percorsi passerebbero a filo
  degli spigoli e i nemici ci si incastrerebbero. Con `cellSize 0.5` e
  `agentRadius 0.7`, un passaggio più stretto di circa 1.5 unità sparisce dalla
  griglia.
- **`Pathfinder`** — classe **statica** (come `VectorUtils`: non va messa su
  nessun GameObject) con A* a 8 direzioni, `DiagonalCost 1.414` e un tetto di
  `MaxExploredNodes 500` nodi esplorati per ricerca.
- **`EnemyBase`** — campi di navigazione condivisi da tutti i nemici:

| Campo | Default | Valore sui prefab che lo serializzano |
|---|---|---|
| `obstacleCheckDistance` | 1 | 1 |
| `avoidanceStrength` | 1 | 1 |
| `pathRecalculateInterval` | 0.3 | 0.3 |
| `waypointReachedDistance` | 0.15 | 0.3 (0.15 su `Boss` e `MiniBoss`) |
| `stuckTimeout` | 0.5 | 0.5 (solo `Boss` e `MiniBoss`) |
| `stuckProgressFraction` | 0.25 | 0.25 (solo `Boss` e `MiniBoss`) |

`MoveTowardsPlayer` usa `rb.MovePosition` e non `rb.linearVelocity`: contro lo
spigolo di un ostacolo il solver azzererebbe la spinta e il nemico resterebbe
incastrato, mentre `MovePosition` scivola lungo il collider. Di conseguenza
`EnemyVisuals` legge `EnemyBase.MoveDirection` e non la velocità del
Rigidbody2D, che nel ramo con percorso è sempre zero.

---

## 13. Pausa e fine partita

`GameManager` legge ESC in `Update()` e alterna la pausa con `TogglePause`.

| Stato | `Time.timeScale` | Pannello | Flag |
|---|---|---|---|
| Gioco | 1 | nessuno | — |
| Pausa | 0 | `PausePanel` | `isPaused` |
| Game over | 0 | `GameOverPanel` | `isGameOver` |
| Vittoria | 0 | `VictoryPanel` | `isGameOver` |

`isGameOver` disattiva la pausa: senza, ESC rimetterebbe `timeScale` a 1 sopra
la schermata di fine partita facendo ripartire il gioco sotto il pannello. Sia
`ShowGameOver` sia `ShowVictory` nascondono prima il pannello di pausa, perché
si può vincere da gioco in pausa (`RoomManager` controlla la vittoria
all'ingresso in stanza).

> Nota: `Time.timeScale = 0` ferma la fisica ma **non** `Update()`. Ogni script
> che legge l'input deve uscire subito quando il gioco è fermo — la guardia in
> cima a `PlayerController.Update()` e a `PlayerAbilities.Update()`. Fa
> eccezione `GameManager.Update()`, che deve continuare a leggere ESC per
> togliere la pausa.

`RestartGame` e `GoToMainMenu` rimettono `timeScale` a 1 prima di caricare la
scena. `MainMenu` è l'indice 0, `SampleScene` è il gioco.

---

## 14. Script (`Assets/Scripts/`)

39 file.

**Player**
`PlayerController` (input, 4 modalità di fuoco, bombe, vita, crediti,
potenziamenti) · `PlayerVisuals` (sprite direzionali e arma orbitante) ·
`PlayerAbilities` (le quattro abilità attive)

**Nemici**
`EnemyBase` (astratta: vita, morte, danno da contatto, crediti, inseguimento con
pathfinding e aggiramento, `MoveDirection`) · `EnemyController` (Walker) ·
`SplitterController` · `ShieldedController` · `DasherController` ·
`TeleporterController` · `TurretController` · `BossBase` · `BossController` ·
`MinibossController` · `EnemyVisuals`

**Dungeon e stanze**
`DungeonGenerator` · `RoomManager` · `RoomBounds` · `RoomNavGrid` ·
`RoomCameraSettings` · `RoomMusic` · `DoorTrigger` · `SecretWall` · `Pathfinder`

**Stanze speciali**
`TreasureRoom` · `ShopRoom` · `ShopItem` · `ShopAbility`

**Proiettili, bombe, pickup**
`ProjectileController` · `EnemyProjectileController` · `BombController` ·
`ExplosionEffect` · `PickupController`

**Sistema e UI**
`GameManager` (game over, vittoria, pausa con ESC) · `MainMenuController` ·
`MinimapController` · `AudioManager` · `CameraShake` · `SpriteAnimator` ·
`VectorUtils` (statica)

---

## 15. Campi non ancora serializzati

Alcuni campi `[SerializeField]` aggiunti dopo l'ultimo salvataggio della scena o
dei prefab non compaiono nel loro YAML. Per questi vale il valore di default
scritto nel codice, e sarà quello che Unity serializzerà al primo risalvataggio.

| Componente | Campo | Default in uso |
|---|---|---|
| `DungeonGenerator` (in scena) | `spawnDoorClearance` | 2.5 |
| `EnemyBase` su `Walker`, `Splitter`, `Splitterling 1`, `Shielded`, `Dasher`, `Teleporter`, `Turret` | `creditDrop`, `creditDropChance` | 1, 0.5 |
| `EnemyBase` sugli stessi prefab | `stuckTimeout`, `stuckProgressFraction` | 0.5, 0.25 |
| `EnemyBase` su `Turret` | `obstacleCheckDistance`, `avoidanceStrength`, `pathRecalculateInterval`, `waypointReachedDistance` | 1, 1, 0.3, 0.15 |

---

## 16. Comandi di gioco

WASD movimento · frecce sparo · Q cambia arma · E bomba · F cambia abilità ·
R usa abilità · Tab mostra e nasconde la minimappa · ESC pausa
