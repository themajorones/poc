# Prototype Current State

## Purpose
File này là snapshot ngắn gọn của prototype ở trạng thái hiện tại, dùng để đọc nhanh flow chính mà không phải lần theo toàn bộ code.

## Scene Flow
- Startup editor mở `ParryBossRushPrototype`
- `ParryBossRushPrototype` có start panel với:
  - character selector
  - `Start Rush`
  - `Tutorial`
- `Tutorial` là scene riêng

## Player Roster
Hệ player hiện tại dùng:
- `PlayerCharacterDefinition`
- `PlayerCharacterRoster`
- `PlayerLoadoutDefinition`

Roster hiện có 3 nhân vật:

1. `Blue`
   - HP: `3`
   - Rage Max: `100`
   - Shot: `Straight`
   - Rage Skill: `Laser`
   - Parry Special: `StraightCounter`

2. `Red`
   - HP: `4`
   - Rage Max: `130`
   - Shot: `Spreadshot`
   - Rage Skill: `ExpandingGlobalRing`
   - Parry Special: `DefensiveRing`

3. `Yellow`
   - HP: `3`
   - Rage Max: `250`
   - Shot: `Chaser`
   - Rage Skill: `StickyProjectile`
   - Parry Special: `Molotov`

## Character Rules
- Boss rush cho phép chọn character từ roster
- character đang chọn được lưu bằng `PlayerPrefs`
- tutorial luôn khóa về `Blue`
- `Red` và `Yellow` không vào tutorial

## Combat Slots
Mỗi player loadout hiện có 4 slot:
- `Primary Shot`
- `Parry Visual Effect`
- `Rage Skill`
- `Parry Special`

Rule equip:
- slot trống thì không dùng được
- không còn fallback gameplay ngầm cho shot / skill / parry special

## Current Shot / Skill / Parry Special Kinds
### `PlayerShotKind`
- `Straight`
- `Spreadshot`
- `Chaser`

### `PlayerSkillKind`
- `Laser`
- `ExpandingGlobalRing`
- `StickyProjectile`

### `PlayerCounterShotKind`
- `StraightCounter`
- `DefensiveRing`
- `Molotov`

## Parry State
Current values:
- `postGrace = 0.25`
- `successInvuln = 0.5`
- `sameTargetRepeatCooldown = 0.85`

The `sameTargetRepeatCooldown` hiện đang áp dụng toàn cục cho cùng một target parry, không chỉ riêng `TrackingParryOrb`.

## Boss Moves Currently Notable
- `ShockwaveTriple`
- `ParryCharge`
- `TrackingParryOrb`

### `TrackingParryOrb`
- viên đạn tím lớn
- tracking player
- parry được
- parry không làm đạn biến mất
- đạn chỉ tắt khi hết lifetime hoặc ra ngoài bounds

## Audio / Settings / Localization
- `AudioManager` singleton
- `ProjectAudioCatalog` hỗ trợ nhiều clip cho mỗi cue
- settings hiện có:
  - master
  - music
  - sfx
  - language
  - main menu
  - close
- localization hiện dùng `TutorialLocalizationAsset`

## Shared Visual / Pooling
- `GameplayRing` pooled visual dùng chung cho:
  - `DefensiveRing`
  - `ExpandingGlobalRing`
  - `ShockwaveTriple`

## Important Tools Still In Use
- `Setup Player Characters`
- `Create Default Player Loadout`
- `Create Spread Ring Variant Loadout`
- `Create Chaser Molotov Loadout`
- `Setup Player Field Ring Templates`
- `Seed Boss Move Assets`
- `Create Audio Catalog`
- `Setup Settings In Open Scene`
- `Setup Project Localization Only`
- `Repair Boss Rush Start Overlay`
- `Repair Tutorial Scene`

## Documentation Rule Going Forward
Khi thêm feature lớn mới, cần cập nhật ít nhất:
- file detail đúng domain
- file snapshot này nếu feature làm đổi flow tổng thể của prototype
