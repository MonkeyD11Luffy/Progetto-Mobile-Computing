# SETUP — stato attuale della scena e degli asset

Documento ricavato leggendo direttamente `Assets/Scenes/SampleScene.unity`,
`ProjectSettings/TagManager.asset`, tutti i `.prefab` sotto `Assets/Prefabs/`
(inclusa la sottocartella `Rooms/`) e tutti gli script in `Assets/Scripts/`.

Descrive **solo lo stato attuale**: nessuna proposta di modifica.

Il dungeon **non è più costruito a mano nella scena**: `SampleScene` non contiene
nessuna stanza. Le stanze vengono istanziate a runtime da `DungeonGenerator` a
partire dai template in `Assets/Prefabs/Rooms/`.

---

## 1. Gerarchia di `SampleScene`

Otto oggetti radice:

```
AudioManager
Canvas
EventSystem
GameManager
Global Light 2D
Main Camera
Player
RoomManager
```

### AudioManager

Componente `AudioManager` con due sorgenti figlie e otto clip assegnate.

| Campo | Valore |
|---|---|
| `sfxSource` | figlio `SFX_source` (AudioSource) |
| `musicSource` | figlio `Music_source` (AudioSource) |
| `shootSingleClip`, `shootSpreadClip`, `shootPierceClip`, `meleeClip` | assegnate |
| `playerHurtClip`, `enemyDeathClip`, `explosionClip`, `pickupClip` | assegnate |
| `pitchVariation` | `0.2` |

Figli: `SFX_source`, `Music_source` (solo `AudioSource`).

### GameManager

| Campo | Valore |
|---|---|
| `gameOverPanel` | `Canvas/GameOverPanel` |
| `victoryPanel` | `Canvas/VictoryPanel` |
| `pausePanel` | `Canvas/PausePanel` |

### Main Camera

Tag `MainCamera`, posizione `(0, 0, -10)`. Componenti:

- `Camera`
- `AudioListener`
- `CameraShake`
- `UniversalAdditionalCameraData` (URP)
- `PixelPerfectCamera` — `Assets PPU: 16`, `Reference Resolution: 240 × 135`

È l'oggetto puntato da `RoomManager.pixelPerfectCamera`.

### Global Light 2D

Solo `Light2D` (URP 2D), tipo *Global*.

### Player

Tag `Player`. Componenti: `SpriteRenderer`, `Rigidbody2D`, `BoxCollider2D`,
`PlayerController`, `PlayerVisuals`.

Valori principali di `PlayerController`:

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

Riferimenti UI assegnati: `healthText`, `bombText`, `weaponText`,
`upgradePopupText`, `meleeVisual` (figlio `MeleeVisual`), `playerVisuals`.

`PlayerVisuals`: `frameRate 8`, `idleFrame 1`, `weaponOrbitRadius 0.28`,
`weaponSortingOffset 1`, `muzzleUp 0.19`, `weaponHideDelay 0.15`, array
`walkDown`/`walkUp`/`walkSide`/`weaponSprites`/`muzzleForward` popolati,
`weaponPivot` → figlio `WeaponPivot`.

Figli: `MeleeVisual` (disattivato, `SpriteRenderer`), `WeaponPivot` (`SpriteRenderer`).

### RoomManager

Un solo GameObject che porta **due** componenti: `RoomManager` e `DungeonGenerator`.
Le stanze generate a runtime diventano suoi figli.

`RoomManager`:

| Campo | Valore |
|---|---|
| `startingRoom` | *None* — assegnato a runtime da `DungeonGenerator.SetStartingRoom` |
| `finalRoom` | *None* — assegnato a runtime da `DungeonGenerator.SetFinalRoom` |
| `pickupPrefabs` | `Pickup_Heal`, `Pickup_Speed`, `Pickup_Bomb` |
| `pickupDropChance` | 0.35 |
| `pickupRadius` | 0.3 |
| `placementStep` | 0.5 |
| `maxPlacementAttempts` | 12 |
| `permanentUpgradePrefabs` | `Pickup_MaxHealthUp`, `Pickup_DamageUp`, `Pickup_SpeedUp`, `Pickup_FireRateUp`, `HealthRegen`, `MeleeArcUp`, `PickUp_ArmorUp`, `ProjectileBounceUp` |
| `minimap` | `Canvas` (componente `MinimapController`) |
| `pixelPerfectCamera` | `Main Camera` |
| `defaultReferenceResolution` | `240 × 135` |
| `doorIgnoreDelay` | 0.25 |

