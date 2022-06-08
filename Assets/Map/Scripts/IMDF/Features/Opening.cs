using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace IMDF
{
    [RequireComponent(typeof(RefferencePointEditor))]
    [ExecuteInEditMode]
    [CanEditMultipleObjects]
    public class Opening : GeometryLine, IRefferencePoint
    {
        public LocalizedName localizedName;
        public LocalizedName altName;

        public Feature.Opening.Category category;

        [Space]
        public Feature.Opening.Door.Type type = Feature.Opening.Door.Type.open;
        public Feature.Opening.Door.Material material = Feature.Opening.Door.Material.metal;
        public bool automatic;

        [Space]
        public bool showDisplayPoint = false;
        bool IRefferencePoint.showDisplayPoint => showDisplayPoint;

        [HideInInspector, SerializeField]
        Vector2 displayPoint_ = Vector2.right;

        [Space]
        [Header("PathNode")]
        public bool generatePathNode;
        public bool single;
        public float distance = 1;
        [SerializeField]
        private Transform[] pathNode;

        public Vector2 displayPoint
        {
            get
            {
                return displayPoint_;
            }
            set
            {
                displayPoint_ = value;
            }
        }

        private void OnValidate()
        {
            if (lastGeneratePathNode != generatePathNode)
            {
                refreshPath = true;
                lastGeneratePathNode = generatePathNode;
            }
            refreshPath = true;
        }

        bool refreshPath = false;
        bool lastGeneratePathNode = false;
        Vector3 lastPosition = Vector3.zero;
        Vector3 secondPoint = Vector3.zero;
        void Update()
        {
            if (transform.position != lastPosition)
            {
                lastPosition = transform.position;
                refreshPath = true;
            }

            if (geometryLineEdit.secondPoint != secondPoint)
            {
                secondPoint = geometryLineEdit.secondPoint;
                refreshPath = true;
            }


            if (refreshPath)
            {
                RefreshPath();
            }
        }

        void RefreshPath()
        {
            refreshPath = false;

            if (generatePathNode)
            {
                if (pathNode.Length == 0)
                {
                    pathNode = new Transform[single ? 1 : 2];
                    pathNode[0] = new GameObject("PathNode0").transform;
                    pathNode[0].GetComponent<PathNode>().associatedFeatures.Add(GetComponentInParent<Unit>());
                    pathNode[0].GetComponent<PathNode>().associatedFeatures = pathNode[0].GetComponent<PathNode>().associatedFeatures.Distinct().ToList();
                    pathNode[0].gameObject.AddComponent<PathNode>();
                    DrawIcon(pathNode[0].gameObject, 4);
                    pathNode[0].SetParent(transform);

                    if (!single)
                    {
                        pathNode[1] = new GameObject("PathNode1").transform;
                        DrawIcon(pathNode[1].gameObject, 4);
                        pathNode[1].SetParent(transform);
                        pathNode[1].gameObject.AddComponent<PathNode>();

                        pathNode[0].GetComponent<PathNode>().Connect(pathNode[1].GetComponent<PathNode>());
                    }
                }
                else
                {
                    if (pathNode.Length == 2 && single)
                    {
                        DestroyImmediate(pathNode[1].gameObject);
                        pathNode = new Transform[1] { pathNode[0] };
                    }
                    else if (pathNode.Length == 1 && !single)
                    {
                        pathNode = new Transform[2] { pathNode[0], new GameObject("PathNode1").transform };
                        DrawIcon(pathNode[1].gameObject, 4);
                        pathNode[1].SetParent(transform);
                        pathNode[1].gameObject.AddComponent<PathNode>();
                        pathNode[0].GetComponent<PathNode>().Connect(pathNode[1].GetComponent<PathNode>());
                    }
                }

                pathNode[0].localPosition = geometryLineEdit.secondPoint / 2;
                // pathNode[0].GetComponent<PathNode>().associatedFeatures.Add(GetComponentInParent<Unit>());
                // pathNode[0].GetComponent<PathNode>().associatedFeatures = pathNode[0].GetComponent<PathNode>().associatedFeatures.Distinct().ToList();
                if (!single) pathNode[1].localPosition = new Vector3(-geometryLineEdit.secondPoint.y, geometryLineEdit.secondPoint.x, 0).normalized * distance + pathNode[0].localPosition;
            }
            else
            {
                if (pathNode.Length != 0)
                {
                    foreach (var item in pathNode)
                    {
                        DestroyImmediate(item.gameObject);
                    }

                    pathNode = new Transform[0];
                }
            }
        }

        private void DrawIcon(GameObject gameObject, int idx)
        {
            var largeIcons = GetTextures("sv_icon_dot", "_pix16_gizmo", 0, 8);
            var icon = largeIcons[idx];
            EditorGUIUtility.SetIconForObject(gameObject, icon.image as Texture2D);
        }

        private GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
        {
            GUIContent[] array = new GUIContent[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = EditorGUIUtility.IconContent(baseName + (startIndex + i) + postFix);
            }
            return array;
        }

    }
}
