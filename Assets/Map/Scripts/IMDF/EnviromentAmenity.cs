using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    public class EnviromentAmenity : GeometryPoint
    {

        public enum DetailLevel
        {
            alwaysShowBig = 0,
            alwaysShow = 1,
            min = 2,
            hiddenMin = 3,
            alwaysShowMin = 4
        }

        public LocalizedName localizedName;
        public LocalizedName altName;
        public Feature.EnviromentAmenity.Category category;
        public DetailLevel detailLevel;


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 1);
        }
    }
}
