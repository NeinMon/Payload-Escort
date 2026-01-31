# ✅ CHECKLIST SETUP UI - Đánh dấu khi hoàn thành

## 🎯 PLAYER PREFAB UI SETUP

### ✅ Bước 1: LocalHUD Canvas (Screen Space)

- [ ] **Tạo Canvas "LocalHUD"**
  - [ ] Render Mode = Screen Space - Overlay
  - [ ] Canvas Scaler = Scale With Screen Size (1920x1080)
  - [ ] Position: (0, 0, 0)

- [ ] **Health Bar**
  - [ ] HealthBarBackground (Image, Bottom-Left, 250x30)
    - [ ] Color: Black (0,0,0,150)
  - [ ] HealthBarFill (Image Filled, Horizontal, Green)
    - [ ] Nested trong Background
    - [ ] Padding 3px
  - [ ] HealthText (TextMeshPro, Center, "100 / 100")
    - [ ] Font Size: 18, Bold
    - [ ] Outline: Black 0.2

- [ ] **Ammo Text**
  - [ ] AmmoText (TextMeshPro, Bottom-Right, "30 / 30")
    - [ ] Font Size: 42, Bold
    - [ ] Alignment: Right Middle
    - [ ] Outline: Black 0.3

- [ ] **Crosshair**
  - [ ] Crosshair (Image, Center, 32x32)
    - [ ] Color: White (255,255,255,200)
    - [ ] Position: (0, 0, 0)

- [ ] **LocalPlayerHUD Script**
  - [ ] Add script vào LocalHUD Canvas
  - [ ] healthBar → HealthBarFill
  - [ ] healthText → HealthText
  - [ ] ammoText → AmmoText
  - [ ] crosshairImage → Crosshair
  - [ ] normalColor = White
  - [ ] hitColor = Red

---

### ✅ Bước 2: NetworkHealthBar Canvas (World Space)

- [ ] **Tạo Canvas "NetworkHealthBar"**
  - [ ] Render Mode = World Space
  - [ ] Position: (0, 2.2, 0) - Trên đầu player
  - [ ] Scale: (0.01, 0.01, 0.01)
  - [ ] Width: 200, Height: 30

- [ ] **Health Bar**
  - [ ] HealthBarBG (Image, Stretch)
    - [ ] Color: Black (0,0,0,180)
  - [ ] HealthBarFill (Image Filled)
    - [ ] Nested trong BG
    - [ ] Color: Green, Horizontal Fill
    - [ ] Padding 2px

- [ ] **NetworkPlayerHealthBar Script**
  - [ ] Add script vào NetworkHealthBar Canvas
  - [ ] canvas → Canvas component
  - [ ] healthBarFill → HealthBarFill
  - [ ] offset = (0, 2.2, 0)
  - [ ] maxDistance = 30

---

### ✅ Bước 3: NetworkNameTag Canvas (World Space)

- [ ] **Tạo Canvas "NetworkNameTag"**
  - [ ] Render Mode = World Space
  - [ ] Position: (0, 2.5, 0) - Trên health bar
  - [ ] Scale: (0.01, 0.01, 0.01)
  - [ ] Width: 300, Height: 50

- [ ] **Name Text**
  - [ ] NameText (TextMeshPro, Center Middle)
    - [ ] Font Size: 36, Bold
    - [ ] Color: White
    - [ ] Outline: Black 0.4

- [ ] **NetworkPlayerNameTag Script**
  - [ ] Add script vào NetworkNameTag Canvas
  - [ ] canvas → Canvas component
  - [ ] nameText → NameText
  - [ ] offset = (0, 2.5, 0)
  - [ ] maxDistance = 50

---

### ✅ Bước 4: Save & Test

- [ ] **Lưu Prefab**
  - [ ] Overrides > Apply All
  - [ ] Ctrl+S
  - [ ] Exit Prefab Mode

- [ ] **Kiểm tra Prefab**
  - [ ] Resources/Player có LocalHUD
  - [ ] Resources/Player có NetworkHealthBar
  - [ ] Resources/Player có NetworkNameTag
  - [ ] Tất cả references đã gắn (không có Missing)

---

## 🧪 TEST CHECKLIST

### Test 1: Local Player UI

