# 🎮 HƯỚNG DẪN SETUP GAME FPS DEATHMATCH

## 📋 DANH SÁCH CÁC FILE ĐÃ TẠO

### ✅ Scripts đã hoàn thành:
1. **Weapons/**
   - `WeaponBase.cs` - Base class cho tất cả vũ khí
   - `RaycastGun.cs` - Súng bắn raycast

2. **Managers/**
   - `GameStats.cs` - Quản lý kills/deaths/assists
   - `MultiplayerFPSGameManager.cs` - Quản lý trận đấu (đã cải thiện)

3. **UI/**
   - `Scoreboard.cs` - Bảng xếp hạng (Tab)
   - `PlayerHUD.cs` - HUD hiển thị health, ammo, crosshair

4. **Player/**
   - `PlayerHealth.cs` - Đã cập nhật với kill tracking

---

## 🔧 BƯỚC 1: SETUP PHOTON

### 1.1 Tạo tài khoản Photon
1. Vào https://www.photonengine.com/
2. Đăng ký tài khoản miễn phí
3. Tạo app mới (PUN2)
4. Copy **App ID**

### 1.2 Cấu hình trong Unity
1. Mở Unity
2. Vào `Window > Photon Unity Networking > PUN Wizard`
3. Dán App ID vào
4. Click "Setup Project"

---

## 🎯 BƯỚC 2: TẠO PLAYER PREFAB

### 2.1 Tạo Player GameObject
```
Hierarchy > Right Click > Create Empty
Tên: "Player"
```

### 2.2 Thêm Components vào Player:
1. **Character Controller**
2. **Photon View** (PhotonView)
   - Observed Components: Transform
   - Synchronize: Transform
3. **Photon Transform View**
4. **FPSController** script
5. **PlayerHealth** script
6. **Audio Source**

### 2.3 Tạo Camera cho Player
```
Hierarchy > Right Click on Player > Camera
Tên: "PlayerCamera"
```
- Tag: "MainCamera"
- Gắn vào field `playerCamera` trong FPSController

### 2.4 Tạo Weapon
```
Hierarchy > Right Click on Player > Create Empty
Tên: "WeaponHolder"
```

Trong WeaponHolder:
```
Create > 3D Object > Cube (tạm thời)
Tên: "Gun"
```
- Add script **RaycastGun**
- Tạo Empty Object tên "FirePoint" (vị trí bắn ra)
- Gắn FirePoint vào field `firePoint` trong RaycastGun
- Add **Line Renderer** cho bulletTrail

### 2.5 Lưu Prefab
```
Kéo Player từ Hierarchy vào folder Assets/Resources/
XÓA Player khỏi Scene (vì sẽ spawn qua Photon)
```

---

## 🗺️ BƯỚC 3: SETUP SCENE

### 3.1 Tạo Game Scene
```
File > New Scene
Tên: "GameScene"
Lưu vào Assets/Scenes/
```

### 3.2 Tạo Map cơ bản
```
Hierarchy > 3D Object > Plane (làm sàn)
Scale: (10, 1, 10)

Tạo thêm Walls (Cubes) để làm tường
```

### 3.3 Tạo Spawn Points
```
Hierarchy > Create Empty
Tên: "SpawnPoint1"
Position: Đặt ở các vị trí khác nhau trên map
```

Tạo 4-8 spawn points và đặt ở các vị trí khác nhau.

### 3.4 Tạo GameManager GameObject
```
Hierarchy > Create Empty
Tên: "GameManager"
```

Add components:
- **MultiplayerFPSGameManager** script
- **GameStats** script
- **PhotonLobbyManager** script (nếu cần auto-connect)

Gắn các references:
- Player Prefab: Kéo từ Resources/Player
- Spawn Points: Kéo tất cả SpawnPoint vào array
- Timer Text, Winner Text (tạo ở bước sau)

---

## 🎨 BƯỚC 4: TẠO UI

### 4.1 Tạo Canvas
```
Hierarchy > UI > Canvas
Canvas Scaler: Scale with Screen Size (1920x1080)
```

### 4.2 Tạo HUD
Trong Canvas tạo:

**1. Health Bar:**
```
UI > Image (tên: HealthBarBackground)
  └── Image (tên: HealthBarFill) - Image Type: Filled
      - Fill Method: Horizontal
      - Color: Green
```

**2. Ammo Text:**
```
UI > TextMeshPro (tên: AmmoText)
Text: "30 / 30"
Font Size: 36
Position: Bottom Right
```

**3. Crosshair:**
```
UI > Image (tên: Crosshair)
Sprite: Tạo crosshair sprite đơn giản
Position: Center (0, 0, 0)
Size: 32x32
```

**4. Timer:**
```
UI > TextMeshPro (tên: TimerText)
Text: "05:00"
Position: Top Center
```

**5. Kill Feed:**
```
UI > TextMeshPro (tên: KillFeedText)
Position: Top Left
Font Size: 24
```

### 4.3 Tạo Scoreboard UI
```
Trong Canvas > UI > Panel (tên: ScoreboardPanel)
- Disable by default
```

Trong ScoreboardPanel:
```
1. UI > TextMeshPro: "SCOREBOARD" (header)
2. UI > Scroll View (tên: PlayerListScroll)
   └── Content (tên: PlayerListContainer)
```

**Tạo PlayerRow Prefab:**
```
Trong Content > UI > Panel (tên: PlayerRow)

Thêm Horizontal Layout Group

Add 5 TextMeshPro:
- RankText (Width: 50)
- NameText (Width: 200)
- KillsText (Width: 80)
- DeathsText (Width: 80)
- KDText (Width: 80)
```

Kéo PlayerRow vào Prefabs, xóa khỏi Scene.

### 4.4 Gắn UI vào Scripts

**PlayerHUD:**
- Thêm PlayerHUD script vào Canvas
- Gắn các references: healthBar, healthText, ammoText, crosshairImage, killFeedText

**Scoreboard:**
- Thêm Scoreboard script vào Canvas
- Gắn: scoreboardPanel, playerListContainer, playerRowPrefab

**MultiplayerFPSGameManager:**
- Gắn: timerText, winnerText

---

## 🔗 BƯỚC 5: KẾT NỐI CÁC SCRIPTS

### 5.1 Sửa FPSController để dùng Weapon
Mở [FPSController.cs](Assets/Scripts/FPSController.cs) và thêm:

```csharp
[Header("Weapon")]
public WeaponBase currentWeapon;

void HandleShooting()
{
    if (Input.GetButtonDown("Fire1"))
    {
        if (currentWeapon != null)
            currentWeapon.Fire();
    }
    
    if (Input.GetKeyDown(KeyCode.R))
    {
        if (currentWeapon != null)
            currentWeapon.Reload();
    }
}
```

### 5.2 Gắn Weapon vào Player
- Mở Player Prefab
- Tìm RaycastGun trong WeaponHolder
- Kéo nó vào field `currentWeapon` trong FPSController

### 5.3 Gắn PlayerHealth references
- Tìm PlayerHealth script trong Player Prefab
- Gắn SpawnPoints array (từ Scene)

---

## 🎮 BƯỚC 6: PHOTON BUILD SETTINGS

### 6.1 Cấu hình Photon Resources
```
Window > Photon Unity Networking > Highlight Server Settings
```

Kiểm tra:
- App ID đã đúng
- Fixed Region: Best Region (hoặc chọn region gần)

### 6.2 Add Scenes to Build Settings
```
File > Build Settings
Add Open Scenes:
- GameScene
```

---

## ✅ BƯỚC 7: TEST GAME

### 7.1 Test Local
1. Chạy scene trong Unity Editor
2. Build game ra .exe
3. Chạy cả Editor và .exe cùng lúc
4. Kiểm tra:
   - ✅ Kết nối Photon
   - ✅ Spawn player
   - ✅ Di chuyển, nhìn
   - ✅ Bắn súng
   - ✅ Damage và respawn
   - ✅ Scoreboard (Tab)
   - ✅ Timer đếm ngược

### 7.2 Debug Tips
Nếu lỗi:
- **Không spawn player:** Kiểm tra Player prefab có trong Resources/
- **Không bắn được:** Kiểm tra Layer Mask trong RaycastGun
- **Không hiện UI:** Kiểm tra Canvas references
- **Lỗi Photon:** Kiểm tra App ID và Internet

---

## 🎯 CÁC TÍNH NĂNG CÒN CÓ THỂ THÊM

### Ưu tiên cao:
1. **Sound Effects** - Bắn súng, hit, death sounds
2. **Muzzle Flash** - Hiệu ứng bắn (Particle System)
3. **Hit Effect** - Hiệu ứng khi trúng đạn
4. **Spawn Protection** - 3s bất tử khi spawn
5. **Multiple Weapons** - Nhiều loại súng (Pistol, Rifle, Shotgun)

### Ưu tiên thấp:
6. **Minimap** - Bản đồ nhỏ
7. **Power-ups** - Health pack, ammo box
8. **Grenades** - Lựu đạn
9. **Leaderboard** - Lưu điểm cao
10. **Chat** - Chat trong game

---

## 📝 NOTES QUAN TRỌNG

### Photon Custom Properties
Game sử dụng Photon Custom Properties để sync:
- Player kills/deaths/assists
- Match timer
- Match active state

### Layer Setup
Đảm bảo:
- Player layer: "Player"
- Weapon raycast hit layer: Bao gồm "Player"

### Performance
- Limit max players: 8-10 người
- Optimize networking: Chỉ sync cần thiết
- Use object pooling cho bullets/effects

---

## 🆘 HỖ TRỢ

Nếu gặp vấn đề:
1. Check Console logs
2. Check Photon Server Settings
3. Verify all script references
4. Test in Editor vs Build

---

**Chúc bạn hoàn thành game thành công! 🎮🔥**
