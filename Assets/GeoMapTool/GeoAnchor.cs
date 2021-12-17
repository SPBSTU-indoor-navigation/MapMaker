using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoAnchor : MonoBehaviour
{
    public double latitude, longitude, altitude;

    public GeoUtility.GeoPosition GeoPosition()
    {
        return new GeoUtility.GeoPosition(latitude, longitude, altitude);
    }
}
