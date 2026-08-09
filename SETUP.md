# SETUP — stato attuale della scena e degli asset

Documento generato leggendo direttamente lo YAML di `Assets/Scenes/SampleScene.unity`,
`ProjectSettings/TagManager.asset`, tutti i `.prefab` in `Assets/Prefabs/` e tutti gli
script in `Assets/Scripts/`.

Descrive **solo lo stato attuale**: nessuna proposta di modifica.

> Nota sullo snapshot: durante la lettura l'editor Unity era aperto e ha rinominato
> `neo_zero_shielded_64x48 1.png` in `neo_zero_shielded_96x72.png` e aggiornato
> `Shielded.prefab` e `Teleporter.prefab`. Quanto segue riflette lo stato **dopo**
> quelle modifiche.

---

## 1. Gerarchia delle stanze

Tutte le stanze sono figlie dirette di `RoomManager` e nella scena salvata sono
**tutte disattivate** (`m_IsActive: 0`): è `RoomManager.EnterRoom()` ad attivarne una
sola a runtime, partendo da `startingRoom`.

Ordine dei figli di `RoomManager`:
`Room_1` · `Room_Secret` · `Room_2` · `Room_3` · `Room_4` · `Room_5` · `MiniBoss_Room` · `Boss_Room`

Convenzioni ricorrenti (valgono ovunque salvo dove indicato):

- I muri hanno tag `Wall`, un `SpriteRenderer` e un `BoxCollider2D` non-trigger
  (`m_IsTrigger: 0`, `m_Offset: {0, 0}`, `m_EdgeRadius: 0`).
- I muri verticali sono ruotati di 90° (`m_LocalRotation z=0.7071068, w=0.7071068`).
- Le porte hanno tag `Door`, `BoxCollider2D` **trigger** con `Size {1, 1}`,
  Scale `{1, 1, 1}` e un `DoorTrigger`.
- Il pavimento è `Grid` (`m_CellSize {1, 1, 0}`, gap 0, layout 0) con figlio `Floor`
  che porta `Tilemap` + `TilemapRenderer` (`m_SortingOrder: -10`).
- I nemici sono **istanze di prefab**, con tag `Enemy` ereditato dal prefab.

Esistono **due schemi di dimensionamento dei muri** in uso:

| Schema | Dove | Transform Scale | BoxCollider2D Size | SpriteRenderer |
|---|---|---|---|---|
| A — dimensione nel collider | solo `Room_1` | `{1, 1, 1}` | `{8, 0.5}` | DrawMode Tiled/Sliced, `m_Size {8, 0.5}` |
| B — dimensione nella Scale | tutte le altre stanze | `{L, 0.5, 1}` | `{1, 1}` | DrawMode Simple, `m_Size {1, 1}` |

---

### Room_1 — stanza iniziale (`startingRoom`)

Transform: Position `{0, 0, 0}`, Scale `{1, 1, 1}`.
Nessun `RoomCameraSettings` → usa `defaultReferenceResolution` (240×135).
**Nessun nemico** → la stanza risulta già libera all'ingresso.

Muri (schema A — Scale sempre `{1, 1, 1}`, dimensione nel collider):

| Oggetto | Position | Scale | BoxCollider2D Size | Note |
|---|---|---|---|---|
| `Wall_Secret` | `{0, 3.9976, 0}` | `{1, 1, 1}` | `{8, 0.5}` | sostituisce il muro superiore; DrawMode Tiled |
| `Wall_Left` | `{-4, -0.0024, 0}` | `{1, 1, 1}` | `{8, 0.5}` | ruotato 90°; `m_SortingOrder: -1` |
| `Wall_Bottom` | `{0, -4.0024, 0}` | `{1, 1, 1}` | `{8, 0.5}` | DrawMode Tiled |
| `Wall_Right` | `{4, -0.0024, 0}` | `{1, 1, 1}` | `{8, 0.5}` | ruotato 90°; DrawMode **Sliced** (gli altri Tiled) |

`Wall_Secret` porta anche uno `SecretWall`:

- `targetRoom` → `Room_Secret`
- `playerSpawnPosition` → `{0, -2.9}`

Pavimento: `Grid` (unica stanza in cui si chiama `Grid` e non `Grid (1)`) → `Floor`
Tilemap `m_Size {8, 8, 1}`, `m_Origin {-4, -4, 0}`, **64 tile**.

Porte:

| Porta | Position | targetRoom | playerSpawnPosition |
|---|---|---|---|
| `Door_ToRoom2` | `{4, 0, 0}` | `Room_2` | `{-2.8, 0}` |

---

### Room_Secret

Transform: Position `{0, 0, 0}`, Scale `{1, 1, 1}`. Nessun `RoomCameraSettings`.
**Nessun nemico.** Vi si accede solo rompendo `Room_1/Wall_Secret` con una bomba.

Muri (schema B — collider sempre `Size {1, 1}`):

| Oggetto | Position | Scale | BoxCollider2D Size |
|---|---|---|---|
| `Wall_Top` | `{0, 3.9976, 0}` | `{8, 0.5, 1}` | `{1, 1}` |
| `Wall_Left` | `{-4, -0.0024, 0}` | `{8, 0.5, 1}` | `{1, 1}` |
| `Wall_Bottom` | `{0, -4.0024, 0}` | `{8, 0.5, 1}` | `{1, 1}` |
| `Wall_Right` | `{4, -0.0024, 0}` | `{8, 0.5, 1}` | `{1, 1}` |

Pavimento: `Grid (1)` → `Floor`, Tilemap `{8, 8, 1}`, origin `{-4, -4, 0}`, **64 tile**.

Porte:

| Porta | Position | targetRoom | playerSpawnPosition |
|---|---|---|---|
| `Door_ToRoom1` | `{0, -4, 0}` | `Room_1` | `{0, 2.8}` |

---

### Room_2 — snodo centrale

Transform: Position `{0, 0, 0}`, Scale `{1, 1, 1}`. Nessun `RoomCameraSettings`.

Muri (schema B): `Wall_Top` `{0, 3.9976, 0}` · `Wall_Left` `{-4, -0.0024, 0}` ·
`Wall_Bottom` `{0, -4.0024, 0}` · `Wall_Right` `{4, -0.0024, 0}` —
tutti Scale `{8, 0.5, 1}`, BoxCollider2D `Size {1, 1}`.

Pavimento: `Grid (1)` → `Floor`, Tilemap `{8, 8, 1}`, origin `{-4, -4, 0}`, **64 tile**.

Porte (4, il numero più alto della scena):

| Porta | Position | targetRoom | playerSpawnPosition |
|---|---|---|---|
| `Door_ToRoom1` | `{-4, -0.0024, 0}` | `Room_1` | `{2.8, 0}` |
| `Door_ToRoom3` | `{0, 4, 0}` | `Room_3` | `{0, -2.8}` |
| `Door_ToRoom4` | `{4, 0, 0}` | `Room_4` | `{-2.9, 0}` |
| `Door_ToRoom5` | `{0, -4, 0}` | `Room_5` | `{0, 2.8}` |

Nemici (3):

| Oggetto | Prefab | Position | Override rispetto al prefab |
|---|---|---|---|
| `Teleporter` | `Teleporter.prefab` | `{-2, -3, 0}` | `BoxCollider2D.m_Size` → `{1.2, 1.6}` |
| `Walker` | `Walker.prefab` | `{3, 3, 0}` | nessuno |
| `Shielded` | `Shielded.prefab` | `{3, -3, 0}` | `m_LocalScale.x` → `0.6`; `m_IsActive` → `1` |

---

### Room_3

Transform: Position `{0, 0, 0}`, Scale `{1, 1, 1}`. Nessun `RoomCameraSettings`.

Muri (schema B): `Wall_Top` `{0, 3.9976, 0}` · `Wall_Left` `{-4, -0.0024, 0}` ·
`Wall_Bottom` `{0, -4.0024, 0}` · `Wall_Right` `{4, -0.0024, 0}` —
Scale `{8, 0.5, 1}`, collider `Size {1, 1}`.

Pavimento: `Grid (1)` → `Floor`, Tilemap `{8, 8, 1}`, origin `{-4, -4, 0}`, **64 tile**.

Porte:

