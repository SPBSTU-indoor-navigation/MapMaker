using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


public class PathNode : IMDF.GeometryPoint
{
    public List<PathNode> neighbors = new List<PathNode>();
    public List<IMDF.FeatureMB> associatedFeatures = new List<IMDF.FeatureMB>();

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("path");
    }

    // private void Update()
    // {

    //     gameObject.layer = LayerMask.NameToLayer("path");
    //     Debug.Log(gameObject.layer);
    // }

    private void OnDrawGizmos()
    {
        // Gizmos.color = Selection.gameObjects.Contains(gameObject) ? Color.blue : Color.yellow;
        // Handles.color = Gizmos.color;
        // Gizmos.DrawSphere(transform.position, 0.1f);
        // Handles.DrawSolidDisc(transform.position, Vector3.forward, 0.6f);

        // neighbors = neighbors.Where(t => t != null).ToList();
        // Gizmos.color = Color.yellow;
        // foreach (var neighbors in neighbors)
        // {
        //     Gizmos.DrawLine(transform.position, neighbors.transform.position);
        // }

        // Gizmos.color = Color.gray;
        // foreach (var associeted in associatedFeatures)
        // {
        //     Gizmos.DrawLine(transform.position, associeted.transform.position);
        // }
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
