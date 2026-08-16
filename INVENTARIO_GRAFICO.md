# Inventario grafico

Report generato leggendo i 43 prefab sotto `Assets/Prefabs/` (ricorsivo) e i
174 `.meta` sotto `Assets/Sprites/`. Nessun file del progetto è stato modificato.

## Come leggere le voci

- **Sprite risolto**: il `guid` di `m_Sprite` è stato cercato fra tutti i `.meta`
  del progetto; l'`internalID` è il `fileID` del riferimento. Il nome fra
  parentesi viene da `internalIDToNameTable` / `nameFileIdTable` del `.meta`,
  cioè è il nome vero della sub-sprite dentro l'atlas.
- `internalID 21300000` è il `fileID` canonico della sprite principale di una
  texture importata in **Single mode**: non è un sub-sprite, quindi non compare
  in nessuna tabella di nomi. Non è un errore.
- **MANCANTE** = `m_Sprite: {fileID: 0}`, cioè campo vuoto.
- **NON SERIALIZZATO** = il campo non compare affatto nello YAML del prefab.
- `Square.png` è la sprite bianca di servizio del package `com.unity.2d.sprite`:
  vive fuori da `Assets/`, quindi è arte segnaposto, non arte di progetto.
- I `SpriteRenderer` dei prefab **annidati** (Boss dentro Boss_Room, MiniBoss
  dentro MiniBoss_Room) sono elencati nel file del prefab sorgente, non in
  quello della stanza: nella stanza non compaiono come documenti propri.

---

## Dettaglio per prefab

### `Assets/Prefabs/AbilityPickup.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `AbilityPickup` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Bomb.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Bomb` | neo_zero_bomba_64x16.png / internalID 1812814157110334840 (neo_zero_bomba_64x16_0) |

**Campi Sprite / Sprite[] serializzati**

- `Bomb` — `BombController`
  - `fuseSprites`: 4 elementi
    0. neo_zero_bomba_64x16.png / internalID 1812814157110334840 (neo_zero_bomba_64x16_0)
    1. neo_zero_bomba_64x16.png / internalID -6149916597694104060 (neo_zero_bomba_64x16_1)
    2. neo_zero_bomba_64x16.png / internalID 40424841103293064 (neo_zero_bomba_64x16_2)
    3. neo_zero_bomba_64x16.png / internalID -9022009606838504396 (neo_zero_bomba_64x16_3)


### `Assets/Prefabs/BombExplosion.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `BombExplosion` | esplAI_estesa.png / internalID 21300000 (sprite singolo, non un sub-sprite) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Boss.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Boss` | neo_zero_boss_128x96.png / internalID 4265802072595417137 (neo_zero_boss_128x96_2) |

**UI Image** (non è uno `SpriteRenderer`, ma ha un campo grafico `m_Sprite`)

| GameObject | Sprite assegnata |
| --- | --- |
| `Fill` | risorsa integrata di Unity (fileID 10907) |
| `Background` | MANCANTE |

**Campi Sprite / Sprite[] serializzati**

- `Boss` — `EnemyVisuals`
  - `walkDown`: 4 elementi
    0. neo_zero_boss_128x96.png / internalID 773098305117154183 (neo_zero_boss_128x96_0)
    1. neo_zero_boss_128x96.png / internalID -7330665363567392138 (neo_zero_boss_128x96_1)
    2. neo_zero_boss_128x96.png / internalID 4265802072595417137 (neo_zero_boss_128x96_2)
    3. neo_zero_boss_128x96.png / internalID -6910130333717946921 (neo_zero_boss_128x96_3)
  - `walkUp`: 4 elementi
    0. neo_zero_boss_128x96.png / internalID 8347729855962012970 (neo_zero_boss_128x96_4)
    1. neo_zero_boss_128x96.png / internalID 1561127757295806373 (neo_zero_boss_128x96_5)
    2. neo_zero_boss_128x96.png / internalID -3697169390022449941 (neo_zero_boss_128x96_6)
    3. neo_zero_boss_128x96.png / internalID -7587831688301520236 (neo_zero_boss_128x96_7)
  - `walkSide`: 4 elementi
    0. neo_zero_boss_128x96.png / internalID -8896312517890671764 (neo_zero_boss_128x96_8)
    1. neo_zero_boss_128x96.png / internalID -2772945183932856777 (neo_zero_boss_128x96_9)
    2. neo_zero_boss_128x96.png / internalID 9181077919173909662 (neo_zero_boss_128x96_10)
    3. neo_zero_boss_128x96.png / internalID 734896567733082515 (neo_zero_boss_128x96_11)


