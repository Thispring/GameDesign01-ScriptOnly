# Cord: Marigold

Unity 기반으로 제작한 3인칭 건슈팅 디펜스 게임입니다.

플레이어는 다양한 무기와 아이템을 활용해 기지를 방어하며, 스테이지마다 등장하는 적을 처치하고 상점을 통해 능력과 장비를 강화하며 게임을 진행합니다.

---

## 📖 프로젝트 소개

Cord: Marigold는 플레이어가 직접 전투에 참여하면서 기지를 방어하는 3인칭 건슈팅 디펜스 게임입니다.

단발, 연발, 저격, 로켓 등 서로 다른 특징을 가진 무기를 상황에 맞게 사용할 수 있으며, 적의 공격 대상은 플레이어뿐만 아니라 설치된 터렛으로 변경될 수 있도록 구현했습니다.

스테이지를 클리어하면 적 처치를 통해 획득한 재화를 사용해 무기, 플레이어 능력, 터렛, 아이템 등을 강화하며 다음 스테이지를 준비할 수 있습니다.

| 항목 | 내용 |
| --- | --- |
| 개발 기간 | 3개월 |
| 개발 인원 | 총 4명 |
| 팀 구성 | 아트 3명 / 기획·프로그래밍 1명 (본인) |
| 플랫폼 | PC |
| 개발 엔진 | Unity |
| 개발 언어 | C# |
| 담당 역할 | 게임 기획 / 클라이언트 프로그래밍 |

---

## 🎮 Gameplay

게임 플레이 영상은 아래 링크에서 확인할 수 있습니다.

