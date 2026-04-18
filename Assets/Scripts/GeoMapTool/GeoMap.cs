using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoMap : MonoBehaviour
{
    public static GeoMap geoMap;

    public GeoUtility.GeoPosition centerPosition;

    public static GeoUtility.GeoPosition CalculateGeo(Vector3 point)
    {
        return CalculateGeo(new Vector2(point.x, point.y));
    }

    public static GeoUtility.GeoPosition CalculateGeo(Vector2 point)
    {
        if (geoMap == null) geoMap = FindObjectOfType<GeoMap>();

        return GeoUtility.VectorToGeo(geoMap.centerPosition, point);
    }
}