### `Assets/Prefabs/DashTrail.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `DashTrail` | vfx_mantis_128x32.png / internalID -1325478213838809258 (vfx_mantis_128x32_0) |

**Campi Sprite / Sprite[] serializzati**

- `DashTrail` — `SpriteAnimator`
  - `frames`: 4 elementi
    0. vfx_mantis_128x32.png / internalID -1325478213838809258 (vfx_mantis_128x32_0)
    1. vfx_mantis_128x32.png / internalID 4082744697144424695 (vfx_mantis_128x32_1)
    2. vfx_mantis_128x32.png / internalID 2432220273590963603 (vfx_mantis_128x32_2)
    3. vfx_mantis_128x32.png / internalID 4623266429382093354 (vfx_mantis_128x32_3)


### `Assets/Prefabs/Dasher.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Dasher` | neo_zero_dasher_128x72_1.png / internalID -3429511978585746987 (neo_zero_dasher_128x72_1_0) |

**Campi Sprite / Sprite[] serializzati**

- `Dasher` — `EnemyVisuals`
  - `walkDown`: 4 elementi
    0. neo_zero_dasher_128x72_1.png / internalID -3429511978585746987 (neo_zero_dasher_128x72_1_0)
    1. neo_zero_dasher_128x72_1.png / internalID 7443360961271710623 (neo_zero_dasher_128x72_1_1)
    2. neo_zero_dasher_128x72_1.png / internalID 946411973577745545 (neo_zero_dasher_128x72_1_2)
    3. neo_zero_dasher_128x72_1.png / internalID 5394080206273674061 (neo_zero_dasher_128x72_1_3)
  - `walkUp`: 4 elementi
    0. neo_zero_dasher_128x72_1.png / internalID -8460590405360785665 (neo_zero_dasher_128x72_1_4)
    1. neo_zero_dasher_128x72_1.png / internalID -8183278051825525537 (neo_zero_dasher_128x72_1_5)
    2. neo_zero_dasher_128x72_1.png / internalID -7533734945731328458 (neo_zero_dasher_128x72_1_6)
    3. neo_zero_dasher_128x72_1.png / internalID -7843799647459027780 (neo_zero_dasher_128x72_1_7)
  - `walkSide`: 4 elementi
    0. neo_zero_dasher_128x72_1.png / internalID -6835596826780756758 (neo_zero_dasher_128x72_1_8)
    1. neo_zero_dasher_128x72_1.png / internalID -9052992187547547747 (neo_zero_dasher_128x72_1_9)
    2. neo_zero_dasher_128x72_1.png / internalID 446340295301592738 (neo_zero_dasher_128x72_1_10)
    3. neo_zero_dasher_128x72_1.png / internalID 5016751034002246519 (neo_zero_dasher_128x72_1_11)


### `Assets/Prefabs/DoorVisuals.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `DoorVisuals` | neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/EnemyProjectile.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `EnemyProjectile` | projAI_nemico_16.png / internalID 21300000 (sprite singolo, non un sub-sprite) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/HealthRegen.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `HealthRegen` | neo_zero_pickup_AI_64x48.png / internalID 495076431 (neo_zero_pickup_AI_64x48_2) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/MeleeArcUp.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `MeleeArcUp` | neo_zero_pickup_AI_64x48.png / internalID -272409111 (neo_zero_pickup_AI_64x48_8) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/MiniBoss.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `MiniBoss` | neo_zero_miniboss_128x72.png / internalID -3428733604482057245 (neo_zero_miniboss_128x72_0) |