[Cord: Marigold Gameplay Video](https://youtu.be/LIh10WImKrg)

---

## 👨‍💻 My Role

### Client Programming

- 플레이어 상태 및 사격 시스템 구현
- 단발, 연발, 저격, 로켓 무기 시스템 구현
- 무기 교체 및 탄약 관리 구현
- 적 생성 및 스테이지 진행 시스템 구현
- 적 행동 및 공격 대상 관리 구현
- 터렛 및 EMP 아이템 시스템 구현
- 오브젝트 풀링 기반 오브젝트 생성 및 관리
- 스테이지 클리어 후 상점 및 성장 시스템 구현
- 플레이어 및 기지 체력 관리
- UI 및 게임 진행 정보 처리
- 사운드 및 게임 환경 설정 기능 구현

---

# 🎯 주요 구현 기능

## 🔫 다형성을 활용한 무기 시스템

서로 다른 공격 방식을 가진 무기를 공통된 인터페이스로 관리하기 위해 `WeaponBase` 추상 클래스를 기반으로 무기 시스템을 구성했습니다.

각 무기는 공통적으로 사격, 재장전, 상태 초기화 등의 기능을 공유하며, 단발, 연발, 저격, 로켓 무기는 각각의 특성에 맞게 기능을 구현했습니다.

`WeaponSwitchSystem`에서는 현재 장착 중인 무기를 `WeaponBase` 타입으로 관리하여, 무기 종류에 따라 개별 클래스를 직접 처리하지 않고 동일한 방식으로 무기 교체와 상태 변경을 수행하도록 구성했습니다.

**관련 코드**

- [WeaponBase.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Weapon/WeaponBase.cs)
- [WeaponSingleRifle.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Weapon/WeaponSingleRifle.cs)
- [WeaponRapidRifle.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Weapon/WeaponRapidRifle.cs)
- [WeaponSniperRifle.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Weapon/WeaponSniperRifle.cs)
- [WeaponRocket.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Weapon/WeaponRocket.cs)
- [WeaponSwitchSystem.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Weapon/WeaponSwitchSystem.cs)

---

## 👾 적 생성 및 스테이지 진행 시스템

스테이지 진행에 따라 적의 등장 수와 생성 주기를 변경할 수 있도록 적 생성 시스템을 구현했습니다.

`EnemyMemoryPool`에서 현재 스테이지를 기준으로 적의 최대 생성 수와 생성 시간을 조정하며, 일반 적과 엘리트 적을 구분하여 생성하도록 구성했습니다.

현재 스테이지의 적을 모두 처치하면 처치 수를 게임 점수에 반영하고, 게임을 일시 정지한 뒤 상점 시스템을 통해 다음 스테이지를 준비할 수 있도록 연결했습니다.

**관련 코드**

- [EnemyMemoryPool.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Enemy/EnemyMemoryPool.cs)
- [EnemySpawnPoint.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Enemy/EnemySpawnPoint.cs)
- [StageManager.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/GameManager/StageManager.cs)

---

## 🎯 적 행동 및 공격 대상 전환 시스템

적의 이동과 공격 동작을 `EnemyFSM`에서 관리했습니다.

적은 기본적으로 지정된 목표를 공격하며, 게임 중 플레이어가 터렛을 설치하면 활성화된 터렛을 새로운 공격 대상으로 설정할 수 있도록 구현했습니다.

터렛이 파괴되면 기존 목표인 플레이어로 다시 대상을 변경하며, 이를 통해 플레이어와 설치물의 상태에 따라 적의 공격 대상이 변경되도록 구성했습니다.

적의 공격은 코루틴을 통해 일정 시간 간격으로 처리하고, 적의 상태에 따라 이동과 공격을 중지하거나 재개할 수 있도록 구현했습니다.

**관련 코드**

- [EnemyFSM.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Enemy/EnemyFSM.cs)
- [EnemyStatusManager.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Enemy/EnemyStatusManager.cs)
- [TurretController.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Item/TurretController.cs)
- [Turret.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Item/Turret.cs)

---

## ♻️ 오브젝트 풀링 기반 생성 관리

게임 중 반복적으로 생성되는 오브젝트를 효율적으로 관리하기 위해 `MemoryPool` 클래스를 구현했습니다.

관리 대상 오브젝트를 미리 생성한 뒤, 사용이 필요한 경우 비활성화된 오브젝트를 찾아 활성화하고 사용이 완료되면 다시 비활성화하여 재사용하도록 구성했습니다.

현재 생성된 오브젝트가 모두 사용 중인 경우에는 일정 개수의 오브젝트를 추가 생성할 수 있도록 했습니다.

이를 적 등장 위치 등 반복적으로 생성되는 게임 오브젝트 관리에 활용했습니다.

**관련 코드**

- [MemoryPool.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/MemoryPool.cs)
- [EnemyMemoryPool.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Enemy/EnemyMemoryPool.cs)

---

## 🛒 스테이지 간 성장 및 상점 시스템

스테이지를 클리어한 후 획득한 재화를 사용해 플레이어와 장비를 강화할 수 있는 상점 시스템을 구현했습니다.

무기 공격력, 플레이어 체력, 기지 회복, 터렛, EMP, 로켓 등 여러 강화 항목을 관리하며, 현재 보유한 재화와 강화 단계에 따라 구매 비용과 능력치를 변경하도록 구성했습니다.

상점에서 변경된 무기 및 플레이어 상태는 다음 스테이지에서도 이어져, 스테이지 진행과 성장 시스템이 연결되도록 구현했습니다.

**관련 코드**

- [ShopManager.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/GameManager/ShopManager.cs)
- [StageManager.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/GameManager/StageManager.cs)
- [PlayerStatusManager.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Player/PlayerStatusManager.cs)
- [WeaponBase.cs](https://github.com/Thispring/GameDesign01-ScriptOnly/blob/main/Script/Weapon/WeaponBase.cs)

---

# 🛠 사용 기술

| 기술 | 활용 |
| --- | --- |
| Unity | 게임 클라이언트 개발 |
| C# | 게임 로직 및 시스템 구현 |
| Unity Physics | Raycast 기반 사격 및 게임 오브젝트 상호작용 처리 |
| Unity UI | 체력, 탄약, 무기 정보 및 게임 인터페이스 구현 |

---

# 🔗 Links

### 🎥 Gameplay Video

[YouTube - Cord: Marigold Gameplay](https://youtu.be/LIh10WImKrg)

---

> 본 리포지토리는 포트폴리오 공개를 목적으로 프로젝트의 스크립트 코드만 포함하고 있습니다.
>
> 게임 에셋 및 전체 Unity 프로젝트 파일은 포함되어 있지 않습니다.
