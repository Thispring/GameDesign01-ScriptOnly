using UnityEngine;

// 다른 Scene에서도 커서를 보이게 하기 위한 스크립트입니다.
public class CursorManager : MonoBehaviour
{
    void Update()
    {
        Cursor.visible = true;
    }
}
