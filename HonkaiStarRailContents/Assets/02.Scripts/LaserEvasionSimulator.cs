using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserEvasionSimulator : MonoBehaviour
{
    public bool isPlayerHit { get; private set; }

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
    [SerializeField, Min(1)] private float roundInterval = 2.5f;
    [SerializeField, Min(1)] private int roundCountDown = 3;
    [SerializeField] private int goal1 = 4;
    [SerializeField] private int goal2 = 6;
    [SerializeField] private float fireInterval = 0.25f;
    [SerializeField] private LaserEvasionTurret[] turret;

    private int[][] turretGraph;
    private Coroutine roundRoutineCor;
    private LaserEvasionTurretCollider[] laserCollider;
    private int avoidCount = 0;

    //----------------------------------------------------

    private void OnValidate()
    {
        PrivSettingGoal();
    }

    private void Awake()
    {
        PrivSettingUI();
        PrivSettingTurret();
        PrivSettingHitColliderEvent();
        PrivActiveLaserColliders(false);
    }

    //----------------------------------------------------

    private void PrivSettingHitColliderEvent()
    {
        laserCollider = new LaserEvasionTurretCollider[turret.Length];
        for (int i = 0; i < laserCollider.Length; i++)
        {
            laserCollider[i] = turret[i].GetComponentInChildren<LaserEvasionTurretCollider>();
            laserCollider[i].Simulator = this;
            laserCollider[i].onPlayerHit += PrivProcessHitResult;
        }
    }

    private void PrivProcessHitResult(bool tf)
    {
        isPlayerHit = tf;
        if(isPlayerHit ==  true)
        {
            stateTxt.text = "회피 실패";
        }
        else if(isPlayerHit == false)
        {
            stateTxt.text = "회피 성공";
            avoidCount += 1;
        }
    }

    private void PrivActiveLaserColliders(bool tf)
    {
        foreach(var i in turret)
        {
            i.BoxCollider.gameObject.SetActive(tf);
        }
    }

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
        recordTxt.text = $"0회 회피 성공\n 0라운드 남음";
    }

    private void PrivReset()
    {
        PrivSettingUI();
        PrivResetLaserLine();
        PrivActiveLaserColliders(false);
        avoidCount = 0;
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
            //라운드 시작
            yield return new WaitForSeconds(roundInterval);
            Debug.Log($"{i}라운드 시작");
            curRoundTxt.text = $"{i} Round";

            //발사 카운트다운
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

            //발사
            foreach (var node in path)
            {
                yield return StartCoroutine(PrivFiringLaser(node.start, node.end));
            }
            yield return new WaitForSeconds(1f);
            PrivResetLaserLine();
            PrivActiveLaserColliders(false);

            //라운드 결과
            PrivProcessHitResult(isPlayerHit);
            foreach (var j in laserCollider)
            {
                isPlayerHit = false;
            }
            recordTxt.text = $"{avoidCount}회 회피 성공\n {totalRound - i}라운드 남음";
            if (avoidCount == goal1)
            {
                goal1Txt.text = $"목표1) {goal1}회 회피 O";
            }
            if (avoidCount == goal2)
            {
                goal2Txt.text = $"목표2) {goal2}회 회피 O";
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

    private void PrivResetLaserLine()
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
