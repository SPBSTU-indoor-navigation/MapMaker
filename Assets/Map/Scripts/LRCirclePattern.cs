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

    [Header("Ellipse")]
    public float b = -1;

    [Space]
    public Vector2 offset;

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

        if (b < 0)
        {
            for (int i = 0; i <= count; i++)
            {

                float t = st + i * step;
                l.Add(new Vector3(
                    radius * Mathf.Sin(t) + (i < count / 2 ? offset.x / 2 : -offset.x / 2),
                    radius * Mathf.Cos(t) + (i < count / 2 ? offset.y / 2 : -offset.y / 2),
                    -5));
            }
        }
        else
        {

            float e = Mathf.Sqrt(1 - (b / radius) * (b / radius));
            for (int i = 0; i <= count; i++)
            {
                float t = st + i * step;

                float r = Mathf.Sqrt(
                    b * b / (1 - e * e * Mathf.Cos(t) * Mathf.Cos(t))
                );
                Vector3 pos = new Vector3(
                    r * Mathf.Sin(t) + (i < count / 2 ? offset.x / 2 : -offset.x / 2),
                    r * Mathf.Cos(t) + (i < count / 2 ? offset.y / 2 : -offset.y / 2),
                    -5
                );
                l.Add(pos);
            }
        }

        if (offset.sqrMagnitude > 0.1f)
        {
            l.Add(l[0]);
        }

        lr.positionCount = l.Count;
        lr.SetPositions(l.ToArray());

    }
}
