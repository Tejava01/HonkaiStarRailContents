using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsekaiMaterialBlockEnd : MonoBehaviour
{
    public event Action onPlayerArrive;
    
    private bool isArrive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isArrive == true)
            return;

        if (other.gameObject.tag== GroupConst.player)
        {
            isArrive = true;   
            onPlayerArrive?.Invoke();
        }
    }
}
