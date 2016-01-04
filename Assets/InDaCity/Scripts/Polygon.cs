using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;

namespace InDaCity
{
    public class Polygon : Object
    {
        private List<Vector2> vertices;
        public Vector2 center;
        public float distance;
        public bool valid = false;
        public Rect boundingBox;

        public float height;

        public List<Vector2> Vertices
        {
            get { return vertices; }
            set {
                vertices = value;
                valid = vertices.Count >= 3;
                if (valid)
                {
                    center = GeomUtil.CenterOfPolygon(vertices);
                    vertices = vertices.OrderBy(point => Mathf.Atan2(point.x - center.x, point.y - center.y)).ToList();
                    distance = center.magnitude;
                    boundingBox = GeomUtil.CalculateBoundingBox(vertices);
                }
            }
        }

        public bool ContainsPoint(Vector2 point)
        {
            for(int i = 0; i < vertices.Count; ++i)
            {
                if (GeomUtil.ClassifyPointAgainstLine(point, vertices[i], vertices[(i + 1) % vertices.Count]) > 0)
                    return false;
            }
            return true;
        }

        public float Area()
        {
            var points = new List<Vector2>(vertices);
            points.Add(points[0]);
            return Mathf.Abs(points.Take(points.Count - 1)
               .Select((p, i) => (points[i + 1].x - p.x) * (points[i + 1].y + p.y))
               .Sum() / 2);
        }
      
        public List<Vector2> CycledVertices()
        {
            var result = new List<Vector2> (vertices);
            result.Add(vertices[0]);
            return result;
        }



        public Polygon(List<Vector2> vertices)
        {
            Vertices = vertices;
        }

        public Polygon(Site site)
        {
            const float eps = 0.001f;

            var newVertices = new List<Vector2>(site.Edges.Count + 1);

            foreach (var edge in site.Edges)
            {
                if (edge.ClippedEnds == null)
                    return;

                var p0 = edge.ClippedEnds[LR.LEFT];
                var p1 = edge.ClippedEnds[LR.RIGHT];

                var begin = new Vector2(p0.x, p0.y);
                var end = new Vector2(p1.x, p1.y);

                bool hasBegin = false;
                bool hasEnd = false;
                foreach (var point in newVertices)
                {
                    if ((point - begin).sqrMagnitude < eps)
                    {
                        hasBegin = true;
                    }
                    if ((point - end).sqrMagnitude < eps)
                    {
                        hasEnd = true;
                    }
                }
                if (!hasBegin)
                    newVertices.Add(begin);
                if (!hasEnd)
                    newVertices.Add(end);
            }

            Vertices = newVertices;
        }

        public void Shrink(float factor = 0.2f)
        {
            if (!valid) return;
            Vertices = vertices.Select(vert => vert + (center - vert) * factor).ToList();
        }

        internal struct Line
        {
            public Vector2 s, e, n;
            public Line(Vector2 ss, Vector2 ee) { s = ss; e = ee; n = GeomUtil.NormalToLine(s, e); }
        }

        public void ShrinkAbs(float absShift)
        {
            if (!valid) return;

            var edges = new List<Line>(vertices.Count - 1);

            var cycledVert = CycledVertices();
            for (int i = 1; i < cycledVert.Count; ++i)
            {
                var line = new Line(cycledVert[i - 1], cycledVert[i]);
                line.s += line.n * absShift;
                line.e += line.n * absShift;
                edges.Add(line);
            }

            edges.Add(edges[0]);

            var results = new List<Vector2>(vertices.Count);

            for(int i = 1; i < edges.Count; ++i)
            {
                results.Add(GeomUtil.LineIntersectionPoint(edges[i - 1].s, edges[i - 1].e, edges[i].s, edges[i].e).point);
            }

            Vertices = results;
            
        }

        public Polygon IntersectionWithPolygon(Polygon other)
        {
            var v1 = vertices;
            var v2 = other.vertices;

            var allVertices = new List<Vector2>(v1);
            allVertices.AddRange(v2);

            var n1 = v1.Count;
            var n2 = v2.Count;
            
            for(int i = 0; i < n1; ++i)
            {
                for(int j = 0; j < n2; ++j)
                {
                    var edge1_s = v1[i];
                    var edge1_e = v1[(i + 1) % n1];
                    var edge2_s = v2[j];
                    var edge2_e = v2[(j + 1) % n2];

                    var intersetion = GeomUtil.LineIntersectionPoint(edge1_s, edge1_e, edge2_s, edge2_e);
                    if(!intersetion.parallel)
                    {
                        var t1 = GeomUtil.PointOnLineParameter(edge1_s, edge1_e, intersetion.point);
                        var t2 = GeomUtil.PointOnLineParameter(edge2_s, edge2_e, intersetion.point);
                        if (0.0f <= t1 && t1 <= 1.0f && 0.0f <= t2 && t2 <= 1.0f)
                        {
                            allVertices.Add(intersetion.point);
                        }
                    }
                }
            }
            
            allVertices = allVertices.Where(v => ContainsPoint(v) && other.ContainsPoint(v) ).ToList();

            var result = new Polygon(allVertices);
            result.height = height;
            return result;
        }

        public List<Polygon> SplitPolygon(Vector2 s, Vector2 e, bool infinite = true)
        {
            var intersections = new List<GeomUtil.IntersectionData>();
            for (int i = 0; i < vertices.Count; ++i)
            {
                int iNext = (i + 1) % vertices.Count;
                var intersection = GeomUtil.LineIntersectionPoint(vertices[i], vertices[iNext], s, e);
                if (intersection.parallel == false)
                {
                    var t = GeomUtil.PointOnLineParameter(vertices[i], vertices[iNext], intersection.point);
                    var t2 = infinite ? 0.5 : GeomUtil.PointOnLineParameter(s, e, intersection.point);

                    if (0.0f <= t && t < 1.0f && 0.0f <= t2 && t2 <= 1.0f)
                    {
                        intersection.index1 = i;
                        intersection.index2 = iNext;
                        intersections.Add(intersection);
                    }
                }
            }

            if (intersections.Count != 2)
                return null;
            else {
                var output = new List<Polygon>(2);

                var isect1 = intersections[0];
                var isect2 = intersections[1];

                var leftPolygon = new List<Vector2>();

                leftPolygon.Add(isect1.point);
                leftPolygon.Add(isect2.point);
                for (int i = isect2.index2; i != isect1.index2; i = (i + 1) % vertices.Count)
                    leftPolygon.Add(vertices[i]);

                leftPolygon = leftPolygon.Distinct().ToList();
                var lp = new Polygon(leftPolygon);
                lp.height = height;
                output.Add(lp);

                var rightPolygon = new List<Vector2>();

                rightPolygon.Add(isect2.point);
                rightPolygon.Add(isect1.point);
                for (int i = isect1.index2; i != isect2.index2; i = (i + 1) % vertices.Count)
                    rightPolygon.Add(vertices[i]);

                rightPolygon = rightPolygon.Distinct().ToList();
                var rp = new Polygon(rightPolygon);
                rp.height = height;
                output.Add(rp);

                return output;
            }          
        }
    }
}