| Porta | Position | targetRoom | playerSpawnPosition |
|---|---|---|---|
| `Door_ToRoom2` | `{0, -4, 0}` | `Room_2` | `{0, 2.8}` |

Nemici (3): `Turret` `{3, 3, 0}` · `Turret (1)` `{-3, 3, 0}` · `Walker` `{0, 2, 0}` —
nessun override oltre alla posizione.

---

### Room_4

Transform: Position `{0, 0, 0}`, Scale `{1, 1, 1}`. Nessun `RoomCameraSettings`.

Muri (schema B): `Wall_Top` `{0, 3.9976, 0}` · `Wall_Left` `{-4, -0.0024, 0}` ·
`Wall_Bottom` `{0, -4.0024, 0}` · `Wall_Right` `{4, -0.0024, 0}` —
Scale `{8, 0.5, 1}`, collider `Size {1, 1}`.

Pavimento: `Grid (1)` → `Floor`, Tilemap `{8, 8, 1}`, origin `{-4, -4, 0}`, **64 tile**.

Porte:

| Porta | Position | targetRoom | playerSpawnPosition |
|---|---|---|---|
| `Door_ToRoom2` | `{-4, 0, 0}` | `Room_2` | `{2.9, 0}` |

Nemici (3):

| Oggetto | Prefab | Position | Override |
|---|---|---|---|
| `Splitter` | `Splitter.prefab` | `{2, 1, 0}` | `m_IsActive` → `1` |
| `Turret` | `Turret.prefab` | `{3, -3, 0}` | nessuno |
| `Walker` | `Walker.prefab` | `{-1, 3, 0}` | nessuno |

---

### Room_5

Transform: Position `{0, 0, 0}`, Scale `{1, 1, 1}`. Nessun `RoomCameraSettings`.

Muri (schema B): `Wall_Top` `{0, 3.9976, 0}` · `Wall_Left` `{-4, -0.0024, 0}` ·
`Wall_Bottom` `{0, -4.0024, 0}` · `Wall_Right` `{4, -0.0024, 0}` —
Scale `{8, 0.5, 1}`, collider `Size {1, 1}`.

Pavimento: `Grid (1)` a Position `{0, -0.0024, 0}` → `Floor`,
Tilemap `{8, 8, 1}`, origin `{-4, -4, 0}`, **64 tile**.

Porte:

| Porta | Position | targetRoom | playerSpawnPosition |
|---|---|---|---|
| `Door_ToRoom2` | `{0, 4, 0}` | `Room_2` | `{0, -2.8}` |
| `Door_ToMiniBoss` | `{0, -4, 0}` | `MiniBoss_Room` | `{0, 2.8}` |

Nemici (3): `Dasher` `{-3, -2, 0}` · `Teleporter` `{0, 0, 0}` · `Dasher (1)` `{2, -2, 0}` —
nessun override oltre alla posizione.

---

### MiniBoss_Room — stanza larga

Transform: Position `{0, 0, 0}`, Scale `{1, 1, 1}`. Nessun `RoomCameraSettings`
(resta quindi a 240×135, pur essendo larga 12 unità).

Muri (schema B, stanza 12×8):

| Oggetto | Position | Scale | BoxCollider2D Size |
|---|---|---|---|
| `Wall_Top` | `{0, 4, 0}` | `{12, 0.5, 1}` | `{1, 1}` |
| `Wall_Left` | `{-6, 0, 0}` | `{8, 0.5, 1}` | `{1, 1}` |
| `Wall_Bottom` | `{0, -4, 0}` | `{12, 0.5, 1}` | `{1, 1}` |
| `Wall_Right` | `{6, 0, 0}` | `{8, 0.5, 1}` | `{1, 1}` |

Pavimento: `Grid (1)` a Position `{0, -0.0024, 0}` → `Floor`,
Tilemap `m_Size {12, 8, 1}`, origin `{-6, -4, 0}`, **96 tile**.

Porte:

| Porta | Position | targetRoom | playerSpawnPosition |
|---|---|---|---|
| `Door_ToRoom5` | `{0, 4, 0}` | `Room_5` | `{0, -2.9}` |
| `Door_ToBossRoom` | `{0, -4, 0}` | `Boss_Room` | `{0, 2.9}` |

Nemici (1): `MiniBoss` da `MiniBoss.prefab` a `{0, -1, 0}`, nessun override oltre alla posizione.

---

### Boss_Room — stanza finale (`finalRoom`)

Transform: Position `{0, 0, 0}`, Scale `{1, 1, 1}`.

**Unica stanza con `RoomCameraSettings`**: `referenceResolution = {320, 180}`.
Entrandoci, `RoomManager.ApplyCameraFor()` scrive 320×180 in `PixelPerfectCamera`;
uscendo verso qualunque altra stanza si torna a 240×135.

Muri (schema B, stanza 18×10):

| Oggetto | Position | Scale | BoxCollider2D Size |
|---|---|---|---|
| `Wall_Top` | `{0, 5, 0}` | `{18, 0.5, 1}` | `{1, 1}` |
| `Wall_Left` | `{-9, 0, 0}` | `{10, 0.5, 1}` | `{1, 1}` |
| `Wall_Bottom` | `{0, -5, 0}` | `{18, 0.5, 1}` | `{1, 1}` |
| `Wall_Right` | `{9, 0, 0}` | `{10, 0.5, 1}` | `{1, 1}` |

Pavimento: `Grid (1)` a Position `{0, -0.0024, 0}` → `Floor`,
Tilemap `m_Size {18, 10, 1}`, origin `{-9, -5, 0}`, **180 tile**.

Porte:

| Porta | Position | targetRoom | playerSpawnPosition |
|---|---|---|---|
| `Door_ToMiniBoss` | `{0, 5, 0}` | `MiniBoss_Room` | `{0, -2.9}` |

Nemici (1): `Boss` da `Boss.prefab` a `{0, -2, 0}`, con override del colore dello
`SpriteRenderer` → `m_Color {r: 0.9811321, g: 0.9811321, b: 0.9811321}`.

> Il colore di base modificato conta: `EnemyBase.Awake()` salva `baseSpriteColor` dallo
> `SpriteRenderer`, quindi il grigio chiaro è ciò a cui il boss torna in fase 1 e a fine
> lampo di danno.

### Riepilogo mappa

```
Room_1 ──(E)── Room_2 ──(N)── Room_3
  │ (muro segreto, N)  ├─(E)── Room_4
Room_Secret            └─(S)── Room_5 ──(S)── MiniBoss_Room ──(S)── Boss_Room
```

Totale nemici piazzati in scena: **14** (3+3+3+3+1+1 su Room_2..Boss_Room; Room_1 e
Room_Secret sono vuote).

---

## 2. Prefab

### Prefab dei nemici

#### `Walker.prefab` — tag `Enemy`

- **Transform**: Position `{-2, 2, 0}`, **Scale `{1, 1, 1}`**
- **SpriteRenderer**: sprite `neo_zero_walker_96x72.png`, Color bianco, SortingOrder 0, DrawMode Simple, Size `{1, 1}`
- **Rigidbody2D**: Dynamic, Mass 1, GravityScale 0, Constraints 4 (FreezeRotation)
- **BoxCollider2D**: **Size `{1, 1}`**, Offset `{0, 0}`, non trigger
- **EnemyController**: `maxHealth: 4` · `contactDamage: 1` · `hitFlashDuration: 0.15` · `deathEffectDuration: 0.2` · `moveSpeed: 2`
- **EnemyVisuals**: `walkDown` / `walkUp` / `walkSide` = 4 sprite ciascuno da `neo_zero_walker_96x72.png` · `frameRate: 6` · `idleFrame: 0` · `moveThreshold: 0.01`
- Riferimenti a None: nessuno

#### `Dasher.prefab` — tag `Enemy`

