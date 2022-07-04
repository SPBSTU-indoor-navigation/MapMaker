using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using IMDF.Feature;
using UnityEditor.SceneManagement;

public class IMDFEditorWindow : EditorWindow
{
    public static float snapping;
    Transform parentPath;
    public string mapName, password;

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

        if (GUILayout.Button("Rescale"))
        {
            var transform = Selection.transforms[0];

            var scale = transform.localScale;

            Undo.RecordObject(transform, "rescale");
            foreach (var line in transform.GetComponentsInChildren<LineRenderer>())
            {
                Undo.RecordObject(line, "rescale");
                Undo.RecordObject(line.transform, "rescale");
                // line.SetPosition(0, line.transform.InverseTransformPoint(Vector3.zero));

                Vector3[] newPoints = new Vector3[line.positionCount];
                line.GetPositions(newPoints);

                var globalPoints = newPoints
                    .Select(t => line.transform.TransformPoint(t))
                    .Select(t => new Vector3(t.x * scale.x, t.y * scale.y, t.z * scale.z))
                    .Select(t => line.transform.InverseTransformPoint(t));

                line.SetPositions(globalPoints.ToArray());
            }


            var polygons = transform.GetComponentsInChildren<PolygonGeometry>().Select(t => t.GetComponent<PolygonCollider2D>()).ToArray();
            foreach (var polygon in polygons)
            {
                Undo.RecordObject(polygon, "rescale");

                polygon.points = polygon.points
                    .Select(t => polygon.transform.TransformPoint(t))
                    .Select(t => new Vector2(t.x * scale.x, t.y * scale.y))
                    .Select(t => polygon.transform.InverseTransformPoint(t))
                    .Select(t => new Vector2(t.x, t.y)).ToArray();
            }



            transform.localScale = Vector3.one;


            foreach (var line in transform.GetComponentsInChildren<LineRenderer>())
            {
                Vector3 delta = line.transform.position - line.GetPosition(0);
                Vector3[] newPoints = new Vector3[line.positionCount];
                line.GetPositions(newPoints);

                var globalPoints = newPoints
                    .Select(t => t + delta);

                line.SetPositions(globalPoints.ToArray());
                line.transform.position -= delta;
            }
        }

        GUILayout.Space(40);
        parentPath = EditorGUILayout.ObjectField("root", parentPath, typeof(Transform), true) as Transform;
        if (GUILayout.Button("Path Generator"))
        {
            if (parentPath)
            {
                foreach (var item in Selection.gameObjects)
                {
                    var lr = item.GetComponentsInChildren<LineRenderer>(true);
                    PathNode last = null;
                    foreach (var line in lr)
                    {
                        var linePoints = new Vector3[line.positionCount];
                        line.GetPositions(linePoints);
                        for (var i = 0; i < linePoints.Length; i++)
                        {
                            var node = new GameObject($"Node_{line.gameObject.name}_{i}").AddComponent<PathNode>();
                            StageUtility.PlaceGameObjectInCurrentStage(node.gameObject);
                            // go.transform.parent = parent;
                            node.transform.SetParent(parentPath);
                            node.transform.position = line.transform.TransformPoint(linePoints[i]);
                            if (last)
                            {
                                last.neighbors.Add(node);
                                node.neighbors.Add(last);
                            }
                            last = node;

                            Debug.Log($"Node_{line.gameObject.name}_{i}");
                            Undo.RegisterCreatedObjectUndo(node.gameObject, "Node");
                        }
                    }
                }
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label("Geojson", EditorStyles.boldLabel);
        if (GUILayout.Button("Serialize"))
        {
            IMDFDecoder.Ser();
        }

        GUILayout.BeginHorizontal();
        mapName = EditorGUILayout.TextField(mapName);
        password = EditorGUILayout.TextField(password);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Send"))
        {
            IMDFDecoder.Send(mapName, password, this);
        }
        // GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        // myString = EditorGUILayout.TextField("Text Field", myString);

        // groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        // myBool = EditorGUILayout.Toggle("Toggle", myBool);
        // myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        // EditorGUILayout.EndToggleGroup();
    }
}
