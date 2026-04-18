using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GeoProbe : MonoBehaviour
{
    public GeoUtility.GeoPosition position;

    private void Update()
    {
        position = GeoMap.CalculateGeo(transform.position);
    }
    // private void OnDrawGizmos()
    // {
    //     var points = FindObjectsOfType<GeoAnchor>();
    //     Handles.color = Color.red;
    //     // Handles.DrawLine(transform.position, transform.TransformPoint(secondPoint), 3);
    // }
}
