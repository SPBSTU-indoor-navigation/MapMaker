using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class LR2Path : MonoBehaviour
{
    Vector3[] points = new Vector3[0];
    LineRenderer lr;

    List<PathNode> lastNodes = new List<PathNode>();

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (!lr) lr = GetComponent<LineRenderer>();

        bool change = false;

        Vector3[] newPoints = new Vector3[lr.positionCount];
        lr.GetPositions(newPoints);
        if (points.Length == lr.positionCount)
        {
            for (var i = 0; i < newPoints.Length; i++)
            {
                if (newPoints[i] != points[i])
                {
                    change = true;
                    break;
                }
            }
        }
        else
        {
            change = true;
        }


        points = newPoints;

        if (change)
        {
            var nodes = GetComponentsInChildren<PathNode>(true).ToList();
            if (nodes.Count == newPoints.Length)
            {
                for (int i = 0; i < newPoints.Length; i++)
                {
                    nodes[i].transform.position = transform.TransformPoint(newPoints[i]);
                }
            }
            else
            {
                nodes.ForEach(t => DestroyImmediate(t.gameObject));
                nodes.Clear();
                for (var i = 0; i < newPoints.Length; i++)
                {
                    var node = new GameObject("Node " + i).AddComponent<PathNode>();
                    nodes.Add(node);
                    node.transform.parent = transform;
                    node.transform.position = transform.TransformPoint(newPoints[i]);
                    if (i > 0)
                    {
                        node.Connect(nodes[i - 1]);
                    }
                }

                if (nodes.Count > 0)
                {
                    nodes[nodes.Count - 1].Connect(nodes[0]);
                }
            }
        }
    }
}