**UI Image** (non è uno `SpriteRenderer`, ma ha un campo grafico `m_Sprite`)

| GameObject | Sprite assegnata |
| --- | --- |
| `Fill` | risorsa integrata di Unity (fileID 10907) |
| `Background` | MANCANTE |

**Campi Sprite / Sprite[] serializzati**

- `MiniBoss` — `EnemyVisuals`
  - `walkDown`: 4 elementi
    0. neo_zero_miniboss_128x72.png / internalID -3428733604482057245 (neo_zero_miniboss_128x72_0)
    1. neo_zero_miniboss_128x72.png / internalID 6866083590777962055 (neo_zero_miniboss_128x72_1)
    2. neo_zero_miniboss_128x72.png / internalID 6979210767977914877 (neo_zero_miniboss_128x72_2)
    3. neo_zero_miniboss_128x72.png / internalID 7855290724658516980 (neo_zero_miniboss_128x72_3)
  - `walkUp`: 4 elementi
    0. neo_zero_miniboss_128x72.png / internalID 4405339542923393645 (neo_zero_miniboss_128x72_4)
    1. neo_zero_miniboss_128x72.png / internalID 4023714875672994367 (neo_zero_miniboss_128x72_5)
    2. neo_zero_miniboss_128x72.png / internalID -4358136457028450122 (neo_zero_miniboss_128x72_6)
    3. neo_zero_miniboss_128x72.png / internalID 6673603203674300739 (neo_zero_miniboss_128x72_7)
  - `walkSide`: 4 elementi
    0. neo_zero_miniboss_128x72.png / internalID -3004939320289359873 (neo_zero_miniboss_128x72_8)
    1. neo_zero_miniboss_128x72.png / internalID -6580461613902318584 (neo_zero_miniboss_128x72_9)
    2. neo_zero_miniboss_128x72.png / internalID -7290934371695931588 (neo_zero_miniboss_128x72_10)
    3. neo_zero_miniboss_128x72.png / internalID 7219054446212510586 (neo_zero_miniboss_128x72_11)


### `Assets/Prefabs/MinimapCell.prefab`

**SpriteRenderer**: nessuno.

**UI Image** (non è uno `SpriteRenderer`, ma ha un campo grafico `m_Sprite`)

| GameObject | Sprite assegnata |
| --- | --- |
| `MinimapCell` | MANCANTE |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/PickUp_ArmorUp.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `PickUp_ArmorUp` | neo_zero_pickup_AI_64x48.png / internalID 1970471583 (neo_zero_pickup_AI_64x48_6) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Pickup_Bomb.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Pickup_Bomb` | neo_zero_pickup_AI_64x48.png / internalID 1748199416 (neo_zero_pickup_AI_64x48_10) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Pickup_Credit.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Pickup_Credit` | moneta_24.png / internalID 21300000 (sprite singolo, non un sub-sprite) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Pickup_DamageUp.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Pickup_DamageUp` | neo_zero_pickup_AI_64x48.png / internalID 1676614825 (neo_zero_pickup_AI_64x48_7) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Pickup_FireRateUp.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Pickup_FireRateUp` | neo_zero_pickup_AI_64x48.png / internalID -1145505141 (neo_zero_pickup_AI_64x48_5) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Pickup_Heal.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Pickup_Heal` | neo_zero_pickup_AI_64x48.png / internalID 1490556164656645054 (neo_zero_pickup_AI_64x48_0) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Pickup_MaxHealthUp.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Pickup_MaxHealthUp` | neo_zero_pickup_AI_64x48.png / internalID 264355891 (neo_zero_pickup_AI_64x48_1) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Pickup_Speed.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Pickup_Speed` | neo_zero_pickup_AI_64x48.png / internalID -948978832 (neo_zero_pickup_AI_64x48_3) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Pickup_SpeedUp.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Pickup_SpeedUp` | neo_zero_pickup_AI_64x48.png / internalID 893452800 (neo_zero_pickup_AI_64x48_4) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/PriceLabel.prefab`

**SpriteRenderer**: nessuno.

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Projectile.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Projectile` | projAI_player_16.png / internalID 21300000 (sprite singolo, non un sub-sprite) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/ProjectileBounceUp.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `ProjectileBounceUp` | neo_zero_pickup_AI_64x48.png / internalID -1306542978 (neo_zero_pickup_AI_64x48_9) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Rooms/Boss_Room.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Wall_Top` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Right` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_S` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_E` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_W` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Bottom` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Left` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_N` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |

