using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PolygonCutter : MonoBehaviour
{
    public PolygonCollider2D[] polygons;

    [ContextMenu("Cut")]
    void Cut()
    {
        var collider = GetComponent<PolygonCollider2D>();

        var i = 0;
        collider.pathCount = polygons.Length + 1;
        foreach (var item in polygons)
        {
            i++;
            collider.SetPath(i, item.points
                .Select(t => transform.InverseTransformPoint(item.transform.TransformPoint(new Vector3(t.x, t.y, 0))))
                .Select(t => new Vector2(t.x, t.y))
                .ToArray());
        }
    }
}
