using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;

namespace InDaCity
{

    public class City : Object
    {
        public List<Polygon> results = new List<Polygon>();

        public int nPoints = 200;
        public float maxSize = 200.0f;
        public int lloydIters = 2;
        public float minArea = 5.0f;
        public float maxHeight = 50.0f;
        public float quarterSize = 10.0f;

        public float streetWidthMin = 1.0f;
        public float streetWidthMid = 2.0f;
        public float streetWidthMax = 4.0f;

        float Rand(float amp)
        {
            float x = Random.Range(0, amp);
            return Random.Range(-x, x);
        }
        
        protected float ComputeHeight(Polygon polygon)
        {
            var mh = Random.Range(0, 10) > 8 ? maxHeight : 0.5f * maxHeight;
            return Random.Range(1, mh * (1 - polygon.distance / maxSize));
        }

        protected bool DiscardByDistance(float distance)
        {
            return (distance > 0.97 || (distance > 0.8 && Random.Range(0, 10) < 5));
        }

        private List<Polygon> QuarterlyDivide(Polygon input)
        {
            var bb = input.boundingBox;

            var xNum = (int) Mathf.Ceil(bb.width / quarterSize);
            var yNum = (int) Mathf.Ceil(bb.height / quarterSize);

            var dx = (bb.width - streetWidthMin * (xNum - 1)) / xNum;
            var dy = (bb.height - streetWidthMin * (yNum - 1)) / yNum;

            var results = new List<Polygon>();

            float x = bb.x;
            for (int xi = 0; xi < xNum; ++xi)
            {
                float y = bb.y;
                for (int yi = 0; yi < yNum; ++yi)
                {
                    var q = new Polygon(new List<Vector2>()
                    {
                        new Vector2(x, y),
                        new Vector2(x, y + dy),
                        new Vector2(x + dx, y + dy),
                        new Vector2(x + dx, y)
                    });
       
                    var qCut = q.IntersectionWithPolygon(input);
                    if (qCut.valid && qCut.Area() >= minArea)
                    {
                        qCut.height = ComputeHeight(qCut);
                        results.Add(qCut);
                    }

                    y += dy + streetWidthMin;
                }
                x += dx + streetWidthMin;
            }
            return results;
        }

        public void CalculateOneQuarterForTest()
        {
            results.Clear();

            var l = new List<Vector2>();
            var n = Random.Range(3, 4);
            for (int i = 0; i < n; ++i)
                l.Add(new Vector2(Random.Range(-20.0f, 20.0f), Random.Range(-20.0f, 20.0f)));
            var poly = new Polygon(l);
        
            results.AddRange(QuarterlyDivide(poly));

            poly.height = 2.0f;
            results.Add(poly);
        }

        public void CalculatePolygons()
        {
            results.Clear();

            // generate random points for Voronoi

            var points = new List<Vector2f>();
            for (int i = 0; i < nPoints; ++i)
            {
                points.Add(new Vector2f(Random.Range(-maxSize, maxSize), Random.Range(-maxSize, maxSize)));
            }

            // perform Voronoi space division

            var voronoi = new Voronoi(points, new Rectf(-maxSize, -maxSize, maxSize * 2.0f, maxSize * 2.0f), lloydIters);

            // for each site (polygon)

            foreach (var kv in voronoi.SitesIndexedByLocation)
            {
                var location = kv.Key;
                var site = kv.Value;

                var distance = location.magnitude / maxSize;

                // if too far from the center -discard it
                if (DiscardByDistance(distance))
                    continue;

                // convert Site to Polygon
                var polygon = new Polygon(site);
                if (polygon.valid)
                {
                    // make streets
                    polygon.ShrinkAbs(streetWidthMid * 0.5f);

                    var quartars = QuarterlyDivide(polygon);
                    results.AddRange(quartars);
                }
            }
        }

    }
}