# Progression Detail: Characters and Loadouts

## Current Structure
Prototype hiện tại chia player data thành 2 lớp:

- `PlayerCharacterDefinition`
- `PlayerLoadoutDefinition`

`PlayerCharacterDefinition` quản lý:
- `playerPrefab`
- `previewSprite`
- `maxHp`
- `rageMax`

`PlayerLoadoutDefinition` quản lý:
- `Primary Shot`
- `Parry Visual Effect`
- `Rage Skill`
- `Parry Special`

Rule runtime hiện tại:
- nếu `Primary Shot` trống thì player không bắn
- nếu `Rage Skill` trống thì player không dùng skill
- nếu `Parry Special` trống thì parry không sinh special gameplay
- `Parry Visual Effect` là visual-only

## Current Roster
Roster hiện có 3 nhân vật:

1. `Blue`
2. `Red`
3. `Yellow`

Asset roster:
- `Assets/PrototypeGenerated/Config/Characters/PlayerCharacterRoster.asset`

### Blue
Character asset:
- `Assets/PrototypeGenerated/Config/Characters/BlueCharacter.asset`

Stats:
- HP: `3`
- Rage Max: `100`

Loadout:
- `Assets/PrototypeGenerated/Data/Player/Blue_Loadout.asset`

Current role:
- nhân vật mặc định
- nhân vật tutorial
- bộ kỹ năng cơ bản dễ học cơ chế

Loadout hiện tại:
- `Primary Shot`: `DefaultPlayerShot`
  - kind: `Straight`
- `Parry Visual Effect`: `DefaultPlayerParryEffect`
- `Rage Skill`: `DefaultPlayerSkill`
  - kind: `Laser`
- `Parry Special`: `DefaultPlayerCounterShot`
  - kind: `StraightCounter`

### Red
Character asset:
- `Assets/PrototypeGenerated/Config/Characters/RedCharacter.asset`

Stats:
- HP: `4`
- Rage Max: `130`

Loadout:
- `Assets/PrototypeGenerated/Data/Player/Red_Loadout.asset`

Current role:
- biến thể cận chiến/rushdown
- mạnh khi đứng gần boss
- parry thiên về kiểm soát projectile

Loadout hiện tại:
- `Primary Shot`: `Spreadshot`
  - kind: `Spreadshot`
  - 4 pellet
  - cone spread
  - tầm ngắn
- `Parry Visual Effect`: `DefaultPlayerParryEffect`
- `Rage Skill`: `ExpandingGlobalRing`
  - kind: `ExpandingGlobalRing`
  - quét rộng
  - xóa projectile thường
  - kích hoạt parry thật với target parryable
- `Parry Special`: `DefensiveRingParrySpecial`
  - kind: `DefensiveRing`
  - sinh ring phòng thủ tại điểm parry
  - xóa projectile không parry được
  - không tự parry target parryable

### Yellow
Character asset:
- `Assets/PrototypeGenerated/Config/Characters/YellowCharacter.asset`

Stats:
- HP: `3`
- Rage Max: `250`

Loadout:
- `Assets/PrototypeGenerated/Data/Player/Yellow_Loadout.asset`

Current role:
- biến thể điều khiển không gian / damage over time
- rage cần nhiều hơn để dùng skill

Loadout hiện tại:
- `Primary Shot`: `ChaserShot`
  - kind: `Chaser`
  - đạn vàng tự đuổi target
  - mất target thì giữ hướng hiện tại và bay thẳng tiếp tới hết lifetime
- `Parry Visual Effect`: `DefaultPlayerParryEffect`
- `Rage Skill`: `StickyProjectileSkill`
  - kind: `StickyProjectile`
  - bắn ra projectile ghim vào boss
  - gây DOT lên HP và stagger theo thời gian
- `Parry Special`: `MolotovParrySpecial`
  - kind: `Molotov`
  - bay thẳng một quãng cố định rồi nổ
  - sinh `MolotovFireZone`
  - fire zone tồn tại 3 giây
  - gây damage theo tick và cho phép chồng nhiều vùng

## Supported Combat Definition Types
### `PlayerShotDefinition`
Current `PlayerShotKind`:
- `Straight`
- `Spreadshot`
- `Chaser`

### `PlayerSkillDefinition`
Current `PlayerSkillKind`:
- `Laser`
- `ExpandingGlobalRing`
- `StickyProjectile`

### `PlayerCounterShotDefinition`
Current `PlayerCounterShotKind`:
- `StraightCounter`
- `DefensiveRing`
- `Molotov`

## Character Selection Rules
Boss rush start panel cho phép chọn character từ roster:
- nút trái / phải
- preview sprite ở giữa
- lưu index bằng `PlayerPrefs`

Tutorial rule:
- tutorial luôn dùng `Blue`
- không cho `Red` hoặc `Yellow` vào tutorial
- prefab tutorial blue được khóa bằng `TutorialBluePlayerMarker`

## Current Authoring Assets
Character assets:
- `Assets/PrototypeGenerated/Config/Characters/BlueCharacter.asset`
- `Assets/PrototypeGenerated/Config/Characters/RedCharacter.asset`
- `Assets/PrototypeGenerated/Config/Characters/YellowCharacter.asset`

Loadout assets:
- `Assets/PrototypeGenerated/Data/Player/Blue_Loadout.asset`
- `Assets/PrototypeGenerated/Data/Player/Red_Loadout.asset`
- `Assets/PrototypeGenerated/Data/Player/Yellow_Loadout.asset`

## Current Tools
- `Tools/ParryShooter/Setup Player Characters`
- `Tools/ParryShooter/Create Default Player Loadout`
- `Tools/ParryShooter/Create Spread Ring Variant Loadout`
- `Tools/ParryShooter/Create Chaser Molotov Loadout`

## Design Direction
Hệ hiện tại đã đủ để mở rộng tiếp:
- thêm character mới vào roster
- thêm shot kind mới
- thêm rage skill kind mới
- thêm parry special kind mới
- thay loadout theo prefab mà không cần làm lại toàn bộ flow chọn nhân vật
