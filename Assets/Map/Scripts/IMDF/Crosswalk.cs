using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IMDF
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Crosswalk : FeatureMB
    {
        [Min(0.01f)]
        public float spacing = 1;
        public float lineWidth = 0.4f;

        [SerializeField]
        Vector2 offsetPoint = Vector2.up / 2;

        LineRenderer lr;

        Vector3 scale = Vector3.zero;

        private void Awake()
        {
            GetComponent<LineRenderer>().useWorldSpace = false;
        }

        private void Update()
        {
            if (scale.Equals(transform.localScale)) return;

            scale = transform.localScale;
            Proc();

        }

        public List<IMDF.Feature.Point[]> Lines()
        {
            var res = new List<IMDF.Feature.Point[]>();
            lr = GetComponent<LineRenderer>();
            for (int i = 0; i < lr.positionCount; i += 2)
            {
                var pos1 = GeoMap.CalculateGeo(transform.TransformPoint(lr.GetPosition(i))).GetPoint();
                var pos2 = GeoMap.CalculateGeo(transform.TransformPoint(lr.GetPosition(i + 1))).GetPoint();

                res.Add(new Feature.Point[] { pos1, pos2 });
            }

            return res;
        }

        private void OnValidate()
        {
            Proc();
        }

        void Proc()
        {
            if (!lr) lr = GetComponent<LineRenderer>();

            float width = transform.localScale.y;
            int count = (int)(width / spacing);
            float delta = 1.0f / count;

            List<Vector3> v = new List<Vector3>();
            for (int i = 0; i <= count; i++)
            {
                float k = i % 2 == 0 ? 1 : -1;
                float offset = offsetPoint.x * i / count;
                v.Add(new Vector3(offset + k * 0.5f, -0.5f + i * delta, 0));
                v.Add(new Vector3(offset + -k * 0.5f, -0.5f + i * delta, 0));
            }

            lr.positionCount = v.Count;
            lr.SetPositions(v.ToArray());

            lr.startWidth = lineWidth / width;
            lr.endWidth = lineWidth / width;
        }

        [CustomEditor(typeof(Crosswalk)), CanEditMultipleObjects]
        public class RefferencePointEditor_Editor : Editor
        {
            protected virtual void OnSceneGUI()
            {
                Crosswalk obj = (Crosswalk)target;
                EditorGUI.BeginChangeCheck();
                var pos = obj.transform.TransformPoint(obj.offsetPoint);
                var newPoint = obj.transform.InverseTransformPoint(Handles.PositionHandle(pos, obj.transform.rotation));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj as Object, "move point");
                    obj.offsetPoint = new Vector2(newPoint.x, 0.5f);
                    obj.OnValidate();
                    EditorUtility.SetDirty(obj as Object);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
                }
            }
        }
    }
}
