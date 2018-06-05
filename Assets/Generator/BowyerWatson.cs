using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public class BowyerWatson
    {
        public static int[] GetTris(IEnumerable<Vector2> points, bool reverse = false)
        {
            var pointsList = points.ToList();
            var paths = new List<int>();
            var triangulation = GetTriangles(points);

            var insertedEdges = new List<TriangleEdge>();

            foreach (var triangle in triangulation)
            {
                if (!reverse)
                {
                    paths.Add(pointsList.IndexOf(triangle.A));
                    paths.Add(pointsList.IndexOf(triangle.B));
                    paths.Add(pointsList.IndexOf(triangle.C));
                }
                else
                {
                    paths.Add(pointsList.IndexOf(triangle.C));
                    paths.Add(pointsList.IndexOf(triangle.B));
                    paths.Add(pointsList.IndexOf(triangle.A));
                }
            }

            return paths.ToArray();
        }

        public static List<Triangle> GetTriangles(IEnumerable<Vector2> points)
        {
            var triangulation = new List<Triangle>();

            // add super triangle
            var superTriangle = Triangle.CreateContaining(points);
            triangulation.Add(superTriangle);

            foreach (var point in points)
            {
                var badTriangles = new List<Triangle>();

                foreach (var triangle in triangulation)
                {
                    if (triangle.IsInsideOfCircumCircle(point))
                    {
                        badTriangles.Add(triangle);
                    }
                }

                var polygon = new List<TriangleEdge>();
                foreach (var triangle in badTriangles)
                {
                    foreach (var edge in triangle.Edges)
                    {
                        if (!edge.IsSharedWith(badTriangles, triangle))
                        {
                            polygon.Add(edge);
                        }
                    }
                }

                foreach (var triangle in badTriangles)
                {
                    triangulation.Remove(triangle);
                }

                foreach (var edge in polygon)
                {
                    triangulation.Add(Triangle.CreateFrom(edge, point));
                }
            }

            foreach (var triangle in triangulation.ToList())
            {
                if (triangle.ContainsAnyPointFrom(superTriangle))
                {
                    triangulation.Remove(triangle);
                }
            }

            return triangulation;
        }
    }
}
