using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParadoxCubeSimulator : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private ParadoxCubeBlock paradoxCubeBlock;

    //-----------------------------------------------------

    public ParadoxCubeBlock ParadoxCubeBlock { get { return paradoxCubeBlock; } }
}
