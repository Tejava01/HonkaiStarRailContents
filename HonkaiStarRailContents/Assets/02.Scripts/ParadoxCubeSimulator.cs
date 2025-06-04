using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParadoxCubeSimulator : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Transform coreBlock;
    [SerializeField] private List<ParadoxCubeRay> rayPivot = new List<ParadoxCubeRay>();

    private List<GameObject> selectList = new List<GameObject>();
    private bool isRotate = false;
    Quaternion pivotRot;
    Vector3 rayDir;
    //-----------------------------------------------------

    private IEnumerator CorRotatePattern()
    {
        isRotate = true;

        //TODO 하 이방법말고 있을텐데
        if (rayDir.y < 0) rayDir = Vector3.up;
        else if (rayDir.x > 0 && rayDir.z > 0) rayDir = new Vector3(-1, 0, -1).normalized;
        else if (rayDir.x < 0 && rayDir.z > 0)rayDir = new Vector3(1, 0, -1).normalized;

        float rotated = 0f;
        float rotationSpeed = 90f / 1f;
        float deltaAngle = rotationSpeed * Time.deltaTime;
        while (rotated < 90f)
        {
            foreach (var blockUnit in selectList)
            {
                blockUnit.transform.RotateAround(coreBlock.position, rayDir, deltaAngle);
            }
            rotated += deltaAngle;

            yield return null;
        }
        Debug.Log(rayDir.normalized);
        Debug.Log(deltaAngle);

        float correction = 90f - rotated;
        if (Mathf.Abs(correction) > 0.01f)
        {
            foreach (var blockUnit in selectList)
            {
                blockUnit.transform.RotateAround(coreBlock.position, rayDir, correction);
            }
        }
        isRotate = false;
        selectList.Clear();
    }

    //---------------------------------------------------------------------------------

    public void DetectBlock(int rayIdx)
    {
        if (isRotate == true) return;

        pivotRot = rayPivot[rayIdx].transform.rotation;
        rayDir = pivotRot * Vector3.down;
        foreach (var ray in rayPivot[rayIdx].RayList)
        {
            Debug.DrawRay(ray.transform.position, rayDir * 5f, Color.green, 1f);
            RaycastHit hit;
            Physics.Raycast(ray.transform.position, rayDir, out hit, 5f);

            selectList.Add(hit.collider.gameObject);
        }

        StartCoroutine(CorRotatePattern());
    }
}
