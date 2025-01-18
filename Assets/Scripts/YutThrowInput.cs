using UnityEngine;

public class YutThrowInput : MonoBehaviour
{
    public YutThrower yutThrower; // YutThrower 스크립트를 연결할 필드

    // 버튼을 누르면 실행될 함수
    public void OnThrowButtonPressed()
    {
        if (yutThrower == null)
        {
            Debug.LogError("YutThrower가 연결되지 않았습니다!");
            return;
        }

        // 윷 던지기 실행
        Vector3 throwDirection = (Vector3.up + Vector3.right).normalized;
        yutThrower.ThrowAllYuts(throwDirection);

        Debug.Log("윷을 던졌습니다!");
    }
}