- **Transform**: Position `{0, 0.0024, 0}`, **Scale `{1, 1, 1}`**
- **SpriteRenderer**: sprite `neo_zero_dasher_128x72_1.png`, Color bianco, SortingOrder 0
- **Rigidbody2D**: Dynamic, Mass 1, GravityScale 0, Constraints 4
- **BoxCollider2D**: **Size `{1.6, 0.9}`**, Offset `{0, 0}`, non trigger
- **DasherController**: `maxHealth: 4` · `contactDamage: 2` · `hitFlashDuration: 0.08` · `deathEffectDuration: 0.2` · `idleDuration: 1.5` · `telegraphDuration: 0.6` · `dashDuration: 0.4` · `dashSpeed: 12` · `idleMoveSpeed: 1` · `telegraphColor {r: 0.0883, g: 0, b: 0.5142, a: 1}` (blu scuro)
- **EnemyVisuals**: 4+4+4 sprite da `neo_zero_dasher_128x72_1.png` · `frameRate: 6` · `idleFrame: 0` · `moveThreshold: 0.01`
- Riferimenti a None: nessuno

#### `Turret.prefab` — tag `Enemy`

- **Transform**: Position `{-3, -3, 0}`, **Scale `{1, 1, 1}`**
- **SpriteRenderer**: sprite `neo_zero_turret_256x32.png`, Color bianco, SortingOrder 0
- **Rigidbody2D**: **Kinematic** (`m_BodyType: 1`), Mass 1, **GravityScale 1**, Constraints 0
- **BoxCollider2D**: **Size `{1, 1}`**, Offset `{0, 0}`, non trigger
- **TurretController**: `maxHealth: 4` · `contactDamage: 1` · `hitFlashDuration: 0.08` · `deathEffectDuration: 0.2` · `projectilePrefab` → `EnemyProjectile.prefab` · `projectileSpeed: 6` · `fireInterval: 2` · `directionSprites` = 8 sprite da `neo_zero_turret_256x32.png`
- **Non ha `EnemyVisuals`**: l'orientamento è gestito da `TurretController.UpdateFacing()` tramite `directionSprites`
- Riferimenti a None: nessuno

#### `Shielded.prefab` — tag `Enemy`

- **Transform** (radice): Position `{3, 3, 0}`, **Scale `{1, 1, 1}`**
- **SpriteRenderer**: sprite `neo_zero_shielded_96x72.png`, Color `{0.9764, 0.9764, 0.9764, 1}`, SortingOrder 0
- **Rigidbody2D**: Dynamic, Mass 1, GravityScale 0, Constraints 4
- **BoxCollider2D**: **Size `{1, 1.1875}`**, Offset `{0, 0}`, non trigger
- **ShieldedController**: `maxHealth: 5` · `contactDamage: 1` · `hitFlashDuration: 0.08` · `deathEffectDuration: 0.2` · `moveSpeed: 1.3` · `shieldVisual` → Transform `ShieldPivot` · `shieldArc: 0.3` · `shieldRotationSpeed: 80`
- **EnemyVisuals**: 4+4+4 sprite da `neo_zero_shielded_96x72.png` · `frameRate: 6` · `idleFrame: 0` · `moveThreshold: 0.01`
  (`walkDown[0]` e `walkDown[2]` puntano allo stesso sprite)
- Figli:
  - `ShieldPivot` — Position `{0, 0, 0}`, **Scale `{1, 1, 1}`**, nessun componente oltre al Transform
    - `Shield` — Position `{0.6, 0, 0}`, **Scale `{0.2, 1, 1}`**, SpriteRenderer con sprite `nemico_shield_24_1.png`, nessun collider
- Riferimenti a None: nessuno

#### `Splitter.prefab` — tag `Enemy`

- **Transform**: Position `{2, 2, 0}`, **Scale `{1.5, 1.5, 1}`**
- **SpriteRenderer**: sprite quadrato built-in (GUID `311925a0…`, non in `Assets/`), Color `{0, 0.4227, 0.9529, 1}` (blu), SortingOrder 0
- **Rigidbody2D**: Dynamic, Mass 1, GravityScale 0, Constraints 4
- **BoxCollider2D**: **Size `{1, 1}`**, Offset `{0, 0}`, non trigger
- **SplitterController**: `maxHealth: 8` · `contactDamage: 1` · `hitFlashDuration: 0.08` · `deathEffectDuration: 0.2` · `moveSpeed: 1.2` · `splitPrefab` → `Splitterling 1.prefab` · `splitCount: 2` · `splitSpread: 0.6`
- **EnemyVisuals**: `walkDown`, `walkUp`, `walkSide` **tutti array vuoti** · `frameRate: 6` · `idleFrame: 0` · `moveThreshold: 0.01`
- Riferimenti a None: nessuno (ma vedi array vuoti in §6)

#### `Splitterling 1.prefab` — tag `Enemy`

- **Transform**: Position `{2, 2, 0}`, **Scale `{0.5, 0.5, 1}`**
- **SpriteRenderer**: sprite quadrato built-in (GUID `311925a0…`), Color `{0.4009, 0.8319, 1, 1}` (azzurro), SortingOrder 0
- **Rigidbody2D**: Dynamic, Mass 1, GravityScale 0, Constraints 4
- **BoxCollider2D**: **Size `{1, 1}`**, Offset `{0, 0}`, non trigger
- **SplitterController**: `maxHealth: 1` · `contactDamage: 1` · `hitFlashDuration: 0.08` · `deathEffectDuration: 0.2` · `moveSpeed: 3.5` · **`splitPrefab` → None** · `splitCount: 2` · `splitSpread: 0.6`
- **EnemyVisuals**: array direzionali **tutti vuoti** · `frameRate: 6` · `idleFrame: 0` · `moveThreshold: 0.01`
- Riferimenti a None: **`splitPrefab`** (intenzionale: chiude la catena di divisione — `SpawnEnemiesAroundSelf` esce subito se il prefab è null)

#### `Teleporter.prefab` — tag `Enemy`

- **Transform**: Position `{-2, 2, 0}`, **Scale `{1, 1, 1}`**
- **SpriteRenderer**: sprite `nemico_teleporter_24_1.png`, Color bianco, SortingOrder 0
- **Rigidbody2D**: **Kinematic** (`m_BodyType: 1`), Mass 1, GravityScale 0, Constraints 4
- **BoxCollider2D**: **Size `{1, 1}`**, Offset `{0, 0}`, non trigger
- **TeleporterController**: `maxHealth: 2` · `contactDamage: 1` · `hitFlashDuration: 0.08` · `deathEffectDuration: 0.2` · `idleDuration: 1.8` · `vanishDuration: 0.4` · `minDistanceFromPlayer: 2.5` · `maxDistanceFromPlayer: 4` · `roomHalfWidth: 3` · `roomHalfHeight: 3` · `projectilePrefab` → `EnemyProjectile.prefab` · `projectileSpeed: 7`
- **Non ha `EnemyVisuals`**
- Riferimenti a None: nessuno

#### `MiniBoss.prefab` — tag `Enemy`

- **Transform** (radice): Position `{0, -2, 0}`, **Scale `{1, 1, 1}`**
- **SpriteRenderer**: sprite da `neo_zero_miniboss_128x72.png` (stesso atlas dei frame di `EnemyVisuals`), Color bianco, SortingOrder 0
- **Rigidbody2D**: Dynamic, Mass 1, GravityScale 0, Constraints 4
- **BoxCollider2D**: **Size `{1.6, 1.3}`**, Offset `{0, 0}`, non trigger
- **MinibossController** (da `EnemyBase` → `BossBase`):
  - da `EnemyBase`: `maxHealth: 15` · `contactDamage: 2` · `hitFlashDuration: 0.08` · `deathEffectDuration: 0.2`
  - da `BossBase`: `moveSpeed: 1.5` · `moveDuration: 1.5` · `telegraphDuration: 0.4` · `telegraphColor` bianco · `projectilePrefab` → `EnemyProjectile.prefab` · `projectileSpeed: 5` · `chargeSpeed: 7` · `chargeDuration: 0.4` · `enrageHealthRatio: 0.4` · `enrageSpeedMultiplier: 1.4` · `enrageColor {1, 0.3, 0.1, 1}` · `healthBarFill` → Image su `Fill` · `healthBarRoot` → GameObject **`Background`** · `deathShakeDuration: 0.25` · `deathShakeMagnitude: 0.12`
  - proprie: `projectilesPerBurst: 5` · `aimedBurstCount: 3` · `aimedSpread: 12`
