using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEvasionTurretCollider : MonoBehaviour
{
    public LaserEvasionSimulator Simulator { private get; set; }
    public event Action<bool> onPlayerHit;

    //----------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (Simulator.isPlayerHit == true) return;

        if (other.gameObject.tag == "Player")
        {
            Debug.Log("충돌");
            onPlayerHit?.Invoke(true);
        }
    }
}
