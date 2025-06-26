// Enemy 정보를 담은 구조체 스크립트 입니다.
// 모든 Enemy에 관련한 스크립트는 Enemy을 앞에 붙여줍니다.
/*
Enemy 유형 별 명칭
소형 -> Small
중형 -> Medium
대형 -> Big
*/
public enum EnemyName { Small = 0, Medium = 1, Big = 2 }
[System.Serializable]
public struct EnemySetting
{
    public EnemyName enemyName; // enemy 이름
    public float enemyHP;   // enemy의 체력
    public int enemyDamage;   // enemy의 공격력
    public float enemyAttackRate;   // enemy의 공격속도
    public float enemyAttackDistacne;   // enemy의 공격 사거리
    public float enemyMoveSpeed;   // enemy의 이동속도
    public bool enemyExplosion;   // enemy의 폭발 여부
    public int enemyCoin;   // enemy의 코인 드랍 여부, 타입별 코인 드랍 차이 존재
    public bool isEliteEnemy;   // Elite 타입 enemy 여부

}
