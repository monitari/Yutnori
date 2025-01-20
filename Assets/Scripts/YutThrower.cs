using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class YutThrower : MonoBehaviour {
    // 윷 오브젝트들 (4개)
    public Rigidbody[] yutRigidbodies;

    // 던지기 힘과 회전 토크 설정
    public float throwForce = 100f; // 던지기 힘 더 강하게
    public float torqueForce = 30f; // 회전 힘 더 강하게

    public Text resultText; // UI 텍스트를 연결할 필드
    public Animator resultTextAnimator; // UI 텍스트 애니메이터를 연결할 필드
    public GameObject bowl; // Bowl 오브젝트를 연결할 필드

    public delegate void YutResultHandler(string result);
    public event YutResultHandler OnYutResult;

    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;

    private bool resultChecked = false;
    private bool isChecking = false;
    private float throwStartTime;

    void Start() {
        if (yutRigidbodies == null || yutRigidbodies.Length == 0) {
            Debug.LogError("YutRigidbodies 배열이 초기화되지 않았습니다!");
            return;
        }

        InitializeYuts();
        DisableYuts(); // 윷 비활성화

        if (resultText != null) resultText.text = ""; // 시작할 때 결과 텍스트 초기화
        if (bowl != null) bowl.SetActive(false); // 시작할 때 Bowl 오브젝트 비활성화
    }

    private void InitializeYuts() {
        initialPositions = new Vector3[yutRigidbodies.Length];
        initialRotations = new Quaternion[yutRigidbodies.Length];

        for (int i = 0; i < yutRigidbodies.Length; i++) {
            initialPositions[i] = yutRigidbodies[i].transform.position;
            initialRotations[i] = yutRigidbodies[i].transform.rotation;
        }

        foreach (Rigidbody yut in yutRigidbodies) {
            yut.useGravity = true;
            if (yut.GetComponent<ConstantForce>() == null) {
                ConstantForce cf = yut.gameObject.AddComponent<ConstantForce>();
                cf.force = Vector3.down * 9.81f; // 추가적인 중력 값 적용
            }
        }
    }

    public void ResetAllYuts() {
        for (int i = 0; i < yutRigidbodies.Length; i++) {
            ResetYut(yutRigidbodies[i], initialPositions[i], initialRotations[i]);
        }
        resultChecked = false;

        if (resultText != null) resultText.text = ""; // 윷을 리셋할 때 결과 텍스트 초기화
    }

    private void ResetYut(Rigidbody yut, Vector3 position, Quaternion rotation) {
        yut.transform.position = position;
        yut.transform.rotation = rotation;
        yut.linearVelocity = Vector3.zero;
        yut.angularVelocity = Vector3.zero;
        yut.isKinematic = true;
    }

    // 윷들의 초기 위치를 섞는 함수
    public void ShuffleYutPositions() {
        for (int i = 0; i < initialPositions.Length; i++) {
            int randomIndex = Random.Range(0, initialPositions.Length);
            Swap(ref initialPositions[i], ref initialPositions[randomIndex]);
            Swap(ref initialRotations[i], ref initialRotations[randomIndex]);
        }
    }

    private void Swap<T>(ref T a, ref T b) {
        T temp = a;
        a = b;
        b = temp;
    }

    // 모든 윷을 던지는 함수
    public void ThrowAllYuts(Vector3 direction) {
        ShuffleYutPositions(); // 윷들의 초기 위치를 섞음
        throwStartTime = Time.time; // 던진 시간 기록

        EnableYuts(); // 윷 활성화

        foreach (Rigidbody yut in yutRigidbodies) {
            yut.isKinematic = false;
            AddThrowForce(yut, direction);
        }
    }

    private void AddThrowForce(Rigidbody yut, Vector3 direction) {
        // 던지기 힘 추가
        Vector3 randomDir = direction + new Vector3(Random.Range(-0.1f, 0.1f), 0f, 0f);
        yut.AddForce(randomDir.normalized * throwForce, ForceMode.VelocityChange);

        // 랜덤 회전 토크 추가
        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * torqueForce;

        yut.AddTorque(randomTorque, ForceMode.VelocityChange);
    }

    // 윷 결과를 판정하는 함수
    public void CheckYutResults() {
        float angleX0 = yutRigidbodies[0].transform.eulerAngles.x;
        bool isYut0Back = Mathf.Abs(Mathf.DeltaAngle(angleX0, 90f)) < 90f;

        int backSideCount = 0; // '등(뒷면)'이 위로 향한 윷 개수

        foreach (Rigidbody yut in yutRigidbodies) {
            float angleX = yut.transform.eulerAngles.x;
            if (Mathf.Abs(Mathf.DeltaAngle(angleX, 90f)) < 90f) // '등(뒷면)'이 위로 향한 경우
                backSideCount++;
        }

        string result = "";
        // 결과를 출력
        switch (backSideCount) {
            case 1:
                result = isYut0Back ? "빽도" : "도";
                break;
            case 2:
                result = "개";
                break;
            case 3:
                result = "걸";
                break;
            case 4:
                result = "윷";
                break;
            case 0:
                result = "모";
                break;
            default:
                result = "예상치 못한 결과!";
                break;
        }

        Debug.Log("윷 결과: " + result);
        if (resultText != null) {
            resultText.text = result + "!"; // 결과를 UI 텍스트에 표시
            StartCoroutine(ShowResultText()); // 결과 텍스트를 애니메이션과 함께 표시
        }
        OnYutResult?.Invoke(result); // 결과 이벤트 호출
    }

    private IEnumerator ShowResultText() {
        if (resultTextAnimator != null) {
            resultTextAnimator.SetTrigger("Show");
        }
        yield return new WaitForSeconds(1f);
        if (resultText != null) resultText.text = ""; // 결과 텍스트 초기화
    }

    void Update() {
        if (throwStartTime <= 0f || resultChecked) return; // 던지기 전이면 아무것도 하지 않음
        if (AllYutsStopped() && !isChecking) StartCoroutine(WaitAndCheckResults()); // 모든 윷이 멈추면 결과 판정
        if (Time.time - throwStartTime > 5f && !resultChecked) { // 5초가 지나도 결과가 나오지 않으면 다시 던짐
            Debug.Log("윷 결과가 나오지 않아 다시 던집니다.");
            ResetAllYuts();
            ThrowAllYuts(Vector3.up);
        }
    }

    private bool AllYutsStopped() {
        foreach (Rigidbody yut in yutRigidbodies) { // 모든 윷이 멈췄는지 확인
            if (yut.linearVelocity.magnitude > 0.15f || yut.angularVelocity.magnitude > 0.15f) return false; // 윷이 멈추지 않았으면 false 반환
        }
        return true;
    }

    private IEnumerator WaitAndCheckResults() {
        isChecking = true;
        yield return new WaitForSeconds(0.5f);
        bool allStillStopped = true;

        foreach (Rigidbody yut in yutRigidbodies) { // 모든 윷이 멈췄는지 확인
            if (yut.linearVelocity.magnitude > 0.15f || yut.angularVelocity.magnitude > 0.15f) { 
                allStillStopped = false;
                break;
            }
        }

        if (allStillStopped) {
            CheckYutResults();
            resultChecked = true;
            enabled = false; // 결과가 나오면 스크립트 비활성화
        }
        isChecking = false;
    }

    // 윷 비활성화 함수
    public void DisableYuts() {
        foreach (Rigidbody yut in yutRigidbodies) yut.gameObject.SetActive(false); // 윷 비활성화
        if (bowl != null) bowl.SetActive(false); // Bowl 오브젝트 비활성화
    }

    // 윷 활성화 함수
    public void EnableYuts() {
        foreach (Rigidbody yut in yutRigidbodies) yut.gameObject.SetActive(true); // 윷 활성화
        if (bowl != null) bowl.SetActive(true); // Bowl 오브젝트 활성화
    }
}
