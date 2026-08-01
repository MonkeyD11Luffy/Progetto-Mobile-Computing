# SETUP — Stato attuale di scena e prefab

Documento generato tramite parsing diretto dei file YAML di Unity (`Assets/Scenes/SampleScene.unity`, `ProjectSettings/TagManager.asset`, `Assets/Prefabs/*.prefab`), seguendo i riferimenti `fileID`/`guid` per ricostruire gerarchia, componenti e campi serializzati. È puramente descrittivo: nessuna proposta, nessun giudizio.

Rigenerato dopo il refactoring degli script (introduzione di `BossBase`, `VectorUtils`, `PlayerVisuals` e rinomina di `TeleporterController` / `MinibossController`) e dopo la revisione della scena in Unity.

> **Nota di lettura sui campi non serializzati.** Unity scrive nel file YAML solo i campi esistenti al momento dell'ultimo salvataggio. Diversi campi aggiunti di recente agli script **non compaiono** in scena/prefab: a runtime prendono il valore di default dello script. Sono segnalati caso per caso e riepilogati nella sezione 5.

---

## 1. Gerarchia della scena

Root della scena (8 oggetti top-level):

```
Main Camera        [MainCamera]  Camera, CameraShake, AudioListener, (+1 script di pacchetto)
Player             [Player]      SpriteRenderer, Rigidbody2D, BoxCollider2D, PlayerController, PlayerVisuals
├── MeleeVisual    [Untagged]    SpriteRenderer
└── WeaponPivot    [Untagged]    SpriteRenderer
Global Light 2D    [Untagged]    (script di pacchetto URP)
AudioManager       [Untagged]    AudioManager
├── SFX_source     [Untagged]    AudioSource
└── Music_source   [Untagged]    AudioSource
EventSystem        [Untagged]    (script di pacchetto)
RoomManager        [Untagged]    RoomManager
└── 8 stanze (vedi sotto)
Canvas             [Untagged]    Canvas, CanvasScaler, GraphicRaycaster
GameManager        [Untagged]    GameManager
```

### RoomManager

Componente `RoomManager` sul GameObject **RoomManager** (`fileID 1684176008`, MonoBehaviour `1684176009`). Campi serializzati:

- `startingRoom`: `{fileID: 1767393748}` → **Room_1**
- `finalRoom`: `{fileID: 504927612}` → **Boss_Room**
- `pickupPrefabs`: `Pickup_Heal`, `Pickup_Speed`, `Pickup_Bomb`
- `pickupDropChance`: `0.35`
- `permanentUpgradePrefabs`: 8 elementi (`Pickup_MaxHealthUp`, `Pickup_DamageUp`, `Pickup_SpeedUp`, `Pickup_FireRateUp`, `HealthRegen`, `MeleeArcUp`, `PickUp_ArmorUp`, `ProjectileBounceUp`)
- `doorIgnoreDelay`: **non serializzato** → default dello script `0.25`

Il Transform di RoomManager ha 8 figli diretti, cioè le 8 stanze, nell'ordine serializzato:

`Room_1`, `Room_Secret`, `Room_2`, `Room_3`, `Room_4`, `Room_5`, `MiniBoss_Room`, `Boss_Room`

Tutte le stanze hanno `m_IsActive: 0` nel file di scena (disattivate finché `RoomManager` non le attiva a runtime).

**Tutti i 14 nemici in scena sono Prefab Instance**: non esistono più nemici come GameObject completi con campi serializzati nella scena (a differenza della versione precedente di questo documento). Tag e valori sono quindi quelli del prefab sorgente; l'unico override è la posizione locale (e il nome, per i duplicati).

### Contenuto delle stanze

