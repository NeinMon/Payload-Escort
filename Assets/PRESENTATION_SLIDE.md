    PAYLOAD ESCORT MODE – GAME DESIGN DOCUMENT
    1. Overview
    Payload Escort Mode là một chế độ chơi FPS multiplayer theo team, trong đó một đội phải hộ tống một vật thể (Payload) đến điểm đích, còn đội kia phải ngăn chặn.
    Mục tiêu của mode này là tạo ra gameplay:
    Không chỉ dựa vào kỹ năng bắn
    Mà còn dựa vào teamwork, kiểm soát vị trí và chiến thuật

    2. Team Structure
    Team	Vai trò
    Attackers (Escort Team)	Hộ tống Payload đến đích
    Defenders (Defense Team)	Ngăn không cho Payload đến đích
    Mỗi trận có 2 team, số người chia đều.

    3. Core Gameplay Loop
    Spawn → Move to Payload → Control Zone → Payload Moves → Fight → Respawn → Repeat
    Gameplay xoay quanh việc chiếm giữ Payload Zone (vùng xung quanh Payload).

    4. Payload Object Design
    4.1 Payload là gì?
    Payload là một Networked Object trong Photon:
    Có PhotonView
    Có vị trí, hướng và tốc độ
    Được đồng bộ cho tất cả client
    Payload luôn di chuyển theo một đường ray định sẵn (Path).

    4.2 Payload States
    Payload có 3 trạng thái chính:
    State	Điều kiện
    Moving	Có ≥1 Attacker, không có Defender
    Stopped	Không có Attacker
    Contested	Có cả Attacker và Defender
    State này được tính bởi Master Client.

    4.3 Payload Movement
    Khi ở trạng thái Moving:
    Payload di chuyển với tốc độ cố định
    Di chuyển theo path đã thiết kế trong map
    Khi Stopped hoặc Contested:
    Payload đứng yên

    5. Payload Zone
    Payload có một vùng tròn xung quanh.
    Player trong vùng sẽ được tính là đang “tác động Payload”
    Photon dùng Trigger Collider để phát hiện player
    Client gửi RPC:
    "Player X entered payload zone"
    Master Client quyết định trạng thái thật.

    6. Checkpoints
    Trên map có nhiều Checkpoint.
    Khi Payload chạm checkpoint:
    Vị trí được lưu
    Nếu bị đẩy lùi hoặc reset → Payload không quay lại trước checkpoint
    Điều này giúp trận đấu không bị kéo dài quá lâu.

    7. Match Timer
    Mỗi trận có thời gian cố định (ví dụ 10 phút).
    Nếu Payload đến đích → Attackers thắng
    Nếu hết giờ → Defenders thắng
    Timer được lưu trong Room Properties để đồng bộ.

    8. Networking Architecture
    8.1 Authority Model
    Thành phần	Ai quyết định
    Payload position	Master Client
    Payload state	Master Client
    Timer	Master Client
    Player input	Local Client
    Client chỉ gửi ý định (enter zone, leave zone), Master xử lý logic.

    8.2 Photon Components
    Photon Feature	Dùng cho
    PhotonView	Payload, Player
    RPC	Enter zone, state update
    Room Properties	Timer, progress
    Custom Properties	Team, score

    9. UI Design
    Game hiển thị:
    Payload Progress Bar
    Current Payload State (Moving / Contested / Stopped)
    Timer
    Team color (Red vs Blue)
    UI dùng Observer Pattern để cập nhật khi state thay đổi.

    10. Design Patterns Used
    Pattern	Vai trò
    State Pattern	Payload states
    Observer Pattern	UI updates
    Command Pattern	Player actions (enter/exit zone)
    Singleton	MatchManager

    12. Planned Gameplay Extensions (Future Features)
    The Payload Escort Mode is designed with an extensible architecture that allows new gameplay mechanics to be added without changing the core system. The following features are planned as future extensions to make the gameplay deeper, more strategic, and more suitable for competitive multiplayer.

    12.1 Dynamic Payload Speed
    The movement speed of the payload will depend on the number of Attackers inside the payload zone.
    The more players escorting the payload, the faster it moves, up to a maximum speed limit.
    This mechanic:
    Encourages team grouping and cooperation
    Creates risk–reward situations (more players near payload means less map control)
    Makes area-of-effect damage and flanking more meaningful

    12.2 Checkpoint Rewards
    When the payload reaches a checkpoint:
    The attacking team receives score or bonuses
    The payload can be repaired or reinforced
    This creates:
    Intermediate goals during the match
    A sense of progression
    Opportunities for comeback after losing ground

    12.3 Payload Sabotage System
    Defenders can directly damage or sabotage the payload to temporarily disable it or push it backward.
    When the payload is sabotaged:
    It enters a “disabled” state
    Attackers must stand near the payload and hold an interaction key to repair it
    This introduces a mini-objective inside the main objective and increases tactical depth.

    12.4 Hero / Class System
    Players will be able to select different roles that interact with the payload in unique ways:
    Class	Ability
    Engineer	Repairs the payload faster and can deploy temporary shields
    Heavy	Pushes the payload more effectively and resists knockback
    Medic	Heals allies near the payload
    Saboteur	Can temporarily reverse or disrupt the payload movement
    This system enables:
    Team composition and role-based strategies
    More meaningful cooperation between players

    Easier application of design patterns such as Strategy and State
