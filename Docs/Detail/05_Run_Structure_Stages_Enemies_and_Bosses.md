# Run Structure Detail: Scenes, Tutorial and Boss Authoring

## Current Scene Structure
Prototype hiện tại đã tách scene rõ hơn:
- `ParryBossRushPrototype`
- `Tutorial`
- `MainMenu` vẫn có thể tồn tại trong build settings/tooling, nhưng flow thực tế hiện xoay quanh `ParryBossRushPrototype` start panel

## Boss Rush Start Panel
### Current Flow
Khi vào `ParryBossRushPrototype` ở trạng thái start:
- hiện panel đầu
- có selector character
- có `Start Rush`
- có `Tutorial`

### Restart Rule
- `Restart` từ boss rush sẽ quay về panel đầu, không auto vào combat

## Tutorial
### Current Purpose
Tutorial là scene riêng để dạy mechanic, không tự nhảy sang boss rush sau khi hoàn tất.

### Current Flow
- intro card
- lesson card
- các lesson mechanic
- fail 1 hit thì reset đúng lesson hiện tại
- end state có:
  - quay về boss rush
  - chơi lại tutorial

### Forced Character Rule
- Tutorial luôn dùng prefab xanh
- không cho character đỏ vào tutorial

## Boss Pattern System
### Core Authoring
Boss move hiện dùng:
- `BossMoveDefinition`
- `BossPatternDefinitions`
- `BossRushScenario`

### Existing Important Boss Moves
- `ShockwaveTriple`
- `ParryCharge`
- `TrackingParryOrb`

## Tracking Parry Orb
### Current Behavior
- boss wind-up rồi bắn 1 projectile tím lớn
- projectile homing theo player
- dùng cùng hệ visual projectile boss hiện có
- lớn hơn bằng scale/radius

### Parry Rule
- parry được
- khi parry:
  - không despawn ngay
  - vẫn trigger full parry logic của player
  - counter projectile / parry special vẫn hoạt động
- projectile chỉ biến mất khi:
  - hết lifetime
  - ra ngoài bounds

### Anti-Exploit
- parry cùng target có cooldown riêng theo config
- hiện default là `0.85s`

## Shared Ring Visual
### Current State
Player rings và một phần boss ring visual đang dùng chung pooled visual `GameplayRing`.

Used by:
- `DefensiveRing`
- `ExpandingGlobalRing`
- `BossShockwaveRing` visual path

### Notes
- `GameplayRing` là pooled template / shared visual
- logic gameplay của player ring và boss ring vẫn riêng

## Tools Still Relevant for Boss/Tutorial Authoring
- `Tools/ParryShooter/Rebuild Prototype Scene`
- `Tools/ParryShooter/Repair Boss Rush Start Overlay`
- `Tools/ParryShooter/Repair Tutorial Scene`
- `Tools/ParryShooter/Setup Player Field Ring Templates`
- `Tools/ParryShooter/Seed Boss Move Assets`

## Editor Startup Behavior
### Current State
- tutorial flow không còn auto-bootstrap lúc mở project
- project startup hiện mở `ParryBossRushPrototype`

### Goal
- tránh lỗi editor tự rebuild scene
- luôn vào đúng scene để tiếp tục test prototype nhanh

## Current Boss Authoring Convenience
### Move Preview
Mỗi `BossMoveDefinition` hiện có `previewSprite` để:
- kéo ảnh preview vào move
- nhận diện move nhanh hơn khi setup boss
- không ảnh hưởng gameplay

## Prototype Content Reality
Prototype hiện không phải full run structure kiểu stage/wave hoàn chỉnh.
Nó là một combat sandbox / boss-rush-heavy prototype với các lớp đã có thật:
- start panel
- character selection
- tutorial scene riêng
- settings + localization + audio
- data-driven player loadout
- data-driven boss move authoring