| Stanza | fileID GO | Nemici (prefab, posizione locale) | Porte |
|---|---|---|---|
| Room_1 | 1767393748 | — | `Door_ToRoom2` → Room_2, spawn `(-2.8, 0)` |
| Room_Secret | 868803657 | — | `Door_ToRoom2` → **Room_1**, spawn `(0, 3)` |
| Room_2 | 1395311197 | Teleporter `(-2,-3)`, Walker `(3,3)`, Shielded `(3,-3)` | `Door_ToRoom1` → Room_1 `(2.8, 0)` · `Door_ToRoom3` → Room_3 `(0, -2.8)` · `Door_ToRoom4` → Room_4 `(-2.9, 0)` · `Door_ToRoom5` → Room_5 `(0, 2.8)` |
| Room_3 | 1536868149 | Turret `(3,3)`, Turret (1) `(-3,3)`, Walker `(0,2)` | `Door_ToRoom2` → Room_2 `(0, 2.8)` |
| Room_4 | 142039400 | Splitter `(2,1)`, Turret `(3,-3)`, Walker `(-1,3)` | `Door_ToRoom2` → Room_2 `(2.9, 0)` |
| Room_5 | 1686258021 | Dasher `(-3,-2)`, Dasher (1) `(2,-2)`, Teleporter `(0,0)` | `Door_ToRoom2` → Room_2 `(0, -2.8)` · `Door_ToMiniBoss` → MiniBoss_Room `(0, 2.8)` |
| MiniBoss_Room | 29436066 | MiniBoss `(0,0)` | `Door_ToRoom5` → Room_5 `(0, -2.9)` · `Door_ToBossRoom` → Boss_Room `(0, 2.9)` |
| Boss_Room | 504927612 | Boss `(0,0)` | `Door_ToMiniBoss` → MiniBoss_Room `(0, -2.9)` |

La posizione indicata per le porte è `playerSpawnPosition`, cioè dove viene teletrasportato il player **nella stanza di destinazione**.

Ogni stanza ha inoltre i muri (`Wall_Top` / `Wall_Left` / `Wall_Bottom` / `Wall_Right`, tag `Wall`, SpriteRenderer + BoxCollider2D); Room_1 ha in più `Wall_Secret` con il componente **SecretWall** (`targetRoom` → Room_Secret, `playerSpawnPosition (0, -2.9)`).

Anomalia riportata così com'è serializzata: in Room_Secret l'oggetto si chiama `Door_ToRoom2` ma il campo `targetRoom` risolve a **Room_1**.

---

## 2. Prefab

25 prefab in `Assets/Prefabs/`. Per ognuno: componenti e campi `[SerializeField]` serializzati. `{fileID: 0}` è marcato **None**.

### Bomb.prefab
GO `Bomb` (tag `Untagged`) — Transform, `BombController`, SpriteRenderer.

| Campo | Valore |
|---|---|
| fuseTime | 2 |
| explosionRadius | 1.5 |
| explosionDamage | 3 |
| explosionEffectPrefab | assegnato → `BombExplosion.prefab` |

### BombExplosion.prefab
GO `BombExplosion` (tag `Untagged`) — Transform, `ExplosionEffect`, SpriteRenderer. `duration: 0.3`, `maxScale: 3`. SpriteRenderer colore `(1, 0.615, 0, 1)`.

### Boss.prefab
4 GameObject: `Boss` (tag `Enemy` — Transform, SpriteRenderer, Rigidbody2D, BoxCollider2D, `BossController`), `HealthBar` (Canvas + CanvasScaler + GraphicRaycaster), `Background` e `Fill` (Image, figli di HealthBar).

Rigidbody2D: `m_GravityScale: 0`, `m_Mass: 10`, `m_BodyType: 0`, `m_Constraints: 4`. BoxCollider2D `(1,1)`, non trigger.

Dopo il refactoring i campi sono dichiarati in due classi: **`BossBase`** (movimento, telegrafo, proiettili, carica, rabbia, barra vita, feedback morte) e **`BossController`** (solo le azioni specifiche e i colori di fase). La serializzazione resta piatta, quindi i valori nel prefab non sono cambiati.

