using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IMDF
{
    [RequireComponent(typeof(RefferencePointEditor))]
    public class Unit : GeometryPolygon, IRefferencePoint
    {

        public LocalizedName localizedName;
        public LocalizedName altName;
        public Feature.Unit.Category category;
        public Feature.RestrictionCategory restriction = Feature.RestrictionCategory.nullable;

        public bool showDisplayPoint = true;


        [HideInInspector, SerializeField]
        Vector2 displayPoint_ = Vector2.zero;

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

        bool IRefferencePoint.showDisplayPoint => showDisplayPoint;

        public void NewObj()
        {
            GetComponent<PolygonGeometry>().color = new Color(Random.Range(0.17f, 0.4f), 0.17f, 0.4f);
            displayPoint = Vector2.zero;
        }



        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI2;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI2;
        }

        private GUIStyle guiStyle = new GUIStyle();
        void OnSceneGUI2(SceneView sceneView)
        {
            Handles.BeginGUI();
            float dist = Vector3.Distance(sceneView.camera.transform.position, transform.position);
            if (dist < 1000)
            {
                guiStyle.fontSize = (int)Mathf.Max(10, Mathf.Min(20, 20 - dist / 10));
                guiStyle.normal.textColor = Color.white;
                guiStyle.alignment = TextAnchor.UpperLeft;

                Handles.Label(transform.position + (showDisplayPoint ? new Vector3(displayPoint.x, displayPoint.y, 0) : Vector3.zero), string.IsNullOrWhiteSpace(altName.ru) ? "-" : altName.ru, guiStyle);
            }
            Handles.EndGUI();

        }

    }
}
