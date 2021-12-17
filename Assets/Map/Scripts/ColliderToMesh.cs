using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class ColliderToMesh : MonoBehaviour
{
    public Color color;
    public PolygonCollider2D[] exclusive;

    bool isDirty = true;
    int order_ = 0;

    public int order
    {
        get
        {

            if (!isDirty)
                return order_;

            if (transform.parent && transform.parent.GetComponent<ColliderToMesh>())
            {
                isDirty = false;
                order_ = transform.parent.GetComponent<ColliderToMesh>().order + 1;
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

    Vector2[] lastPoints = new Vector2[0];

    // Update is called once per frame
    void Update()
    {
        // CalculateOrder();
        isDirty = true;

        PolygonCollider2D p = GetComponent<PolygonCollider2D>();
        Vector2[] t = p.points;

        for (var i = 0; i < t.Length; i++)
        {
            if (t.Length != lastPoints.Length || t[i] != lastPoints[i])
                Snap(ref t[i]);
        }

        lastPoints = t;

        p.points = t;

        Mesh m = p.CreateMesh(true, true);
        Vector3[] vert = m.vertices;

        for (var i = 0; i < vert.Length; i++)
        {
            vert[i] = Quaternion.Inverse(transform.rotation) * (vert[i] - transform.position) + Vector3.back * order;
        }

        Color[] c = new Color[vert.Length];
        for (var i = 0; i < c.Length; i++)
        {
            c[i] = color;
        }

        m.SetVertices(vert);
        m.SetColors(c);

        m.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = m;
    }

    [ContextMenu("RecalculateCeneter")]
    void RecalculateCeneter()
    {
        PolygonCollider2D p = GetComponent<PolygonCollider2D>();

        Vector2 sum = p.points.Aggregate(Vector2.zero, (a, v) => a + v);
        Vector2 avg = sum / p.points.Length;

        Debug.Log(avg);

        Vector2[] t = p.points;
        for (var i = 0; i < p.points.Length; i++)
        {
            t[i] -= avg;
        }

        p.points = t;
        Vector3 delta = new Vector3(avg.x, avg.y, 0);
        transform.position += delta;
        for (var i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).transform.position -= delta;
        }
    }

}
