using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEvasionTurret : MonoBehaviour
{
    [SerializeField] private LaserEvasionTurretCollider TurretCollider;
    [SerializeField] private LineRenderer LineRenderer;

    //---------------------------------------------
    public BoxCollider BoxCollider { get; private set; }

    public LaserEvasionTurretCollider GetCollider() => TurretCollider;

    //---------------------------------------------
    private void Awake()
    {
        BoxCollider = GetComponentInChildren<BoxCollider>();
        DoTurretReset();
    }

    public void DoSettingLineRenderer(float width, Color color, Vector3 start, Vector3 end)
    {
        LineRenderer.startWidth = width;
        LineRenderer.endWidth = width;
        LineRenderer.startColor = color;
        LineRenderer.endColor = color;
        LineRenderer.SetPosition(0, start);
        LineRenderer.SetPosition(1, end);
    }

    public void DoTurretReset()
    {
        TurretCollider.gameObject.SetActive(false);
        DoSettingLineRenderer(0f, Color.clear, Vector3.zero, Vector3.zero);
    }
}
