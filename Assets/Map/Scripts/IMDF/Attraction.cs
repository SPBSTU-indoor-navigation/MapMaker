using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    public class Attraction : GeometryPoint, IAnnotation
    {
        public Feature.Attraction.Category category;
        public LocalizedName localizedName;
        public LocalizedName altName;
        public LocalizedName shortName;
        public string image;
        public Building building;

        Guid? IAnnotation.identifier => guid;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 1);
        }
    }
}