**Campi Sprite / Sprite[] serializzati**

- `Door_S` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_E` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_W` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_N` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)

**Prefab annidato**: `Boss.prefab`


### `Assets/Prefabs/Rooms/MiniBoss_Room.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Wall_Left` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Top` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_N` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_S` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_E` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_W` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Bottom` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Right` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |

**Campi Sprite / Sprite[] serializzati**

- `Door_N` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_S` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_E` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_W` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)

**Prefab annidato**: `MiniBoss.prefab`


### `Assets/Prefabs/Rooms/Room_8x8 debris.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Door_E` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Spawn_3` | neo_zero_tileset_03.png / internalID -1636359160385557226 (neo_zero_tileset_03_4) |
| `Spawn_6` | neo_zero_tileset_03.png / internalID -1636359160385557226 (neo_zero_tileset_03_4) |
| `Spawn_5` | neo_zero_tileset_03.png / internalID -1636359160385557226 (neo_zero_tileset_03_4) |
| `Wall_Top` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Spawn_1` | neo_zero_tileset_03.png / internalID -1636359160385557226 (neo_zero_tileset_03_4) |
| `Door_S` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_W` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_N` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Spawn_4` | neo_zero_tileset_03.png / internalID -1636359160385557226 (neo_zero_tileset_03_4) |
| `Wall_Bottom` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Left` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Obj` | neo_zero_props_02_free.png / internalID 8799516450748602278 (neo_zero_props_02_free_14) |
| `Wall_Right` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Spawn_2` | neo_zero_tileset_03.png / internalID -1636359160385557226 (neo_zero_tileset_03_4) |

**Campi Sprite / Sprite[] serializzati**

- `Door_E` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_S` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_W` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_N` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)


### `Assets/Prefabs/Rooms/Room_8x8 obstacle.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Wall_Left` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_N` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Bottom` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Right` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_W` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `debris (2)` | neo_zero_props_02_free.png / internalID -8019517311763130377 (neo_zero_props_02_free_11) |
| `debris (1)` | neo_zero_props_02_free.png / internalID 54990008418286210 (neo_zero_props_02_free_23) |
| `Door_S` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `debris` | neo_zero_props_02_free.png / internalID -7561120717017732116 (neo_zero_props_02_free_12) |
| `Wall_Top` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `debris (3)` | neo_zero_props_02_free.png / internalID -7561120717017732116 (neo_zero_props_02_free_12) |
| `Door_E` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |

**Campi Sprite / Sprite[] serializzati**

- `Door_N` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_W` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_S` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_E` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)


### `Assets/Prefabs/Rooms/Room_8x8 path.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Door_N` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Right` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_S` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Path (5)` | neo_zero_props_02_free.png / internalID -5563820319223353970 (neo_zero_props_02_free_19) |
| `Door_E` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Path (6)` | neo_zero_props_02_free.png / internalID -5563820319223353970 (neo_zero_props_02_free_19) |
| `Wall_Top` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Path (7)` | neo_zero_props_02_free.png / internalID -5563820319223353970 (neo_zero_props_02_free_19) |
| `Wall_Left` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Path` | neo_zero_props_02_free.png / internalID -5563820319223353970 (neo_zero_props_02_free_19) |
| `Path (2)` | neo_zero_props_02_free.png / internalID -5563820319223353970 (neo_zero_props_02_free_19) |
| `Path (1)` | neo_zero_props_02_free.png / internalID -5563820319223353970 (neo_zero_props_02_free_19) |
| `Door_W` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Bottom` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |

**Campi Sprite / Sprite[] serializzati**

