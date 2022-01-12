using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using IMDF.Feature;

public class IMDFEditorWindow : EditorWindow
{
    public static float snapping;

    // Add menu named "My Window" to the Window menu
    [MenuItem("IMDF/Controll window")]
    static void Init()
    {
        IMDFEditorWindow window = (IMDFEditorWindow)EditorWindow.GetWindow(typeof(IMDFEditorWindow));
        window.Show();
    }


    void Recenter(PolygonCollider2D[] colliders)
    {
        Undo.RecordObjects(colliders.Select(t => t).ToArray(), "Recenter");
        Undo.RecordObjects(colliders.Select(t => t.transform).ToArray(), "Recenter");
        Undo.RecordObjects(colliders.Select(t => t.GetComponent<IRefferencePoint>()).Where(t => t != null).Select(t => t as Object).ToArray(), "Recenter");
        foreach (var item in colliders)
        {
            RecalculateCeneter(item);
        }
    }

    void CreateBuilding()
    {
        var obj = FindObjectOfType<IMDF.Venue>();
        var go = new GameObject("Building");
        go.transform.SetParent(obj.transform);
        go.AddComponent<IMDF.Building>();
        go.transform.localPosition = Vector3.zero;

        Undo.RegisterCreatedObjectUndo(go, "Create opening");
        Selection.SetActiveObjectWithContext(go, go);
    }

    void CreateLevel()
    {
        var obj = FindObjectOfType<IMDF.Building>();
        if (Selection.transforms.Length != 0 && Selection.transforms[0].GetComponentInParent<IMDF.Building>())
        {
            obj = Selection.transforms[0].GetComponentInParent<IMDF.Building>();
        }
        var go = new GameObject("Level");
        go.transform.SetParent(obj.transform);
        go.AddComponent<IMDF.Level>();
        go.transform.localPosition = Vector3.zero;

        Undo.RegisterCreatedObjectUndo(go, "Create opening");
        Selection.SetActiveObjectWithContext(go, go);
    }

    void CreateUnit()
    {
        var obj = FindObjectOfType<IMDF.Level>();
        if (Selection.transforms.Length != 0 && Selection.transforms[0].GetComponentInParent<IMDF.Level>())
        {
            obj = Selection.transforms[0].GetComponentInParent<IMDF.Level>();
        }
        var go = new GameObject("Unit");
        go.transform.SetParent(obj.transform);
        go.AddComponent<IMDF.Unit>().NewObj();
        go.transform.localPosition = Vector3.zero;

        Undo.RegisterCreatedObjectUndo(go, "Create opening");
        Selection.SetActiveObjectWithContext(go, go);
    }

    void CreateOpening()
    {
        var obj = FindObjectOfType<IMDF.Unit>();
        if (Selection.transforms.Length != 0 && Selection.transforms[0].GetComponentInParent<IMDF.Unit>())
        {
            obj = Selection.transforms[0].GetComponentInParent<IMDF.Unit>();
        }

        var go = new GameObject("Opening");
        go.transform.SetParent(obj.transform);
        go.AddComponent<IMDF.Opening>();
        go.transform.localPosition = Vector3.zero;

        Undo.RegisterCreatedObjectUndo(go, "Create opening");
        Selection.SetActiveObjectWithContext(go, go);
    }

    void RecalculateCeneter(PolygonCollider2D p)
    {
        Vector2 sum = p.points.Aggregate(Vector2.zero, (a, v) => a + v);
        Vector2 avg = sum / p.points.Length;

        Vector2[] t = p.points;
        for (var i = 0; i < p.points.Length; i++)
        {
            t[i] -= avg;
        }

        p.points = t;
        Vector3 delta = new Vector3(avg.x, avg.y, 0);
        p.transform.position += delta;
        for (var i = 0; i < p.transform.childCount; i++)
        {
            p.transform.GetChild(i).transform.position -= delta;
        }

        if (p.gameObject.TryGetComponent<IRefferencePoint>(out var point) && point.showDisplayPoint)
        {
            point.displayPoint -= new Vector2(delta.x, delta.y);
        }
    }