- **EnemyVisuals**: 4+4+4 sprite da `neo_zero_miniboss_128x72.png` · `frameRate: 6` · `idleFrame: 0` · `moveThreshold: 0.01`
- Figli (barra della vita in World Space):
  - `HealthBar` — RectTransform, **Scale `{0.0125, 0.002, 0.01}`**, anchoredPosition `{0, 0.8}`, sizeDelta `{250, 1}`, anchorMin/Max `{0, 0}`, pivot `{0.5, 0.5}`
    - `Canvas` RenderMode 2 (World Space), SortingOrder 0
    - `CanvasScaler` UiScaleMode 0, ReferenceResolution `{800, 600}`, ReferencePixelsPerUnit 100
    - `GraphicRaycaster`
    - `Background` — RectTransform Scale `{1, 1, 1}`, sizeDelta `{0, 88}`, anchor stretch; Image Color `{0.283, 0.2817, 0.2817, 1}`, **Sprite None**, Type Simple
    - `Fill` — RectTransform Scale `{1, 1, 1}`, sizeDelta `{0, 88}`, anchor stretch; Image Color `{1, 0.0236, 0.0236, 1}` (rosso), Type **Filled** (`m_Type: 3`), FillAmount 1, FillMethod 0 (Horizontal), sprite UI built-in
- Riferimenti a None: `Background/Image.m_Sprite`
- **Nota**: `healthBarRoot` punta al figlio `Background`, non alla radice `HealthBar`.
  `BossBase.Start()` fa `healthBarRoot.SetActive(true)` e `Die()` lo disattiva: alla morte
  sparisce lo sfondo grigio, mentre `Fill` (il riempimento rosso) resta attivo.

#### `Boss.prefab` — tag `Enemy`

- **Transform** (radice): Position `{0, 0.0024, 0}`, **Scale `{2.5, 2.5, 1}`**
- **SpriteRenderer**: sprite da `neo_zero_boss_128x96.png`, Color bianco, SortingOrder 0
- **Rigidbody2D**: Dynamic, **Mass 10**, GravityScale 0, Constraints 4
- **BoxCollider2D**: **Size `{1, 1}`**, Offset `{0, 0}`, non trigger
  (la scala 2.5 del Transform lo porta a 2.5×2.5 unità effettive)
- **BossController** (da `EnemyBase` → `BossBase`):
  - da `EnemyBase`: `maxHealth: 40` · `contactDamage: 2` · `hitFlashDuration: 0.08` · `deathEffectDuration: 0.2`
  - da `BossBase`: `moveSpeed: 1.2` · `moveDuration: 2` · `telegraphDuration: 0.5` · `telegraphColor` bianco · `projectilePrefab` → `EnemyProjectile.prefab` · `projectileSpeed: 5` · `chargeSpeed: 9` · `chargeDuration: 0.5` · `enrageHealthRatio: 0.15` · `enrageSpeedMultiplier: 1.3` · `enrageColor {1, 0, 0, 1}` · `healthBarFill` → Image su `Fill` · `healthBarRoot` → GameObject **`Background`** · `deathShakeDuration: 0.3` · `deathShakeMagnitude: 0.15`
  - proprie: `radialCount: 8` · `volleySpread: 15` · `spiralWaveCount: 3` · `spiralBulletsPerWave: 5` · `spiralWaveInterval: 0.3` · `spiralRotationStep: 20` · `spiralProjectileSpeed: 3.5` · `maxChargeChain: 2` · `chargeChainChance: 0.5` · `minionPrefab` → **`Dasher.prefab`** · `minionCount: 2` · `minionSpawnRadius: 1.2` · `enragePulseSpeed: 6` · `phase2Color {1, 0.6, 0.2, 1}` · `phase3Color {1, 0.2, 0.2, 1}`
- **EnemyVisuals**: 4+4+4 sprite da `neo_zero_boss_128x96.png` · `frameRate: 6` · `idleFrame: 0` · `moveThreshold: 0.01`
- Figli: stessa struttura del MiniBoss —
  `HealthBar` (RectTransform **Scale `{0.0125, 0.002, 0.01}`**, anchoredPosition `{0, 0.8}`, sizeDelta `{250, 1}`; Canvas World Space + CanvasScaler + GraphicRaycaster)
  → `Background` (Scale `{1, 1, 1}`, Image grigia, **Sprite None**) e `Fill` (Scale `{1, 1, 1}`, Image rossa Filled)
- Riferimenti a None: `Background/Image.m_Sprite`
- Stessa nota su `healthBarRoot` del MiniBoss

### Prefab dei proiettili e delle bombe

#### `Projectile.prefab` — tag `PlayerProjectile`

- **Transform**: Position `{0, 0.0024, 0}`, **Scale `{1, 1, 1}`**
- **SpriteRenderer**: sprite `projAI_player_16.png`, Color bianco, SortingOrder 0
- **Rigidbody2D**: Dynamic, Mass 1, GravityScale 0, **Constraints 0** (rotazione libera)
- **CircleCollider2D**: **Radius `0.25`**, Offset `{0, 0}`, **trigger**
- **ProjectileController**: `lifetime: 3` (unico campo serializzato; `damage`, `piercing`, `bouncesLeft` sono impostati a runtime da `PlayerController.SpawnProjectile`)
- Riferimenti a None: nessuno

#### `EnemyProjectile.prefab` — tag `EnemyProjectile`

- **Transform**: Position `{0, 0.0024, 0}`, **Scale `{1, 1, 1}`**
- **SpriteRenderer**: sprite `projAI_nemico_16.png`, Color bianco, SortingOrder 0
- **Rigidbody2D**: Dynamic, Mass 1, GravityScale 0, **Constraints 0**
- **CircleCollider2D**: **Radius `0.25`**, Offset `{0, 0}`, **trigger**
- **EnemyProjectileController**: `lifetime: 4` · `damage: 1`
- Riferimenti a None: nessuno

#### `Bomb.prefab`

- **Transform**: Position `{0, 0.0024, 0}`, **Scale `{0.6, 0.6, 1}`**
- **SpriteRenderer**: sprite built-in (GUID `a86470a3…`, non in `Assets/`), Color **nero** `{0, 0, 0, 1}`, SortingOrder 0
- **BombController**: `fuseTime: 2` · `explosionRadius: 1.5` · `explosionDamage: 3` · `explosionEffectPrefab` → `BombExplosion.prefab`
- **Nessun collider e nessun Rigidbody2D**: il danno è tutto via `Physics2D.OverlapCircleAll`
- Riferimenti a None: nessuno

#### `BombExplosion.prefab`

- **Transform**: Position `{0, 0.0024, 0}`, **Scale `{0.5, 0.5, 1}`**
- **SpriteRenderer**: sprite `esplAI_estesa.png`, Color bianco, SortingOrder 0
- **ExplosionEffect**: `duration: 0.3` · `maxScale: 3`
- **Nessun collider**: è solo visivo (`ExplosionEffect.Update` sovrascrive comunque `localScale` da 0.5 a 3)
- Riferimenti a None: nessuno

### Prefab dei pickup

Tutti hanno la stessa struttura: `Transform` + `SpriteRenderer` (sprite da
`neo_zero_pickup_AI_64x48.png`, Color bianco, SortingOrder 0) + `CircleCollider2D`
**trigger** + `PickupController`. Nessuno ha Rigidbody2D. Nessun riferimento a None.

I campi numerici di `PickupController` sono **identici in tutti gli 11 prefab** — solo
`pickupType` cambia:

`healAmount: 2` · `speedMultiplier: 1.5` · `boostDuration: 5` · `bombAmount: 1` ·
`maxHealthIncrease: 2` · `damageIncrease: 1` · `speedIncrease: 1` ·
`fireRateIncrease: 0.05` · `healthRegenChanceIncrease: 0.15` ·
`contactDamageReductionIncrease: 1` · `meleeArcIncrease: 0.15` ·
`projectileBounceIncrease: 1`

