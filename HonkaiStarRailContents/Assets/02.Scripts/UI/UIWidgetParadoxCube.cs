using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetParadoxCube : MonoBehaviour
{
    [Header("Simulator")]
    [SerializeField] private ParadoxCubeSimulator simulator;

    [Header("PivotCentor")]
    [SerializeField] private Button secondFloor;
    [SerializeField] private Button firstFloor;
    [SerializeField] private Button secondRow;
    [SerializeField] private Button firstRow;
    [SerializeField] private Button firstCol;
    [SerializeField] private Button secondCol;

    [Header("PivotRightBottom")]
    [SerializeField] private Button rightSwipe;
    [SerializeField] private Button leftSwipe;


    private bool isRotate = false;
    //---------------------------------------------------------

    public void Swipe()
    {
        if (isRotate == true) return;

        simulator.ParadoxCubeBlock.Swipe(1);
    }
}
