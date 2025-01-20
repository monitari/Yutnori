using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerMover : MonoBehaviour {
    public Transform[] pathPoints;
    public int teamID;
    private int currentPointIndex = 0;
    private int score = 0;
    private bool isMoving = false;
    private Vector3 originalPosition;

    private List<GameObject> teamPieces = new List<GameObject>();
    private List<PlayerMover> stackedPieces = new List<PlayerMover>();

    private TurnManager turnManager;

    public Material outlineMaterial;

    private Renderer[] renderers;
    private Material[][] originalMaterials;

    private void Awake() {
        InitializeRenderers();
        InitializeOutlineMaterial();
        turnManager = FindFirstObjectByType<TurnManager>();
    }

    private void InitializeRenderers() {
        try {
            Renderer ownRenderer = GetComponent<Renderer>();
            renderers = ownRenderer != null ? new Renderer[] { ownRenderer } : GetComponentsInChildren<Renderer>();

            if (renderers == null || renderers.Length == 0) {
                Debug.LogError($"[{gameObject.name}] Renderer를 찾을 수 없습니다!");
                return;
            }

            originalMaterials = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++) {
                if (renderers[i] != null && renderers[i].materials != null) {
                    originalMaterials[i] = renderers[i].materials;
                } else {
                    Debug.LogError($"[{gameObject.name}] {i}번째 Renderer 또는 Material이 null입니다!");
                }
            }
            Debug.Log($"[{gameObject.name}] Renderer 초기화 완료: {renderers.Length}개");
        } catch (System.Exception e) {
            Debug.LogError($"[{gameObject.name}] Renderer 초기화 중 오류 발생: {e.Message}");
        }
    }

    private void InitializeOutlineMaterial() {
        if (outlineMaterial == null) {
            outlineMaterial = Resources.Load<Material>("OutlineMaterial");
            if (outlineMaterial == null) {
                Debug.LogError($"[{gameObject.name}] OutlineMaterial을 Resources 폴더에서 찾을 수 없습니다!");
            }
        }
    }

    void Start() {
        if (pathPoints == null || pathPoints.Length == 0) Debug.LogError("PathPoints 배열이 설정되지 않았습니다!");
        if (renderers == null || renderers.Length == 0) InitializeRenderers();
    }

    public void MovePlayer(string yutResult) {
        if (isMoving) return;
        DisableOutline();
        int moveSteps = GetMoveSteps(yutResult);
        StartCoroutine(MoveSteps(moveSteps));
    }

    private IEnumerator MoveSteps(int steps) {
        isMoving = true;
        originalPosition = pathPoints[currentPointIndex].position;
        int remainingSteps = Mathf.Abs(steps);
        int direction = steps >= 0 ? 1 : -1;

        // 윷과 bowl 비활성화
        if (turnManager != null && turnManager.yutThrower != null) {
            turnManager.yutThrower.DisableYuts();
        }

        while (remainingSteps > 0) {
            int nextIndex = GetNextIndex(currentPointIndex, direction);
            if (nextIndex < 0 || nextIndex >= pathPoints.Length) break;
            yield return StartCoroutine(MoveToPosition(nextIndex));
            remainingSteps--;
        }
        isMoving = false;

        HandleCollision();
        if (turnManager != null) {
            turnManager.NextPlayer();
            if (turnManager.throwButton != null) {
                turnManager.throwButton.gameObject.SetActive(true); // 이동이 끝나면 throwButton 활성화
            }
        }
    }

    private int GetNextIndex(int currentIndex, int direction) {
        return direction == -1 ? GetBackwardIndex(currentIndex) : GetForwardIndex(currentIndex);
    }

    private int GetBackwardIndex(int currentIndex) {
        switch (currentIndex) {
            case 0: return 0;
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
        endPos.y = 2;
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 centerPos = (startPos + endPos) * 0.5f + Vector3.up * 2f;

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

        yield return new WaitForSeconds(0.1f);
    }

    private void HandleCollision() {
        var piecesOnSameTile = FindObjectsByType<PlayerMover>(FindObjectsSortMode.None)
            .Where(p => p.currentPointIndex == currentPointIndex && p != this);

        foreach (var piece in piecesOnSameTile) {
            if (!IsSameTeam(piece)) {
                Debug.Log($"상대 팀 말 잡음! 위치: {currentPointIndex}");
                piece.gameObject.SetActive(false);
                score += 1;
            } else {
                StackPiece(piece);
                Debug.Log($"팀 말 합체: {currentPointIndex}");
            }
        }
    }

    private bool IsSameTeam(PlayerMover other) {
        return this.teamID == other.teamID;
    }

    private void StackPiece(PlayerMover piece) {
        if (!stackedPieces.Contains(piece)) {
            stackedPieces.Add(piece);
            piece.transform.SetParent(this.transform);
            piece.transform.localScale *= 1.1f;
        }
    }

    public void ResetToStart() {
        currentPointIndex = 0;
        Vector3 startPosition = pathPoints[0].position;
        startPosition.y = 2;
        transform.position = startPosition;

        foreach (var piece in stackedPieces) {
            piece.transform.SetParent(null);
            piece.transform.localScale /= 1.1f;
            piece.gameObject.SetActive(false);
        }
        stackedPieces.Clear();
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

    void OnMouseDown() {
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null) turnManager.OnExistingPieceClicked(this);
        else Debug.LogError("TurnManager를 찾을 수 없습니다!");
    }

    public void EnableOutline() {
        if (outlineMaterial == null) {
            Debug.LogError($"[{gameObject.name}] OutlineMaterial이 할당되지 않았습니다!");
            return;
        }
        if (renderers == null || renderers.Length == 0) {
            Debug.LogError($"[{gameObject.name}] Renderers가 초기화되지 않았습니다!");
            InitializeRenderers();
            if (renderers == null || renderers.Length == 0) return;
        }
        foreach (Renderer renderer in renderers) {
            if (renderer != null) {
                List<Material> mats = renderer.materials.ToList();
                if (!mats.Contains(outlineMaterial)) {
                    mats.Add(outlineMaterial);
                    renderer.materials = mats.ToArray();
                }
            }
        }
        Debug.Log($"[{gameObject.name}] Outline enabled");
    }

    public void DisableOutline() {
        if (renderers == null || originalMaterials == null) {
            Debug.LogError($"[{gameObject.name}] Renderer 또는 OriginalMaterials가 초기화되지 않았습니다!");
            InitializeRenderers();
            if (renderers == null || originalMaterials == null) return;
        }
        for (int i = 0; i < renderers.Length; i++) {
            if (renderers[i] != null && originalMaterials[i] != null) {
                renderers[i].materials = originalMaterials[i];
            }
        }
        Debug.Log($"[{gameObject.name}] Outline disabled");
    }
}
