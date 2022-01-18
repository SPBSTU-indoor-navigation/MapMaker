using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
[ExecuteAlways]
public class LR2Polygon : MonoBehaviour
{
    public float width = 2;

    float _width = 0;
    Vector3[] points = new Vector3[0];
    LineRenderer lr;
    PolygonCollider2D collider;

    Vector3 lastPosition;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        collider = GetComponent<PolygonCollider2D>();
    }

    void Update()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (!collider) collider = GetComponent<PolygonCollider2D>();

        Vector3[] newPoints = new Vector3[lr.positionCount];
        lr.GetPositions(newPoints);

        bool change = lastPosition != transform.position || _width != width;
        lastPosition = transform.position;
        _width = width;
        if (points.Length == lr.positionCount)
        {
            for (var i = 0; i < newPoints.Length; i++)
            {
                if (newPoints[i] != points[i])
                {
                    change = true;
                    break;
                }
            }
        }
        else
        {
            change = true;
        }


        points = newPoints;

        if (change)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i].z = -5;
            }
            lr.SetPositions(points);

            List<Vector3> colL = new List<Vector3>();
            List<Vector3> colR = new List<Vector3>();

            var startPoint = (points[0]);
            var dir = (points[1]) - startPoint;
            dir = (Quaternion.Euler(0, 0, 90) * dir).normalized * width / 2;

            colL.Add(startPoint + dir);
            colR.Add(startPoint - dir);


            for (int i = 1; i < points.Length - 1; i++)
            {
                var last = (points[i - 1]);
                var next = (points[i + 1]);
                var g = (points[i]);

                dir = ((last - g).normalized + (next - g).normalized).normalized * width / 2;

                var p1 = g + dir;
                var p2 = g - dir;

                var intersect = Intersect(colL.Last(), p1, colR.Last(), p2);
                colL.Add(!intersect ? p1 : p2);
                colR.Add(!intersect ? p2 : p1);
            }

            var endPoint = (points.Last());
            dir = (points[points.Length - 2]) - endPoint;
            dir = (Quaternion.Euler(0, 0, 90) * dir).normalized * width / 2;

            var p_1 = endPoint + dir;
            var p_2 = endPoint - dir;

            var intersect2 = Intersect(colL.Last(), p_1, colR.Last(), p_2);
            colL.Add(!intersect2 ? p_1 : p_2);
            colR.Add(!intersect2 ? p_2 : p_1);

            colR.Reverse();
            collider.SetPath(0, colL.Concat(colR).Select(t => new Vector2(t.x, t.y)).ToArray());
        }
    }

    bool Intersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        var denominator = (p4.y - p3.y) * (p1.x - p2.x) - (p4.x - p3.x) * (p1.y - p2.y);

        var numerator_a = (p4.x - p2.x) * (p4.y - p3.y) - (p4.x - p3.x) * (p4.y - p2.y);
        var numerator_b = (p1.x - p2.x) * (p4.y - p2.y) - (p4.x - p2.x) * (p1.y - p2.y);
        var Ua = numerator_a / denominator;
        var Ub = numerator_b / denominator;

        return Ua >= 0 && Ua <= 1 && Ub >= 0 && Ub <= 1;
    }
}