| Prefab | `pickupType` | Enum | Transform Scale | Collider Radius |
|---|---|---|---|---|
| `Pickup_Heal` | 0 | `Heal` | `{0.4, 0.4, 1}` | `0.5` |
| `Pickup_Speed` | 1 | `SpeedBoost` | `{0.3, 0.3, 1}` | `0.49999997` |
| `Pickup_Bomb` | 2 | `Bomb` | `{0.4, 0.4, 1}` | `0.5` |
| `Pickup_MaxHealthUp` | 3 | `MaxHealthUp` | `{0.6, 0.6, 1}` | `0.49999997` |
| `Pickup_DamageUp` | 4 | `DamageUp` | `{0.6, 0.6, 1}` | `0.49999997` |
| `Pickup_SpeedUp` | 5 | `SpeedUp` | `{0.6, 0.6, 1}` | `0.49999997` |
| `Pickup_FireRateUp` | 6 | `FireRateUp` | `{0.6, 0.6, 1}` | `0.49999997` |
| `HealthRegen` | 7 | `HealthRegenUp` | `{0.6, 0.6, 1}` | `0.49999997` |
| `PickUp_ArmorUp` | 8 | `ArmorUp` | `{0.6, 0.6, 1}` | `0.49999997` |
| `MeleeArcUp` | 9 | `MeleeArcUp` | `{0.6, 0.6, 1}` | `0.49999997` |
| `ProjectileBounceUp` | 10 | `ProjectileBounceUp` | `{0.6, 0.6, 1}` | `0.49999997` |

Tutti hanno Position `{0, 0.0024, 0}` e ognuno usa uno sprite ritagliato diverso dallo
stesso atlas `neo_zero_pickup_AI_64x48.png`.

I primi tre (`Heal`, `Speed`, `Bomb`) sono quelli in `RoomManager.pickupPrefabs`
(drop a fine stanza); gli altri otto sono in `permanentUpgradePrefabs`
(ricompensa da boss e da stanza segreta).

---

## 3. Tag e Layer

### Tag definiti in `ProjectSettings/TagManager.asset`

Cinque tag personalizzati, oltre a quelli built-in di Unity:

| Tag | Usato da (script) | Oggetti che lo portano |
|---|---|---|
| `EnemyProjectile` | `PlayerController.cs:267` (`MeleeAttack` — il corpo a corpo li distrugge) | `EnemyProjectile.prefab` |
| `Enemy` | `BombController.cs:49`, `ProjectileController.cs:44`, `PlayerController.cs:277`, `RoomManager.cs:209` (`CountEnemies`) | prefab `Walker`, `Dasher`, `Turret`, `Shielded`, `Splitter`, `Splitterling 1`, `Teleporter`, `MiniBoss`, `Boss` → **14 istanze in scena** (Room_2..Boss_Room) |
| `PlayerProjectile` | **nessuno script lo legge** | `Projectile.prefab` |
| `Door` | `RoomManager.cs:188` (`SetDoorsActive`) | **13 oggetti in scena** (elenco sotto) |
| `Wall` | `EnemyProjectileController.cs:26`, `ProjectileController.cs:62` (`BounceOffWall`) | **32 oggetti in scena** (4 muri × 8 stanze) |

Tag built-in usati:

| Tag | Usato da (script) | Oggetti che lo portano |
|---|---|---|
| `Player` | `EnemyBase.cs:40` (`FindGameObjectWithTag`) e `:145`, `BombController.cs:39`, `EnemyProjectileController.cs:15`, `PickupController.cs:32`, `DoorTrigger.cs:11`, `SecretWall.cs:53`, `RoomManager.cs:65` | 1 oggetto: `Player` |
| `MainCamera` | nessuno script (usato da Unity per `Camera.main`) | 1 oggetto: `Main Camera` |
| `Untagged` | — | 50 oggetti (manager, Canvas, UI, Grid, Floor, le stanze stesse, `MeleeVisual`, `WeaponPivot`) |

Oggetti con tag `Door` (13):

```
Room_1/Door_ToRoom2            Room_2/Door_ToRoom1   Room_2/Door_ToRoom3
Room_2/Door_ToRoom4            Room_2/Door_ToRoom5   Room_3/Door_ToRoom2
Room_4/Door_ToRoom2            Room_5/Door_ToRoom2   Room_5/Door_ToMiniBoss
Room_Secret/Door_ToRoom1       MiniBoss_Room/Door_ToRoom5
MiniBoss_Room/Door_ToBossRoom  Boss_Room/Door_ToMiniBoss
```

Oggetti con tag `Wall` (32): `Wall_Top`, `Wall_Left`, `Wall_Bottom`, `Wall_Right` in
ognuna delle 8 stanze — con l'eccezione di `Room_1`, dove al posto di `Wall_Top` c'è
`Wall_Secret` (che ha comunque tag `Wall`).

> `Room_1/Wall_Secret` ha tag `Wall`: dopo `Reveal()` il collider resta solido e il
> passaggio avviene per collisione, quindi il tag serve anche a far rimbalzare/assorbire
> i proiettili contro di esso.

### Layer

`TagManager.asset` non definisce **nessun layer personalizzato**: restano solo i built-in
(`Default`, `TransparentFX`, `Ignore Raycast`, `Water`, `UI`), con gli slot 3 e 6–31 vuoti.

Layer effettivamente usati in scena:

| Layer | Oggetti |
|---|---|
| 0 — `Default` | tutto il gameplay: `Player`, nemici, muri, porte, pavimenti, manager, `Main Camera`, `Global Light 2D` |
| 5 — `UI` | `Canvas` e tutti i suoi discendenti (testi, pannelli, bottoni) |

Sorting Layers: solo `Default` (uniqueID 0). La profondità è gestita interamente con
`m_SortingOrder`: pavimento `-10`, `Room_1/Wall_Left` `-1`, muri e nemici `0`,
`Player` e `WeaponPivot` `10`.

---

## 4. Player e Canvas

### GameObject `Player` — tag `Player`, layer 0

**Transform**: Position `{0, 0.0024, 0}`, **Scale `{1, 1, 1}`**, rotazione identità.

**SpriteRenderer**: sprite da `neo_zero_char_01.png`, Color bianco,
**SortingOrder 10**, DrawMode Simple, Size `{1, 1}`.

**Rigidbody2D**: Dynamic, Mass 1, GravityScale 0, Constraints 4 (FreezeRotation),
Simulated, CollisionDetection Discrete, Interpolate None.

**BoxCollider2D**: **Size `{0.875, 1.4375}`**, **Offset `{0, 0.1875}`**, non trigger, EdgeRadius 0.

**PlayerController** — tutti i 34 campi serializzati:

| Gruppo | Campo | Valore |
|---|---|---|
| Movimento | `moveSpeed` | `5` |
| Sparo | `projectilePrefab` | → `Projectile.prefab` |
| | `projectileSpeed` | `10` |
| | `fireCooldown` | `0.3` |
| | `projectileDamage` | `1` |
| | `firePointDistance` | `0.5` |
| Armi | `spreadAngle` | `20` (default nel codice: 25) |
| | `spreadCooldownMult` | `1.6` (default nel codice: 2.4) |
| | `pierceCooldownMult` | `1.3` |
| | `spreadLifetime` | `0.35` |
| Corpo a corpo | `meleeRange` | `1.1` |
| | `meleeArc` | `0.3` |
| | `meleeDamageMult` | `2` |
| | `meleeCooldownMult` | `1.4` |
| Bombe | `bombPrefab` | → `Bomb.prefab` |
| | `maxBombs` | `3` |
| | `bombCooldown` | `1` |
| Vita | `maxHealth` | `6` |
| | `invulnerabilityDuration` | `0.8` (default nel codice: 0.5) |
| | `healthRegenInterval` | `5` |
| | `healthRegenChance` | `0` |
| | `contactDamageReduction` | `0` |
| Potenziamenti sparo | `projectileBounces` | `0` |
| Feedback danno | `hitFlashDuration` | `0.1` |
| | `hitShakeDuration` | `0.15` |
| | `hitShakeMagnitude` | `0.1` |
| UI | `healthText` | → `Canvas/HealthText` |
| | `bombText` | → `Canvas/BombText` |
| | `weaponText` | → `Canvas/WeaponText` |
| | `upgradePopupText` | → `Canvas/UpgradePopUpText` |
| | `upgradePopupDuration` | `1.5` |
| Feedback | `meleeVisual` | → Transform `Player/MeleeVisual` |
| | `meleeVisualDuration` | `0.1` |
| | `playerVisuals` | → `PlayerVisuals` sullo stesso GameObject `Player` |

Riferimenti a None: **nessuno**.

**PlayerVisuals** — tutti i 13 campi serializzati:

