using UnityEngine;
using UnityEngine.UI; // 추가
using System;
using System.Linq; // 추가

public class TurnManager : MonoBehaviour {
    public YutThrower yutThrower;
    
    [SerializeField]
    private PlayerGroup[] playerMoverGroups; // 직렬화된 PlayerGroup 배열로 변경

    public Button addNewPieceButton; // 새 기물 추가 버튼
    public Button throwButton; // 던지기 버튼 필드 추가
    private int currentPlayerIndex = 0;
    private bool waitingForUserChoice = false; // 유저 선택을 대기하는 상태 플래그
    private string currentYutResult; // 현재 윷 결과 저장

    void Start() {
        if (yutThrower != null) {
            yutThrower.DisableYuts();
            if (playerMoverGroups != null && playerMoverGroups.Length > 0) {
                // 모든 플레이어 기물을 비활성화
                foreach (PlayerGroup group in playerMoverGroups) {
                    foreach (PlayerMover mover in group.movers) mover.gameObject.SetActive(false);
                }
                yutThrower.OnYutResult += MoveCurrentPlayer;
            } 
            else Debug.LogError("PlayerMover 배열이 올바르게 설정되지 않았습니다!");
        }
        if (addNewPieceButton != null) addNewPieceButton.gameObject.SetActive(false); // 시작할 때 버튼 비활성화
        else Debug.LogError("AddNewPieceButton이 할당되지 않았습니다!");
        if (throwButton != null) throwButton.gameObject.SetActive(true); // 시작할 때 던지기 버튼 활성화
        else Debug.LogError("ThrowButton이 할당되지 않았습니다!");
    }

    public void OnThrowButtonPressed() {
        if (yutThrower == null) return;
        yutThrower.ResetAllYuts();
        yutThrower.enabled = true;
        yutThrower.ThrowAllYuts(Vector3.up);
        Debug.Log("윷을 던졌습니다!");
        if (addNewPieceButton != null) addNewPieceButton.gameObject.SetActive(false); // 버튼 비활성화
    }

    private void MoveCurrentPlayer(string yutResult) {
        PlayerGroup currentGroup = playerMoverGroups[currentPlayerIndex];
        bool hasActivePiece = currentGroup.movers.Any(m => m.gameObject.activeSelf); // 활성화된 기물이 있는지 확인
        
        if (hasActivePiece) { // 활성화된 기물이 하나라도 있으면 사용자 선택 대기
            waitingForUserChoice = true;
            currentYutResult = yutResult; // 현재 윷 결과 저장
            ShowActionSelectionUI(currentGroup);
            EnableOutlines(currentGroup); // 외곽선 활성화
        }
        else ActivateNewPiece(yutResult, currentGroup); // 활성화된 기물이 없으면 새 기물 활성화
        if (throwButton != null) throwButton.gameObject.SetActive(true); // 플레이어의 턴에 던지기 버튼 활성화
    }

    private void EnableOutlines(PlayerGroup currentGroup) {
        foreach (PlayerMover mover in currentGroup.movers) {
            if (mover.gameObject.activeSelf) mover.EnableOutline(); // 활성화된 기물만 외곽선 활성화
        }
    }

    private void DisableOutlines(PlayerGroup currentGroup) {
        foreach (PlayerMover mover in currentGroup.movers) {
            if (mover.gameObject.activeSelf) mover.DisableOutline(); // 활성화된 기물만 외곽선 비활성화
        }
    }

    public void OnAddNewPieceClicked() { // 새 기물 추가 버튼 클릭 시 호출
        if (!waitingForUserChoice) return; 
        PlayerGroup currentGroup = playerMoverGroups[currentPlayerIndex]; // 현재 플레이어 그룹
        ActivateNewPiece(currentYutResult, currentGroup); // 새 기물 활성화
        waitingForUserChoice = false; // 사용자 선택 대기 종료
        addNewPieceButton.gameObject.SetActive(false); // 버튼 비활성화
        DisableOutlines(currentGroup); // 외곽선 비활성화
    }

    public void OnExistingPieceClicked(PlayerMover selectedPiece) {  // 기존 기물 클릭 시 호출
        if (!waitingForUserChoice) return; 
        selectedPiece.MovePlayer(currentYutResult);
        waitingForUserChoice = false;
        if (addNewPieceButton != null) addNewPieceButton.gameObject.SetActive(false); // 버튼 비활성화
        else Debug.LogError("addNewPieceButton이 설정되지 않았습니다!");
        Debug.Log($"Player {currentPlayerIndex}의 기존 기물이 이동되었습니다.");
        DisableOutlines(playerMoverGroups[currentPlayerIndex]); // 외곽선 비활성화
    }

    private void ShowActionSelectionUI(PlayerGroup currentGroup) { // 사용자 선택 UI 표시
        bool canAddNewPiece = currentGroup.movers.Any(m => !m.gameObject.activeSelf);
        if (addNewPieceButton != null) addNewPieceButton.gameObject.SetActive(canAddNewPiece);
        if (!canAddNewPiece) Debug.Log("모든 기물이 활성화되어 새 기물을 추가할 수 없습니다. 기존 기물을 골라 이동하세요.");
        else Debug.Log("새 기물을 추가하거나 기존 기물을 선택하세요.");
    }

    private void ActivateNewPiece(string yutResult, PlayerGroup currentGroup) {
        PlayerMover moverToActivate = currentGroup.movers.FirstOrDefault(m => !m.gameObject.activeSelf);
        if (moverToActivate != null) {
            moverToActivate.gameObject.SetActive(true);
            moverToActivate.ResetToStart();
            moverToActivate.MovePlayer(yutResult);
            Debug.Log($"Player {currentPlayerIndex}의 새로운 기물이 활성화되고 이동되었습니다.");
        } else {
            Debug.LogWarning("활성화 가능한 기물이 더 이상 없습니다!");
            NextPlayer(); // 모든 기물이 활성화된 경우에도 턴 넘김
        }
    }

    public void NextPlayer() {
        currentPlayerIndex = (currentPlayerIndex + 1) % playerMoverGroups.Length; // 다음 플레이어 인덱스로 변경
    }

    [Serializable]
    public class PlayerGroup {
        public PlayerMover[] movers; // 각 플레이어의 PlayerMover 배열
    }
}