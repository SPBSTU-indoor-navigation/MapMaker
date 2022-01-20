using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class GeometryMultyLineEdit : MonoBehaviour
{

    LineRenderer lr;
    int lastCount = 0;
    private void OnEnable()
    {
        lr = GetComponent<LineRenderer>();
    }

    private void OnValidate()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (lr.positionCount != lastCount)
        {
            Vector3[] points = new Vector3[lr.positionCount];
            lr.GetPositions(points);

            for (int i = 0; i < points.Length; i++)
            {
                points[i].z = 0;
            }
            lr.SetPositions(points);
            lastCount = lr.positionCount;
        }
    }
}
