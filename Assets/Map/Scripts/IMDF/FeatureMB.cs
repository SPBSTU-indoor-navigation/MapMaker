using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IMDF
{
    [ExecuteAlways]
    public class FeatureMB : MonoBehaviour
    {
        public System.Guid guid;

        public void GenerateGUID()
        {
            guid = System.Guid.NewGuid();
        }
    }

    [RequireComponent(typeof(PolygonGeometry))]
    public class GeometryPolygon : FeatureMB
    {
        public Feature.GeoJSONGeometry GetGeoJSONGeometry()
        {
            return GetGeoJSONGeometry(GetComponents<PolygonCollider2D>());
        }

        public static Feature.GeoJSONGeometry GetGeoJSONGeometry(PolygonCollider2D[] colliders)
        {
            Feature.Point[] Process(Transform transform, Vector2[] points)
            {
                var coodrinates = points.Select(t => transform.TransformPoint(t)).Select(t => GeoMap.CalculateGeo(t).GetPoint());
                coodrinates = coodrinates.Append(coodrinates.First());

                return coodrinates.ToArray();
            }

            Feature.GeoPolygon GetGeoPolygon(PolygonCollider2D collider)
            {
                var transform = collider.transform;

                List<IMDF.Feature.Polygon> polygons = new List<Feature.Polygon>();
                for (int i = 0; i < collider.pathCount; i++)
                {
                    polygons.Add(new IMDF.Feature.Polygon(Process(transform, collider.GetPath(i))));
                }


                return new Feature.GeoPolygon(polygons.ToArray());
            }

            return new Feature.GeoJSONPolygon(colliders.Select(t => GetGeoPolygon(t)).ToArray());
        }
    }

    [RequireComponent(typeof(GeometryLineEdit))]
    public class GeometryLine : FeatureMB
    {
        public Feature.Point[] GetPoints()
        {
            return new Feature.Point[2] {
             GeoMap.CalculateGeo(transform.position).GetPoint(),
             GeoMap.CalculateGeo(transform.TransformPoint(GetComponent<GeometryLineEdit>().secondPoint)).GetPoint() };
        }
    }

    public class GeometryPoint : FeatureMB
    {
        public Feature.Point GetPoint()
        {
            return GeoMap.CalculateGeo(transform.position).GetPoint();
        }
    }

}