    void RecalculateMapSize()
    {
        var map = FindObjectOfType<GeoMap>();
        if (map == null) Debug.LogError("GeoMap is null");
        var anchors = FindObjectsOfType<GeoAnchor>();
        if (anchors.Length < 2) Debug.LogError("GeoAnchor count MUST BE >= 2");

        map.transform.localScale = Vector3.one;

        double totalDistance = 0;
        for (var i = 0; i < anchors.Length; i++)
            for (var j = i + 1; j < anchors.Length; j++)
                totalDistance += GeoUtility.distance(anchors[i].GeoPosition(), anchors[j].GeoPosition());

        double targetScale = 0;

        for (var i = 0; i < anchors.Length; i++)
        {
            for (var j = i + 1; j < anchors.Length; j++)
            {
                float delta = Vector3.Distance(anchors[i].transform.localPosition, anchors[j].transform.localPosition);
                double geoDelta = GeoUtility.distance(anchors[i].GeoPosition(), anchors[j].GeoPosition());

                double scale = geoDelta / delta;
                targetScale += scale * geoDelta / totalDistance;
                Debug.Log(scale);
            }
        }

        map.transform.localScale = Vector3.one * (float)targetScale;


        double latitude = 0;
        double longitude = 0;
        for (var i = 0; i < anchors.Length; i++)
        {
            var t = GeoUtility.VectorToGeo(anchors[i].GeoPosition(), -anchors[i].transform.position);
            latitude += t.latitude;
            longitude += t.longitude;
        }

        latitude /= anchors.Length;
        longitude /= anchors.Length;

        map.centerPosition = new GeoUtility.GeoPosition(latitude, longitude);


    }

    void OnGUI()
    {
        snapping = EditorGUILayout.Slider("Snap", snapping, 0, 10);
        GUILayout.Space(20);
        GUILayout.Label("ReAlign");
        EditorGUILayout.BeginHorizontal();
        {

            PolygonCollider2D[] colliders = Selection.gameObjects.Select(t => t.GetComponent<PolygonCollider2D>()).Where(t => t != null).ToArray();
            EditorGUI.BeginDisabledGroup(colliders.Length == 0);
            {
                if (GUILayout.Button("Selected"))
                {
                    Recenter(colliders);
                }
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("All"))
            {
                Recenter(FindObjectsOfType<PolygonCollider2D>());
            }
        }
        EditorGUILayout.EndHorizontal();



        IRefferencePoint[] points = Selection.gameObjects.Select(t => t.GetComponent<IRefferencePoint>()).Where(t => t != null).Where(t => t.showDisplayPoint).ToArray();
        EditorGUI.BeginDisabledGroup(points.Length == 0);
        {
            if (GUILayout.Button("Center display point"))
            {
                Undo.RecordObjects(points.Select(t => t as Object).ToArray(), "move point");
                foreach (var item in points)
                {
                    item.displayPoint = Vector3.zero;
                }
                EditorUtility.SetDirty(Selection.gameObjects[0]);
            }
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(20);
        GUILayout.Label("Geo Map", EditorStyles.boldLabel);
        if (GUILayout.Button("Recalculate map size")) RecalculateMapSize();

        GUILayout.Space(20);
        GUILayout.Label("Create", EditorStyles.boldLabel);

        if (GUILayout.Button("Create building")) CreateBuilding();

        if (GUILayout.Button("Create level")) CreateLevel();

        if (GUILayout.Button("Create Unit")) CreateUnit();

        if (GUILayout.Button("Create Opening")) CreateOpening();


        GUILayout.FlexibleSpace();
        GUILayout.Label("Geojson", EditorStyles.boldLabel);
        if (GUILayout.Button("Serialize"))
        {
            IMDFDecoder.Ser();
        }
        // GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        // myString = EditorGUILayout.TextField("Text Field", myString);

        // groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        // myBool = EditorGUILayout.Toggle("Toggle", myBool);
        // myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        // EditorGUILayout.EndToggleGroup();
    }
}