| Campo | Valore | Dichiarato in |
|---|---|---|
| maxHealth | 40 | EnemyBase |
| contactDamage | 2 | EnemyBase |
| hitFlashDuration | 0.08 | EnemyBase |
| deathEffectDuration | 0.2 | EnemyBase |
| moveSpeed | 1.2 | BossBase |
| moveDuration | 2 | BossBase |
| telegraphDuration | 0.5 | BossBase |
| telegraphColor | `(1,1,1,1)` | BossBase |
| projectilePrefab | assegnato → `EnemyProjectile.prefab` | BossBase |
| projectileSpeed | 5 | BossBase |
| chargeSpeed | 9 | BossBase |
| chargeDuration | 0.5 | BossBase |
| enrageHealthRatio | 0.15 | BossBase |
| enrageSpeedMultiplier | 1.3 | BossBase |
| enrageColor | `(1,0,0,1)` | BossBase |
| healthBarFill | assegnato → `Fill` (fileID interno 7393254670952261867) | BossBase |
| healthBarRoot | assegnato → `Background` (fileID interno 7864239742568175789) | BossBase |
| deathShakeDuration | 0.3 | BossBase |
| deathShakeMagnitude | 0.15 | BossBase |
| radialCount | 8 | BossController |
| volleySpread | 15 | BossController |
| spiralWaveCount | 3 | BossController |
| spiralBulletsPerWave | 5 | BossController |
| spiralWaveInterval | 0.3 | BossController |
| spiralRotationStep | 20 | BossController |
| spiralProjectileSpeed | 3.5 | BossController |
| maxChargeChain | 2 | BossController |
| chargeChainChance | 0.5 | BossController |
| minionPrefab | assegnato → `Dasher.prefab` | BossController |
| minionCount | 2 | BossController |
| **minionSpawnRadius** | **non serializzato** → default `1.2` | BossController |
| enragePulseSpeed | 6 | BossController |
| phase2Color | `(1, 0.6, 0.2, 1)` | BossController |
| phase3Color | `(1, 0.2, 0.2, 1)` | BossController |

Su `Fill`: `m_Material` **None**, `m_Sprite` assegnato (sprite UI di default), `m_Type: 3` (Filled). Su `Background`: `m_Material` **None**, `m_Sprite` **None**.

### Dasher.prefab
GO `Dasher` (tag `Enemy`) — Transform, SpriteRenderer, Rigidbody2D (`m_Mass: 1`, `m_Constraints: 4`), `DasherController`, BoxCollider2D `(1,1)`.

| Campo | Valore |
|---|---|
| maxHealth | 4 |
| contactDamage | 2 |
| idleDuration | 1.5 |
| telegraphDuration | 0.6 |
| dashDuration | 0.4 |
| dashSpeed | 12 |
| idleMoveSpeed | 1 |
| telegraphColor | `(0.088, 0, 0.514, 1)` |

Nota: `hitFlashDuration` / `deathEffectDuration` (da `EnemyBase`) non risultano serializzati in questo file.

### EnemyProjectile.prefab
GO `EnemyProjectile` (tag `EnemyProjectile`) — Transform, SpriteRenderer, `EnemyProjectileController`, Rigidbody2D, CircleCollider2D (trigger, radius 0.5). `lifetime: 4`.

### MiniBoss.prefab
4 GameObject: `MiniBoss` (tag `Enemy` — Transform, SpriteRenderer, Rigidbody2D, BoxCollider2D, **`MinibossController`**), `HealthBar` (Canvas + CanvasScaler + GraphicRaycaster), `Fill` e `Background` (Image).

> Lo script è ora `Assets/Scripts/MinibossController.cs` (prima il file si chiamava `MiniBossController.cs` con la classe `MinibossController`: nome file e nome classe non coincidevano). Il GUID del `.meta` è invariato, quindi il riferimento nel prefab è rimasto valido.

Rigidbody2D: `m_Mass: 1`, `m_Constraints: 4`. BoxCollider2D `(1,1)`.

