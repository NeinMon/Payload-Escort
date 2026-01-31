# 🎨 HƯỚNG DẪN SETUP UI - NGẮN GỌN

## 🔧 BƯỚC 1: MỞ PLAYER PREFAB

```
Project > Assets > Resources > Player > Double-click
```

---

## 🖼️ BƯỚC 2: TẠO LOCAL HUD CANVAS

```
Right-click Player > UI > Canvas → Rename "LocalHUD"
```

**Canvas Settings:**
- Render Mode: **Screen Space - Overlay**
- Canvas Scaler: **Scale With Screen Size** (1920x1080)

---

## ❤️ BƯỚC 3: TẠO HEALTH BAR

### 3.1 HealthBarBackground
```
LocalHUD > UI > Image → "HealthBarBackground"
```
- Anchors: Bottom-Left (0, 0)
- Position: (20, 80, 0), Size: 250x30
- Color: Black (0, 0, 0, 150)

### 3.2 HealthBarFill
```
HealthBarBackground > UI > Image → "HealthBarFill"
```
- Anchors: Stretch, Offset: (3, -3, -3, 3)
- Color: Green
- Image Type: **Filled**, Horizontal, Fill Amount: 1

### 3.3 HealthText
```
HealthBarBackground > UI > Text - TextMeshPro → "HealthText"
```
- Text: "100 / 100", Font Size: 18, Bold
- Alignment: Center, Outline: Black 0.2

---

## 🔫 BƯỚC 4: TẠO AMMO & CROSSHAIR

### 4.1 AmmoText
```
LocalHUD > UI > Text - TextMeshPro → "AmmoText"
```
- Anchors: Bottom-Right (1, 0)
- Position: (-30, 80, 0), Size: 180x60
- Text: "30 / 30", Font Size: 42, Bold
- Alignment: Right, Outline: Black 0.3

### 4.2 Crosshair
```
LocalHUD > UI > Image → "Crosshair"
```
- Anchors: Center (0.5, 0.5)
- Position: (0, 0, 0), Size: 32x32
- Color: White (255, 255, 255, 200)

---

## 📜 BƯỚC 5: ADD SCRIPT & REFERENCES

```
Select LocalHUD Canvas > Add Component > LocalPlayerHUD
```

**Gắn References:**
- healthBar → HealthBarFill
- healthText → HealthText
- ammoText → AmmoText
- crosshairImage → Crosshair
- normalColor → White, hitColor → Red

**Lưu:** Ctrl+S

---

## 🌐 BƯỚC 6: NETWORK HEALTH BAR (3D)

```
Player > UI > Canvas → "NetworkHealthBar"
```

**Canvas:**
- Render Mode: **World Space**
- Position: (0, 2.2, 0), Scale: (0.01, 0.01, 0.01)
- Width: 200, Height: 30

**Health Bar:**
```
NetworkHealthBar > UI > Image → "HealthBarBG"
  └─ UI > Image → "HealthBarFill"
```
- HealthBarBG: Black (0, 0, 0, 180), Stretch
- HealthBarFill: Green, Filled Horizontal, Offset: (2, -2, -2, 2)

**Script:**
```
Add Component > NetworkPlayerHealthBar
```
- canvas → Canvas, healthBarFill → HealthBarFill

---

## 👤 BƯỚC 7: NETWORK NAME TAG

```
Player > UI > Canvas → "NetworkNameTag"
```

**Canvas:**
- World Space, Position: (0, 2.5, 0)
- Scale: (0.01, 0.01, 0.01), Width: 300, Height: 50

**Name Text:**
```
NetworkNameTag > UI > Text - TextMeshPro → "NameText"
```
- Font Size: 36, Bold, Center
- Outline: Black 0.4

**Script:**
```
Add Component > NetworkPlayerNameTag
```
- canvas → Canvas, nameText → NameText

**Lưu:** Ctrl+S > Exit Prefab Mode

---

## ✅ CHECKLIST

- [ ] LocalHUD: Screen Space Overlay
- [ ] HealthBarFill: Type = Filled
- [ ] All scripts added & references assigned
- [ ] Prefab saved (Ctrl+S)
- [ ] NetworkHealthBar: World Space, Y = 2.2
- [ ] NetworkNameTag: World Space, Y = 2.5

---

## 🐛 TROUBLESHOOTING

**UI không hiển thị:** Check Canvas Render Mode, GameObject Active
**Text mờ:** Enable Outline, Thickness 0.3-0.4
**3D UI không face camera:** Check Camera tag = "MainCamera"
**UI hiển thị cho remote:** Check `if (!photonView.IsMine)` trong script

---

## 🎉 HOÀN TẤT!

Test với 2 clients:
- ✅ LocalHUD chỉ mình thấy
- ✅ NetworkHealthBar của người khác sync
- ✅ Name tag hiển thị nickname
