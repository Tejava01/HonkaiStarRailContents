using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class LaserEvasionSimulator : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Min(1)] private int totalRound = 6;
    [SerializeField, Min(1)] private float roundInterval = 2.5f;
    [SerializeField, Min(1)] private int roundCountDown = 3;

    [SerializeField] private int goal1 = 4;
    [SerializeField] private int goal2 = 6;

    [SerializeField] private float fireInterval = 0.25f;
    [SerializeField] private LaserEvasionTurret[] turret;

    [Header("UI")]
    [SerializeField] private UIWidgetScreen UIWidgetScreen;

    private int[][] turretGraph;
    private Coroutine roundRoutineCor;

    private bool isHit = false;
    private int avoidCount = 0;

    List<(int start, int end)> path = new List<(int start, int end)>();
    Queue<int> turretQ = new Queue<int>();

    List<int> currentAvailable = new List<int>();
    int[] visitedAry = null;

    WaitForSeconds wait_1Second = new WaitForSeconds(1f);
    WaitForSeconds wait_RoundInterval;
    WaitForSeconds wait_FireInterval;

    //----------------------------------------------------
    private void OnValidate()
    {
        goal1 = Mathf.Clamp(goal1, 1, totalRound);
        goal2 = Mathf.Clamp(goal2, 1, totalRound);
    }

    private void Awake()
    {
        visitedAry = new int[turret.Length];
        turretGraph = new int[turret.Length][];
        wait_RoundInterval = new WaitForSeconds(roundInterval);
        wait_FireInterval = new WaitForSeconds(fireInterval);

        PrivUIReset();
        PrivTurretSetting();
        PrivSettingHitColliderEvent();
    }

    //----------------------------------------------------
    private void PrivUIReset()
    {
        UIWidgetScreen.StartBtn.onClick.AddListener(OnClickBtnStartRound);
        UIWidgetScreen.StartBtn.gameObject.SetActive(true);
        UIWidgetScreen.ResetBtn.onClick.AddListener(OnClickBtnReset);
        UIWidgetScreen.ResetBtn.gameObject.SetActive(false);
        UIWidgetScreen.StateTxt.text = $"";
        UIWidgetScreen.CurRoundTxt.text = $"0 Round";
        UIWidgetScreen.Goal1Txt.text = $"목표1) {goal1}회 회피";
        UIWidgetScreen.Goal2Txt.text = $"목표2) {goal2}회 회피";
        UIWidgetScreen.RecordTxt.text = $"0회 회피 성공\n 0라운드 남음";
    }

    private void PrivSettingHitColliderEvent()
    {
        for (int i = 0; i < turret.Length; i++)
        {
            var collider = turret[i].GetCollider();
            collider.onPlayerHit += PrivProcessHitResult;
        }
    }

    private void PrivTurretSetting()
    {
        int n = turret.Length;

        for (int i = 0; i < n; i++)
        {
            turretGraph[i] = new int[n - 3];
            for (int j = 0; j < n - 3; j++)
            {
                turretGraph[i][j] = (i + j + 2) % n;
            }
        }
    }

    private void PrivProcessHitResult(bool tf)
    {
        if(tf ==  true)
            UIWidgetScreen.StateTxt.text = "회피 실패";

        isHit = tf;
    }

    #region ==================Round Routine=================================
    private IEnumerator RoundRoutine()
    {
        for (int i = 1; i <= totalRound; i++)
        {
            //라운드 시작
            yield return wait_RoundInterval;
            PrivRoundStart(i);

            //경로 지정
            PrivSettingLaserPath(i + 1);
            foreach (var node in path)
            {
                PrivDrawPathLine(node.start, node.end);
            }

            //카운트 다운
            for (int j = roundCountDown; j > 0; j--)
            {
                UIWidgetScreen.StateTxt.text = $"{j}";
                yield return wait_1Second;
            }
            UIWidgetScreen.StateTxt.text = "";

            //발사
            foreach (var node in path)
            {
                PrivFiringLaser(node.start, node.end);
                yield return wait_FireInterval;
            }
            yield return wait_1Second;

            //라운드 결과
            PrivCheckRoundResult(i);

            PrivTurretReset();
        }

        Debug.Log("종료");
        UIWidgetScreen.ResetBtn.gameObject.SetActive(true);
    }

    private void PrivRoundStart(int currentRound)
    {
        Debug.Log($"{currentRound}라운드 시작");
        UIWidgetScreen.CurRoundTxt.text = $"{currentRound} Round";
    }

    private void PrivCheckRoundResult(int currentRound)
    {
        if (isHit == false)
        {
            UIWidgetScreen.StateTxt.text = "회피 성공";
            avoidCount++;
        }

        UIWidgetScreen.RecordTxt.text = $"{avoidCount}회 회피 성공\n {totalRound - currentRound}라운드 남음";
        if (avoidCount == goal1)
        {
            UIWidgetScreen.Goal1Txt.text = $"목표1) {goal1}회 회피 O";
        }
        if (avoidCount == goal2)
        {
            UIWidgetScreen.Goal2Txt.text = $"목표2) {goal2}회 회피 O";
        }
    }
    #endregion

    private void PrivSettingLaserPath(int maxCount)
    {
        path.Clear();
        turretQ.Clear();
        for (int i = 0; i < visitedAry.Length; i++)
            visitedAry[i] = 0;

        int runCount = 0;
        int n = turret.Length;
        int start = UnityEngine.Random.Range(0, n);

        turretQ.Enqueue(start);
        visitedAry[start] = 1;

        while (turretQ.Count > 0)
        {
            currentAvailable.Clear();

            int curNode = turretQ.Dequeue();

            foreach (int node in turretGraph[curNode])
            {
                if (visitedAry[node] == 0)
                    currentAvailable.Add(node);
            }

            // 연결할 곳이 없거나 레이저상한에 걸림
            if (currentAvailable.Count == 0 || runCount == maxCount)
            {
                Debug.Log("경로 계산 완료");
                break;
            }

            int next = UnityEngine.Random.Range(0, currentAvailable.Count);
            turretQ.Enqueue(currentAvailable[next]);
            visitedAry[currentAvailable[next]] = 1; 

            path.Add((curNode, currentAvailable[next]));
            runCount += 1;
        }
    }

    private void PrivFiringLaser(int start, int end)
    {
        Debug.Log($"{start}번에서 {end}번 연결");

        turret[start].DoSettingLineRenderer(1f, Color.green, turret[start].gameObject.transform.position, turret[end].gameObject.transform.position);
        PrivSettingBoxCollider(turret[start]);
    }

    private void PrivSettingBoxCollider(LaserEvasionTurret turret)
    {
        LineRenderer line = turret.GetComponent<LineRenderer>();
        Vector3 start = line.GetPosition(0);
        Vector3 end = line.GetPosition(1);
        Vector3 center = (start + end) / 2f;
        Vector3 direction = end - start;
        float length = direction.magnitude;

        // 크기 설정 (x: 두께, y: 높이, z: 길이)
        turret.BoxCollider.size = new Vector3(1f, 1f, length);
        // 위치 및 회전 설정
        turret.BoxCollider.gameObject.transform.position = center;
        turret.BoxCollider.gameObject.transform.rotation = Quaternion.LookRotation(direction.normalized);
        turret.BoxCollider.gameObject.SetActive(true);
    }

    private void PrivDrawPathLine(int start, int end)
    {
        Vector3 startPos = turret[start].transform.position;
        Vector3 endPos = turret[end].transform.position;

        //전체 거리의 30%지점 구하기(벡터 러프함수 있음) 
        Vector3 previewEndPos = startPos + (endPos - startPos) * 0.3f;

        turret[start].DoSettingLineRenderer(0.5f, Color.red, startPos, previewEndPos);
    }

    //---------------------------------------------------
    private void PrivTurretReset()
    {
        foreach (LaserEvasionTurret turret in turret)
            turret.DoTurretReset();
    }

    //-------------------------------------------------------
    private void OnClickBtnStartRound()
    {
        UIWidgetScreen.StartBtn.gameObject.SetActive(false);

        //코루틴 중복방지
        if (roundRoutineCor != null)
            StopCoroutine(roundRoutineCor);

        roundRoutineCor = StartCoroutine(RoundRoutine());
    }

    private void OnClickBtnReset()
    {
        PrivUIReset();
        PrivTurretReset();
        avoidCount = 0;
    }
}