| Campo | Valore | Dichiarato in |
|---|---|---|
| maxHealth | 15 | EnemyBase |
| contactDamage | 2 | EnemyBase |
| hitFlashDuration | 0.08 | EnemyBase |
| deathEffectDuration | 0.2 | EnemyBase |
| moveSpeed | 1.5 | BossBase |
| moveDuration | 1.5 | BossBase |
| telegraphDuration | 0.4 | BossBase |
| telegraphColor | `(1,1,1,1)` | BossBase |
| projectilePrefab | assegnato → `EnemyProjectile.prefab` | BossBase |
| projectileSpeed | 5 | BossBase |
| chargeSpeed | 7 | BossBase |
| chargeDuration | 0.4 | BossBase |
| enrageHealthRatio | 0.4 | BossBase |
| enrageSpeedMultiplier | 1.4 | BossBase |
| enrageColor | `(1, 0.3, 0.1, 1)` | BossBase |
| healthBarFill | assegnato → `Fill` (fileID interno 3725787053202305065) | BossBase |
| healthBarRoot | assegnato → `Background` (fileID interno 3050016494842750956) | BossBase |
| deathShakeDuration | 0.25 | BossBase |
| deathShakeMagnitude | 0.12 | BossBase |
| projectilesPerBurst | 5 | MinibossController |
| aimedBurstCount | 3 | MinibossController |
| aimedSpread | 12 | MinibossController |

### Pickup (11 prefab)
Tutti con Transform, SpriteRenderer, CircleCollider2D (trigger, radius 0.5), `PickupController`. Cambia solo `pickupType`; gli altri campi restano ai valori di default del prefab da cui sono stati duplicati.

| Prefab | pickupType |
|---|---|
| Pickup_Heal | 0 (Heal) |
| Pickup_Speed | 1 (SpeedBoost) |
| Pickup_Bomb | 2 (Bomb) |
| Pickup_MaxHealthUp | 3 (MaxHealthUp) |
| Pickup_DamageUp | 4 (DamageUp) |
| Pickup_SpeedUp | 5 (SpeedUp) |
| Pickup_FireRateUp | 6 (FireRateUp) |
| HealthRegen | 7 (HealthRegenUp) |
| PickUp_ArmorUp | 8 (ArmorUp) |
| MeleeArcUp | 9 (MeleeArcUp) |
| ProjectileBounceUp | 10 (ProjectileBounceUp) |

Valori comuni: `healAmount 2`, `speedMultiplier 1.5`, `boostDuration 5`, `bombAmount 1`, `maxHealthIncrease 2`, `damageIncrease 1`, `speedIncrease 1`, `fireRateIncrease 0.05`, `healthRegenChanceIncrease 0.15`, `contactDamageReductionIncrease 1`, `meleeArcIncrease 0.15`, `projectileBounceIncrease 1`. I prefab più vecchi (`Pickup_Heal`, `Pickup_Speed`, `Pickup_Bomb`, `Pickup_DamageUp`, `Pickup_FireRateUp`, `Pickup_MaxHealthUp`, `Pickup_SpeedUp`) hanno solo il sottoinsieme di campi esistente al momento del loro salvataggio.

### Projectile.prefab
GO `Projectile` (tag `PlayerProjectile`) — Transform, SpriteRenderer, Rigidbody2D, CircleCollider2D (trigger, radius 0.5), `ProjectileController`. `lifetime: 3`.

### Shielded.prefab
3 GameObject: `Shielded` (tag `Enemy` — Transform, SpriteRenderer, `ShieldedController`, Rigidbody2D, BoxCollider2D), `Shield` e `ShieldPivot` (figli).

| Campo | Valore |
|---|---|
| maxHealth | 5 |
| contactDamage | 1 |
| moveSpeed | 1.3 |
| shieldVisual | assegnato → `Shield` (fileID interno 2031629430023782870) |
| shieldArc | 0.3 |
| shieldRotationSpeed | 80 |

### Splitter.prefab
GO `Splitter` (tag `Enemy`) — Transform, SpriteRenderer, Rigidbody2D, BoxCollider2D, `SplitterController`.

| Campo | Valore |
|---|---|
| maxHealth | 8 |
| contactDamage | 1 |
| moveSpeed | 1.2 |
| splitPrefab | assegnato → `Splitterling 1.prefab` |
| splitCount | 2 |
| splitSpread | 0.6 |