| Campo | Valore |
|---|---|
| `walkDown` | 3 sprite da `neo_zero_char_01.png` |
| `walkUp` | 3 sprite da `neo_zero_char_01.png` |
| `walkSide` | 3 sprite da `neo_zero_char_01.png` |
| `frameRate` | `8` |
| `idleFrame` | `1` |
| `weaponPivot` | → Transform `Player/WeaponPivot` |
| `weaponRenderer` | → SpriteRenderer su `Player/WeaponPivot` |
| `weaponSprites` | 4 sprite da `neo_zero_armi_20x16.png` (0 Single, 1 Spread, 2 Piercing, 3 Melee) |
| `weaponOrbitRadius` | `0.28` (default nel codice: 0.35) |
| `weaponSortingOffset` | `1` |
| `muzzleForward` | `[0.5625, 0.5, 0.6875, 0.75]` |
| `muzzleUp` | `0.19` |
| `weaponHideDelay` | `0.15` |

Riferimenti a None: **nessuno**.

**Figli di `Player`:**

- `MeleeVisual` — **disattivato** (`m_IsActive: 0`), Position `{0, 0, 0}`,
  **Scale `{0.3, 1.2, 1}`**; SpriteRenderer con sprite quadrato built-in
  (GUID `311925a0…`), Color bianco, SortingOrder 0. Viene acceso/spento da
  `PlayerController.ShowMeleeVisual()`.
- `WeaponPivot` — attivo, Position `{0, 0, 0}`, **Scale `{1, 1, 1}`**;
  SpriteRenderer con **`m_Sprite: None`**, Color bianco, **SortingOrder 10**,
  `m_Size {1.25, 1}`. Lo sprite viene assegnato ogni frame da
  `PlayerVisuals.UpdateWeapon()` pescando da `weaponSprites`.

### GameObject `Canvas` — layer 5 (UI)

RectTransform: anchoredPosition `{0, 0}`, sizeDelta `{0, 0}`, anchorMin/Max `{0, 0}`,
pivot `{0, 0}`, **Scale `{0, 0, 0}`** (valore salvato; viene sovrascritto a runtime
dal Canvas in Screen Space Overlay).

- **Canvas**: `m_RenderMode: 0` (Screen Space – Overlay), SortingOrder 0, PlaneDistance 100, PixelPerfect 0
- **CanvasScaler**: UiScaleMode 0 (Constant Pixel Size), ScaleFactor 1,
  ReferencePixelsPerUnit 100, ReferenceResolution `{800, 600}`, ScreenMatchMode 0, MatchWidthOrHeight 0
- **GraphicRaycaster**: IgnoreReversedGraphics 1, BlockingObjects 0

Gerarchia UI:

```
Canvas  (layer 5)
├── HealthText              TextMeshProUGUI
├── BombText                TextMeshProUGUI
├── WeaponText              TextMeshProUGUI
├── GameOverPanel           [DISATTIVO]  Image nera α 0.784
│   ├── GameOverText        TextMeshProUGUI  "HAI PERSO"
│   ├── RestartButton       Image + Button → GameManager.RestartGame
│   │   └── Text (TMP)      TextMeshProUGUI  "Ricomincia"
│   └── MenuButton          Image + Button → GameManager.GoToMainMenu
│       └── Text (TMP)      TextMeshProUGUI  "Main Menu"
├── VictoryPanel            [DISATTIVO]  TextMeshProUGUI  "VITTORIA"
│   ├── Sfondo              Image  {0.039, 0.063, 0.094, α 0.863}
│   ├── MainMenuButton      Image + Button → GameManager.GoToMainMenu
│   │   └── Text (TMP)      TextMeshProUGUI  "Main menu"
│   └── PlayAgainButton (1) Image + Button → GameManager.RestartGame
│       └── Text (TMP)      TextMeshProUGUI  "Rigioca"
└── UpgradePopUpText        [DISATTIVO]  TextMeshProUGUI  "New Text"
```

> `VictoryPanel` non ha una propria `Image` di sfondo: porta direttamente il
> `TextMeshProUGUI` "VITTORIA", e lo sfondo scuro è il figlio `Sfondo`.
> `GameOverPanel`, invece, ha l'Image sul pannello e il testo in un figlio separato.

Layout dei RectTransform:

| Oggetto | anchoredPosition | sizeDelta | anchorMin / anchorMax | pivot |
|---|---|---|---|---|
| `HealthText` | `{10, -10}` | `{250, 50}` | `{0,1}` / `{0,1}` | `{0, 1}` |
| `BombText` | `{10, -77}` | `{200, 50}` | `{0,1}` / `{0,1}` | `{0, 1}` |
| `WeaponText` | `{10, -115}` | `{350, 50}` | `{0,1}` / `{0,1}` | `{0, 1}` |
| `UpgradePopUpText` | `{0, -125}` | `{500, 50}` | `{0.5,1}` / `{0.5,1}` | `{0.5, 0.5}` |
| `GameOverPanel` | `{0, 0}` | `{0, 0}` | `{0,0}` / `{1,1}` (stretch) | `{0.5, 0.5}` |
| `GameOverText` | `{0, 0}` | `{200, 50}` | `{0.5,0.5}` / `{0.5,0.5}` | `{0.5, 0.5}` |
| `RestartButton` | `{0, -100}` | `{160, 30}` | `{0.5,0.5}` / `{0.5,0.5}` | `{0.5, 0.5}` |
| `MenuButton` | `{0, -160}` | `{160, 30}` | `{0.5,0.5}` / `{0.5,0.5}` | `{0.5, 0.5}` |
| `VictoryPanel` | `{0, 0}` | `{0, 0}` | `{0,0}` / `{1,1}` (stretch) | `{0.5, 0.5}` |
| `Sfondo` | `{0, 0}` | `{0, 0}` | `{0,0}` / `{1,1}` (stretch) | `{0.5, 0.5}` |
| `PlayAgainButton (1)` | `{0, -150}` | `{160, 30}` | `{0.5,0.5}` / `{0.5,0.5}` | `{0.5, 0.5}` |
| `MainMenuButton` | `{0, -200}` | `{160, 30}` | `{0.5,0.5}` / `{0.5,0.5}` | `{0.5, 0.5}` |
| ogni `Text (TMP)` dei bottoni | `{0, 0}` | `{0, 0}` | `{0,0}` / `{1,1}` (stretch) | `{0.5, 0.5}` |

Tutti gli oggetti UI hanno Scale `{1, 1, 1}` (tranne `Canvas`, `{0, 0, 0}`).

### Aggancio di ogni `TextMeshProUGUI`

| GameObject | Testo salvato | fontSize | Allineamento | Agganciato a |
|---|---|---|---|---|
| `Canvas/HealthText` | `Vita: 6/6` | 64 | H 1 (Left), V 256 (Middle) | **`PlayerController.healthText`** |
| `Canvas/BombText` | `Bombe: 3` | 36 | H 1, V 256 | **`PlayerController.bombText`** |
| `Canvas/WeaponText` | `Arma: Singolo` | 36 | H 1, V 256 | **`PlayerController.weaponText`** |
| `Canvas/UpgradePopUpText` | `New Text` | 40 | H 2 (Center), V 512 | **`PlayerController.upgradePopupText`** |
| `Canvas/GameOverPanel/GameOverText` | `HAI PERSO` | 64 | H 2, V 512 | **nessun campo di script** (testo statico) |
| `Canvas/VictoryPanel` (sul pannello) | `VITTORIA` | 64 | H 2, V 512 | **nessun campo di script** (testo statico) |
| `…/RestartButton/Text (TMP)` | `Ricomincia` | 24 | H 2, V 512 | **nessun campo** (etichetta del Button) |
| `…/MenuButton/Text (TMP)` | `Main Menu` | 24 | H 2, V 512 | **nessun campo** (etichetta del Button) |
| `…/MainMenuButton/Text (TMP)` | `Main menu` | 24 | H 2, V 512 | **nessun campo** (etichetta del Button) |
| `…/PlayAgainButton (1)/Text (TMP)` | `Rigioca` | 24 | H 2, V 512 | **nessun campo** (etichetta del Button) |

Tutti i TextMeshProUGUI usano lo stesso font asset (GUID `8f586378…`, esterno a
`Assets/Sprites`), Color bianco, `enableAutoSizing: 0`, `fontSizeMin: 18`, `fontSizeMax: 72`.

