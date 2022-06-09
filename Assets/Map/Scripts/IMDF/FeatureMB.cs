using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IMDF
{
    [ExecuteAlways]
    public class FeatureMB : MonoBehaviour
    {
        public string id;

        private System.Guid guid_;
        public System.Guid guid
        {
            get
            {
                return guid_;
            }
            set
            {
                guid_ = value;
                id = value.ToString();
            }
        }

        public virtual void GenerateGUID()
        {
            guid = UUIDStorage.shared.GetGUID(this);
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
        GeometryLineEdit _geometryLineEdit;
        protected GeometryLineEdit geometryLineEdit
        {
            get
            {
                if (_geometryLineEdit == null)
                    _geometryLineEdit = GetComponent<GeometryLineEdit>();
                return _geometryLineEdit;
            }
        }

        public Feature.Point[] GetGeoPoints()
        {
            return new Feature.Point[2] {
             GeoMap.CalculateGeo(transform.position).GetPoint(),
             GeoMap.CalculateGeo(transform.TransformPoint(GetComponent<GeometryLineEdit>().secondPoint)).GetPoint() };
        }

        public Vector3[] GetPoints()
        {
            return new Vector3[2] {
                transform.position,
                transform.TransformPoint(GetComponent<GeometryLineEdit>().secondPoint) };
        }
    }

    [RequireComponent(typeof(GeometryMultyLineEdit))]
    public class GeometryMultyLine : FeatureMB
    {
        public Feature.Point[] GetPoints()
        {
            var lr = GetComponent<LineRenderer>();
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);

            return positions.Select(t => GeoMap.CalculateGeo(transform.TransformPoint(t)).GetPoint()).ToArray();
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
