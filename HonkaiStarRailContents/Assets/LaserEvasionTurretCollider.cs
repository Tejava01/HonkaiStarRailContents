using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEvasionTurretCollider : MonoBehaviour
{
    //클래스 관계역전 조심할것
    [SerializeField] private string ColliderHitTag = "Player";

    public event Action<bool> onPlayerHit;

    //----------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ColliderHitTag)
        {
            onPlayerHit?.Invoke(true);
        }
    }
}
