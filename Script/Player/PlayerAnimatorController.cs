using UnityEngine;

// Player의 애니메이션에 관련된 스크립트 입니다.
// 모든 Player에 관련한 스크립트는 Player을 앞에 붙여줍니다.
public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField]    // Inspector에서 끌어서 사용 
    private Animator animator;

    // 외부에서 호출 할 수 있도록 메소드를 정의
    public void Play(string stateName, int layer, float normalizedTime)
    {
        animator.Play(stateName, layer, normalizedTime);
    }

    public void OnReload()  // 재장전 애니메이션
    {
        animator.SetTrigger("onReload");
    }

    public void OnAttack()
    {
        animator.SetTrigger("triggerShot");
    }

    public bool CurrentAnimationIs(string name) // name 애니메이션을 받고
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(name);    // 해당 애니메이션이 재생 중인지 확인 후 반환
    }

    // 애니메이션 상태를 bool로 제어
    public void SetAnimationBool(string parameterName, bool value)
    {
        animator.SetBool(parameterName, value); // Animator의 Bool 파라미터 설정
    }
}
