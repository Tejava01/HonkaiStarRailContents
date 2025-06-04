using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParadoxCubeRay : MonoBehaviour
{
    [SerializeField] private List<GameObject> rayList = new List<GameObject>();
    
    //-------------------------------------------------------------------------------

    public List<GameObject> RayList { get { return rayList; } }
}
