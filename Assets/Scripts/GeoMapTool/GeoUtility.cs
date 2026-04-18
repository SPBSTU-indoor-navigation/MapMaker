using System;
using UnityEngine;


public class GeoUtility
{
    [System.Serializable]
    public class GeoPosition
    {
        public double latitude, longitude, altitude;

        public GeoPosition(double latitude, double longitude, double altitude = 0)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = altitude;
        }

        public IMDF.Feature.Point GetPoint()
        {
            return new IMDF.Feature.Point(longitude, latitude);
        }
    }

    public const double metersInLatDegree = 111194.92664f;
    public static Vector3 GeoToVector(GeoPosition pivot, GeoPosition position)
    {
        double x = (position.longitude - pivot.longitude) * metersInLatDegree * System.Math.Cos(Mathf.Deg2Rad * (pivot.latitude + position.latitude) / 2f);
        double z = (position.latitude - pivot.latitude) * metersInLatDegree;
        return new Vector3((float)x, (float)(position.altitude - pivot.altitude), (float)z);
    }

    public static GeoPosition VectorToGeo(GeoPosition pivot, Vector2 position)
    {
        double latitude = (position.y / metersInLatDegree + pivot.latitude);
        double longitude = position.x / (metersInLatDegree * System.Math.Cos(Mathf.Deg2Rad * (pivot.latitude + latitude) / 2.0)) + pivot.longitude;

        return new GeoPosition(latitude, longitude, 0);

    }

    public static double distance(GeoPosition point1, GeoPosition point2)
    {
        if ((point1.latitude == point2.latitude) && (point1.longitude == point2.longitude))
        {
            return 0;
        }
        else
        {
            double theta = point1.longitude - point2.longitude;
            double dist = Math.Sin(Mathf.Deg2Rad * point1.latitude) * Math.Sin(Mathf.Deg2Rad * (point2.latitude)) +
                Math.Cos(Mathf.Deg2Rad * (point1.latitude)) * Math.Cos(Mathf.Deg2Rad * (point2.latitude)) * Math.Cos(Mathf.Deg2Rad * (theta));
            dist = Math.Acos(dist);
            dist = Mathf.Rad2Deg * dist;
            dist = dist * 60 * 1.1515;
            dist = dist * 1.609344 * 1000;
            return (dist);
        }
    }

}

