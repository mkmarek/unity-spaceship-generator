using System;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public abstract class GenMeshFace
    {
        public abstract Vector3 Normal { get; }

        public abstract Vector2 Size { get; }

        public GenMeshVertex[] Vertices { get; private set; }
        public abstract float AspectRatio { get; }

        public int MaterialIndex { get; internal set; }
        public abstract float Width { get; }
        public abstract float Height { get; }

        public abstract GenMeshVertex LeftTop { get; }
        public abstract GenMeshVertex LeftBottom { get; }
        public abstract GenMeshVertex RightBottom { get; }
        public abstract GenMeshVertex RightTop { get; }

        public GenMeshFace(GenMeshVertex[] vertices)
        {
            this.Vertices = vertices;
        }

        public Vector3 CalculateCenterBounds()
        {
            var sum = Vector3.zero;

            foreach (var vertex in this.Vertices)
            {
                sum += vertex.Coordinates;
            }

            return sum / this.Vertices.Length;
        }

        public abstract GenMeshFace Clone();

        public abstract int[] GetTriangles();

        public abstract GenMeshFace[] Extrude();

        public abstract GenMeshFace[] Subdivide(int numberOfCuts);

        public abstract float Area();
    }
}
