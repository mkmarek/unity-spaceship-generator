using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public class GenMeshTriangleFace : GenMeshFace
    {
        public GenMeshTriangleFace(GenMeshVertex a, GenMeshVertex b, GenMeshVertex c) : base(new GenMeshVertex[] { a, b, c })
        {
        }

        public override Vector3 Normal
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public override Vector2 Size { get { throw new System.NotImplementedException(); } }

        public override float AspectRatio { get { throw new System.NotImplementedException(); } }

        public override float Width { get { throw new System.NotImplementedException(); } }

        public override float Height { get { throw new System.NotImplementedException(); } }

        public override GenMeshVertex LeftTop { get { throw new System.NotImplementedException(); } }

        public override GenMeshVertex LeftBottom { get { throw new System.NotImplementedException(); } }

        public override GenMeshVertex RightBottom { get { throw new System.NotImplementedException(); } }

        public override GenMeshVertex RightTop { get { throw new System.NotImplementedException(); } }

        public override float Area()
        {
            throw new System.NotImplementedException();
        }

        public override GenMeshFace Clone()
        {
            return new GenMeshTriangleFace(this.Vertices[0].Clone(), this.Vertices[1].Clone(), this.Vertices[2].Clone());
        }

        public override GenMeshFace[] Extrude()
        {
            throw new System.NotImplementedException();
        }

        public override int[] GetTriangles()
        {
            return new int[]
            {
                this.Vertices[0].Index,
                this.Vertices[1].Index,
                this.Vertices[2].Index
            };
        }

        public override GenMeshFace[] Subdivide(int numberOfCuts)
        {
            var faces = new List<GenMeshFace>() { this };

            for (var i = 0; i < numberOfCuts; i++)
            {
                var tmpFaces = faces.ToList();
                faces.Clear();

                foreach (var face in tmpFaces.ToList())
                {
                    var a = GetMidpoint(face.Vertices[1], face.Vertices[0]);
                    var b = GetMidpoint(face.Vertices[2], face.Vertices[1]);
                    var c = GetMidpoint(face.Vertices[0], face.Vertices[2]);


                    faces.Add(new GenMeshTriangleFace(face.Vertices[0], new GenMeshVertex(a), new GenMeshVertex(c)));
                    faces.Add(new GenMeshTriangleFace(face.Vertices[1], new GenMeshVertex(b), new GenMeshVertex(a)));
                    faces.Add(new GenMeshTriangleFace(face.Vertices[2], new GenMeshVertex(c), new GenMeshVertex(b)));
                    faces.Add(new GenMeshTriangleFace(new GenMeshVertex(a), new GenMeshVertex(b), new GenMeshVertex(c)));
                }
            }

            return faces.ToArray();
        }

        private static Vector3 GetMidpoint(GenMeshVertex a, GenMeshVertex b)
        {
            var p = (a.Coordinates + b.Coordinates) / 2;

            var length = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);

            return new Vector3(p.x / length, p.y / length, p.z / length);
        }
    }
}
