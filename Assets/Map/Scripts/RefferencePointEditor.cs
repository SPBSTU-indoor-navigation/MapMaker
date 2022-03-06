using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public interface IAddress
{
    IMDF.Address address { get; }
}

public interface IOccupant
{
    IMDF.Feature.Occupant occupant { get; }
}

public interface IAnchor
{
    IMDF.Feature.Anchor anchor { get; }
}

public interface IRefferencePoint
{
    Vector2 displayPoint { get; set; }
    bool showDisplayPoint
    {
        get
        {
            return false;
        }
    }
}

public class RefferencePointEditor : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        IRefferencePoint r = GetComponent<IRefferencePoint>();
        if (r.showDisplayPoint)
        {
            var pos = transform.TransformPoint(r.displayPoint);
            if (TryGetComponent<ColliderToMesh>(out var c))
            {
                Gizmos.DrawSphere(new Vector3(pos.x, pos.y, -c.order), 0.2f);
            }
            else
            {
                Gizmos.DrawSphere(pos, 0.2f);

            }
        }
    }


    public static IMDF.Feature.GeoJSONPoint GetGeoJSONPoint(IRefferencePoint point)
    {
        // if (!point.showDisplayPoint) return null;


        var obj = point as MonoBehaviour;
        if (point.showDisplayPoint)
            return new IMDF.Feature.GeoJSONPoint(GeoMap.CalculateGeo(obj.transform.TransformPoint(point.displayPoint)).GetPoint());
        else
            return new IMDF.Feature.GeoJSONPoint(GeoMap.CalculateGeo(obj.transform.position).GetPoint());
    }


    [CustomEditor(typeof(RefferencePointEditor)), CanEditMultipleObjects]
    public class RefferencePointEditor_Editor : Editor
    {
        protected virtual void OnSceneGUI()
        {
            RefferencePointEditor obj = (RefferencePointEditor)target;
            IRefferencePoint r = obj.GetComponent<IRefferencePoint>();
            if (r.showDisplayPoint)
            {
                EditorGUI.BeginChangeCheck();
                var pos = obj.transform.TransformPoint(r.displayPoint);
                var newPoint = obj.transform.InverseTransformPoint(Handles.PositionHandle(pos, Quaternion.identity));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(r as Object, "move point");
                    r.displayPoint = newPoint;
                    EditorUtility.SetDirty(r as Object);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
                }
            }
        }
    }
}
