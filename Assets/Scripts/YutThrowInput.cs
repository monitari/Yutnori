using UnityEngine;
using UnityEngine.UI; // 추가
using System.Collections;

public class YutThrowInput : MonoBehaviour {
    public YutThrower yutThrower; // YutThrower 스크립트를 연결할 필드
    public Button throwButton; // 던지기 버튼 필드 추가

    void Start() {
        if (yutThrower != null) {
            yutThrower.DisableYuts(); // 시작할 때 윷 비활성화
        }
    }

    public void OnThrowButtonPressed() { // 윷 던지기 버튼 클릭 시 호출
        if (yutThrower == null) {
            Debug.LogError("YutThrower가 연결되지 않았습니다!");
            return;
        }
        yutThrower.ResetAllYuts();
        Vector3 throwDirection = Vector3.up;
        yutThrower.enabled = true;
        yutThrower.ThrowAllYuts(throwDirection);
        Debug.Log("윷을 던졌습니다!");
        if (throwButton != null) throwButton.gameObject.SetActive(false); // 버튼 비활성화
    }
}
