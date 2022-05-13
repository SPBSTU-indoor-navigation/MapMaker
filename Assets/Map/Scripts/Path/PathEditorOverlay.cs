using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor.Toolbars;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;

[Overlay(typeof(SceneView), "Path creator", true)]
public class PathEditorOverlay : Overlay
{
    ToggleExample lineToggle;
    ToggleExample enableToggle;
    public override VisualElement CreatePanelContent()
    {
        lineToggle = new ToggleExample();
        enableToggle = new ToggleExample();
        enableToggle.onToggle += (bool value) =>
        {
            if (value)
            {
                SceneView.duringSceneGui += this.OnSceneGUI;
                Debug.Log(SceneView.lastActiveSceneView);
            }
            else
            {
                SceneView.duringSceneGui -= this.OnSceneGUI;
            }
        };

        var root = new VisualElement() { name = "NavPath" };
        root.Add(new Label("Enable"));
        root.Add(enableToggle);
        root.Add(new Label("Line"));
        root.Add(lineToggle);
        return root;
    }



    bool skipUp = false;
    PathNode lineStart = null;

    Transform lastNearetPointOutline = null;
    Vector2 lastMousePosRender = Vector2.zero;
    void OnSceneGUI(SceneView sceneView)
    {
        if (!enableToggle.value) return;

        Event e = Event.current;

        var nearestNode = NearestMouseNode();

        if (nearestNode != null)
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(nearestNode.transform.position, Vector3.forward, 1f);
            if (lastNearetPointOutline != nearestNode.transform)
            {
                lastNearetPointOutline = nearestNode.transform;
                SceneView.RepaintAll();
            }
        }

        if (e.isKey && e.type == EventType.KeyDown)
        {
            if (!e.command && !e.shift && !e.alt)
            {

                if (skipUp)
                {
                    skipUp = false;
                    return;
                }

                if (e.keyCode == KeyCode.C)
                {
                    e.Use();
                    CreateNodeLast();
                }
                else if (e.keyCode == KeyCode.V)
                {
                    e.Use();
                    CreateNodeNearest();
                }
                else if (e.keyCode == KeyCode.Z)
                {
                    e.Use();
                    Debug.Log("DEL");

                    if (nearestNode != null)
                    {
                        DeleteNode(nearestNode);
                    }
                }
                else if (e.keyCode == KeyCode.L)
                {
                    lineToggle.isEnabled = !lineToggle.isEnabled;
                    lineToggle.SetValueWithoutNotify(lineToggle.isEnabled);
                    e.Use();
                    if (!lineToggle.isEnabled)
                    {
                        lineStart = null;
                    }
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    e.Use();
                    lineStart = null;
                }
            }

            if (!e.command && !e.shift && e.alt)
            {
                if (e.keyCode == KeyCode.Z)
                {
                    e.Use();
                    if (nearestNode != null)
                    {
                        Undo.DestroyObjectImmediate(nearestNode);
                    }
                }
                else if (e.keyCode == KeyCode.C)
                {
                    e.Use();
                    lastCratedNode = CreateNode();
                }
            }

        }

        if (e.isMouse && e.type == EventType.MouseDown && e.button == 0)
        {
            if (lineToggle.isEnabled)
            {
                if (!lineStart)
                {
                    if (nearestNode != null)
                    {
                        lineStart = nearestNode;
                    }
                }
                else
                {
                    var associeted = NearestAsscosieted();
                    if (associeted)
                    {
                        Undo.RecordObject(lineStart, "Connect associeted");
                        lineStart.associatedFeatures.Add(associeted);
                        lineStart = null;
                        return;
                    }

                    if (e.alt)
                    {
                        if (nearestNode)
                        {
                            Undo.RecordObjects(new Object[] { lineStart, nearestNode }, "Remove Line");
                            lineStart.neighbors = lineStart.neighbors.Where(n => n != nearestNode).ToList();
                            nearestNode.neighbors = nearestNode.neighbors.Where(n => n != lineStart).ToList();
                            lineStart = null;
                        }
                    }
                    else
                    {
                        if (NearestMouseNode(20))
                        {
                            if (nearestNode)
                            {
                                Undo.RecordObjects(new Object[] { lineStart, nearestNode }, "Add Line");
                                lineStart.Connect(nearestNode);
                                lineStart = null;
                            }
                        }
                        else
                        {
                            var newNode = CreateNode();

                            Undo.RecordObject(lineStart, "Add Line");
                            newNode.Connect(lineStart);
                            lineStart = newNode;
                        }
                    }
                }
            }
        }

