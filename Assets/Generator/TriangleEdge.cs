using System.Collections.Generic;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public class TriangleEdge
    {
        public Vector2 PointA { get; private set; }
        public Vector2 PointB { get; private set; }

        public TriangleEdge(Vector2 a, Vector2 b)
        {
            this.PointA = a;
            this.PointB = b;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TriangleEdge))
            {
                return false;
            }

            var edge = (TriangleEdge)obj;

            return edge.PointA == this.PointA && edge.PointB == this.PointB;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal bool IsSharedWith(List<Triangle> otherTriangles, Triangle excluding)
        {
            foreach (var triangle in otherTriangles)
            {
                if (triangle != excluding)
                {
                    foreach (var edge in triangle.Edges)
                    {
                        if (edge.Equals(this))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
