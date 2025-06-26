using UnityEngine;

// Enemy의 애니메이션에 관한 스크립트입니다.
// 모든 Enemy에 관련한 스크립트는 Enemy을 앞에 붙여줍니다.
public class EnemyAnimatorController : MonoBehaviour
{
    [SerializeField]    // Inspector에서 끌어서 사용 
    private Animator animator;

    // 외부에서 호출 할 수 있도록 메소드를 정의
    public void Play(string stateName, int layer, float normalizedTime)
    {
        animator.Play(stateName, layer, normalizedTime);
    }

    // 사망 애니메이션
    public void isDeath()
    {
        animator.SetTrigger("isDeath");
    }

    public void isBack()
    {
        animator.SetTrigger("isBack");
    }

    public bool CurrentAnimationIs(string name) // name 애니메이션을 받고
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(name);    // 해당 애니메이션이 재생 중인지 확인 후 반환
    }

    public void SetBool(string parameterName, bool value)
    {
        animator.SetBool(parameterName, value);
    }

    // 이동 방향에 따라 애니메이션 설정
    public void SetMoveDirection(int direction)
    {
        // 랜덤으로 뽑힌 정수 값을 통해 이동 애니메이션 출력
        animator.SetInteger("isMove", direction); // MoveDirection 파라미터에 값 전달
    }
}
