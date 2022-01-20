using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace IMDF
{
    public class DetailLine : FeatureMB
    {
        public IMDF.Feature.EnviromentDetail.Category category;
        public List<IMDF.Feature.Point[]> Lines()
        {
            var lines = GetComponentsInChildren<LineRenderer>();
            var res = new List<IMDF.Feature.Point[]>();

            foreach (var line in lines)
            {
                Vector3[] points = new Vector3[line.positionCount];
                line.GetPositions(points);

                res.Add(points.Select(t => GeoMap.CalculateGeo(line.transform.TransformPoint(t)).GetPoint()).ToArray());
            }


            return res;
        }

    }
}
