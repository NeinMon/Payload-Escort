# ✅ CHECKLIST HOÀN THIỆN GAME FPS DEATHMATCH

## 📦 CÁC SCRIPT ĐÃ TẠO

### ✅ Core Systems
- [x] `WeaponBase.cs` - Base class cho vũ khí
- [x] `RaycastGun.cs` - Súng bắn raycast  
- [x] `WeaponManager.cs` - Quản lý chuyển súng
- [x] `GameStats.cs` - Tracking kills/deaths
- [x] `MultiplayerFPSGameManager.cs` - Quản lý trận đấu (updated)
- [x] `PlayerHealth.cs` - Hệ thống máu (updated)
- [x] `SpawnProtection.cs` - Bảo vệ khi spawn

### ✅ UI Systems  
- [x] `Scoreboard.cs` - Bảng xếp hạng (Tab)
- [x] `PlayerHUD.cs` - HUD (health, ammo, crosshair)
- [x] `KillFeedManager.cs` - Thông báo kills

### ✅ Documentation
- [x] `SETUP_GUIDE.md` - Hướng dẫn setup chi tiết
- [x] `QUICK_CHECKLIST.md` - Checklist này

---

## 🔧 SETUP TRONG UNITY (Làm theo thứ tự)

### 1. Photon Setup
- [ ] Tạo tài khoản Photon
- [ ] Lấy App ID
- [ ] Paste vào PUN Wizard
- [ ] Setup Project

### 2. Player Prefab
- [ ] Tạo Player GameObject
- [ ] Add CharacterController
- [ ] Add PhotonView + PhotonTransformView
- [ ] Add FPSController script
- [ ] Add PlayerHealth script
- [ ] Add SpawnProtection script
- [ ] Add Audio Source
- [ ] Tạo PlayerCamera (tag: MainCamera)
- [ ] Tạo WeaponHolder
  - [ ] Add Gun object
  - [ ] Add RaycastGun script
  - [ ] Add FirePoint
  - [ ] Add Line Renderer (bullet trail)