### "Splitterling 1.prefab"
GO `Splitterling 1` (tag `Enemy`) — stessi componenti. `maxHealth 1`, `contactDamage 1`, `moveSpeed 3.5`, `splitPrefab` **None** (non si divide oltre), `splitCount 2`, `splitSpread 0.6`.

### Teleporter.prefab
GO `Teleporter` (tag `Enemy`) — Transform, SpriteRenderer, Rigidbody2D (`m_BodyType: 1` Kinematic), **`TeleporterController`**, BoxCollider2D.

> Lo script è ora `Assets/Scripts/TeleporterController.cs` (prima il file si chiamava `Teleporter Controller.cs`, con uno spazio, mentre la classe era `TeleporterController`). Il GUID del `.meta` è invariato.

| Campo | Valore |
|---|---|
| idleDuration | 1.8 |
| vanishDuration | 0.4 |
| minDistanceFromPlayer | 2.5 |
| maxDistanceFromPlayer | 4 |
| projectilePrefab | assegnato → `EnemyProjectile.prefab` |
| projectileSpeed | 7 |
| maxHealth | 2 |

Non serializzati (default dello script): `roomHalfWidth 3`, `roomHalfHeight 3`, `contactDamage 1`, `hitFlashDuration 0.08`, `deathEffectDuration 0.2`.

### Turret.prefab
GO `Turret` (tag `Enemy`) — Transform, SpriteRenderer, Rigidbody2D (Kinematic), BoxCollider2D, `TurretController`. `maxHealth 4`, `contactDamage 1`, `projectilePrefab` → `EnemyProjectile.prefab`, `projectileSpeed 6`, `fireInterval 2`.

### Walker.prefab
GO `Walker` (tag `Enemy`) — Transform, SpriteRenderer, `EnemyController`, Rigidbody2D, BoxCollider2D. `maxHealth 4`, `contactDamage 1`, `moveSpeed 2`.

### Prefab non più presenti / non referenziati

- **`Player.prefab` non esiste più** nel progetto (cancellato insieme al suo `.meta`). Il Player vive solo come GameObject nella scena, descritto nella sezione 4.
- **`old.prefab`** è presente ma non tracciato da git e referenzia uno script `NewMonoBehaviourScript` che non esiste in `Assets/Scripts/`: è un prefab con riferimento rotto, non usato da nulla.

---

## 3. Tag e Layer

### Tag (da `ProjectSettings/TagManager.asset`)

Tag custom definiti: `EnemyProjectile`, `Enemy`, `PlayerProjectile`, `Door`, `Wall`.
Tag built-in di Unity usati: `Player`, `MainCamera`, `Untagged`.

| Tag | Usato negli script (`CompareTag` / `FindGameObjectWithTag`) | Chi lo porta |
|---|---|---|
| `Enemy` | `BombController.cs:49`, `ProjectileController.cs:44`, `PlayerController.cs:277`, `RoomManager.cs:188` | i 9 prefab nemici; 14 istanze in scena |
| `Wall` | `ProjectileController.cs:62`, `EnemyProjectileController.cs:26` | tutti i `Wall_*` (32 in scena) |
| `Door` | `RoomManager.cs:167` | tutti i `Door_To*` (13 in scena) |
| `Player` | `EnemyBase.cs:40`, `EnemyBase.cs:145`, `RoomManager.cs:60`, `DoorTrigger.cs:11`, `SecretWall.cs:42`, `PickupController.cs:32`, `EnemyProjectileController.cs:15`, `BombController.cs:39` | GO `Player` in scena |
| `EnemyProjectile` | `PlayerController.cs:267` (parata corpo a corpo) | `EnemyProjectile.prefab` |
| `PlayerProjectile` | nessun uso negli script: assegnato al prefab ma mai riletto | `Projectile.prefab` |
| `MainCamera` | non referenziato (nessun `Camera.main` negli script) | GO `Main Camera` |

La ricerca di `Enemy` e `Door` fatta da `RoomManager` è **ricorsiva** su tutta la gerarchia della stanza (prima si fermava ai figli diretti).

