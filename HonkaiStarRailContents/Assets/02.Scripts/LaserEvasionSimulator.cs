using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LaserEvasionSimulator : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField, Min(1)] private int totalRound = 6;
    [SerializeField, Min(1)] private float roundDelay = 2.5f;
    [SerializeField, Min(1)] private int roundCountDown = 3;
    [SerializeField, Min(1)] private int goal1 = 4;
    [SerializeField, Min(1)] private int goal2 = 6;
    [SerializeField, Min(1)] private float fireDelay = 0.25f;
    [SerializeField] private LaserEvasionTurret[] turret;

    [Header("UI")]
    [SerializeField] private UIWidgetScreen UIWidgetScreen;

    private int[][] turretGraphAry;
    private Coroutine roundRoutineCor;
    private bool isHit = false;
    private int avoidCount = 0;
    List<(int start, int end)> laserPathList = new List<(int , int )>();
    Queue<int> turretQ = new Queue<int>();
    int[] visitedAry = null;
    List<int> availableNodeList = new List<int>();

    WaitForSeconds wait_1Second = new WaitForSeconds(1f);
    WaitForSeconds wait_RoundDelay;
    WaitForSeconds wait_FireDelay;

    //----------------------------------------------------

    private void Awake()
    {
        turretGraphAry = new int[turret.Length][];
        visitedAry = new int[turret.Length];
        wait_RoundDelay = new WaitForSeconds(roundDelay);
        wait_FireDelay = new WaitForSeconds(fireDelay);

        PrivResetUI();
        PrivSettingTurretGraph();
        PrivSettingHitColliderEvent();
    }

    //----------------------------------------------------
    private void PrivResetUI()
    {
        UIWidgetScreen.StartBtn.onClick.AddListener(OnClickBtnStartRound);
        UIWidgetScreen.StartBtn.gameObject.SetActive(true);
        UIWidgetScreen.ResetBtn.onClick.AddListener(OnClickBtnReset);
        UIWidgetScreen.ResetBtn.gameObject.SetActive(false);
        UIWidgetScreen.CurRoundTxt.text = $"0 Round";
        UIWidgetScreen.StateTxt.text = $"";
        UIWidgetScreen.Goal1Txt.text = $"목표1) {goal1}회 회피";
        UIWidgetScreen.Goal2Txt.text = $"목표2) {goal2}회 회피";
        UIWidgetScreen.RecordTxt.text = $"0회 회피 성공\n 0라운드 남음";
    }

    private void PrivSettingTurretGraph()
    {
        int n = turret.Length;

        for (int i = 0; i < n; i++)
        {
            turretGraphAry[i] = new int[n - 3];
            for (int j = 0; j < n - 3; j++)
                turretGraphAry[i][j] = (i + j + 2) % n;
        }
    }

    private void PrivSettingHitColliderEvent()
    {
        for (int i = 0; i < turret.Length; i++)
        {
            var collider = turret[i].GetCollider();
            collider.onPlayerHit += PrivFailAvoid;
        }
    }

    private void PrivFailAvoid()
    {
        UIWidgetScreen.StateTxt.text = "회피 실패";
        isHit = false;
    }

    #region ==================Round Routine=================================
    private IEnumerator RoundRoutine()
    {
        for (int i = 1; i <= totalRound; i++)
        {
            //라운드 시작
            yield return wait_RoundDelay;
            PrivRoundStart(i);

            //경로 지정 및 안내
            PrivSettingLaserPath(i + 1);
            foreach (var node in laserPathList)
                PrivDrawPathLine(node.start, node.end);

            //카운트 다운
            for (int j = roundCountDown; j > 0; j--)
            {
                UIWidgetScreen.StateTxt.text = $"{j}";
                yield return wait_1Second;
            }
            UIWidgetScreen.StateTxt.text = "";

            //발사
            foreach (var node in laserPathList)
            {
                PrivFiringLaser(node.start, node.end);
                yield return wait_FireDelay;
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
            avoidCount += 1;
        }
        UIWidgetScreen.RecordTxt.text = $"{avoidCount}회 회피 성공\n {totalRound - currentRound}라운드 남음";
        if (avoidCount == goal1)
            UIWidgetScreen.Goal1Txt.text = $"목표1) {goal1}회 회피 O";
        if (avoidCount == goal2)
            UIWidgetScreen.Goal2Txt.text = $"목표2) {goal2}회 회피 O";
    }
    #endregion

    private void PrivSettingLaserPath(int maxCount)
    {
        laserPathList.Clear();
        turretQ.Clear();
        for (int i = 0; i < turret.Length; i++)
            visitedAry[i] = 0;

        int runCount = 0;
        int start = UnityEngine.Random.Range(0, turret.Length);

        turretQ.Enqueue(start);
        visitedAry[start] = 1;

        while (turretQ.Count > 0)
        {
            availableNodeList.Clear();

            int curNode = turretQ.Dequeue();
            // 아직 방문 안 한 노드만 후보로
            foreach (int nextNode in turretGraphAry[curNode])
            {
                if (visitedAry[nextNode] == 0)
                    availableNodeList.Add(nextNode);
            }

            // 연결할 곳이 없거나 레이저상한에 걸림
            if (availableNodeList.Count == 0 || runCount == maxCount)
            {
                Debug.Log("경로 계산 완료");
                break;
            }

            int next = availableNodeList[UnityEngine.Random.Range(0, availableNodeList.Count)];
            turretQ.Enqueue(next);
            visitedAry[next] = 1;

            laserPathList.Add((curNode, next));
            runCount += 1;
        }
    }
    private void PrivDrawPathLine(int start, int end)
    {
        Vector3 startPos = turret[start].transform.position;
        Vector3 endPos = turret[end].transform.position;

        //전체 거리의 30%지점 구하기(벡터 러프함수 있음) 
        Vector3 previewEndPos = startPos + (endPos - startPos) * 0.3f;

        turret[start].DoSettingLineRenderer(0.5f, Color.red, startPos, previewEndPos);
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
        PrivResetUI();
        PrivTurretReset();
        avoidCount = 0;
    }
}
