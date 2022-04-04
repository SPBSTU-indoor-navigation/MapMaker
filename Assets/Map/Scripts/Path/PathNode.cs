using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


public class PathNode : IMDF.GeometryPoint
{
    public List<PathNode> neighbors = new List<PathNode>();
    public List<IMDF.FeatureMB> associatedFeatures = new List<IMDF.FeatureMB>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Selection.gameObjects.Contains(gameObject) ? Color.blue : Color.yellow;
        Handles.color = Gizmos.color;
        Gizmos.DrawSphere(transform.position, 1f);
        Handles.DrawSolidDisc(transform.position, Vector3.forward, 0.6f);
    }

    public void Connect(PathNode node, bool biDirectinal = true)
    {
        if (!neighbors.Contains(node))
            neighbors.Add(node);

        if (biDirectinal)
            node.neighbors.Add(this);

        neighbors = neighbors.Where(t => t != null && t != this).ToList();
    }

    public void Fix()
    {
        neighbors = neighbors.Where(t => t != null && t != this).ToList();
    }
}
