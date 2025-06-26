using UnityEngine;

// 게임에 등장하는 SkyBox의 컬러를 관리하는 스크립트 입니다.
public class SkyBoxManager : MonoBehaviour
{
    // 다른 스크립트나 객체를 참조
    [Header("References")]
    [SerializeField]
    private StageManager stageManager;
    [SerializeField]
    private VictorySceneAnimator victorySceneAnimator;
    [SerializeField]
    private DefeatSceneAnimator defeatSceneAnimator;

    // 해당 스크립트에 필요한 숫자 변수들
    [Header("Values")]
    private static int skyBoxNum;   // 다른 씬에서도 동일한 컬러를 위해 static 선언

    [Header("Color")]
    public Color startColor = new Color32(0x80, 0x80, 0x80, 0xFF); // #808080 
    public Color midGameColor = new Color32(0x87, 0x65, 0x5D, 0xFF); // #87655D
    public Color endGameColor = new Color32(0x84, 0x3A, 0x15, 0xFF); // #843A15

    void Start()
    {
        if (RenderSettings.skybox.HasProperty("_Tint"))
            RenderSettings.skybox.SetColor("_Tint", startColor);
    }

    void Update()
    {
        // stageManager의 stageNumber변수를 이용해 스테이지 번호 별, 컬러 설정
        if (stageManager != null) skyBoxNum = stageManager.stageNumber;
        if (victorySceneAnimator != null || defeatSceneAnimator != null)    // 승, 패 씬에서만 작동되도록 하는 조건
        {
            switch (skyBoxNum)
            {
                case <= 3: // 스테이지 1~3
                    if (RenderSettings.skybox.HasProperty("_Tint"))
                        RenderSettings.skybox.SetColor("_Tint", startColor);
                    break;
                case >= 4 and <= 7: // 스테이지 4~7
                    if (RenderSettings.skybox.HasProperty("_Tint"))
                        RenderSettings.skybox.SetColor("_Tint", midGameColor);
                    break;
                case >= 8 and <= 10: // 스테이지 8~10
                    if (RenderSettings.skybox.HasProperty("_Tint"))
                        RenderSettings.skybox.SetColor("_Tint", endGameColor);
                    break;
                default:
                    break;
            }
        }
        else if (stageManager != null)  // 게임 씬 작동 조건
        {
            switch (skyBoxNum)
            {
                case <= 3: // 스테이지 1~3
                    if (RenderSettings.skybox.HasProperty("_Tint"))
                        RenderSettings.skybox.SetColor("_Tint", startColor);
                    break;
                case >= 4 and <= 7: // 스테이지 4~7
                    if (RenderSettings.skybox.HasProperty("_Tint"))
                        RenderSettings.skybox.SetColor("_Tint", midGameColor);
                    break;
                case >= 8 and <= 10: // 스테이지 8~10
                    if (RenderSettings.skybox.HasProperty("_Tint"))
                        RenderSettings.skybox.SetColor("_Tint", endGameColor);
                    break;
                default:
                    break;
            }
        }
    }
}
