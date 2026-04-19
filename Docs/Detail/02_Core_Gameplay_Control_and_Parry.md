# Core Gameplay Detail: Control and Parry

## Current Prototype Goal
Prototype hiện tại tập trung vào một loop shmup một tay:
- kéo để di chuyển
- tự bắn khi có `Primary Shot`
- phản đòn bằng hất nhanh lên khi đang giữ điều khiển
- dùng skill khi Rage đầy

## Control Rules
### Movement
- Player được điều khiển bằng kéo/chạm.
- Ship có `pointer offset`, không nằm đúng dưới ngón tay.
- Feel điều khiển hiện tại được xem là mốc cần giữ ổn định.

### Auto Shot
- Player chỉ tự bắn nếu `Player Loadout > Primary Shot` được equip.
- Nếu slot này trống thì player không bắn.
- Tutorial có thể tắt auto-shot bằng flow riêng.

### Skill Input
- Desktop: `Space` / `E`
- Mobile: nút `Skill`
- Nếu `Player Loadout > Rage Skill` trống thì player không dùng được skill.

## Parry Rules
### Activation
- Không có nút parry riêng.
- Parry được kích hoạt bằng upward burst gesture khi đang giữ điều khiển.

### Timing
- `burstDuration = 0.12s`
- `postGrace = 0.25s`
- `parryWindow = 0.12s`
- `successInvuln = 0.5s`

### Same-Target Repeat Cooldown
- Prototype hiện có cooldown theo từng target:
- `sameTargetRepeatCooldown = 0.85s`
- Nghĩa là cùng một projectile/target vừa bị parry thì không thể bị parry lại ngay trong `0.85s`.
- Target khác vẫn parry bình thường.

## Hitbox Model
- `hurtbox < sprite body < parry zone`
- Hurtbox nhỏ hơn sprite để feel bớt gắt.
- Parry zone lớn hơn sprite một chút để commit parry rõ hơn.

## Current Parry Outputs
Khi parry thành công, hệ hiện tại chia thành 3 lớp:

### 1. Parry Visual Effect
- Chỉ là visual-only.
- Không chứa gameplay special.

### 2. Parry Special
- Là gameplay effect khi parry thành công.
- Lấy từ `Player Loadout > Parry Special`
- Ví dụ hiện có:
  - counter projectile bay thẳng lên
  - `DefensiveRing`
- Nếu slot này trống thì parry không sinh special gameplay.

### 3. Rage Gain
- Parry hợp lệ sẽ cộng Rage theo rule hiện tại của config / target.

## Current Player Combat Additions
### Spreadshot
- Là `Primary Shot` mới.
- Bắn ra 4 pellet đỏ theo hình nón phía trước.
- Tầm ngắn.
- Damage mỗi pellet thấp hơn đạn thường.
- Mạnh ở gần, yếu rõ khi ở xa.

### DefensiveRing
- Hiện là một `Parry Special`, không còn là `Parry Effect`.
- Spawn tại điểm parry thành công.
- Nở ra, giữ một lúc, rồi fade out.
- Xóa các projectile/attack không thể parry chạm vào nó.
- Nếu attack có thể parry thì nó không tự parry thay player.

### ExpandingGlobalRing
- Hiện là một `Rage Skill`.
- Cast từ player, không khóa di chuyển như laser.
- Tạo một ring lớn quét rộng.
- Projectile thường chạm vào thì biến mất.
- Các target có thể parry sẽ đi qua đúng luồng parry thật của player.
- Vì vậy nếu player đang equip `Parry Special`, skill-parry cũng gọi đúng output đó.

## Special Boss Interactions Already Supported
### Parry Charge
- Skill ring có thể parry `ParryCharge`
- Vẫn damage boss như logic parry hiện tại

### Shockwave Triple
- Từng ring được xử lý độc lập
- Có thể bị skill-parry theo từng ring riêng

### TrackingParryOrb
- Là projectile tím tracking mới của boss
- Parry được nhưng không despawn ngay
- Chỉ biến mất khi hết lifetime hoặc ra ngoài bounds
- Sau khi parry vẫn tiếp tục homing
- Không thể bị spam parry trên cùng target nhờ `sameTargetRepeatCooldown`

## Current Feel Constraints
Những phần hiện được xem là locked feel trong prototype:
- drag / touch feel
- pointer offset
- upward parry gesture
- parry burst timing
- post-grace timing
- one-hand control philosophy

Nếu sửa tiếp, ưu tiên giữ feel và chỉ thay logic hệ thống bao quanh.
