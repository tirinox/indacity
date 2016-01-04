using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace InDaCity
{
    public class GeomUtil : Object
    {
        public struct IntersectionData
        {
            public bool parallel;
            public Vector2 point;
            public int index1, index2;
        }

        public static IntersectionData LineIntersectionPoint(Vector2 ps1, Vector2 pe1, Vector2 ps2,
                                      Vector2 pe2)
        {
            var result = new IntersectionData();

            float A1 = pe1.y - ps1.y;
            float B1 = ps1.x - pe1.x;
            float C1 = A1 * ps1.x + B1 * ps1.y;

            float A2 = pe2.y - ps2.y;
            float B2 = ps2.x - pe2.x;
            float C2 = A2 * ps2.x + B2 * ps2.y;

            float delta = A1 * B2 - A2 * B1;
            if (delta == 0)
                result.parallel = true;
            else
            {
                result.point = new Vector2(
                    (B2 * C1 - B1 * C2) / delta,
                    (A1 * C2 - A2 * C1) / delta
                );

                result.parallel = false;
            }
            return result;
        }

        public static float PointOnLineParameter(Vector2 s, Vector2 e, Vector2 point)
        {
            var delta = e - s;
            if (delta.x != 0)
                return (point.x - s.x) / delta.x;
            else
                return (point.y - s.y) / delta.y;
        }

        public static Vector2 CenterOfPolygon(List<Vector2> vertices)
        {
            return vertices.Aggregate(Vector2.zero, (acc, x) => acc + x) / vertices.Count;
        }

        public static int ClassifyPointAgainstLine(Vector2 testPoint, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 c = testPoint, a = lineStart, b = lineEnd;
            float expr = ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x));
            return Mathf.Abs(expr) < 0.001f ? 0 : (expr < 0.0f ? -1 : 1); //(expr > 0.0f ? 1 : (expr < 0.0f ? -1 : 0));
        }

        public static Rect CalculateBoundingBox(List<Vector2> vertices)
        {
            float xmin = vertices[0].x,
            xmax = xmin,
            ymin = vertices[0].y,
            ymax = ymin;
            for (int i = 1; i < vertices.Count; ++i)
            {
                var v = vertices[i];
                if (xmax < v.x) xmax = v.x;
                else if (xmin > v.x) xmin = v.x;
                if (ymax < v.y) ymax = v.y;
                else if (ymin > v.y) ymin = v.y;
            }

            return new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public static Vector2 NormalToLine(Vector2 s, Vector2 e)
        {
            var d = e - s;
            return (new Vector2(d.y, -d.x)).normalized;
        }
    }
}