- [ ] Add WeaponManager script
- [ ] Kéo Player vào **Resources/** folder
- [ ] Xóa Player khỏi Hierarchy

### 3. Scene Setup
- [ ] Tạo GameScene
- [ ] Tạo Plane (sàn)
- [ ] Tạo Walls
- [ ] Tạo 4-8 SpawnPoints
- [ ] Tạo GameManager GameObject
  - [ ] Add MultiplayerFPSGameManager
  - [ ] Add GameStats
  - [ ] Add PhotonLobbyManager (optional)
- [ ] Gắn references vào GameManager:
  - [ ] Player Prefab
  - [ ] Spawn Points array

### 4. UI Setup
- [ ] Tạo Canvas (Scale with Screen Size)
- [ ] **HUD:**
  - [ ] Health Bar (Background + Fill)
  - [ ] Ammo Text
  - [ ] Crosshair Image
  - [ ] Timer Text (top center)
  - [ ] Kill Feed Text (top left)
- [ ] **Scoreboard:**
  - [ ] ScoreboardPanel (disabled)
  - [ ] Header text
  - [ ] Scroll View
  - [ ] PlayerListContainer
  - [ ] PlayerRow Prefab (Rank, Name, Kills, Deaths, K/D)
- [ ] **Match UI:**
  - [ ] Match Start Panel (disabled)
  - [ ] Match End Panel (disabled)
  - [ ] Winner Text
- [ ] **Kill Feed:**
  - [ ] Kill Feed Container (Vertical Layout)
  - [ ] Kill Feed Item Prefab
- [ ] Add scripts to Canvas:
  - [ ] PlayerHUD
  - [ ] Scoreboard
  - [ ] KillFeedManager
- [ ] Gắn tất cả UI references

### 5. Script Connections
- [ ] FPSController:
  - [ ] playerCamera → PlayerCamera
  - [ ] currentWeapon → WeaponManager
- [ ] WeaponManager:
  - [ ] weapons[] → Danh sách súng
- [ ] RaycastGun:
  - [ ] firePoint → FirePoint
  - [ ] bulletTrail → Line Renderer
  - [ ] hitLayers → Layer Mask (bao gồm Player)
- [ ] PlayerHealth:
  - [ ] spawnPoints[] → Spawn Points
- [ ] MultiplayerFPSGameManager:
  - [ ] playerPrefab → Resources/Player
  - [ ] spawnPoints[] → Spawn Points
  - [ ] timerText → Timer Text
  - [ ] winnerText → Winner Text
- [ ] PlayerHUD:
  - [ ] healthBar → Health Bar Fill
  - [ ] healthText → Health Text
  - [ ] ammoText → Ammo Text
  - [ ] crosshairImage → Crosshair
  - [ ] killFeedText → Kill Feed Text
  - [ ] playerHealth → Auto (Runtime)
  - [ ] currentWeapon → Auto (Runtime)
- [ ] Scoreboard:
  - [ ] scoreboardPanel → Scoreboard Panel
  - [ ] playerListContainer → Player List Container
  - [ ] playerRowPrefab → Player Row Prefab
- [ ] KillFeedManager:
  - [ ] killFeedContainer → Kill Feed Container
  - [ ] killFeedItemPrefab → Kill Feed Item Prefab

### 6. Build Settings
- [ ] Add GameScene to Build Settings
- [ ] Check target platform (Windows/Mac/Linux)

---

## 🧪 TESTING

### Pre-Test Checklist
- [ ] Player prefab trong Resources/
- [ ] Photon App ID đã setup
- [ ] Tất cả UI references đã gắn
- [ ] SpawnPoints đã tạo và gắn
- [ ] Layer setup đúng

### Test Items
- [ ] Kết nối Photon thành công
- [ ] Player spawn đúng vị trí
- [ ] Di chuyển mượt mà
- [ ] Camera nhìn đúng (mouse look)
- [ ] Bắn súng hoạt động
- [ ] Raycast hit player khác
- [ ] Damage và health bar giảm
- [ ] Death và respawn
- [ ] Kill counting đúng
- [ ] Scoreboard hiện (Tab)
- [ ] Scoreboard data đúng
- [ ] Timer đếm ngược
- [ ] Match end khi hết giờ
- [ ] Winner announcement
- [ ] Kill feed hiển thị
- [ ] Ammo display
- [ ] Reload hoạt động
- [ ] Spawn protection (3s)

### Multi-Player Test
- [ ] Build game ra .exe
- [ ] Chạy Editor + Build cùng lúc
- [ ] 2 players thấy nhau
- [ ] Bắn nhau hoạt động
- [ ] Network sync tốt
- [ ] Kills/deaths sync
- [ ] Scoreboard sync

---

## 🎯 OPTIONAL IMPROVEMENTS (Sau khi core hoàn thành)

### Ưu tiên 1 (Dễ làm)
- [ ] **Sound Effects**
  - [ ] Bắn súng
  - [ ] Hit marker
  - [ ] Death sound
  - [ ] Reload sound
- [ ] **Visual Effects**
  - [ ] Muzzle flash (Particle)
  - [ ] Hit spark (Particle)
  - [ ] Blood effect
- [ ] **More Weapons**
  - [ ] Pistol (fast, low damage)
  - [ ] Rifle (balanced)
  - [ ] Shotgun (spread, high damage)
- [ ] **Polish**
  - [ ] Better crosshair
  - [ ] Health bar animation
  - [ ] Smooth camera shake khi bắn

### Ưu tiên 2 (Trung bình)
- [ ] **Game Modes**
  - [ ] Team Deathmatch
  - [ ] Free For All (đã có)
  - [ ] Capture the Flag
- [ ] **Power-ups**
  - [ ] Health pack
  - [ ] Ammo box
  - [ ] Shield
  - [ ] Speed boost
- [ ] **Minimap**
  - [ ] Top-down camera
  - [ ] Player icons
  - [ ] Rotate with player

### Ưu tiên 3 (Nâng cao)
- [ ] Grenades
- [ ] Melee attack
- [ ] Player customization (skins)
- [ ] Rank system
- [ ] Achievements
- [ ] Stats screen
- [ ] Settings menu

---

## 🐛 COMMON ISSUES & FIXES

### Player không spawn
✅ Check: Player prefab phải ở trong `Assets/Resources/`
✅ Check: PhotonNetwork.Instantiate dùng tên chính xác

### Không bắn được
✅ Check: Layer Mask trong RaycastGun
✅ Check: FirePoint position và rotation
✅ Check: Input System hoặc Input Manager

### UI không hiện
✅ Check: Canvas có Camera reference (nếu dùng World Space)
✅ Check: Tất cả references đã gắn
✅ Check: UI không bị hidden

### Photon lỗi kết nối
✅ Check: App ID đúng
✅ Check: Internet connection
✅ Check: Firewall không block
✅ Check: Photon Server Settings

### Kills không sync
✅ Check: GameStats.Instance đã tồn tại
✅ Check: Player CustomProperties setup
✅ Check: RPC calls đúng target

---

## 📚 HỌC THÊM

### Unity Photon PUN2
- Docs: https://doc.photonengine.com/pun/v2/
- Tutorials: YouTube "Photon PUN 2 Tutorial"

### FPS Controller
- Brackeys FPS Tutorial
- Unity Input System docs

### Networking Best Practices
- Limit RPC calls
- Use Custom Properties cho persistent data
- Sync only what's necessary

---

## ✅ KẾT LUẬN

Sau khi hoàn thành checklist này, bạn sẽ có:
- ✅ Multiplayer FPS hoàn chỉnh
- ✅ Kill/Death tracking
- ✅ Scoreboard realtime
- ✅ Match timer & winner
- ✅ Full UI (HUD, Scoreboard)
- ✅ Weapon system
- ✅ Spawn protection

**Chúc bạn thành công! 🎮🔥**

Nếu cần hỗ trợ thêm, hãy tham khảo `SETUP_GUIDE.md` chi tiết hơn!