- `Door_N` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_S` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_E` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_W` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)


### `Assets/Prefabs/Rooms/Room_8x8 pillars.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Door_W` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Right` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Left` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_S` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Bottom` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Pillar (3)` | neo_zero_props_02_free.png / internalID -3091483069343970624 (neo_zero_props_02_free_21) |
| `Door_E` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Pillar (1)` | neo_zero_props_02_free.png / internalID -3091483069343970624 (neo_zero_props_02_free_21) |
| `Pillar (2)` | neo_zero_props_02_free.png / internalID -3091483069343970624 (neo_zero_props_02_free_21) |
| `Wall_Top` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_N` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Pillar` | neo_zero_props_02_free.png / internalID -3091483069343970624 (neo_zero_props_02_free_21) |

**Campi Sprite / Sprite[] serializzati**

- `Door_W` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_S` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_E` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_N` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)


### `Assets/Prefabs/Rooms/Room_8x8 shop.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Wall_Top` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Left` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_W` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_S` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Right` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_E` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Bottom` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_N` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |

**Campi Sprite / Sprite[] serializzati**

- `Room_8x8 shop` — `ShopRoom`
  - `abilitySprites`: 4 elementi
    0. neo_zero_abilita_128x32.png / internalID 1334043512 (neo_zero_abilita_128x32_1)
    1. neo_zero_abilita_128x32.png / internalID 776923470 (neo_zero_abilita_128x32_2)
    2. neo_zero_abilita_128x32.png / internalID -1609852990 (neo_zero_abilita_128x32_0)
    3. neo_zero_abilita_128x32.png / internalID -258496169 (neo_zero_abilita_128x32_3)
- `Door_W` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_S` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_E` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_N` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)


### `Assets/Prefabs/Rooms/Room_8x8 treasure.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Door_N` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Bottom` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Left` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_S` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Right` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_E` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Top` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_W` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |

**Campi Sprite / Sprite[] serializzati**

- `Door_N` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_S` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_E` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_W` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)


### `Assets/Prefabs/Rooms/Room_8x8.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Door_E` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Top` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_N` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_W` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Left` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_S` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Bottom` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Right` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |

**Campi Sprite / Sprite[] serializzati**

- `Door_E` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_N` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_W` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_S` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)


### `Assets/Prefabs/Rooms/Room_Secret.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Door_W` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Right` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_N` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_S` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Door_E` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Top` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Left` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |
| `Wall_Bottom` | Square.png / internalID 7482667652216324306  [package Unity, non in Assets/] |

**Campi Sprite / Sprite[] serializzati**

- `Door_W` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_N` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_S` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)
- `Door_E` — `DoorTrigger`
  - `openSprite`: neo_zero_tileset_03.png / internalID 36024015 (neo_zero_tileset_03_70)
  - `closedSprite`: neo_zero_tileset_03.png / internalID -1301808215 (neo_zero_tileset_03_71)


### `Assets/Prefabs/ShieldVisual.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `ShieldVisual` | shield_32.png / internalID -7250838573452026398 (shield_32_0) |

**Campi Sprite / Sprite[] serializzati**

- `ShieldVisual` — `SpriteAnimator`
  - `frames`: 4 elementi
    0. shield_32.png / internalID -7250838573452026398 (shield_32_0)
    1. shield_32.png / internalID 977735627519868253 (shield_32_1)
    2. shield_32.png / internalID -3801158366406982806 (shield_32_2)
    3. shield_32.png / internalID -628511252136705835 (shield_32_3)


### `Assets/Prefabs/Shielded.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Shielded` | neo_zero_shielded_96x72.png / internalID 2959305750356399837 (neo_zero_shielded_96x72_0) |
| `Shield` | nemico_shield_24_1.png / internalID -7639613554886755590 (nemico_shield_24_1_0) |

**Campi Sprite / Sprite[] serializzati**

