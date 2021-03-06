using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{

    public class Venue : GeometryPolygon, IRefferencePoint, IAddress
    {
        public LocalizedName localizedName;
        public LocalizedName altName;
        public string hours;
        public string phone;
        public string website;

        public Feature.Venue.Category category;
        public Feature.RestrictionCategory restriction = Feature.RestrictionCategory.nullable;
        public AddressContainer address;
        public FeatureMB defaultPathBegin;

        [HideInInspector]
        public Vector2 displayPoint { get; set; }

        Address IAddress.address => address.address;

        private void Start()
        {
            GetComponent<PolygonGeometry>().color = new Color(0.17f, 0.17f, 0.17f);
        }

        public override void GenerateGUID()
        {
            base.GenerateGUID();
            address.GenerateGUID();
        }
    }
}
