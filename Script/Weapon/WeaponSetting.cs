// 게임에 존재하는 특수 돌격 소총에 대한 스크립트 입니다.
// 모든 무기에 관련한 스크립트는 Weapon을 앞에 붙여줍니다.
/*
무기의 종류가 여러 종류일 때 공용으로 사용하는 변수들은 구조체로 묶어서 정의하면 변수가 추가/삭제될 때
구조체에 선언하기 때문에 추가/삭제에 대한 관리가 용의함

사격 종류 별 명칭
단발 -> Single = 0
연발 -> Rapid = 1
저격 -> Sniper = 2
로켓 -> Rocket = 3
*/
public enum WeaponName { Single = 0, Rapid = 1, Sniper = 2, Rocket = 3 } // 무기의 이름을 나타내는 WeaponName 열거형
[System.Serializable]
public struct WeaponSetting
{
    public WeaponName weaponName;   // 무기 이름
    public int currentAmmo; // 현재 탄약 수
    public int maxAmmo; // 최대 탄약 수
    public float attackRate;    // 공격속도
    public float attackDistance;    // 공격 사거리
    public bool isAutomaticAttack; // 연속 공격 여부

    public float singleDamage; // Single 타입의 데미지
    public float rapidDamage;  // Rapid 타입의 데미지
    public float sniperDamage; // Sniper 타입의 데미지

}
