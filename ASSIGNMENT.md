# Code Study Split (4 people)

Scope
- Include: game scripts under Assets/Scripts (gameplay, heroes, payload escort, weapons, networking, lobby, managers, characters, audio, root scripts)
- Exclude: UI/HUD creation code (Assets/Scripts/UI, Assets/Editor/*HUD* and related UI builders), third-party and demo code (Assets/Photon, Assets/JMO Assets)

## Person 1: Core player + weapons (12 files)
Focus: player control, combat loop, weapon handling
Files
- Assets/Scripts/FPSController.cs
- Assets/Scripts/Player/FPSPlayerController.cs
- Assets/Scripts/Player/SpawnProtection.cs
- Assets/Scripts/PlayerControllerNetwork.cs
- Assets/Scripts/PlayerHealth.cs
- Assets/Scripts/Weapons/WeaponBase.cs
- Assets/Scripts/Weapons/WeaponManager.cs
- Assets/Scripts/Weapons/RaycastGun.cs
- Assets/Scripts/Weapons/WeaponCameraAligner.cs
- Assets/Scripts/PlayerColorAssigner.cs
- Assets/Scripts/Managers/GameStats.cs
- Assets/Scripts/MultiplayerFPSGameManager.cs
- Assets/Scripts/Characters/SciFiWarriorAnimator.cs
- Assets/Scripts/Characters/UpperBodyAim.cs
## Person 2: Heroes system core (12 files)
Focus: hero definitions, input, skill flow (non-turret)
Files
- Assets/Scripts/Heroes/HeroAssigner.cs
- Assets/Scripts/Heroes/HeroDefinition.cs
- Assets/Scripts/Heroes/HeroHoldSkill.cs
- Assets/Scripts/Heroes/HeroInputController.cs
- Assets/Scripts/Heroes/HeroRoster.cs
- Assets/Scripts/Heroes/HeroRuntime.cs
- Assets/Scripts/Heroes/HeroSelectionUtils.cs
- Assets/Scripts/Heroes/HeroSkillBehaviour.cs
- Assets/Scripts/Heroes/HeroSkillDefinition.cs
- Assets/Scripts/Heroes/HeroSkillReadyEffects.cs
- Assets/Scripts/Heroes/HeroSkillSlot.cs
## Person 3: Payload + turret skills (12 files)
Focus: payload flow + turret gameplay logic
Files
- Assets/Scripts/PayloadEscort/PayloadEscortMatchManager.cs
- Assets/Scripts/PayloadEscort/PayloadController.cs
- Assets/Scripts/PayloadEscort/PayloadCheckpoint.cs
- Assets/Scripts/PayloadEscort/PayloadZone.cs
- Assets/Scripts/PayloadEscort/PayloadZoneIndicator.cs
- Assets/Scripts/PayloadEscort/PayloadTeam.cs
- Assets/Scripts/Heroes/Skills/EngineerTurretOverdriveSkill.cs
- Assets/Scripts/Heroes/Skills/EngineerTurretState.cs
- Assets/Scripts/Heroes/Skills/TurretAutoAttack.cs
- Assets/Scripts/Heroes/Skills/TurretHealth.cs
- Assets/Scripts/Heroes/Skills/TurretOverdriveEffect.cs
- Assets/Scripts/Heroes/Skills/TurretOwner.cs
- Assets/Scripts/Heroes/Skills/EngineerDeployTurretSkill.cs
- Assets/Scripts/Heroes/Skills/TurretVisualSwap.cs
## Person 4: Networking + lobby + misc systems (13 files)
Focus: session flow, lobby, roles, visuals, audio
Files
- Assets/Scripts/PhotonLobbyManager.cs
- Assets/Scripts/Lobby/LobbyManager.cs
- Assets/Scripts/Networking/LocalOnlyRendererDisabler.cs
- Assets/Scripts/Networking/NetworkPlayerHealthBar.cs
- Assets/Scripts/Networking/NetworkPlayerNameTag.cs
- Assets/Scripts/PayloadEscort/PlayerRole.cs
- Assets/Scripts/PayloadEscort/PlayerRoleAssigner.cs
- Assets/Scripts/Audio/SingleAudioListenerEnforcer.cs

Notes
- UI/HUD scripts excluded by request (Assets/Scripts/UI and Editor HUD builders).
- Third-party Photon and JMO demo assets excluded for study focus.
