using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEvasionSimulator : MonoBehaviour
{
    [SerializeField, Min(1)] private int totalRound = 6;
    [SerializeField] private int goal1 = 4;
    [SerializeField] private int goal2 = 6;

    //----------------------------------------------------

    private void OnValidate()
    {
        SettingGoal();
    }

    //----------------------------------------------------

    private void SettingGoal()
    {
        goal1 = Mathf.Clamp(goal1, 1, totalRound);
        goal2 = Mathf.Clamp(goal2, 1, totalRound);
    }
}