- `Shielded` — `EnemyVisuals`
  - `walkDown`: 4 elementi
    0. neo_zero_shielded_96x72.png / internalID 2959305750356399837 (neo_zero_shielded_96x72_0)
    1. neo_zero_shielded_96x72.png / internalID 9158704985551393034 (neo_zero_shielded_96x72_1)
    2. neo_zero_shielded_96x72.png / internalID 2959305750356399837 (neo_zero_shielded_96x72_0)
    3. neo_zero_shielded_96x72.png / internalID 5681285473553416712 (neo_zero_shielded_96x72_3)
  - `walkUp`: 4 elementi
    0. neo_zero_shielded_96x72.png / internalID 989027554288294617 (neo_zero_shielded_96x72_4)
    1. neo_zero_shielded_96x72.png / internalID 2517885703358321720 (neo_zero_shielded_96x72_5)
    2. neo_zero_shielded_96x72.png / internalID -7267567067719793262 (neo_zero_shielded_96x72_6)
    3. neo_zero_shielded_96x72.png / internalID 4693181708430778469 (neo_zero_shielded_96x72_7)
  - `walkSide`: 4 elementi
    0. neo_zero_shielded_96x72.png / internalID -2381496131299616125 (neo_zero_shielded_96x72_8)
    1. neo_zero_shielded_96x72.png / internalID -9034738852396845664 (neo_zero_shielded_96x72_9)
    2. neo_zero_shielded_96x72.png / internalID 5306486315582813556 (neo_zero_shielded_96x72_10)
    3. neo_zero_shielded_96x72.png / internalID -4263584731815103436 (neo_zero_shielded_96x72_11)


### `Assets/Prefabs/ShockwaveEffect.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `ShockwaveEffect` | shockwave_80_1.png / internalID 239417142734518084 (shockwave_80_1_0) |

**Campi Sprite / Sprite[] serializzati**

- `ShockwaveEffect` — `SpriteAnimator`
  - `frames`: 4 elementi
    0. shockwave_80_1.png / internalID 239417142734518084 (shockwave_80_1_0)
    1. shockwave_80_1.png / internalID 2751416824323264239 (shockwave_80_1_1)
    2. shockwave_80_1.png / internalID -4420729291102798460 (shockwave_80_1_2)
    3. shockwave_80_1.png / internalID 3347567049406907998 (shockwave_80_1_3)


### `Assets/Prefabs/Splitter.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Splitter` | neo_zero_splitter_96x72.png / internalID -1535756103582637189 (neo_zero_splitter_96x72_0) |

**Campi Sprite / Sprite[] serializzati**

- `Splitter` — `EnemyVisuals`
  - `walkDown`: 4 elementi
    0. neo_zero_splitter_96x72.png / internalID -1535756103582637189 (neo_zero_splitter_96x72_0)
    1. neo_zero_splitter_96x72.png / internalID -8183643777478913615 (neo_zero_splitter_96x72_2)
    2. neo_zero_splitter_96x72.png / internalID -2936627106843499732 (neo_zero_splitter_96x72_1)
    3. neo_zero_splitter_96x72.png / internalID -1535756103582637189 (neo_zero_splitter_96x72_0)
  - `walkUp`: 4 elementi
    0. neo_zero_splitter_96x72.png / internalID 195334783562699594 (neo_zero_splitter_96x72_4)
    1. neo_zero_splitter_96x72.png / internalID 7839209194567785879 (neo_zero_splitter_96x72_6)
    2. neo_zero_splitter_96x72.png / internalID 195334783562699594 (neo_zero_splitter_96x72_4)
    3. neo_zero_splitter_96x72.png / internalID 6477461779376029515 (neo_zero_splitter_96x72_7)
  - `walkSide`: 4 elementi
    0. neo_zero_splitter_96x72.png / internalID 4530329364892247421 (neo_zero_splitter_96x72_8)
    1. neo_zero_splitter_96x72.png / internalID -4063327606794354989 (neo_zero_splitter_96x72_9)
    2. neo_zero_splitter_96x72.png / internalID 4530329364892247421 (neo_zero_splitter_96x72_8)
    3. neo_zero_splitter_96x72.png / internalID 6103061276241759437 (neo_zero_splitter_96x72_10)


