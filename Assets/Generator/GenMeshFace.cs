using UnityEngine;

namespace ProceduralSpaceShip
{
    public class GenMeshFace
    {
        public Vector3 Normal
        {
            get
            {
                var a = RightBottom.Coordinates - LeftBottom.Coordinates;
                var b = LeftTop.Coordinates - LeftBottom.Coordinates;

                return Vector3.Cross(a, -b).normalized;
            }
        }

        public Vector2 Size
        {
            get
            {
                var width = Mathf.Abs((LeftTop.Coordinates - RightTop.Coordinates).magnitude);
                var height = Mathf.Abs(LeftTop.Coordinates.y - LeftBottom.Coordinates.y);

                return new Vector2(width, height);
            }
        }

        public GenMeshVertex[] Vertices { get; private set; }
        public float AspectRatio
        {
            get
            {
                var faceAspectRatio = Mathf.Max(
                    0.01f,
                    (LeftTop.Coordinates - RightTop.Coordinates).magnitude / (LeftTop.Coordinates - LeftBottom.Coordinates).magnitude);

                if (faceAspectRatio < 1.0f)
                {
                    faceAspectRatio = 1.0f / faceAspectRatio;
                }

                return faceAspectRatio;
            }
        }

        public GenMeshVertex LeftTop { get { return Vertices[0]; } }
        public GenMeshVertex LeftBottom { get { return Vertices[1]; } }
        public GenMeshVertex RightBottom { get { return Vertices[2]; } }
        public GenMeshVertex RightTop { get { return Vertices[3]; } }

        public int MaterialIndex { get; internal set; }

        public GenMeshFace()
        {
            this.Vertices = new GenMeshVertex[4];
        }

        public GenMeshFace(GenMeshVertex leftTop, GenMeshVertex leftBottom, GenMeshVertex rightBottom, GenMeshVertex rightTop) : this()
        {
            this.Vertices[0] = leftTop;
            this.Vertices[1] = leftBottom;
            this.Vertices[2] = rightBottom;
            this.Vertices[3] = rightTop;
        }

        public Vector3 CalculateCenterBounds()
        {
            return (LeftTop.Coordinates + LeftBottom.Coordinates + RightBottom.Coordinates + RightTop.Coordinates) / 4;
        }

        internal GenMeshFace Clone()
        {
            return new GenMeshFace(
                this.LeftTop.Clone(),
                this.LeftBottom.Clone(),
                this.RightBottom.Clone(),
                this.RightTop.Clone()
            );
        }

        public int[] GetTriangles()
        {
            return new int[]
            {
                LeftTop.Index,
                RightBottom.Index,
                LeftBottom.Index,

                LeftTop.Index,
                RightTop.Index,
                RightBottom.Index,
            };
        }
    }
}