I quattro pannelli/testi agganciati a script sono gli unici referenziati; i restanti sei
sono etichette statiche.

### Altri oggetti di scena collegati

- **`GameManager`** (Position `{0, 0.0024, 0}`, Scale `{1, 1, 1}`) —
  `gameOverPanel` → `Canvas/GameOverPanel`, `victoryPanel` → `Canvas/VictoryPanel`. Nessun None.
- **`RoomManager`** (Position `{0, 0.0024, 0}`, Scale `{1, 1, 1}`):
  - `startingRoom` → `Room_1`
  - `finalRoom` → `Boss_Room`
  - `pickupPrefabs` (3) → `Pickup_Heal`, `Pickup_Speed`, `Pickup_Bomb`
  - `pickupDropChance` → `0.35` (default nel codice: 0.5)
  - `permanentUpgradePrefabs` (8) → `Pickup_MaxHealthUp`, `Pickup_DamageUp`, `Pickup_SpeedUp`, `Pickup_FireRateUp`, `HealthRegen`, `MeleeArcUp`, `PickUp_ArmorUp`, `ProjectileBounceUp`
  - `pixelPerfectCamera` → `PixelPerfectCamera` su `Main Camera`
  - `defaultReferenceResolution` → `{240, 135}`
  - `doorIgnoreDelay` → `0.25`
  - Nessun None.
- **`AudioManager`** (Position `{0, 0.0024, 0}`, Scale `{1, 1, 1}`):
  - `sfxSource` → AudioSource su figlio `SFX_source` (Volume 0.2, PlayOnAwake 0, Loop 0, clip None)
  - `musicSource` → AudioSource su figlio `Music_source` (Volume 0.11, **PlayOnAwake 1**, **Loop 1**, clip None)
  - clip: `Single.wav`, `Spread.wav`, `Penetrativo.wav`, `melee.wav`, `PlayerHit.wav`, `EnemyDeath.wav`, `Explosion.wav`, `Pickup.wav` — tutti assegnati
  - `pitchVariation` → `0.2` (default nel codice: 0.1)
  - Nessun None nei campi dello script.
- **`EventSystem`** — `EventSystem` + `InputSystemUIInputModule` (Input System package).
- **`Global Light 2D`** — `Light2D` di tipo Global, Intensity 1, Color bianco.

---

## 5. Camera

### GameObject `Main Camera` — tag `MainCamera`, layer 0

**Transform**: Position `{0, 0, -10}`, **Scale `{1, 1, 1}`**, rotazione identità.

**Camera**:

| Proprietà | Valore |
|---|---|
| `m_ClearFlags` | 2 (Solid Color) |
| `m_BackGroundColor` | `{0.19215687, 0.3019608, 0.4745098, 0}` |
| `orthographic` | **1** (proiezione ortografica) |
| `orthographic size` | **4.21875** |
| `field of view` | 34 (non usato in ortografica) |
| `near clip plane` | 0.3 |
| `far clip plane` | 1000 |
| `m_Depth` | -1 |
| `m_HDR` | 1 |
| `m_AllowMSAA` | 0 |
| `m_TargetDisplay` | 0 |

**CameraShake** — nessun campo `[SerializeField]`: durata e ampiezza arrivano come
argomenti da `PlayerController.TakeDamage` e da `BossBase.Die`.

**AudioListener** — presente, nessuna configurazione.

**UniversalAdditionalCameraData** (URP): RenderShadows 1, CameraType 0 (Base),
RendererIndex -1, VolumeLayerMask 1, RenderPostProcessing 0, Antialiasing 0,
Dithering 0, StopNaN 0.

**PixelPerfectCamera** — presente:

| Proprietà | Valore |
|---|---|
| **Assets Pixels Per Unit** (`m_AssetsPPU`) | **16** |
| **Reference Resolution X** (`m_RefResolutionX`) | **240** |
| **Reference Resolution Y** (`m_RefResolutionY`) | **135** |
| `m_CropFrame` | 0 (None) |
| `m_GridSnapping` | 0 (None) |
| `m_FilterMode` | 0 |
| `m_UpscaleRT` | 0 |
| `m_PixelSnapping` | 0 |
| `m_StretchFill` | 0 |

Coerenze da notare:

- `m_AssetsPPU: 16` corrisponde al PPU di import di **tutte** le texture in
  `Assets/Sprites/` tranne `neo_zero_props_02_free.png` (vedi ultima sezione).
- 135 px / (2 × 16 px per unità) = **4.21875**, esattamente l'`orthographic size`
  salvato: i due valori sono allineati.
- `RoomManager.defaultReferenceResolution` è anch'esso `{240, 135}`, quindi la
  risoluzione salvata sulla camera coincide con quella applicata all'ingresso in
  tutte le stanze tranne `Boss_Room` (che passa a 320×180 → mezza altezza 5.625 unità,
  sufficiente per la stanza alta 10 unità).

---

## 6. Riferimenti mancanti

Scansione di ogni campo `[SerializeField]` di ogni script del progetto, in scena e nei
prefab, alla ricerca di `{fileID: 0}` (None) e di array a zero elementi.

### Riferimenti a `None` in campi di script

| Dove | Componente | Campo | Note |
|---|---|---|---|
| `Splitterling 1.prefab` | `SplitterController` | **`splitPrefab`** | Unico None su un campo di script in tutto il progetto. È coerente col comportamento voluto: chiude la catena di divisione, perché `SpawnEnemiesAroundSelf` esce subito se il prefab è null. |

**Nella scena `SampleScene.unity` non c'è nessun `[SerializeField]` a None.**
Tutti i riferimenti di `PlayerController`, `PlayerVisuals`, `RoomManager`, `GameManager`,
`AudioManager`, `DoorTrigger` (×13) e `SecretWall` risultano assegnati.

### Array serializzati a zero elementi

| Dove | Componente | Campi vuoti |
|---|---|---|
| `Splitter.prefab` | `EnemyVisuals` | `walkDown`, `walkUp`, `walkSide` |
| `Splitterling 1.prefab` | `EnemyVisuals` | `walkDown`, `walkUp`, `walkSide` |

Effetto: `EnemyVisuals.Update()` calcola direzione e frame ma la guardia
`frames != null && frame < frames.Length` non passa mai, quindi `sr.sprite` non
viene mai riassegnato. I due Splitter restano allo sprite quadrato del prefab
(blu e azzurro). Il componente è presente ma inerte; `sr.flipX` viene comunque
scritto in base alla direzione.

### Riferimenti a `None` su componenti Unity (non script)

| Dove | Componente | Campo |
|---|---|---|
| `Player/WeaponPivot` (scena) | `SpriteRenderer` | `m_Sprite` — assegnato ogni frame da `PlayerVisuals.UpdateWeapon()` |
| `Boss.prefab` → `HealthBar/Background` | `UI.Image` | `m_Sprite` — Image a colore pieno, non serve uno sprite |
| `MiniBoss.prefab` → `HealthBar/Background` | `UI.Image` | `m_Sprite` — idem |
| `AudioManager/SFX_source` (scena) | `AudioSource` | `m_audioClip` — corretto: la sorgente usa solo `PlayOneShot` |
| `AudioManager/Music_source` (scena) | `AudioSource` | `m_audioClip` — **`PlayOnAwake: 1` e `Loop: 1` ma nessuna clip**: alla partenza non suona nulla, e `AudioManager.StopMusic()` non ha nulla da fermare |

### GUID riferiti ma non presenti in `Assets/`

Questi riferimenti puntano ad asset built-in di Unity o dei pacchetti, non a file del
progetto. Non sono "mancanti", ma non sono documentabili leggendo `Assets/`:

| GUID | Usato da |
|---|---|
| `311925a0…` | sprite quadrato di default: muri e porte di tutte le stanze, `Player/MeleeVisual`, `Splitter`, `Splitterling 1` |
| `a86470a3…` | sprite di `Bomb.prefab` |
| `a97c1056…` | materiale degli SpriteRenderer di `Boss` e `MiniBoss` (Sprite-Lit-Default di URP) |
| `8f586378…` | font asset TMP condiviso da tutti i `TextMeshProUGUI` |
| `0000000000000000f000000000000000` sub `10907` | sprite UI built-in usato dagli Image `Fill` delle barre della vita |
| `ca9f5fa9…` | Input Actions asset di `InputSystemUIInputModule` su `EventSystem` |

