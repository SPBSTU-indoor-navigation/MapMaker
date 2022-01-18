using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    public class EnviromentAmenity : GeometryPoint
    {

        public LocalizedName localizedName;
        public LocalizedName altName;
        public Feature.EnviromentAmenity.Category category;
        public int detailLevel;


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 1);
        }
    }
}
