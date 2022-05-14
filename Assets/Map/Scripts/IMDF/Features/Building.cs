using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    [RequireComponent(typeof(RefferencePointEditor))]
    public class Building : GeometryPolygon, IRefferencePoint, IAddress
    {
        public LocalizedName localizedName;
        public LocalizedName altName;
        public AddressContainer addressId;
        public Feature.Building.Category category = Feature.Building.Category.unspecified;
        public Feature.RestrictionCategory restriction = Feature.RestrictionCategory.nullable;

        public float rotation;
        public bool showDisplayPoint = false;
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

        [Space]
        public PolygonGeometry aerial;
        public PolygonGeometry subterranean;


        bool IRefferencePoint.showDisplayPoint => showDisplayPoint;

        Address IAddress.address => addressId.address;

        void Start()
        {
            // GetComponent<PolygonGeometry>().color = new Color(0.33f, 0.33f, 0.33f);
        }

        public override void GenerateGUID()
        {
            base.GenerateGUID();
            addressId.GenerateGUID();
        }

    }


}
