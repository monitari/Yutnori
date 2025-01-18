using UnityEngine;

public class YutThrower : MonoBehaviour
{
    // 윷 오브젝트들 (4개)
    public Rigidbody[] yutRigidbodies;

    // 던지기 힘과 회전 토크 설정
    public float throwForce = 10f; // 던지기 힘
    public float torqueForce = 5f; // 회전 힘

    // 모든 윷을 던지는 함수
    public void ThrowAllYuts(Vector3 direction)
    {
        foreach (Rigidbody yut in yutRigidbodies)
        {
            yut.isKinematic = false;
            // 던지기 힘 추가
            yut.AddForce(direction * throwForce, ForceMode.Impulse);

            // 랜덤 회전 토크 추가
            Vector3 randomTorque = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * torqueForce;

            yut.AddTorque(randomTorque, ForceMode.Impulse);
        }
    }

    // 윷 결과를 판정하는 함수
    public void CheckYutResults()
    {
        int backSideCount = 0; // '등(뒷면)'이 위로 향한 윷 개수

        foreach (Rigidbody yut in yutRigidbodies)
        {
            Vector3 up = yut.transform.up;

            // 뒷면이 위를 향한 경우
            if (up.y > 0.9f)
            {
                backSideCount++;
            }
        }

        // 결과를 출력
        switch (backSideCount)
        {
            case 1:
                Debug.Log("윷 결과: 도");
                break;
            case 2:
                Debug.Log("윷 결과: 개");
                break;
            case 3:
                Debug.Log("윷 결과: 걸");
                break;
            case 4:
                Debug.Log("윷 결과: 윷");
                break;
            case 0:
                Debug.Log("윷 결과: 모");
                break;
            default:
                Debug.LogError("예상치 못한 결과!");
                break;
        }
    }

    // 모든 윷이 멈췄는지 확인
    void Update()
    {
        bool allStopped = true;

        foreach (Rigidbody yut in yutRigidbodies)
        {
            if (yut.linearVelocity.magnitude > 0.1f || yut.angularVelocity.magnitude > 0.1f)
            {
                allStopped = false;
                break;
            }
        }

        // 윷이 모두 멈췄다면 결과 판정
        if (allStopped)
        {
            CheckYutResults();
        }
    }
}
