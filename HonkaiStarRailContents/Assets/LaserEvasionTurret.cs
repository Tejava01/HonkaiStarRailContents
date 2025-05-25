using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEvasionTurret : MonoBehaviour
{
    public BoxCollider BoxCollider { get; private set; }

    //---------------------------------------------

    private void Awake()
    {
        BoxCollider = GetComponentInChildren<BoxCollider>();
    }
}