---

## 7. Campi non serializzati

Confronto sistematico fra i campi `[SerializeField]` dichiarati in ogni script (inclusi
quelli ereditati da `EnemyBase` e `BossBase`) e le chiavi effettivamente presenti nello
YAML di ogni componente, in scena e in tutti i prefab.

**Risultato: nessun campo dichiarato risulta assente dallo YAML.**

Tutti i componenti hanno la serie completa dei propri campi serializzati, catena di
ereditarietà inclusa. Alcuni conteggi di verifica:

| Componente | Campi dichiarati (con ereditati) | Campi nello YAML |
|---|---|---|
| `PlayerController` | 34 | 34 |
| `PlayerVisuals` | 13 | 13 |
| `BossController` | 4 (`EnemyBase`) + 15 (`BossBase`) + 15 = 34 | 34 |
| `MinibossController` | 4 + 15 + 3 = 22 | 22 |
| `TeleporterController` | 4 + 8 = 12 | 12 |
| `TurretController` | 4 + 4 = 8 | 8 |
| `DasherController` | 4 + 6 = 10 | 10 |
| `ShieldedController` | 4 + 4 = 8 | 8 |
| `SplitterController` | 4 + 4 = 8 | 8 |
| `EnemyController` | 4 + 1 = 5 | 5 |
| `EnemyVisuals` | 6 | 6 |
| `PickupController` | 13 | 13 |
| `RoomManager` | 8 | 8 |
| `AudioManager` | 11 | 11 |
| `RoomCameraSettings` | 1 | 1 |

`CameraShake` e `VectorUtils` non dichiarano campi `[SerializeField]`
(`VectorUtils` è statica e non è un `MonoBehaviour`).

> Questo era il punto debole storico del progetto: `Teleporter.prefab` era rimasto
> indietro e non serializzava `contactDamage`, `hitFlashDuration`, `deathEffectDuration`,
> `roomHalfWidth` e `roomHalfHeight`, che a runtime cadevano sui default del codice.
> Nello stato attuale il prefab è stato riaperto e risalvato in Unity, e quei cinque
> campi sono presenti nello YAML.

### Casi affini (non "assenti", ma con lo stesso effetto pratico)

Non sono campi mancanti dallo YAML, ma valgono la stessa attenzione perché a runtime
il componente si comporta come se il valore non ci fosse:

| Componente | Campo | Effetto pratico |
|---|---|---|
| `Splitter` / `Splitterling 1` → `EnemyVisuals` | `walkDown`, `walkUp`, `walkSide` serializzati **vuoti** | nessun frame direzionale: lo sprite non cambia mai. `frameRate` e `idleFrame` restano senza effetto, mentre `flipX` continua a essere aggiornato. |
| `Splitterling 1` → `SplitterController` | `splitPrefab` a **None** | nessuna ulteriore divisione, e nessuna registrazione in `RoomManager.RegisterEnemySpawn` (la guardia `prefab == null` esce prima). |
| `Music_source` → `AudioSource` | `m_audioClip` a **None** con `PlayOnAwake: 1` | nessuna musica di sottofondo, nonostante la sorgente e il riferimento in `AudioManager.musicSource` siano configurati. |

### Campi impostati solo a runtime (per costruzione)

Non serializzati perché il valore arriva da codice, non dall'Inspector:

| Componente | Campi | Chi li imposta |
|---|---|---|
| `ProjectileController` | `damage`, `piercing`, `bouncesLeft` | `PlayerController.SpawnProjectile()` via `SetDamage` / `SetPiercing` / `SetBounces`; solo `lifetime` è serializzato (`3`) ed è sovrascritto con `spreadLifetime` (`0.35`) per l'arma Spread |
| `CameraShake` | durata e ampiezza | argomenti di `Shake()` da `PlayerController.TakeDamage` (`0.15` / `0.1`) e `BossBase.Die` (valori del prefab) |
| `EnemyBase` | `currentHealth`, `baseSpriteColor`, `player` | `Awake()` e `Start()` |

---

## Import degli sprite

Impostazioni lette da ogni `.png.meta` in `Assets/Sprites/`.
Sprite Mode: 1 = Single, 2 = Multiple. Filter Mode: 0 = Point, 1 = Bilinear.
Tutte le texture hanno `enableMipMap: 0`, `maxTextureSize: 2048`, `spriteExtrude: 1`.

| Texture | Pixels Per Unit | Filter Mode | Sprite Mode | Sprite ritagliati |
|---|---|---|---|---|
| `esplAI_estesa.png` | 16 | Point | Single | 1 |
| `nemico_shield_24_1.png` | 16 | Point | Multiple | 1 |
| `nemico_teleporter_24_1.png` | 16 | Point | Multiple | 1 |
| `neo_zero_armi_20x16.png` | 16 | Point | Multiple | 4 |
| `neo_zero_boss_128x96.png` | 16 | Point | Multiple | 12 |
| `neo_zero_buildings_02.png` | 16 | Point | Multiple | 21 |
| `neo_zero_char_01.png` | 16 | Point | Multiple | 27 |
| `neo_zero_dasher_128x72_1.png` | 16 | Point | Multiple | 12 |
| `neo_zero_miniboss_128x72.png` | 16 | Point | Multiple | 12 |
| `neo_zero_pickup_AI_64x48.png` | 16 | Point | Multiple | 11 |
| **`neo_zero_props_02_free.png`** | **100** | **Bilinear** | Multiple | 27 |
| `neo_zero_shielded_96x72.png` | 16 | Point | Multiple | 12 |
| `neo_zero_tileset_03.png` | 16 | Point | Multiple | 121 |
| `neo_zero_turret_256x32.png` | 16 | Point | Multiple | 8 |
| `neo_zero_walker_96x72.png` | 16 | Point | Multiple | 12 |
| `projAI_nemico_16.png` | 16 | Point | Single | 1 |
| `projAI_player_16.png` | 16 | Point | Single | 1 |

### Texture fuori standard

**`neo_zero_props_02_free.png` — PPU 100 e Filter Mode Bilinear.**

È l'unica delle 17 texture che devia da entrambi i valori usati ovunque:

- **PPU 100 invece di 16**: `PixelPerfectCamera.m_AssetsPPU` è 16, quindi questa
  texture verrebbe disegnata a 1/6,25 della dimensione delle altre e i suoi pixel non
  cadrebbero sulla griglia della camera.
- **Filter Mode Bilinear invece di Point**: l'interpolazione sfoca i pixel, in contrasto
  con la resa nitida di tutto il resto.

Nessun prefab e nessun oggetto della scena referenzia attualmente questa texture: è
importata nel progetto ma non usata, quindi la deviazione non ha effetto a runtime.

Le altre 16 texture sono tutte coerenti: **PPU 16** (allineato a `m_AssetsPPU` della
`PixelPerfectCamera`) e **Filter Mode Point**.

### Note sugli atlas

- `neo_zero_tileset_03.png` (121 sprite) alimenta i Tilemap `Floor` di tutte le stanze
  ed è usato **solo** dalla scena (49 riferimenti), da nessun prefab.
- `neo_zero_char_01.png` (27 sprite) fornisce i 9 frame usati da `PlayerVisuals`
  (3 per direzione).
- `neo_zero_pickup_AI_64x48.png` (11 sprite) è condiviso da tutti e 11 i prefab di pickup,
  uno sprite ciascuno.
- Gli atlas dei nemici (`boss`, `miniboss`, `dasher`, `walker`, `shielded`) hanno 12 sprite
  ciascuno = 4 frame × 3 direzioni, esattamente quanto serve a `EnemyVisuals`.
- `neo_zero_turret_256x32.png` ha 8 sprite = le 8 direzioni di `TurretController.directionSprites`.
- `neo_zero_armi_20x16.png` ha 4 sprite = le 4 armi di `PlayerVisuals.weaponSprites`.
- Due atlas non sono referenziati né dalla scena né da alcun prefab:
  `neo_zero_buildings_02.png` (21 sprite) e `neo_zero_props_02_free.png` (27 sprite).