`DungeonGenerator`: vedi [sezione 5](#5-dungeongenerator).

---

## 2. Struttura del Canvas

`Canvas` in **Screen Space – Overlay**, con `CanvasScaler` (modo *Constant Pixel Size*),
`GraphicRaycaster` e il componente `MinimapController`.

```
Canvas
├── HealthText            TextMeshProUGUI
├── BombText              TextMeshProUGUI
├── WeaponText            TextMeshProUGUI
├── GameOverPanel         (disattivato) Image
│   ├── GameOverText      TextMeshProUGUI
│   ├── RestartButton     Image + Button → GameManager.RestartGame
│   │   └── Text (TMP)
│   └── MenuButton        Image + Button → GameManager.GoToMainMenu
│       └── Text (TMP)
├── PausePanel            (disattivato) Image
│   ├── PauseText         TextMeshProUGUI
│   ├── RestartButton     Image + Button → GameManager.RestartGame
│   │   └── Text (TMP)
│   ├── ResumeButton      Image + Button → GameManager.ResumeGame
│   │   └── Text (TMP)
│   └── MenuButton        Image + Button → GameManager.GoToMainMenu
│       └── Text (TMP)
├── VictoryPanel          (disattivato) TextMeshProUGUI
│   ├── Sfondo            Image
│   ├── MainMenuButton    Image + Button → GameManager.GoToMainMenu
│   │   └── Text (TMP)
│   └── PlayAgainButton (1)  Image + Button → GameManager.RestartGame
│       └── Text (TMP)
├── UpgradePopUpText      (disattivato) TextMeshProUGUI
└── MinimapPanel          solo RectTransform (contenitore delle celle)
```

`MinimapController` (sulla radice `Canvas`):

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

### EventSystem

`EventSystem` + `InputSystemUIInputModule` (nuovo Input System, usato **solo** per
l'interazione con la UI; il gameplay legge l'input legacy `Input.GetKey`).

---

## 3. Tag e Layer

Tag personalizzati definiti in `TagManager.asset` — cinque:

| Tag | Usato da |
|---|---|
| `Enemy` | tutti i prefab nemico; conteggio stanza di `RoomManager`, danno di bomba e proiettili, `TeleporterController.IsFree` |
| `EnemyProjectile` | `EnemyProjectile` |
| `PlayerProjectile` | `Projectile` |
| `Door` | oggetti porta nei prefab stanza; `RoomManager.SetDoorsActive` li cerca per tag |
| `Wall` | muri e ostacoli nei prefab stanza; `RoomNavGrid`, `Pathfinder`, `EnemyBase`, `RoomManager` e `DungeonGenerator` li cercano per tag |

Il tag `Player` usato da `PlayerController`/`EnemyBase` è quello predefinito di Unity.

**Layer**: nessun layer personalizzato. Sono presenti solo i predefiniti
(`Default`, `TransparentFX`, `Ignore Raycast`, `Water`, `UI`), e tutti gli oggetti
di gioco stanno su `Default`. Un solo Sorting Layer, `Default`.

---

## 4. Template di stanza (`Assets/Prefabs/Rooms/`)

Otto prefab. Cinque sono le stanze comuni pescate a caso, gli altri tre hanno un
ruolo dedicato.

### Struttura comune

Ogni template ha sulla radice:

- **`RoomBounds`** — `halfWidth` / `halfHeight`, cioè le semi-dimensioni interne
  misurate dal centro della stanza. Letto da `TeleporterController.Start()` e
  richiesto da `RoomNavGrid`.
- **`RoomNavGrid`** — `cellSize`. Il campo `agentRadius` **non è serializzato in
  nessun prefab**, quindi vale il default dello script (`0.7`).
- **`RoomCameraSettings`** — presente **solo** su `Boss_Room`.

E come figli:

- **4 muri perimetrali** `Wall_Top` / `Wall_Left` / `Wall_Bottom` / `Wall_Right`,
  tag `Wall`, con `SpriteRenderer` e `BoxCollider2D`.
- **`Grid` → `Floor`**, tilemap del pavimento (`Grid`, `Tilemap`, `TilemapRenderer`).
- **4 porte** `Door_N` / `Door_S` / `Door_E` / `Door_W`, tag `Door`, con
  `SpriteRenderer`, `BoxCollider2D` (trigger) e `DoorTrigger`.
- **punti di spawn** `Spawn_0`, `Spawn_1`, … dove previsti.

### `DoorTrigger` nei template

Il campo `direction` è l'enum `DoorTrigger.Direction` serializzato come intero:

| Valore | Direzione | Porta |
|---|---|---|
| `0` | North | `Door_N` |
| `1` | South | `Door_S` |
| `2` | East | `Door_E` |
| `3` | West | `Door_W` |

`arrivalPosition` è il punto in cui viene messo il player quando entra **in quella
stanza** attraversando **quella porta**. Nelle stanze 8×8 vale:

| Porta | `direction` | `arrivalPosition` |
|---|---|---|
| `Door_N` | 0 | `(0, 2.8)` |
| `Door_S` | 1 | `(0, -2.8)` |
| `Door_E` | 2 | `(2.8, 0)` |
| `Door_W` | 3 | `(-2.8, 0)` |

In **tutti** i template `targetRoom` è *None*: viene assegnato a runtime da
`DungeonGenerator.ConnectDoors` tramite `DoorTrigger.Connect`. Anche
`playerSpawnPosition` è sovrascritto a runtime, quindi i valori salvati nei prefab
non hanno effetto.

### Stanze comuni (array `roomPrefabs`)

Tutte con `RoomBounds 4 × 4` e `RoomNavGrid cellSize 0.5`, muri a ±4, porte a ±4.

| Prefab | Punti di spawn | Ostacoli (figli di `Obstacle`, tag `Wall`) |
|---|---|---|
| `Room_8x8` | `Spawn_0` (0,0), `Spawn_1` (−3,−3), `Spawn_2` (3,−3), `Spawn_3` (3,3), `Spawn_4` (−3,3), `Spawn_5` (1,1), `Spawn_6` (0,−1) | nessuno |
| `Room_8x8 debris` | `Spawn_1` (−3,−3), `Spawn_2` (3,−3), `Spawn_3` (3,3), `Spawn_4` (−3,3), `Spawn_5` (1,2), `Spawn_6` (−1,2) | `Obj` (0,0) — un blocco centrale |
| `Room_8x8 obstacle` | `Spawn_0` (0,0), `Spawn_1` (−2,−3), `Spawn_2` (3,−3), `Spawn_3` (3,3) | `debris` (0, 1.5), `debris (1)` (0, −1.7), `debris (2)` (−2, 0.9), `debris (3)` (2.4, 2) |
| `Room_8x8 path` | `Spawn_0` (0,0), `Spawn_1` (−2,−3), `Spawn_2` (3,−3), `Spawn_3` (3,3) | `Path` ×6: (0, ±1.5), (1, ±1.5), (−1, ±1.5) — due barriere orizzontali |
| `Room_8x8 pillars` | `Spawn_0` (0,0), `Spawn_1` (−2,−3), `Spawn_2` (3,−3), `Spawn_3` (3,3) | `Pillar` ×4 a (±2, ±2) |

> Nota: i contenuti di `Room_8x8 debris` e `Room_8x8 obstacle` sono invertiti
> rispetto ai nomi dei file — il prefab *debris* contiene un oggetto chiamato `Obj`,
> quello *obstacle* contiene quattro oggetti chiamati `debris`.

I punti di spawn di `Room_8x8 debris` hanno anche uno `SpriteRenderer`; negli altri
template sono GameObject vuoti.

### `Boss_Room`

| | |
|---|---|
| `RoomCameraSettings.referenceResolution` | `320 × 180` |
| `RoomBounds` | `halfWidth 8`, `halfHeight 4` |
| `RoomNavGrid.cellSize` | `0.25` |
| Muri | `Wall_Left/Right` a x = ∓9, `Wall_Top/Bottom` a y = ±5 |
| Porte | `Door_W` (−9,0) · `Door_N` (0,5) · `Door_E` (9,0) · `Door_S` (0,−5) |
| `arrivalPosition` | `(∓7.8, 0)` per E/W, `(0, ±3.8)` per N/S |
| Punti di spawn | nessuno |
| Contenuto | un'istanza del prefab `Boss` in `(0, 0)` |

### `MiniBoss_Room`

| | |
|---|---|
| `RoomBounds` | `halfWidth 5`, `halfHeight 4` |
| `RoomNavGrid.cellSize` | `0.25` |
| Muri | `Wall_Left/Right` a x = ∓6, `Wall_Top/Bottom` a y = ±4 |
| Porte | `Door_W` (−6,0) · `Door_N` (0,4) · `Door_E` (6,0) · `Door_S` (0,−4) |
| `arrivalPosition` | `(∓4.8, 0)` per E/W, `(0, ±2.8)` per N/S |
| Punti di spawn | nessuno |
| Contenuto | un'istanza del prefab `MiniBoss` in `(0, 0)` |

Il miniboss arriva quindi dal prefab della stanza, non dal codice: il campo
`DungeonGenerator.minibossPrefab` è **non assegnato**.

### `Room_Secret`

`RoomBounds 4 × 4`, `RoomNavGrid cellSize 0.5`, quattro muri, quattro porte
complete di `DoorTrigger`, nessun punto di spawn, nessun nemico.

Non contiene nessun componente `SecretWall`: è `DungeonGenerator` ad aggiungerlo a
runtime al muro della stanza ospite, tingendolo con `secretWallTint`.

---

## 5. `DungeonGenerator`

Sul GameObject `RoomManager`, accanto al componente omonimo. Gira in `Awake()`.

| Campo | Valore in scena |
|---|---|
| `roomPrefabs` | `Room_8x8`, `Room_8x8 debris`, `Room_8x8 obstacle`, `Room_8x8 path`, `Room_8x8 pillars` |
| `bossRoomPrefab` | `Boss_Room` |
| `minibossRoomPrefab` | `MiniBoss_Room` |
| `secretRoomPrefab` | `Room_Secret` |
| `secretWallTint` | `0.85, 0.85, 0.9, 1` |
| `roomCount` | 8 |
| `gridSize` | 9 |
| `enemyPrefabs` | `Dasher`, `Shielded`, `Teleporter`, `Turret`, `Walker`, `Splitter` |
| `minibossPrefab` | *None* |
| `minEnemiesPerRoom` | 1 |
| `maxEnemiesPerRoom` | 3 |
| `minibossDistanceBias` | 1 |
| `minimap` | `Canvas` (`MinimapController`) |
| `randomSeed` | 0 (seed casuale a ogni partita) |

Il primo elemento di `roomPrefabs` (`Room_8x8`) è il template usato per la stanza
iniziale; gli altri vengono pescati a caso per le stanze comuni.

Sequenza eseguita in `Awake()`:

1. `ValidateRoomPrefabs()` — controlla che ogni prefab stanza abbia esattamente
   quattro `DoorTrigger`, uno per direzione, e stampa `Debug.LogError` sui prefab
   incompleti o con direzioni duplicate.
2. `GenerateLayout()` — espansione a coda su griglia `gridSize × gridSize` dalla
   cella centrale fino a `roomCount` celle.
3. Scelta della cella boss (distanza di Manhattan massima dalla partenza) e della
   cella miniboss (roulette wheel pesata su `distanza ^ minibossDistanceBias`,
   escluse partenza, cella boss e celle adiacenti alla partenza).
4. `InstantiateRooms()` — un prefab per cella, figlio del `RoomManager`, disattivato.
5. `ConnectDoors()` — collega le porte delle celle adiacenti e registra le adiacenze
   reali per la minimappa.
6. `AttachSecretRoom()` — istanzia `Room_Secret` fuori dal grafo e la collega a una
   stanza ospite tramite un `SecretWall` aggiunto a runtime.
7. `RemoveUnconnectedDoors()` — distrugge il GameObject delle porte rimaste senza
   `targetRoom`.
8. `PopulateRooms()` — nemici sui figli `Spawn_*`, esclusa la stanza iniziale, quella
   boss e quella del miniboss.
9. `SetStartingRoom` / `SetFinalRoom` sul `RoomManager`, `MinimapController.Build`,
   `VerifyConnectivity` (visita in ampiezza sui collegamenti reali, `Debug.LogError`
   sulle stanze irraggiungibili).

Sono attivi due `Debug.Log` temporanei: distanze di boss e miniboss dalla partenza
(in `Awake`), e il rilevamento di stallo dei nemici (in `EnemyBase`).

---

## 6. Prefab dei nemici

Tutti hanno tag `Enemy`, `SpriteRenderer`, `Rigidbody2D` e un `Collider2D`.
I valori di `EnemyBase` (`maxHealth`, `contactDamage`, `hitFlashDuration`,
`deathEffectDuration`, e dove serializzati `obstacleCheckDistance`,
`avoidanceStrength`, `pathRecalculateInterval`, `waypointReachedDistance`) compaiono
in testa a ogni componente.

| Prefab | Script | Vita | Danno contatto | Collider | Altro |
|---|---|---|---|---|---|
| `Walker` | `EnemyController` | 4 | 1 | Box 1 × 1 | `moveSpeed 2`, `EnemyVisuals` |
| `Splitter` | `SplitterController` | 8 | 1 | Box 1.3 × 1.3 | `moveSpeed 1.2`, `splitPrefab` → `Splitterling 1`, `splitCount 2`, `splitSpread 0.6`, `EnemyVisuals` |
| `Splitterling 1` | `SplitterController` | 1 | 1 | Box | `moveSpeed 3.5`, `splitPrefab` *None* (non si divide oltre), `EnemyVisuals` |
| `Shielded` | `ShieldedController` | 5 | 1 | Box 1 × 1 | `moveSpeed 1.3`, `shieldArc 0.3`, `shieldRotationSpeed 30`, `shieldVisual` → `ShieldPivot/Shield`, `EnemyVisuals` |
| `Dasher` | `DasherController` | 4 | 2 | Box 1.6 × 1.1 | `idleDuration 1.5`, `telegraphDuration 0.6`, `dashDuration 0.4`, `dashSpeed 12`, `idleMoveSpeed 1`, `EnemyVisuals` |
| `Teleporter` | `TeleporterController` | 2 | 1 | Box | `idleDuration 1.8`, `vanishDuration 0.4`, distanza dal player 2.5–4, `roomHalfWidth/Height 3` (sovrascritti da `RoomBounds`), `obstacleMargin 0.25`, `maxPlacementAttempts 12`, `arrivalIndicatorPrefab` → `TeleporterIndicator`, `projectilePrefab` → `EnemyProjectile`, `projectileSpeed 7` |
| `Turret` | `TurretController` | 4 | 1 | Box | `projectilePrefab` → `EnemyProjectile`, `projectileSpeed 6`, `fireInterval 2`, `directionSprites` (8 sprite) |
| `MiniBoss` | `MinibossController` | 15 | 2 | Box | vedi sotto |
| `Boss` | `BossController` | 40 | 2 | Box | vedi sotto |

### `MiniBoss`

`moveSpeed 1.5`, `moveDuration 1.5`, `telegraphDuration 0.4`, `chargeSpeed 7`,
`chargeDuration 0.4`, `enrageHealthRatio 0.4`, `enrageSpeedMultiplier 1.4`,
`enrageColor (1, 0.3, 0.1)`, `deathShakeDuration 0.25`, `deathShakeMagnitude 0.12`,
`projectilesPerBurst 5`, `aimedBurstCount 3`, `aimedSpread 12`,
`projectilePrefab` → `EnemyProjectile`, `projectileSpeed 5`.

Figlio `HealthBar` (Canvas in world space) con `Background` e `Fill`, collegati a
`healthBarRoot` e `healthBarFill`.

### `Boss`

`moveSpeed 1.2`, `moveDuration 2`, `telegraphDuration 0.5`, `chargeSpeed 9`,
`chargeDuration 0.5`, `enrageHealthRatio 0.15`, `enrageSpeedMultiplier 1.3`,
`deathShakeDuration 0.3`, `deathShakeMagnitude 0.15`.

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
| `Projectile` | `PlayerProjectile` | `SpriteRenderer`, `Rigidbody2D`, `CircleCollider2D`, `ProjectileController` | `lifetime 3` (danno, perforazione, rimbalzi impostati da `PlayerController` a runtime) |
| `EnemyProjectile` | `EnemyProjectile` | `SpriteRenderer`, `Rigidbody2D`, `CircleCollider2D`, `EnemyProjectileController` | `lifetime 4`, `damage 1` |
| `Bomb` | — | `SpriteRenderer`, `BombController` | `fuseTime 2`, `explosionRadius 1.5`, `explosionDamage 3`, `explosionEffectPrefab` → `BombExplosion`, `fuseSprites` (array) |
| `BombExplosion` | — | `SpriteRenderer`, `ExplosionEffect` | `duration 0.3`, `maxScale 3` |
| `TeleporterIndicator` | — | `SpriteRenderer` | nessun collider |
| `MinimapCell` | — | `CanvasRenderer`, `Image` | cella della minimappa |

---

## 8. Prefab dei pickup

Tutti hanno `SpriteRenderer`, `CircleCollider2D` e `PickupController`. Il campo
`pickupType` è l'enum `PickupController.PickupType` serializzato come intero.

| Prefab | `pickupType` | Effetto | Gruppo |
|---|---|---|---|
| `Pickup_Heal` | 0 `Heal` | `healAmount 2` | drop di stanza |
| `Pickup_Speed` | 1 `SpeedBoost` | `speedMultiplier 1.5` per `boostDuration 5` | drop di stanza |
| `Pickup_Bomb` | 2 `Bomb` | `bombAmount 1` | drop di stanza |
| `Pickup_MaxHealthUp` | 3 `MaxHealthUp` | `maxHealthIncrease 2` | permanente |
| `Pickup_DamageUp` | 4 `DamageUp` | `damageIncrease 1` | permanente |
| `Pickup_SpeedUp` | 5 `SpeedUp` | `speedIncrease 1` | permanente |
| `Pickup_FireRateUp` | 6 `FireRateUp` | `fireRateIncrease 0.05` | permanente |
| `HealthRegen` | 7 `HealthRegenUp` | `healthRegenChanceIncrease 0.15` | permanente |
| `PickUp_ArmorUp` | 8 `ArmorUp` | `contactDamageReductionIncrease 1` | permanente |
| `MeleeArcUp` | 9 `MeleeArcUp` | `meleeArcIncrease 0.15` | permanente |
| `ProjectileBounceUp` | 10 `ProjectileBounceUp` | `projectileBounceIncrease 1` | permanente |

Ogni prefab porta comunque **tutti** i campi di `PickupController` con gli stessi
valori; conta solo quello corrispondente al proprio `pickupType`.

I tre pickup di stanza sono in `RoomManager.pickupPrefabs`, gli otto permanenti in
`RoomManager.permanentUpgradePrefabs`.

---

## 9. Script (`Assets/Scripts/`)

32 file.

**Player**
`PlayerController` (input, 4 modalità di fuoco, bombe, vita, potenziamenti) ·
`PlayerVisuals` (sprite direzionali e arma orbitante)

**Nemici**
`EnemyBase` (astratta: vita, morte, danno da contatto, inseguimento con pathfinding
e aggiramento, `MoveDirection`, gizmo del percorso) · `EnemyController` (Walker) ·
`SplitterController` · `ShieldedController` · `DasherController` ·
`TeleporterController` · `TurretController` · `BossBase` · `BossController` ·
`MinibossController` · `EnemyVisuals`

**Dungeon e stanze**
`DungeonGenerator` · `RoomManager` · `RoomBounds` · `RoomNavGrid` ·
`RoomCameraSettings` · `DoorTrigger` · `SecretWall` · `Pathfinder`

**Proiettili, bombe, pickup**
`ProjectileController` · `EnemyProjectileController` · `BombController` ·
`ExplosionEffect` · `PickupController`

**Sistema e UI**
`GameManager` (game over, vittoria, pausa con ESC) · `MainMenuController` ·
`MinimapController` · `AudioManager` · `CameraShake` · `VectorUtils` (statica)

---

## 10. Campi non ancora serializzati nei prefab

Alcuni campi `[SerializeField]` aggiunti di recente non compaiono nello YAML dei
prefab, che non sono stati risalvati dopo la modifica dello script. Per questi vale
il valore di default scritto nel codice:

| Componente | Campo | Default in uso |
|---|---|---|
| `RoomNavGrid` | `agentRadius` | 0.7 |
| `EnemyBase` | `stuckTimeout` | 0.5 |
| `EnemyBase` | `stuckProgressFraction` | 0.25 |
| `EnemyBase` (su `Turret`, `Boss`, `MiniBoss`) | `obstacleCheckDistance`, `avoidanceStrength`, `pathRecalculateInterval`, `waypointReachedDistance` | 1, 1, 0.3, 0.15 |

Sui prefab `Walker`, `Splitter`, `Splitterling 1`, `Shielded`, `Dasher` e
`Teleporter` i quattro campi di navigazione **sono** serializzati, con
`waypointReachedDistance` a `0.3` invece del default `0.15`.

---

## 11. Comandi di gioco

WASD movimento · frecce sparo · Q cambia arma · E bomba · Tab minimappa ·
ESC pausa