### Layer

Definiti: 0 Default, 1 TransparentFX, 2 Ignore Raycast, 4 Water, 5 UI; il resto vuoto. Nessun uso di `LayerMask` o `gameObject.layer` negli script. In scena gli oggetti sotto `Canvas` stanno su layer 5 (UI), tutto il resto su layer 0.

---

## 4. Player e Canvas

### GameObject `Player` (fileID 539440560, tag `Player`)

Componenti: Transform, SpriteRenderer, Rigidbody2D (`m_Mass: 1`, `m_Constraints: 4`), BoxCollider2D `(1,1)`, `PlayerController` (MonoBehaviour `539440565`), `PlayerVisuals` (MonoBehaviour `539440566`).
Figli: `MeleeVisual` (Transform + SpriteRenderer) e **`WeaponPivot`** (Transform + SpriteRenderer).

**`PlayerController`** — campi serializzati:

| Campo | Valore |
|---|---|
| moveSpeed | 5 |
| projectilePrefab | assegnato → `Projectile.prefab` |
| projectileSpeed | 10 |
| fireCooldown | 0.3 |
| projectileDamage | 1 |
| firePointDistance | 0.5 |
| spreadAngle | 20 |
| spreadCooldownMult | 1.6 |
| pierceCooldownMult | 1.3 |
| spreadLifetime | 0.35 |
| meleeRange | 1.1 |
| meleeArc | 0.3 |
| meleeDamageMult | 2 |
| meleeCooldownMult | 1.4 |
| bombPrefab | assegnato → `Bomb.prefab` |
| maxBombs | 3 |
| bombCooldown | 1 |
| maxHealth | 6 |
| invulnerabilityDuration | 0.8 |
| healthRegenInterval | 5 |
| healthRegenChance | 0 |
| contactDamageReduction | 0 |
| projectileBounces | 0 |
| hitFlashDuration | 0.1 |
| hitShakeDuration | 0.15 |
| hitShakeMagnitude | 0.1 |
| healthText | assegnato → `HealthText` |
| bombText | assegnato → `BombText` |
| weaponText | assegnato → `WeaponText` |
| upgradePopupText | assegnato → `UpgradePopUpText` |
| upgradePopupDuration | 1.5 |
| meleeVisual | assegnato → `MeleeVisual` |
| meleeVisualDuration | 0.1 |
| **playerVisuals** | **non serializzato** → **None** a runtime |

Conseguenza operativa del campo `playerVisuals` non assegnato: `Fire()` usa il ramo di riserva `transform.position + direction * firePointDistance` invece di `PlayerVisuals.MuzzlePosition`. I proiettili partono dal centro del player, non dalla canna dell'arma, finché il riferimento non viene trascinato nell'Inspector.

**`PlayerVisuals`** — campi serializzati:

| Campo | Valore |
|---|---|
| walkDown | 3 sprite da `neo_zero_char_01.png` |
| walkUp | 3 sprite da `neo_zero_char_01.png` |
| walkSide | 3 sprite da `neo_zero_char_01.png` |
| frameRate | 8 |
| idleFrame | 1 |
| weaponPivot | assegnato → `WeaponPivot` (fileID 67214885) |
| weaponRenderer | assegnato → SpriteRenderer di `WeaponPivot` (fileID 67214884) |
| weaponSprites | 4 sprite da `neo_zero_armi_20x16.png` (0 Single, 1 Spread, 2 Piercing, 3 Melee) |
| weaponOrbitRadius | 0.28 |
| weaponSortingOffset | 1 |
| **muzzleForward** | **non serializzato** → default `{0.5625, 0.5, 0.6875, 0.75}` |
| **muzzleUp** | **non serializzato** → default `0.19` |
| **weaponHideDelay** | **non serializzato** → default `0.15` |

### Altri componenti in scena

