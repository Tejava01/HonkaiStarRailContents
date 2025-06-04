using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetParadoxCube : MonoBehaviour
{
    [Header("Simulator")]
    [SerializeField] private ParadoxCubeSimulator simulator;

    [Header("PivotCentor")]
    [SerializeField] private Button top;
    [SerializeField] private Button bottom;
    [SerializeField] private Button left;
    [SerializeField] private Button right;
    [SerializeField] private Button back;
    [SerializeField] private Button forward;

    [Header("PivotRightBottom")]
    [SerializeField] private Button leftSwipe;
    [SerializeField] private Button rightSwipe;

    //---------------------------------------------------------

    public void RayBlock(int rayIdx)
    {
        simulator.DetectBlock(rayIdx);
    }
}