### `Assets/Prefabs/Splitterling 1.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Splitterling 1` | neo_zero_splitterling_64x48.png / internalID 4303449009119613035 (neo_zero_splitterling_64x48_0) |

**Campi Sprite / Sprite[] serializzati**

- `Splitterling 1` — `EnemyVisuals`
  - `walkDown`: 4 elementi
    0. neo_zero_splitterling_64x48.png / internalID 4303449009119613035 (neo_zero_splitterling_64x48_0)
    1. neo_zero_splitterling_64x48.png / internalID 8221793323272851459 (neo_zero_splitterling_64x48_1)
    2. neo_zero_splitterling_64x48.png / internalID 4903551771205920784 (neo_zero_splitterling_64x48_2)
    3. neo_zero_splitterling_64x48.png / internalID 3342091157818938946 (neo_zero_splitterling_64x48_3)
  - `walkUp`: 4 elementi
    0. neo_zero_splitterling_64x48.png / internalID -6153918035120087040 (neo_zero_splitterling_64x48_4)
    1. neo_zero_splitterling_64x48.png / internalID -4139166805554858585 (neo_zero_splitterling_64x48_5)
    2. neo_zero_splitterling_64x48.png / internalID 5097377343736224200 (neo_zero_splitterling_64x48_6)
    3. neo_zero_splitterling_64x48.png / internalID 4062867881348355310 (neo_zero_splitterling_64x48_7)
  - `walkSide`: 4 elementi
    0. neo_zero_splitterling_64x48.png / internalID -7863023125175856473 (neo_zero_splitterling_64x48_9)
    1. neo_zero_splitterling_64x48.png / internalID 5855667574571913772 (neo_zero_splitterling_64x48_8)
    2. neo_zero_splitterling_64x48.png / internalID -3567544396381446234 (neo_zero_splitterling_64x48_10)
    3. neo_zero_splitterling_64x48.png / internalID -2125576270745858536 (neo_zero_splitterling_64x48_11)


### `Assets/Prefabs/Teleporter.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Teleporter` | nemico_teleporter_24_1.png / internalID 4024420540613375730 (nemico_teleporter_24_1_0) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/TeleporterIndicator.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `TeleporterIndicator` | nemico_teleporter_24_1.png / internalID 4024420540613375730 (nemico_teleporter_24_1_0) |

**Campi Sprite / Sprite[] serializzati**: nessuno (il prefab non ha componenti che ne dichiarano).


### `Assets/Prefabs/Turret.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Turret` | neo_zero_turret_256x32.png / internalID 1810429176119811874 (neo_zero_turret_256x32_0) |

**Campi Sprite / Sprite[] serializzati**

- `Turret` — `TurretController`
  - `directionSprites`: 8 elementi
    0. neo_zero_turret_256x32.png / internalID 1810429176119811874 (neo_zero_turret_256x32_0)
    1. neo_zero_turret_256x32.png / internalID 1649473792291162202 (neo_zero_turret_256x32_1)
    2. neo_zero_turret_256x32.png / internalID -2872207545022430745 (neo_zero_turret_256x32_2)
    3. neo_zero_turret_256x32.png / internalID 9098868591587381789 (neo_zero_turret_256x32_3)
    4. neo_zero_turret_256x32.png / internalID 8758251338150175011 (neo_zero_turret_256x32_4)
    5. neo_zero_turret_256x32.png / internalID 3904459997027841871 (neo_zero_turret_256x32_5)
    6. neo_zero_turret_256x32.png / internalID 5574889176712077826 (neo_zero_turret_256x32_6)
    7. neo_zero_turret_256x32.png / internalID 2478129024445583238 (neo_zero_turret_256x32_7)


### `Assets/Prefabs/Walker.prefab`

**SpriteRenderer**

| GameObject | Sprite assegnata |
| --- | --- |
| `Walker` | neo_zero_walker_96x72.png / internalID 2609183553645755923 (neo_zero_walker_96x72_2) |

**Campi Sprite / Sprite[] serializzati**

