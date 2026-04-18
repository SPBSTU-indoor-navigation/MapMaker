using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace IMDF
{
    public class DashedDetailLine : DetailLine
    {
        public float length = 1;
        public float spacing = 1;


        public override List<IMDF.Feature.Point[]> Lines()
        {
            var lines = GetComponentsInChildren<LineRenderer>();
            var res = new List<IMDF.Feature.Point[]>();

            foreach (var line in lines)
            {
                Vector3[] points = new Vector3[line.positionCount];
                line.GetPositions(points);

                var currentSegment = new List<Vector3>();
                var currentLength = 0f;
                var targetLength = length;
                var isSpace = false;

                currentSegment.Add(points[0]);
                var lastPoint = points[0];
                var index = 1;

                while (index < points.Length)
                {
                    var target = points[index];
                    var dist = Vector3.Distance(lastPoint, target);

                    if (currentLength + dist < targetLength)
                    {
                        currentLength += dist;
                        if (!isSpace)
                            currentSegment.Add(target);
                        lastPoint = target;
                        index++;
                        continue;
                    }

                    var point = Vector3.Lerp(lastPoint, target, (targetLength - currentLength) / dist);
                    lastPoint = point;

                    if (!isSpace)
                    {
                        currentSegment.Add(point);
                        res.Add(currentSegment.Select(t => GeoMap.CalculateGeo(line.transform.TransformPoint(t)).GetPoint()).ToArray());
                        currentSegment.Clear();
                        currentLength = 0;
                        targetLength = spacing;
                        isSpace = true;
                    }
                    else
                    {
                        currentSegment.Add(point);
                        currentLength = 0;
                        targetLength = length;
                        isSpace = false;
                    }
                }

                if (currentSegment.Count > 0)
                {
                    currentSegment.Add(points[points.Length - 1]);
                    res.Add(currentSegment.Select(t => GeoMap.CalculateGeo(line.transform.TransformPoint(t)).GetPoint()).ToArray());
                }
            }

            return res;
        }

    }
}
