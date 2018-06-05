using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public class GenMeshComplexFace : GenMeshFace
    {
        private int[] triangles;

        public GenMeshComplexFace(GenMeshVertex[] vertices, int[] triangles) : base(vertices)
        {
            this.triangles = triangles;
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
            return new GenMeshComplexFace(Vertices, triangles);
        }

        public override GenMeshFace[] Extrude()
        {
            throw new System.NotImplementedException();
        }

        public override int[] GetTriangles()
        {
            return this.triangles.Select(e => Vertices.ElementAt(e).Index).ToArray();
        }

        public override GenMeshFace[] Subdivide(int numberOfCuts)
        {
            throw new System.NotImplementedException();
        }
    }
}
