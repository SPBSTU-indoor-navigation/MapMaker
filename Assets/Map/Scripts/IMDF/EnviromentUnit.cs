using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IMDF
{
    public class EnviromentUnit : GeometryPolygon, IRefferencePoint
    {

        public LocalizedName localizedName;
        public LocalizedName altName;
        public Feature.EnviromentUnit.Category category;

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
    }
}
