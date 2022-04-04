using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class LRCirclePattern : MonoBehaviour
{
    LineRenderer lr;

    public float radius = 5, start, end = 360;
    public int count = 20;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
    }

    private void OnValidate()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        List<Vector3> l = new List<Vector3>();

        float step = Mathf.Deg2Rad * (end - start) / count;
        float st = Mathf.Deg2Rad * start;

        for (int i = 0; i <= count; i++)
        {
            l.Add(new Vector3(radius * Mathf.Sin(st + i * step), radius * Mathf.Cos(st + i * step), -5));
        }

        lr.positionCount = l.Count;
        lr.SetPositions(l.ToArray());

    }
}
