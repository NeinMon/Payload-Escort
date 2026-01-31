# 1. Project Overview

**Project Type:** Multiplayer FPS Sandbox / Showcase Project

**Target Platforms:** PC (StandaloneWindows 64-bit)

**Core Features / Pillars:**
- Modular multiplayer FPS foundation built atop Photon Networking.
- AI-assisted content generation pipelines (Unity AI Toolkit integration).
- Combined third-party showcase scenes (HairPack, LowPolyFPSLite, Cosmic Retro Blasters).
- Demonstrations of Photon PUN demos for multiplayer mechanics (chat, lobby, rooms, procedural world, etc.).
- Input System (New) integrated character control and interaction support.
- Built-in Render Pipeline used for maximum compatibility across demo content.

# 2. Gameplay Flow / User Loop

**Major States:**
- **Launcher / Hub:** Player authenticates and connects to Photon services.
- **Lobby:** Player selects or creates rooms.
- **Gameplay (Rooms):** First-person combat or exploration environment; each player controls a character prefab with network synchronization.
- **End / Exit:** Players leave or disconnect gracefully, returning to the launcher.

**Core Loop:** Connect → Join/Create Lobby → Spawn Player → Interact/Combat → Loop or Quit.

**State Transitions:**
- From launcher (PunBasics-Launcher scene) to multiplayer rooms (PunBasics-Room for N).
- From menus to demo showcase scenes (Hair, Skeleton, Weapon Demos).

# 3. Architecture (Runtime + Editor)

**Runtime Systems:**
- **Multiplayer System (Photon PUN):** Handles rooms, players, networking, and synchronization.
- **Player Controller:** FPSPlayerController integrates the Input System with movement, look, jump, and action.
- **Game Manager:** (MultiplayerFPSGameManager or PunBasics GameManager) manages game state transitions and synchronization.
- **NetworkPlayer:** Manages instantiation and camera linking for each networked user.
- **Visual Demos:** Hair, Skeleton, Retro Blaster scenes provide isolated showcases for art and animation.

**Editor Tooling:**
- AI Toolkit integration for content generation and management.
- Photon editor scripts for configuration and server settings automation.
- Configurable Input Action asset via Input System.

**Entry Points:**
- Default: `Assets/_Recovery/0.unity` or `Assets/Scenes/SampleScene.unity` for initial testing.
- Demo flow entry via `Assets/Photon/.../Launcher` scenes for full multiplayer demo.

**Patterns & Communication:**
- Event-driven (Photon callbacks, Input events).
- Component-based modular architecture; strong separation between UI, Networking, Player, Manager scripts.

# 4. Scene Overview & Responsibilities

**Scene List:**
- `_Recovery/0.unity` – Recovery and composite environment test scene integrating LowPoly assets and networked player entities.
- `LowPolyFPS_Lite_Demo` – Core FPS playground environment using modular prefabs.
- `HairPack/Demo` – Showcases hair physics and UnityChan shaders.
- `Art_Prototype_Stylized/Scene_Skeleton` – Skeletal model animation showcase with variants.
- `Cosmic_Retro_Blaster` scenes – Weapon art and FX demos.
- Photon demo scenes – Chat, Lobby, Procedural, Cockpit, SlotCar, etc., each demonstrating a networking concept.

**Loading Strategy:** Each scene acts independently; switching occurs via Photon scene synchronization or manual editor load.

**Responsibilities:**
- Gameplay demo scenes own local managers.
- Lobby/Chat scenes depend on Photon services.
- _Recovery and LowPoly scenes own geometry, ambient lighting, and FPS controller initialization.

**Constraints:** Scenes expect prefabs (`NetworkPlayer`, `SkeletonCharacter`, `GameManager`) to exist.

# 5. UI System

**Framework:** UGUI (`com.unity.ugui`), no UITK layers.

**Navigation:** Standard UGUI canvases with event-driven button callbacks. Photon demos use UI canvases with tabbed panels.

**Binding Logic:** Manual bindings through MonoBehaviours (e.g., Button.onClick).

**UI Style:**
- Photon demos: clean, minimalistic sans-serif fonts (OpenSans, Jura).
- Hair demo: anime-style rounded panels.
- Consistent hierarchy: `Canvas > Panels > Controls > Text`.

# 6. Asset & Data Model

**Asset Style:**
- Mixture of stylized (LowPolyFPSLite, HairPack) and realistic sci-fi (Cosmic Retro Blasters).
- Textures mostly 512–2K; emissive and metalness maps in use for demos.

**Data Formats:**
- Prefabs for entity composition.
- FBX models for geometry and rigged meshes.
- ScriptableObjects/Settings used for Photon configurations.
- Input actions stored via `InputSystem_Actions.inputactions`.

**Asset Organization:**
- Folder based by module: `LowPolyFPSLite`, `HairPack`, `Photon`, `Cosmic_Retro_Blasters`, `Art_Prototype_Stylized`.

**Naming & Versioning Rules:** Use consistent capitalized asset prefixes per pack (`M_`, `T_`, `P_`, `SM_`, `SKM_`).

# 7. Project Structure (Repo & Folder Taxonomy)

**Folder Layout:**
- `Assets/Scripts`: Core gameplay, networking, managers.
- `Assets/LowPolyFPSLite`: Art and prefabs for FPS level.
- `Assets/HairPack`: Character customization demo with physics.
- `Assets/Photon`: Networking and PUN demo systems.
- `Assets/Art_Prototype_Stylized`: Stylized skeleton model variants.

**Scene & Prefab Notes:**
- Prefabs are self-contained with materials and meshes.
- All major prefabs have their own subfolders per category.

**Conventions:** PascalCase for classes, snake_case for folder components, consistent prefab naming: `[Category]_[Name]`.

# 8. Technical Dependencies

**Unity & Pipeline:** Unity 6000.3.3f1, Built-in Render Pipeline.

**Third-Party Packages:**
- Photon Chat, Photon Realtime, Photon PUN (all bundled under /Photon/).
- HairPack assets (UnityChan-based shaders).
- AI Toolkit and Generators (Unity AI pre-1.5). 

**External Services:** Photon Cloud; no other online services configured.

# 9. Build & Deployment

**Build Steps:**
1. Open Build Settings → Add main demo scenes.
2. Ensure Photon Server Settings asset has valid AppID.
3. Build target: StandaloneWindows64.
4. Press Build or use batch build.

**Supported Platforms:** Windows only (currently configured).

**CI/CD:** None embedded; suitable for Unity Cloud Build or manual Jenkins integration.

**Environment Requirements:** Photon AppID, Internet access for multiplayer features.

# 10. Style, Quality & Testing

**Code Style:**
- Standard C# naming and Unity conventions.
- One class per file.

**Performance Guidelines:**
- Use static batching and light baking (LowPolyFPSLite scenes contain baked lighting data).
- Limit real-time lights in FPS scenes.

**Testing Strategy:**
- PlayMode tests available via Photon demos.
- Editor testing for prefabs and animation.

**Validation Rules:**
- Photon configuration must be validated via PhotonServerSettings.

# 11. Notes, Caveats & Gotchas

**Known Issues:**
- Built-in pipeline inconsistency if packs use HDRP materials; may require conversion.
- AI Toolkit references unused in the base gameplay loop.

**Dependency Rules:**
- Modifying Photon prefabs may break demo flows.
- InputAction rebinding impacts FPSPlayerController functionality.

**Deprecated Systems:** None officially deprecated, but legacy demos may include outdated UI transitions.

**Platform Caveats:** Multiplayer demos require network connection; single-player modes limited.
