using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathParent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {

        foreach (var node in GetComponentsInChildren<PathNode>(true))
        {
            node.neighbors = node.neighbors.Where(t => t != null).ToList();
            Gizmos.color = Color.yellow;
            foreach (var neighbors in node.neighbors)
            {
                Gizmos.DrawLine(node.transform.position, neighbors.transform.position);
            }

            Gizmos.color = Color.gray;
            foreach (var associeted in node.associatedFeatures)
            {
                Gizmos.DrawLine(node.transform.position, associeted.transform.position);
            }
        }
    }
}
