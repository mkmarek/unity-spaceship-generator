using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public class Triangle
    {
        public Vector2 A { get; set; }
        public Vector2 B { get; set; }
        public Vector2 C { get; set; }
        public IEnumerable<TriangleEdge> Edges { get; private set; }
        public Vector2 CircumCenter { get; private set; }
        public float CircumRadius { get; private set; }

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            this.A = a;
            this.B = b;
            this.C = c;

            CreateEdges(a, b, c);
            CreateCircumCenter(a, b, c);
        }

        private void CreateEdges(Vector2 a, Vector2 b, Vector2 c)
        {
            var edges = new List<TriangleEdge>();

            edges.Add(new TriangleEdge(a, b));
            edges.Add(new TriangleEdge(b, c));
            edges.Add(new TriangleEdge(a, c));

            this.Edges = edges;
        }

        private float Pow2(float a)
        {
            return a * a;
        }

        private void CreateCircumCenter(Vector2 a, Vector2 b, Vector2 c)
        {
            var d = 2f * ((a.x * (b.y - c.y)) + (b.x * (c.y - a.y)) + (c.x * (a.y - b.y)));

            var uX =
                (((Pow2(a.x) + Pow2(a.y)) * (b.y - c.y)) +
                ((Pow2(b.x) + Pow2(b.y)) * (c.y - a.y)) +
                ((Pow2(c.x) + Pow2(c.y)) * (a.y - b.y))) / d;

            var uY =
                (((Pow2(a.x) + Pow2(a.y)) * (c.x - b.x)) +
                ((Pow2(b.x) + Pow2(b.y)) * (a.x - c.x)) +
                ((Pow2(c.x) + Pow2(c.y)) * (b.x - a.x))) / d;

            this.CircumCenter = new Vector2(uX, uY);
            this.CircumRadius = Vector2.Distance(this.CircumCenter, a);
        }

        public static Triangle CreateContaining(IEnumerable<Vector2> points)
        {
            var center = GetCenter(points);
            var mostDistantNodeOnX = GetMostDistantNodeOnX(points, center);
            var mostDistantNodeOnY = GetMostDistantNodeOnY(points, center);

            var xScale = Mathf.Abs(center.x - mostDistantNodeOnX.x);
            var yScale = Mathf.Abs(center.y - mostDistantNodeOnY.y);

            return new Triangle(
                new Vector2(center.x - xScale * 100, center.y + yScale * 100),
                new Vector2(center.x, center.y - yScale * 100),
                new Vector2(center.x + xScale * 100, center.y + yScale * 100)
                );
        }

        private static Vector2 GetMostDistantNodeOnX(IEnumerable<Vector2> points, Vector2 center)
        {
            var point = points.First();
            var dist = 0f;

            foreach (var p in points)
            {
                if (Mathf.Abs(center.x - p.x) > dist)
                {
                    point = p;
                    dist = Mathf.Abs(center.x - p.x);
                }
            }

            return point;
        }

        private static Vector2 GetMostDistantNodeOnY(IEnumerable<Vector2> points, Vector2 center)
        {
            var point = points.First();
            var dist = 0f;

            foreach (var p in points)
            {
                if (Mathf.Abs(center.y - p.y) > dist)
                {
                    point = p;
                    dist = Mathf.Abs(center.y - p.y);
                }
            }

            return point;
        }

        private static Vector2 GetCenter(IEnumerable<Vector2> points)
        {
            var center = Vector2.zero;

            foreach (var point in points)
            {
                center += point;
            }

            center /= points.Count();

            return center;
        }

        public bool IsInsideOfCircumCircle(Vector2 pos)
        {
            return Vector2.Distance(this.CircumCenter, pos) <= this.CircumRadius;
        }

        public static Triangle CreateFrom(TriangleEdge edge, Vector2 point)
        {
            return new Triangle(point, edge.PointA, edge.PointB);
        }

        public bool ContainsAnyPointFrom(Triangle otherTriangle)
        {
            var points = new List<Vector2>()
            {
                this.A,
                this.B,
                this.C,
            };

            var points2 = new List<Vector2>()
            {
                otherTriangle.A,
                otherTriangle.B,
                otherTriangle.C,
            };

            return points.Any(p => points2.Any(p2 => p == p2));
        }
    }
}
