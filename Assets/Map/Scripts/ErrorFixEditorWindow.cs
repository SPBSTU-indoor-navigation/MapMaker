using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;

public class ErrorFixEditorWindow : EditorWindow
{
    public static float snapping;
    Transform parentPath;
    public string mapName, password;

    // Add menu named "My Window" to the Window menu
    [MenuItem("IMDF/Error Fixer")]
    static void Init()
    {
        ErrorFixEditorWindow window = (ErrorFixEditorWindow)EditorWindow.GetWindow(typeof(ErrorFixEditorWindow));
        window.Show();
    }

    private Vector2 scrollPosition;
    private List<(string, Object)> problems = new();

    void Analyze()
    {
        Debug.Log("Analyzing...");
        problems.Clear();

        var openings = StageUtility.GetCurrentStageHandle().FindComponentsOfType<IMDF.Opening>();

        problems.AddRange(
            openings
            .Where(o => o.pathNode.Where(p => !p).Any())
            .Select(o => ($"Opening {o.name} has empty path node", o as Object))
        );

        var classRoomNamers = StageUtility.GetCurrentStageHandle().FindComponentsOfType<ClassroomNamer>();

        problems.AddRange(
            classRoomNamers
            .Select(c =>
            {
                var u = c.GetComponent<IMDF.Unit>();
                if (!u) return ("Cannot find Unit component on ClassroomNamer", c);

                var ru = c.templateRU;
                var en = c.templateEN;

                if (string.IsNullOrWhiteSpace(ru))
                    c.dictRU.TryGetValue(u.occupantCategory, out ru);

                if (string.IsNullOrWhiteSpace(en))
                    c.dictEN.TryGetValue(u.occupantCategory, out en);

                if (ru == null) return ($"Cannot parse RU template. MB wrong category: {u.occupantCategory}", c);
                if (en == null) return ($"Cannot parse EN template. MB wrong category: {u.occupantCategory}", c);

                return (null, c);
            })
            .Where(c => c.Item1 != null)
            .Select(c => ($"ClassroomNamer: {c.Item1}", c.c as Object))
        );

        var pathNodes = StageUtility.GetCurrentStageHandle().FindComponentsOfType<PathNode>();

        problems.AddRange(
            pathNodes
            .Where(p => p.associatedFeatures.Where(f => !f).Any())
            .Select(p => ($"PathNode {p.name} has empty associated feature", p as Object))
        );


        var units = StageUtility.GetCurrentStageHandle().FindComponentsOfType<IMDF.Unit>();
        problems.AddRange(
            units
            .Where(u => u.category != IMDF.Feature.Unit.Category.unspecified && u.generateOccupant && u.occupantCategory == IMDF.Feature.Occupant.Category.unspecified)
            .Select(u => ($"Unit {u.name} has unspecified category", u as Object))
        );

        var attractions = StageUtility.GetCurrentStageHandle().FindComponentsOfType<IMDF.Attraction>();
        problems.AddRange(
            attractions
            .Where(a => a.building == null)
            .Select(a => ($"Attraction {a.name} has empty path node", a as Object))
        );
    }

    void OnGUI()
    {

        if (GUILayout.Button("Reanalyze")) // Add a button next to the name
        {
            Analyze();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (var problem in problems)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(problem.Item1); // Display the name
            if (GUILayout.Button("Focus", GUILayout.Width(100))) // Add a button next to the name
            {
                Selection.activeObject = problem.Item2;
                SceneView.FrameLastActiveSceneView();
                EditorGUIUtility.PingObject(problem.Item2);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

    }
}