- `Walker` — `EnemyVisuals`
  - `walkDown`: 4 elementi
    0. neo_zero_walker_96x72.png / internalID 2004902879067164879 (neo_zero_walker_96x72_0)
    1. neo_zero_walker_96x72.png / internalID -8219397959613254314 (neo_zero_walker_96x72_1)
    2. neo_zero_walker_96x72.png / internalID 2609183553645755923 (neo_zero_walker_96x72_2)
    3. neo_zero_walker_96x72.png / internalID -412045188947647153 (neo_zero_walker_96x72_3)
  - `walkUp`: 4 elementi
    0. neo_zero_walker_96x72.png / internalID 8112918248409167196 (neo_zero_walker_96x72_4)
    1. neo_zero_walker_96x72.png / internalID 2191650228906504390 (neo_zero_walker_96x72_5)
    2. neo_zero_walker_96x72.png / internalID -7475410645821433079 (neo_zero_walker_96x72_6)
    3. neo_zero_walker_96x72.png / internalID 565343974522415823 (neo_zero_walker_96x72_7)
  - `walkSide`: 4 elementi
    0. neo_zero_walker_96x72.png / internalID -6113690529437154101 (neo_zero_walker_96x72_8)
    1. neo_zero_walker_96x72.png / internalID -8196037698668108982 (neo_zero_walker_96x72_9)
    2. neo_zero_walker_96x72.png / internalID 3528509181494430579 (neo_zero_walker_96x72_10)
    3. neo_zero_walker_96x72.png / internalID -7617867709090185172 (neo_zero_walker_96x72_11)


---

## Copertura dei campi dichiarati negli script

Gli 8 script del progetto che dichiarano campi `Sprite` / `Sprite[]`, e dove
finiscono. Un campo può risultare `NON SERIALIZZATO` solo se il componente non
compare in nessun prefab: in quel caso il campo vive su un oggetto di scena.

| Script | Campi | Presente in prefab |
| --- | --- | --- |
| `BombController` | `fuseSprites[]` | sì, 1 istanza |
| `DoorTrigger` | `openSprite`, `closedSprite` | sì, 40 istanze |
| `EnemyVisuals` | `walkDown[]`, `walkUp[]`, `walkSide[]` | sì, 7 istanze |
| `PlayerAbilities` | `abilityIcons[]` | **no** — solo su oggetti di scena |
| `PlayerVisuals` | `walkDown[]`, `walkUp[]`, `walkSide[]`, `weaponSprites[]` | **no** — solo su oggetti di scena |
| `ShopRoom` | `abilitySprites[]` | sì, 1 istanza |
| `SpriteAnimator` | `frames[]` | sì, 3 istanze |
| `TurretController` | `directionSprites[]` | sì, 1 istanza |

---

## Prefab con almeno un campo grafico scoperto

Ordinati per numero di campi vuoti (decrescente).

| # vuoti | Prefab | Cosa è scoperto |
| --- | --- | --- |
| 1 | `Assets/Prefabs/Boss.prefab` | `Background` UI Image.m_Sprite |
| 1 | `Assets/Prefabs/MiniBoss.prefab` | `Background` UI Image.m_Sprite |
| 1 | `Assets/Prefabs/MinimapCell.prefab` | `MinimapCell` UI Image.m_Sprite |

### Nota di lettura

Tutti e tre i buchi sono `UI Image` con `m_Sprite` vuoto, non
`SpriteRenderer`: sono i rettangoli pieni della barra della vita dei boss e
della cella della minimappa. Un `Image` senza sprite disegna un rettangolo
del proprio `m_Color`, ed è **esattamente** l'aspetto voluto qui — la cella
della minimappa viene colorata da `MinimapController.Paint()` e la barra
della vita da `BossBase.UpdateHealthBarUI()`. Non c'è quindi nessuna arte
davvero mancante: sono campi legittimamente lasciati vuoti.

Il punto che merita attenzione è un altro, e non risulta da un campo vuoto:
i muri e le porte di tutte le stanze, più `AbilityPickup`, usano
`Square.png` del package 2D di Unity come sprite. È arte segnaposto
importata da un package, non dal progetto.
