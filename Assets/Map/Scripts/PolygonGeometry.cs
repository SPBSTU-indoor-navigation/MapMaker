using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonGeometry : MonoBehaviour
{
    public Color color;
    public float additionalOrder = 0;
    PolygonCollider2D[] polygons;
    int order_ = 0;
    bool isDirty = true;

    public int order
    {
        get
        {

            if (!isDirty)
                return order_;

            if (transform.parent && transform.parent.GetComponent<PolygonGeometry>())
            {
                isDirty = false;
                order_ = transform.parent.GetComponent<PolygonGeometry>().order + 1;
                return order_;
            }
            return 0;
        }
    }

    private void Start()
    {
        if (GetComponent<MeshRenderer>().sharedMaterial == null)
        {
            GetComponent<PolygonCollider2D>().points = new Vector2[] { new Vector2(0, 0), new Vector2(0, 5), new Vector2(5, 5), new Vector2(5, 0) };
            GetComponent<MeshRenderer>().material = Resources.FindObjectsOfTypeAll(typeof(Material)).First(t => t.name == "IMDFMAT") as Material;
        }
    }

    void Snap(ref Vector2 v)
    {
        bool isPrefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null || PrefabUtility.GetPrefabInstanceHandle(gameObject) != null;

        if (IMDFEditorWindow.snapping > 0 && !(PrefabUtility.IsPartOfAnyPrefab(gameObject) && !(StageUtility.GetCurrentStage() is PrefabStage)))
        {

            var point = transform.TransformPoint(v);
            point.x = Mathf.Round(point.x / IMDFEditorWindow.snapping) * IMDFEditorWindow.snapping;
            point.y = Mathf.Round(point.y / IMDFEditorWindow.snapping) * IMDFEditorWindow.snapping;

            point = transform.InverseTransformPoint(point);
            v.x = point.x;
            v.y = point.y;
        }
    }

    Dictionary<PolygonCollider2D, List<Vector2[]>> lastPoints = new Dictionary<PolygonCollider2D, List<Vector2[]>>();

    private void Update()
    {
        polygons = GetComponents<PolygonCollider2D>();
        List<CombineInstance> combines = new List<CombineInstance>();

        bool changed = GetComponent<MeshFilter>().sharedMesh == null;

        foreach (var item in polygons)
        {
            if (lastPoints.TryGetValue(item, out List<Vector2[]> paths))
            {
                if (lastPoints[item].Count != item.pathCount)
                {
                    changed = true;
                    lastPoints[item] = new List<Vector2[]>();
                    for (int i = 0; i < item.pathCount; i++)
                    {
                        lastPoints[item].Add(item.GetPath(i));
                    }
                }

                for (int i = 0; i < item.pathCount; i++)
                {
                    var t = item.GetPath(i);

                    for (int k = 0; k < t.Length; k++)
                    {
                        if (lastPoints[item][i].Length == t.Length && t[k] != lastPoints[item][i][k])
                        {
                            changed = true;
                            if (!GetComponent<LR2Polygon>())
                            {
                                Snap(ref t[k]);
                            }
                        }

                    }

                    lastPoints[item][i] = t;
                    if (changed)
                    {
                        item.SetPath(i, t);
                    }
                }
            }
            else
            {
                lastPoints.Add(item, new List<Vector2[]>());
                changed = true;
            }
        }


        if (changed || isDirty)
        {
            isDirty = false;
            foreach (var item in polygons)
            {
                Mesh m = item.CreateMesh(true, true);

                Vector3[] vert = m.vertices;

                for (var i = 0; i < vert.Length; i++)
                {
                    vert[i] = Quaternion.Inverse(transform.rotation) * (vert[i] - transform.position) + Vector3.back * (order + additionalOrder);
                }

                Color[] c = new Color[vert.Length];
                for (var i = 0; i < c.Length; i++)
                {
                    c[i] = color;
                }

                m.SetVertices(vert);
                m.SetColors(c);

                m.RecalculateNormals();
                m.RecalculateBounds();
                var comb = new CombineInstance();
                comb.mesh = m;
                comb.transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one);
                combines.Add(comb);
            }

            var result = new Mesh();
            result.CombineMeshes(combines.ToArray());

            DestroyImmediate(GetComponent<MeshFilter>().sharedMesh);
            GetComponent<MeshFilter>().sharedMesh = result;

            foreach (var item in combines)
            {
                DestroyImmediate(item.mesh);
            }
        }
    }

    private void OnValidate()
    {
        isDirty = true;
    }

    [CustomEditor(typeof(PolygonGeometry))]

    [CanEditMultipleObjects]
    public class PolygonGeometryEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var polygonGeometry = target as PolygonGeometry;
            serializedObject.Update();
            if (GUILayout.Button("Add hole"))
            {
                var collider = polygonGeometry.GetComponent<PolygonCollider2D>();
                Undo.RecordObject(collider, "Add hole");
                collider.pathCount = collider.pathCount + 1;
                collider.SetPath(collider.pathCount - 1, new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0) });
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
