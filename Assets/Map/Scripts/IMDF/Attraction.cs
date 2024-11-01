using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    public class Attraction : GeometryPoint, IAnnotation
    {
        [Serializable]
        public class Authors
        {
            public bool enbled;

            public LocalizedName shortText;
            public LocalizedName title;
            public LocalizedNameMultiline description;
            public LocalizedName authorsTitle;
            public LocalizedName[] authors;
        }


        public Feature.Attraction.Category category;
        public LocalizedName localizedName;
        public LocalizedName altName;
        public LocalizedName shortName;
        public string image;
        public Building building;
        [Space]
        public LocalizedNameMultiline description;
        [Space]
        public Authors authors;

        Guid? IAnnotation.identifier => guid;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 1);
        }
    }
}
