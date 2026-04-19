# Combat Resources Detail: Rage, Skill and Combat UI

## Current Prototype Scope
Prototype hiện tại chưa có hệ bubble buff hoàn chỉnh như design dài hạn. Hệ resource đang chạy thật trong code là:
- HP
- Rage
- Skill
- Boss break / weak zone charge

## HP
### Current System
- HP hiện tại được lấy theo character đang chọn.
- Không còn cố định cứng 3 máu cho mọi player.
- `PlayerCharacterDefinition` giữ:
  - `maxHp`
  - `rageMax`

### HUD
- HUD hiện tại tự sinh pip HP theo số máu của character.
- Các pip được gom dưới `HpPipGroup` và xếp bằng `HorizontalLayoutGroup`.
- Mục tiêu là tránh kiểu `hp_0`, `hp_1`, `hp_2` bị lệch thủ công trong scene.

## Rage
### Current System
- Rage được lưu trong `RageSystem`.
- Mức max Rage hiện theo character, không còn khóa cứng toàn cục.
- `RageSystem.Max => GameController.CurrentRageMax`

### Current Rage Sources
- parry thành công
- counter projectile hit
- weak zone của boss break
- laser skill gây damage theo thời gian
- tutorial flow có thể override/mô phỏng Rage riêng cho lesson

### Current Rage Spend
- Dùng đầy thanh Rage để kích hoạt `Rage Skill`.
- Nếu `Player Loadout > Rage Skill` trống thì dù Rage đầy vẫn không dùng được skill.

## Skill
### Current Supported Skills
- `Laser`
- `ExpandingGlobalRing`

### Equip Rule
- Skill chỉ hoạt động nếu slot `Player Loadout > Rage Skill` có asset.
- Không còn fallback kiểu không equip mà vẫn dùng laser mặc định.

### Laser
- Cast tại chỗ
- Có thể khóa movement tùy config skill
- Dùng lane hit allowance để gây damage boss

### Expanding Global Ring
- Không khóa movement trong sample loadout hiện tại
- Spawn ring lớn, quét projectile thường và parry những target parry được
- Dùng chung parry output của player

## Character-Specific Stats
Prototype hiện đã có bảng character riêng:
- `PlayerCharacterDefinition`
- `PlayerCharacterRoster`

Mỗi character hiện quản lý:
- preview sprite
- player prefab
- max HP
- max Rage

Loadout không nằm trong character SO. Loadout vẫn nằm trên prefab/player authoring như hệ riêng.

## Settings and Persistence
### Stored Settings
- `MasterVolume`
- `MusicVolume`
- `SfxVolume`
- `Locale`
- selected character index

### Defaults
- volume mặc định lần đầu: `0.5 / 0.5 / 0.5`
- locale mặc định lấy từ player prefs hoặc locale đầu roster localization

## Settings Panel
### Current Contents
- Master
- Music
- SFX
- Language
- Main Menu
- Close

### Main Menu Button
- Từ gameplay hoặc tutorial, nút này sẽ load lại `ParryBossRushPrototype` với `autoStart = false`
- Mục tiêu là quay về đúng panel đầu:
  - chọn character
  - Start Rush
  - Tutorial

## Audio
### Current System
- `AudioManager` singleton dùng chung giữa scene
- `ProjectAudioCatalog` chứa cue audio
- mỗi cue có thể có nhiều clip

### Music Rule
- cùng cue thì giữ bài đang phát
- bài đang phát hết thì loop lại chính bài đó
- đổi cue mới random bài mới trong pool

### SFX Rule
- mỗi event random 1 clip trong cue pool
- xác suất đều nhau
- có thể trùng lại clip vừa phát

### Shared Volume Rule
- music và sfx đều đi qua cùng một hệ settings chung
- không nên có chuyện scene này to hơn scene kia chỉ vì scene

## Long-Term Notes
Bubble buff vẫn có thể quay lại về sau như một layer design riêng, nhưng hiện chưa phải hệ gameplay chính của prototype đang chạy.
