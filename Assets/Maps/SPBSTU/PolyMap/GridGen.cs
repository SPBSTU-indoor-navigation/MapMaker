using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteAlways]
public class GridGen : MonoBehaviour
{
    public GameObject pref;
    public Vector2 size;
    public Vector2 step;

    private void OnEnable()
    {
        foreach (var item in GetComponentsInChildren<Transform>().Where(t => t != transform))
        {
            DestroyImmediate(item.gameObject);
        }

        for (var i = -size.x; i < size.x; i += step.x)
        {
            for (var j = -size.y; j < size.y; j += step.y)
            {
                var go = Instantiate(pref, new Vector3(i, j, 0), Quaternion.identity, gameObject.transform);
            }
        }
    }
}
