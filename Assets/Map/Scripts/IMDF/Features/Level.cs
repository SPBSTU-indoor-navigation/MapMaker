using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    [RequireComponent(typeof(RefferencePointEditor))]
    public class Level : GeometryPolygon, IRefferencePoint
    {
        public LocalizedName localizedName;
        public LocalizedName shortName;
        public int ordinal = 0;
        public bool outdoor = false;
        public Feature.Level.Category category = Feature.Level.Category.unspecified;
        public Building[] buildings;

        public bool showDisplayPoint = false;
        [HideInInspector]
        public Vector2 displayPoint { get; set; }

        bool IRefferencePoint.showDisplayPoint => showDisplayPoint;

        private void Start()
        {
            if (buildings == null || buildings.Length == 0)
                buildings = GetComponentsInParent<Building>();

            GetComponent<PolygonGeometry>().color = new Color(0.17f, 0.4f, 0.17f);
            displayPoint = transform.position;
        }
    }
}
