using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    [RequireComponent(typeof(RefferencePointEditor))]
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

    }
}