- [ ] **Run game trong Editor**
  - [ ] Player spawn thành công
  - [ ] LocalHUD hiển thị:
    - [ ] Health bar góc dưới trái
    - [ ] Ammo text góc dưới phải
    - [ ] Crosshair giữa màn hình
  - [ ] Health bar màu xanh lá (100% HP)
  - [ ] Ammo hiển thị "30 / 30" (hoặc số khác)

### Test 2: Network UI (2 Players)

- [ ] **Build game + Run Editor**
  - [ ] Cả 2 players spawn
  - [ ] Player 1 (Editor):
    - [ ] Thấy LocalHUD của mình
    - [ ] KHÔNG thấy health bar trên đầu mình
    - [ ] Thấy health bar + name của Player 2
  - [ ] Player 2 (Build):
    - [ ] Thấy LocalHUD của mình
    - [ ] KHÔNG thấy health bar trên đầu mình
    - [ ] Thấy health bar + name của Player 1

### Test 3: Damage & Sync

- [ ] **Player 1 bắn Player 2**
  - [ ] Player 2 health bar giảm (cả local và network)
  - [ ] Player 1 thấy hitmarker (đỏ 0.1s)
  - [ ] Console log: "Damaged PlayerName for 25 damage"

- [ ] **Player 2 bắn Player 1**
  - [ ] Player 1 health bar giảm
  - [ ] Player 2 thấy hitmarker
  - [ ] Sync 2 chiều hoạt động ✅

### Test 4: Death & Respawn

- [ ] **Player 1 kill Player 2**
  - [ ] Player 2 die
  - [ ] Chờ 5s
  - [ ] Player 2 respawn tại spawn point
  - [ ] Health bar reset về 100%
  - [ ] Ammo reset về max
  - [ ] Object cũ bị xóa hoàn toàn ✅

- [ ] **Player 2 kill Player 1**
  - [ ] Player 1 die và respawn
  - [ ] Không còn ghost objects ✅

### Test 5: UI Distance & Facing

- [ ] **Di chuyển players**
  - [ ] Health bar + name luôn face camera
  - [ ] Ẩn khi player quá xa (>30m cho health, >50m cho name)
  - [ ] Hiện lại khi gần

### Test 6: Scoreboard & Stats

- [ ] **Nhấn Tab**
  - [ ] Scoreboard hiển thị
  - [ ] Kills/Deaths đúng
  - [ ] K/D ratio tính đúng
  - [ ] Local player highlight màu vàng

---

## 🐛 TROUBLESHOOTING

### ❌ UI không hiển thị

**Check:**
- [ ] Canvas enabled
- [ ] LocalPlayerHUD script enabled
- [ ] photonView.IsMine = true (cho local player)
- [ ] References không missing

### ❌ Health bar không sync

**Check:**
- [ ] PlayerHealth có PhotonView
- [ ] RPC TakeDamageFromPlayer được gọi
- [ ] NetworkPlayerHealthBar script hoạt động

### ❌ Respawn không đúng

**Check:**
- [ ] PlayerHealth.cs đã update (code mới)
- [ ] photonView.IsMine check đúng
- [ ] MultiplayerFPSGameManager.GetRandomSpawnPoint() tồn tại

### ❌ Name tag không hiện

**Check:**
- [ ] Canvas World Space
- [ ] Position (0, 2.5, 0) đúng
- [ ] Scale 0.01
- [ ] Camera có tag MainCamera

### ❌ UI quá to/nhỏ

**Fix:**
- [ ] LocalHUD: Canvas Scaler Reference Resolution
- [ ] NetworkUI: Scale = 0.01

---

## 📊 EXPECTED RESULTS

### Khi hoàn thành, bạn sẽ có:

✅ **LocalHUD (cho mỗi player):**
- Health bar với color gradient (xanh → vàng → đỏ)
- Ammo counter realtime
- Crosshair với hitmarker effect

✅ **Network UI (3D trên đầu):**
- Health bar visible cho người khác
- Player name tags
- Auto face camera
- Distance culling

✅ **Gameplay:**
- Damage sync 2 chiều hoàn hảo
- Respawn đúng cho tất cả players
- Kill/Death tracking chính xác
- Không còn ghost objects

---

## 🎉 HOÀN TẤT!

Khi tất cả checkbox đã tick ✅, game của bạn đã sẵn sàng!

**Next steps:**
- Polish UI design
- Add sound effects
- Add visual effects (muzzle flash, hit sparks)
- Add more weapons
- Test với 4-8 players

**Chúc mừng! 🎮🔥**