- **`CameraShake`** (su `Main Camera`): nessun campo `[SerializeField]`.
- **`AudioManager`**: `sfxSource` → `SFX_source`, `musicSource` → `Music_source`, tutti e 8 i clip assegnati (`shootSingleClip`, `shootSpreadClip`, `shootPierceClip`, `meleeClip`, `playerHurtClip`, `enemyDeathClip`, `explosionClip`, `pickupClip`), `pitchVariation: 0.2`.
- **`GameManager`**: `gameOverPanel` → `GameOverPanel`, `victoryPanel` → `VictoryPanel`. Entrambi assegnati.

### Canvas

```
Canvas                          [Canvas + CanvasScaler + GraphicRaycaster]
├── HealthText                  [TextMeshProUGUI]
├── BombText                    [TextMeshProUGUI]
├── WeaponText                  [TextMeshProUGUI]
├── GameOverPanel               [Image]
│   ├── GameOverText            [TextMeshProUGUI]
│   ├── RestartButton           [Image + Button]
│   │   └── Text (TMP)          [TextMeshProUGUI]
│   └── MenuButton              [Image + Button]
│       └── Text (TMP)          [TextMeshProUGUI]
├── VictoryPanel                [TextMeshProUGUI]
│   ├── MainMenuButton          [Image + Button]
│   │   └── Text (TMP)          [TextMeshProUGUI]
│   └── PlayAgainButton (1)     [Image + Button]
│       └── Text (TMP)          [TextMeshProUGUI]
└── UpgradePopUpText            [TextMeshProUGUI]
```

`VictoryPanel` non ha un componente `Image`: il GameObject porta direttamente un `TextMeshProUGUI`, a differenza di `GameOverPanel` che è un `Image`.

Le barre della vita di Boss e MiniBoss **non** stanno su questo Canvas: ogni prefab ha un proprio Canvas annidato (`HealthBar`), quindi `healthBarFill` / `healthBarRoot` puntano a oggetti interni al prefab.

---

## 5. Riferimenti mancanti e campi non serializzati

### Campi `[SerializeField]` a `{fileID: 0}` (None)

| File | GameObject | Script | Campo |
|---|---|---|---|
| `Assets/Prefabs/Splitterling 1.prefab` | Splitterling 1 | SplitterController | splitPrefab (intenzionale: non si divide oltre) |
| `Assets/Prefabs/Boss.prefab` | Fill / Background | Image (componente Unity UI) | m_Material, m_Sprite (su Background) |
| `Assets/Prefabs/MiniBoss.prefab` | Fill / Background | Image (componente Unity UI) | m_Material, m_Sprite (su Background) |

Nella scena non risulta nessun campo `{fileID: 0}` tra i riferimenti degli script di progetto.

### Campi aggiunti agli script dopo l'ultimo salvataggio (assenti dallo YAML)

Prendono il valore di default dello script finché scena/prefab non vengono risalvati da Unity.

| Dove | Campo | Default | Effetto |
|---|---|---|---|
| `Player` (scena) | `PlayerController.playerVisuals` | None | **i proiettili partono dal centro del player, non dalla canna** — va assegnato a mano |
| `Player` (scena) | `PlayerVisuals.muzzleForward` | `{0.5625, 0.5, 0.6875, 0.75}` | nessuno finché `playerVisuals` è None |
| `Player` (scena) | `PlayerVisuals.muzzleUp` | `0.19` | idem |
| `Player` (scena) | `PlayerVisuals.weaponHideDelay` | `0.15` | l'arma sparisce 0.15 s dopo l'ultimo colpo |
| `RoomManager` (scena) | `doorIgnoreDelay` | `0.25` | finestra in cui i trigger delle porte vengono ignorati dopo un cambio stanza |
| `Boss.prefab` | `BossController.minionSpawnRadius` | `1.2` | stesso raggio di evocazione di prima (era fisso nel codice) |
| `Teleporter.prefab` | `roomHalfWidth`, `roomHalfHeight` | `3`, `3` | limiti entro cui il Teleporter si riposiziona |

### Asset con riferimenti rotti

- `Assets/Prefabs/old.prefab` — referenzia `NewMonoBehaviourScript`, script inesistente.
