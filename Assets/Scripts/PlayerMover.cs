using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerMover : MonoBehaviour {
    public Transform[] pathPoints; // 이동 경로 포인트들
    private int currentPointIndex = 0;
    private int score = 0; // 점수 관리
    private bool isMoving = false; // 이동 중 여부
    private Vector3 originalPosition; // 이동 전 가장 처음 위치

    private List<GameObject> teamPieces = new List<GameObject>(); // 같은 팀 말 관리

    void Start() {
        if (pathPoints == null || pathPoints.Length == 0) Debug.LogError("PathPoints 배열이 설정되지 않았습니다!");
    }

    public void MovePlayer(string yutResult) {
        if (isMoving) return;
        int moveSteps = GetMoveSteps(yutResult);
        StartCoroutine(MoveSteps(moveSteps));
    }

    private IEnumerator MoveSteps(int steps) {
        isMoving = true;
        originalPosition = pathPoints[currentPointIndex].position;
        int remainingSteps = Mathf.Abs(steps);
        int direction = steps >= 0 ? 1 : -1;

        while (remainingSteps > 0) {
            int nextIndex = GetNextIndex(currentPointIndex, direction);
            if (nextIndex < 0 || nextIndex >= pathPoints.Length) break; // 시작 지점 이전이나 경로를 벗어나는 이동 금지
            yield return StartCoroutine(MoveToPosition(nextIndex));
            remainingSteps--;
        }
        isMoving = false;
    }

    private int GetNextIndex(int currentIndex, int direction) {
        return direction == -1 ? GetBackwardIndex(currentIndex) : GetForwardIndex(currentIndex);
    }

    private int GetBackwardIndex(int currentIndex) {
        switch (currentIndex) {
            case 0: return 19;
            case 15: return 28;
            case 28: return 27;
            case 27: return 20;
            case 20: return 24;
            case 24: return 23;
            case 23: return 5;
            case 22: return 21;
            case 21: return 20;
            case 26: return 25;
            case 25: return 10;
            default: return currentIndex - 1 < 0 ? pathPoints.Length - 1 : currentIndex - 1;
        }
    }

    private int GetForwardIndex(int currentIndex) {
        switch (currentIndex) {
            case 5: return ShouldMoveTo(5, 23, 6);
            case 10: return ShouldMoveTo(10, 25, 11);
            case 20: return ShouldMoveTo(20, 21, 27);
            case 23: return 24;
            case 24: return 20;
            case 27: return 28;
            case 28: return 15;
            case 25: return 26;
            case 26: return 20;
            case 21: return 22;
            case 22: return 0;
            case 19: return 0;
            default: return currentIndex + 1 >= pathPoints.Length ? 0 : currentIndex + 1;
        }
    }

    private int ShouldMoveTo(int checkIndex, int trueIndex, int falseIndex) {
        return originalPosition == pathPoints[checkIndex].position || !isMoving ? trueIndex : falseIndex;
    }

    private IEnumerator MoveToPosition(int targetIndex) {
        if (targetIndex < 0 || targetIndex >= pathPoints.Length) {
            Debug.LogError($"잘못된 이동 위치: {targetIndex}");
            yield break;
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = pathPoints[targetIndex].position;
        endPos.y = 2; // y 포지션을 기본값 2로 설정
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 centerPos = (startPos + endPos) * 0.5f + Vector3.up * 2f; // 점프 효과

        while (elapsed < duration) {
            float t = elapsed / duration;
            Vector3 m1 = Vector3.Lerp(startPos, centerPos, t);
            Vector3 m2 = Vector3.Lerp(centerPos, endPos, t);
            transform.position = Vector3.Lerp(m1, m2, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        currentPointIndex = targetIndex;

        HandleCollision(); // 같은 칸에 다른 팀 말이 있으면 처리
    }

    private void HandleCollision() { // 충돌 처리: 같은 칸에 상대 말이 있으면 잡기
        var piecesOnSameTile = FindObjectsByType<PlayerMover>(FindObjectsSortMode.None).Where(p => p.currentPointIndex == currentPointIndex && p != this);

        foreach (var piece in piecesOnSameTile) {
            if (!teamPieces.Contains(piece.gameObject)) {// 상대 팀 말 잡기
                Debug.Log($"상대 팀 말 잡음! 위치: {currentPointIndex}");
                piece.ResetToStart();
                score += 1;
            } 
            else Debug.Log($"팀 말 합체: {currentPointIndex}");
        }
    }

    private void ResetToStart() {
        currentPointIndex = 0;
        Vector3 startPosition = pathPoints[0].position;
        startPosition.y = 2; // y 포지션을 기본값 2로 설정
        transform.position = startPosition;
    }

    public int GetMoveSteps(string yutResult) {
        switch (yutResult) {
            case "빽도": return -1;
            case "도": return 1;
            case "개": return 2;
            case "걸": return 3;
            case "윷": return 4;
            case "모": return 5;
            default: return 0;
        }
    }
}
