using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GeometryLineEdit : MonoBehaviour
{
    public float length = 1f;
    [SerializeField, HideInInspector]
    public Vector3 secondPoint = Vector3.right;

    private void Start()
    {
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Handles.DrawLine(transform.position, transform.TransformPoint(secondPoint), 3);
    }

    [CustomEditor(typeof(GeometryLineEdit))]
    public class GeometryLineEdit_Editor : Editor
    {
        protected virtual void OnSceneGUI()
        {
            GeometryLineEdit obj = (GeometryLineEdit)target;
            var point = obj.transform.TransformPoint(obj.secondPoint);
            EditorGUI.BeginChangeCheck();
            var newPoint = obj.transform.InverseTransformPoint(Handles.PositionHandle(point, Quaternion.LookRotation(obj.secondPoint, Vector3.back)));
            newPoint.z = 0;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(obj, "Change line direction");
                obj.secondPoint = obj.length > 0 ? Vector3.Normalize(newPoint) * obj.length : newPoint;
            }
        }
    }
}
