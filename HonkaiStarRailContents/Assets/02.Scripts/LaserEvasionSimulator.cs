using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserEvasionSimulator : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button startBtn;
    [SerializeField] private Button resetBtn;
    [SerializeField] private Text curRoundTxt;
    [SerializeField] private Text stateTxt;
    [SerializeField] private Text goal1Txt;
    [SerializeField] private Text goal2Txt;
    [SerializeField] private Text recordTxt;

    [Header("Setting")]
    [SerializeField, Min(1)] private int totalRound = 6;
    [SerializeField, Min(1)] private int roundInterval = 2;
    [SerializeField, Min(1)] private int roundCountDown = 3;
    [SerializeField] private int goal1 = 4;
    [SerializeField] private int goal2 = 6;
    [SerializeField] private float fireInterval = 0.25f;
    [SerializeField] private LaserEvasionTurret[] turret;

    private int[][] turretGraph;
    private Coroutine roundRoutineCor;

    //----------------------------------------------------

    private void OnValidate()
    {
        PrivSettingGoal();
    }

    private void Awake()
    {
        PrivSettingUI();
        PrivSettingTurret();
    }

    //----------------------------------------------------

    private void PrivSettingUI()
    {
        startBtn.onClick.AddListener(PrivStartRound);
        startBtn.gameObject.SetActive(true);
        resetBtn.onClick.AddListener(PrivReset);
        resetBtn.gameObject.SetActive(false);
        stateTxt.text = $"";
        curRoundTxt.text = $"0 Round";
        goal1Txt.text = $"목표1) {goal1}회 회피";
        goal2Txt.text = $"목표2) {goal2}회 회피";
    }

    private void PrivReset()
    {
        PrivSettingUI();
        PrivResetLaser();
    }

    private void PrivStartRound()
    {
        startBtn.gameObject.SetActive(false);
        //코루틴 중복방지
        if (roundRoutineCor != null)
        {
            StopCoroutine(roundRoutineCor);
        }
        roundRoutineCor = StartCoroutine(RoundRoutine());
    }

    private IEnumerator RoundRoutine()
    {
        for (int i = 1; i <= totalRound; i++)
        {
            yield return new WaitForSeconds(roundInterval);
            Debug.Log($"{i}번째 라운드 시작");
            curRoundTxt.text = $"{i} Round";

            PrivResetLaser();
            List<(int start, int end)> path = PrivSettingLaserPath(i + 1);
            foreach (var node in path)
            {
                PrivDrawPathLine(node.start, node.end);
            }
            for (int j = roundCountDown; j > 0; j--)
            {
                stateTxt.text = $"{j}";
                yield return new WaitForSeconds(1f);
            }
            stateTxt.text = "";

            foreach (var node in path)
            {
                yield return StartCoroutine(PrivFiringLaser(node.start, node.end));
            }
        }

        Debug.Log("종료");
        resetBtn.gameObject.SetActive(true);
    }

    private void PrivSettingGoal()
    {
        goal1 = Mathf.Clamp(goal1, 1, totalRound);
        goal2 = Mathf.Clamp(goal2, 1, totalRound);
    }

    private void PrivSettingTurret()
    {
        int n = turret.Length;
        turretGraph = new int[n][];

        for (int i = 0; i < n; i++)
        {
            turretGraph[i] = new int[n-3];
            for (int j = 0; j < n-3; j++)
            {
                turretGraph[i][j] = (i + j + 2) % n;
            }
        }
    }

    private void PrivResetLaser()
    {
        foreach (LaserEvasionTurret turret in turret)
        {
            PrivSettingLineRenderer(turret, 0f, Color.clear, Vector3.zero, Vector3.zero);
        }
    }

    private List<(int, int)> PrivSettingLaserPath(int maxCount)
    {
        List<(int start, int end)> path = new List<(int, int)>();
        int runCount = 0;
        int n = turret.Length;
        Queue<int> turretQ = new Queue<int>();
        int[] turretVisited = new int[n];
        int start = UnityEngine.Random.Range(0, n);
        turretQ.Enqueue(start);
        turretVisited[start] = 1;

        while (turretQ.Count > 0)
        {
            int curNode = turretQ.Dequeue();

            // 아직 방문 안 한 노드만 후보로
            List<int> canVisit = new List<int>();
            foreach (int node in turretGraph[curNode])
            {
                if (turretVisited[node] == 0)
                    canVisit.Add(node);
            }

            // 연결할 곳이 없거나 레이저상한에 걸림
            if (canVisit.Count == 0 || runCount == maxCount)
            {
                Debug.Log("경로 계산 완료");
                break;
            }

            int next = canVisit[UnityEngine.Random.Range(0, canVisit.Count)];
            turretQ.Enqueue(next);
            turretVisited[next] = 1;

            path.Add((curNode, next));
            runCount += 1;
        }

        return path;
    }

    private IEnumerator PrivFiringLaser(int start, int end)
    {
        yield return new WaitForSeconds(fireInterval);

        Debug.Log($"{start}번에서 {end}번 연결");
        PrivSettingLineRenderer(turret[start], 1f, Color.green, turret[start].gameObject.transform.position, turret[end].gameObject.transform.position);
        //TODO 캡슐콜라이더 동적관리하기
    }

    private void PrivDrawPathLine(int start, int end)
    {
        Vector3 startPos = turret[start].transform.position;
        Vector3 endPos = turret[end].transform.position;

        //전체 거리의 30%지점 구하기
        Vector3 previewEndPos = startPos + (endPos - startPos) * 0.3f;

        PrivSettingLineRenderer(turret[start], 0.5f, Color.red, startPos, previewEndPos);
    }

    private void PrivSettingLineRenderer(LaserEvasionTurret turret, float width, Color color, Vector3 start, Vector3 end)
    {
        LineRenderer line = turret.GetComponent<LineRenderer>();
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = color;
        line.endColor = color;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}
