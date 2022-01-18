using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    public class Amenity : GeometryPoint
    {
        public Feature.Amenity.CategoryMin category;
        public LocalizedName localizedName;
        public LocalizedName altName;
        public Unit[] units;

        [Space]
        public string hours;
        public string phone;
        public string website;
        public Address address;

        [Space]
        public int detailLevel;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 1);
        }
    }
}
