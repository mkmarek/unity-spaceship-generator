using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public class GenMeshSquareFace : GenMeshFace
    {
        public override Vector3 Normal
        {
            get
            {
                var a = RightBottom.Coordinates - LeftBottom.Coordinates;
                var b = LeftTop.Coordinates - LeftBottom.Coordinates;

                return Vector3.Cross(a, -b).normalized;
            }
        }

        public override Vector2 Size
        {
            get
            {
                var width = Mathf.Abs((LeftTop.Coordinates - RightTop.Coordinates).magnitude);
                var height = Mathf.Abs(LeftTop.Coordinates.y - LeftBottom.Coordinates.y);

                return new Vector2(width, height);
            }
        }

        public override float AspectRatio
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

        public override GenMeshVertex LeftTop { get { return Vertices[0]; } }
        public override GenMeshVertex LeftBottom { get { return Vertices[1]; } }
        public override GenMeshVertex RightBottom { get { return Vertices[2]; } }
        public override GenMeshVertex RightTop { get { return Vertices[3]; } }

        public override float Width { get { return (RightTop.Coordinates - LeftTop.Coordinates).magnitude; } }
        public override float Height { get { return (LeftBottom.Coordinates - LeftTop.Coordinates).magnitude; } }

        public GenMeshSquareFace() : base(new GenMeshVertex[4])
        {
        }

        public GenMeshSquareFace(GenMeshVertex leftTop, GenMeshVertex leftBottom, GenMeshVertex rightBottom, GenMeshVertex rightTop) : this()
        {
            this.Vertices[0] = leftTop;
            this.Vertices[1] = leftBottom;
            this.Vertices[2] = rightBottom;
            this.Vertices[3] = rightTop;
        }

        public override GenMeshFace Clone()
        {
            return new GenMeshSquareFace(
                this.LeftTop.Clone(),
                this.LeftBottom.Clone(),
                this.RightBottom.Clone(),
                this.RightTop.Clone()
            );
        }

        public override int[] GetTriangles()
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

        public override GenMeshFace[] Extrude()
        {
            var frontFace = this.Clone() as GenMeshSquareFace;
            var leftFace = new GenMeshSquareFace(this.LeftTop, this.LeftBottom, frontFace.LeftBottom, frontFace.LeftTop);
            var topFace = new GenMeshSquareFace(this.LeftTop, frontFace.LeftTop, frontFace.RightTop, this.RightTop);
            var rightFace = new GenMeshSquareFace(frontFace.RightTop, frontFace.RightBottom, this.RightBottom, this.RightTop);
            var bottomFace = new GenMeshSquareFace(frontFace.LeftBottom, this.LeftBottom, this.RightBottom, frontFace.RightBottom);

            return new GenMeshFace[]
            {
                frontFace,
                leftFace,
                topFace,
                rightFace,
                bottomFace
            };
        }

        public override GenMeshFace[] Subdivide(int numberOfCuts)
        {
            var steps = numberOfCuts + 2;

            var topSize = (this.RightTop.Coordinates - this.LeftTop.Coordinates).magnitude;
            var bottomSize = (this.RightBottom.Coordinates - this.LeftBottom.Coordinates).magnitude;
            var leftSize = (this.LeftTop.Coordinates - this.LeftBottom.Coordinates).magnitude;
            var rightSize = (this.RightTop.Coordinates - this.RightBottom.Coordinates).magnitude;

            var result = new List<GenMeshFace>();

            var topPoints = new List<Vector3>();
            var bottomPoints = new List<Vector3>();
            var leftPoints = new List<Vector3>();
            var rightPoints = new List<Vector3>();


            for (var i = 0; i < steps; i++)
            {
                // horizontal
                var hfrom = Vector3.Lerp(this.LeftTop.Coordinates, this.RightTop.Coordinates, ((float)i) / (steps - 1));
                var hto = Vector3.Lerp(this.LeftBottom.Coordinates, this.RightBottom.Coordinates, ((float)i) / (steps - 1));

                // vertical
                var vfrom = Vector3.Lerp(this.LeftTop.Coordinates, this.LeftBottom.Coordinates, ((float)i) / (steps - 1));
                var vto = Vector3.Lerp(this.RightTop.Coordinates, this.RightBottom.Coordinates, ((float)i) / (steps - 1));

                topPoints.Add(hfrom);
                bottomPoints.Add(hto);
                leftPoints.Add(vfrom);
                rightPoints.Add(vto);
            }

            var points = new Vector3[steps, steps];

            for (var x = 0; x < steps; x++)
            {
                for (var y = 0; y < steps; y++)
                {
                    var top = topPoints[x];
                    var bottom = bottomPoints[x];
                    var left = leftPoints[y];
                    var right = rightPoints[y];

                    Vector3 intersection;
                    if (!GenMesh.LineLineIntersection(out intersection, top, (bottom - top), left, (right - left)))
                    {
                        throw new InvalidOperationException("Points here should always intersect");
                    }

                    points[x, y] = intersection;
                }
            }

            for (var x = 0; x < steps - 1; x++)
            {
                for (var y = 0; y < steps - 1; y++)
                {
                    result.Add(new GenMeshSquareFace(
                        new GenMeshVertex(points[x, y]),
                        new GenMeshVertex(points[x, y + 1]),
                        new GenMeshVertex(points[x + 1, y + 1]),
                        new GenMeshVertex(points[x + 1, y])));
                }
            }

            return result.ToArray();
        }

        public override float Area()
        {
            return this.Width * this.Height;
        }
    }
}