        if (lineStart)
        {
            Handles.color = e.alt ? Color.red : Color.green;
            if (TryGetMousePos(out var mousePos))
            {
                Handles.DrawLine(lineStart.transform.position, mousePos);

                var associeted = NearestAsscosieted();
                if (associeted)
                {
                    Handles.color = Color.gray;
                    Handles.DrawLine(mousePos, associeted.transform.position);
                }
                else if ((e.alt && nearestNode) || NearestMouseNode(20))
                {
                    Handles.DrawLine(mousePos, (nearestNode ?? NearestMouseNode(20)).transform.position);
                }

                if (Vector2.Distance(lastMousePosRender, Event.current.mousePosition) > 5f)
                {
                    lastMousePosRender = Event.current.mousePosition;
                    SceneView.RepaintAll();
                }
            }
        }
    }


    void DeleteNode(PathNode node)
    {
        var fromNodes = FindObjectsOfType<PathNode>().Where(t => t.neighbors.Contains(node)).ToArray();

        Undo.RecordObjects(fromNodes.Concat(node.neighbors).Where(t => t).ToArray(), "delete node");

        foreach (var item in fromNodes)
        {
            item.neighbors.Remove(node);
            item.neighbors.AddRange(node.neighbors.Where(t => t));
            item.neighbors.AddRange(fromNodes);
            item.neighbors = item.neighbors.Distinct().Where(t => t != item).ToList();
        }

        foreach (var neighbord in node.neighbors.Where(t => t))
        {
            neighbord.neighbors.AddRange(fromNodes);
            neighbord.neighbors.AddRange(node.neighbors);
            neighbord.neighbors = neighbord.neighbors.Distinct().Where(t => t != neighbord).ToList();
        }
        Undo.DestroyObjectImmediate(node.gameObject);
    }

    PathNode lastCratedNode = null;
    void CreateNodeNearest()
    {
        var nearestNode = NearestMouseNode(float.MaxValue);
        if (nearestNode != null)
        {
            lastCratedNode = CreateNode();
            nearestNode.Connect(lastCratedNode);
        }
    }

    void CreateNodeLast()
    {
        var node = CreateNode();
        if (lastCratedNode)
        {
            node.Connect(lastCratedNode);
        }
        lastCratedNode = node;
    }

    Transform lastNode = null;
    PathNode CreateNode()
    {
        if (TryGetMousePos(out Vector3 pos))
        {
            var node = new GameObject("PathNode").AddComponent<PathNode>();
            node.transform.position = pos;


            if (lastNode)
            {
                node.transform.SetParent(lastNode.transform.parent);
            }
            else if (Selection.activeTransform)
            {
                node.transform.SetParent(Selection.activeTransform.GetComponent<PathNode>() ? Selection.activeTransform.parent : Selection.activeTransform);
            }
            else if (FindObjectOfType<PathParent>() != null)
            {
                node.transform.SetParent(FindObjectOfType<PathParent>().transform);
            }

            Undo.RegisterCreatedObjectUndo(node.gameObject, "create node");
            lastNode = node.transform;

            return node;
        }
        return null;
    }

    PathNode NearestMouseNode(float distance = 200)
    {
        if (TryGetMousePos(out Vector3 point))
        {
            var nodes = FindObjectsOfType<PathNode>();
            return nodes.Where(t => t.isActiveAndEnabled)
                        .OrderBy(t => (t.transform.position - point).sqrMagnitudeXY())
                        .Where(t => (t.transform.position - point).sqrMagnitudeXY() < distance)
                        .FirstOrDefault();
        }
        return null;
    }

    IMDF.FeatureMB NearestAsscosieted(float distance = 20)
    {
        if (TryGetMousePos(out Vector3 point))
        {
            var fratures = FindObjectsOfType<IMDF.Occupant>().Select(t => t as IMDF.FeatureMB)
                                        .Concat(FindObjectsOfType<IMDF.Attraction>())
                                        .Concat(FindObjectsOfType<IMDF.Amenity>())
                                        .Concat(FindObjectsOfType<IMDF.EnviromentAmenity>())
                                        .Concat(FindObjectsOfType<MonoBehaviour>()
                                                .Where(t => t.isActiveAndEnabled)
                                                .OfType<IOccupant>()
                                                .Where(t => t.hasOccupant)
                                                .Select(t => t as IMDF.FeatureMB)
                                                );
            return fratures.OrderBy(t => (t.transform.position - point).sqrMagnitudeXY())
                        .Where(t => (t.transform.position - point).sqrMagnitudeXY() < distance)
                        .FirstOrDefault();
        }
        return null;
    }

    bool TryGetMousePos(out Vector3 pos)
    {
        var mousePos = Event.current.mousePosition;
        var ray = HandleUtility.GUIPointToWorldRay(mousePos);

        var plane = new Plane(Vector3.forward, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            pos = ray.GetPoint(distance);
            return true;
        }
        pos = Vector3.zero;
        return false;
    }

    T[] FindObjectsOfType<T>() where T : Component
    {
        return StageUtility.GetCurrentStageHandle().FindComponentsOfType<T>();
    }

    T FindObjectOfType<T>() where T : Component
    {
        return StageUtility.GetCurrentStageHandle().FindComponentOfType<T>();
    }
}


[EditorToolbarElement(id, typeof(SceneView))]
class ToggleExample : EditorToolbarToggle
{
    public const string id = "ExampleToolbar/Toggle";
    public System.Action<bool> onToggle;
    bool isEnabled_ = false;
    public bool isEnabled
    {
        set
        {
            isEnabled_ = value;
            text = isEnabled ? "ON" : "OFF";
        }
        get
        {
            return isEnabled_;
        }
    }

    public ToggleExample()
    {
        this.RegisterValueChangedCallback(Test);
        isEnabled = isEnabled_;
    }

    void Test(ChangeEvent<bool> evt)
    {
        isEnabled = evt.newValue;
        onToggle?.Invoke(evt.newValue);
    }

}

public static class IMDFExtensions
{
    public static float sqrMagnitudeXY(this Vector3 v)
    {
        return v.x * v.x + v.y * v.y;
    }
}
