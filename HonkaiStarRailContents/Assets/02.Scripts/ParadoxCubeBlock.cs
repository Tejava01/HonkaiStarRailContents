using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockUnit
{
    public List<GameObject> blockUnitList;
}

public class ParadoxCubeBlock : MonoBehaviour
{
    [SerializeField] private Transform coreBlock;
    [SerializeField] private List<BlockUnit> patternList = new List<BlockUnit>();

    //---------------------------------------------------------------------------------

    public void Swipe(int patternIdx)
    {
        foreach (var blockUnit in patternList[patternIdx].blockUnitList)
        {
            blockUnit.transform.RotateAround(coreBlock.position, Vector3.down, 90f);
        }
    }
}
