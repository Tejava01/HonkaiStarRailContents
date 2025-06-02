using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsekaiMaterialTeleport : MonoBehaviour
{
    [SerializeField] IsekaiMaterialTeleport connectTeleport;

    private bool teleportDone = false;

    //------------------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (connectTeleport.teleportDone == true)
            return;

        if (other.gameObject.tag == GroupConst.player)
        {
            teleportDone = true;
            connectTeleport.teleportDone = true;
            other.transform.position = connectTeleport.transform.position;
        }
    }
}
