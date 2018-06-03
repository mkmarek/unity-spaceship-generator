using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public enum Axis
    {
        Horizontal,
        Vertical
    }

    public class GenMesh
    {
        public GenMeshVertex[] Vertices { get { return Faces.SelectMany(e => e.Vertices).ToArray(); } }
        public List<GenMeshFace> Faces { get; set; }

        public static GenMesh CreateCube(float size = 1)
        {
            var mesh = new GenMesh();

            // create 8 vertices the notation is XYZ so for vertex that's on top face on the left top it will be LTT
            var LTT = new GenMeshVertex(new Vector3(-size, +size, -size));
            var RTT = new GenMeshVertex(new Vector3(+size, +size, -size));
            var RTB = new GenMeshVertex(new Vector3(+size, +size, +size));
            var LTB = new GenMeshVertex(new Vector3(-size, +size, +size));

            var LBT = new GenMeshVertex(new Vector3(-size, -size, -size));
            var RBT = new GenMeshVertex(new Vector3(+size, -size, -size));
            var RBB = new GenMeshVertex(new Vector3(+size, -size, +size));
            var LBB = new GenMeshVertex(new Vector3(-size, -size, +size));

            // create 6 faces

            // top face
            mesh.Faces.Add(new GenMeshFace(RTT, RTB, LTB, LTT));

            // left face
            mesh.Faces.Add(new GenMeshFace(LTB, LBB, LBT, LTT));

            // right face
            mesh.Faces.Add(new GenMeshFace(RTT, RBT, RBB, RTB));

            // front face
            mesh.Faces.Add(new GenMeshFace(RTB, RBB, LBB, LTB));

            // back face
            mesh.Faces.Add(new GenMeshFace(LTT, LBT, RBT, RTT));

            // bottom face
            mesh.Faces.Add(new GenMeshFace(LBT, LBB, RBB, RBT));

            return mesh;
        }

        private GenMesh()
        {
            this.Faces = new List<GenMeshFace>();
        }

        public void Scale(Vector3 scaleVector, GenMeshVertex[] vertices)
        {
            var center = Vector3.zero;

            foreach (var vert in vertices)
            {
                center += vert.Coordinates;
            }

            center = center / vertices.Count();

            foreach (var vert in vertices)
            {
                var centered = vert.Coordinates - center;
                vert.Coordinates = new Vector3(centered.x * scaleVector.x, centered.y * scaleVector.y, centered.z * scaleVector.z) + center;
            }
        }

        public void Translate(Vector3 translation, GenMeshVertex[] vertices)
        {
            foreach (var vert in vertices)
            {
                vert.Coordinates += translation;
            }
        }

        public void Rotate(GenMeshVertex[] vertices, Vector3 center, Quaternion quaterion)
        {
            foreach (var vert in vertices)
            {
                var centered = vert.Coordinates - center;
                vert.Coordinates = quaterion * vert.Coordinates + center;
            }
        }

        public GenMeshFace[] ExtrudeDiscreetFace(GenMeshFace face)
        {
            var frontFace = face.Clone();
            var leftFace = new GenMeshFace(face.LeftTop, face.LeftBottom, frontFace.LeftBottom, frontFace.LeftTop);
            var topFace = new GenMeshFace(face.LeftTop, frontFace.LeftTop, frontFace.RightTop, face.RightTop);
            var rightFace = new GenMeshFace(frontFace.RightTop, frontFace.RightBottom, face.RightBottom, face.RightTop);
            var bottomFace = new GenMeshFace(frontFace.LeftBottom, face.LeftBottom, face.RightBottom, frontFace.RightBottom);

            this.Faces.Add(frontFace);
            this.Faces.Add(leftFace);
            this.Faces.Add(topFace);
            this.Faces.Add(rightFace);
            this.Faces.Add(bottomFace);

            this.Faces.Remove(face);

            return new GenMeshFace[]
            {
                frontFace,
                leftFace,
                topFace,
                rightFace,
                bottomFace
            };
        }

        public void Scale(Vector3 scaleVector, Matrix4x4 faceSpace, GenMeshVertex[] vertices)
        {
            foreach (var vert in vertices)
            {
                var center = (Vector3)(faceSpace * vert.Coordinates);
                var centered = vert.Coordinates - center;
                vert.Coordinates = new Vector3(centered.x * scaleVector.x, centered.y * scaleVector.y, centered.z * scaleVector.z) + center;
            }
        }

        public Mesh ToUnityMesh()
        {
            var mesh = new Mesh();
            var tmpFaces = this.Faces.Select(e => e.Clone()).ToArray();

            this.IndexAllVertexes(tmpFaces);
            var vertices = tmpFaces.SelectMany(f => f.Vertices.Select(e => e.Coordinates)).ToArray();

            var centerOfMass = Vector3.zero;
            foreach (var vertex in vertices) centerOfMass += vertex;
            centerOfMass /= vertices.Length;

            mesh.vertices = vertices.Select(v => v - centerOfMass).ToArray();
            mesh.triangles = tmpFaces.SelectMany(face => face.GetTriangles()).ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        private void IndexAllVertexes(IEnumerable<GenMeshFace> tmpFaces)
        {
            var index = 0;
            foreach (var vertex in tmpFaces.SelectMany(f => f.Vertices))
            {
                vertex.Index = index;
                index++;
            }
        }

        internal void Symmetrize(Axis axis)
        {
            // TODO
        }

        internal GenMeshFace[] Subdivide(GenMeshFace face, int numberOfCuts)
        {
            var steps = numberOfCuts + 2;

            var topSize = (face.RightTop.Coordinates - face.LeftTop.Coordinates).magnitude;
            var bottomSize = (face.RightBottom.Coordinates - face.LeftBottom.Coordinates).magnitude;
            var leftSize = (face.LeftTop.Coordinates - face.LeftBottom.Coordinates).magnitude;
            var rightSize = (face.RightTop.Coordinates - face.RightBottom.Coordinates).magnitude;

            var result = new List<GenMeshFace>();

            var topPoints = new List<Vector3>();
            var bottomPoints = new List<Vector3>();
            var leftPoints = new List<Vector3>();
            var rightPoints = new List<Vector3>();


            for (var i = 0; i < steps; i++)
            {
                // horizontal
                var hfrom = Vector3.Lerp(face.LeftTop.Coordinates, face.RightTop.Coordinates, ((float)i) / (steps - 1));
                var hto = Vector3.Lerp(face.LeftBottom.Coordinates, face.RightBottom.Coordinates, ((float)i) / (steps - 1));

                // vertical
                var vfrom = Vector3.Lerp(face.LeftTop.Coordinates, face.LeftBottom.Coordinates, ((float)i) / (steps - 1));
                var vto = Vector3.Lerp(face.RightTop.Coordinates, face.RightBottom.Coordinates, ((float)i) / (steps - 1));

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
                    if (!LineLineIntersection(out intersection, top, (bottom - top), left, (right - left)))
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
                    result.Add(new GenMeshFace(
                        new GenMeshVertex(points[x, y]),
                        new GenMeshVertex(points[x, y + 1]),
                        new GenMeshVertex(points[x + 1, y + 1]),
                        new GenMeshVertex(points[x + 1, y])));
                }
            }

            this.Faces.Remove(face);
            this.Faces.AddRange(result);
            return result.ToArray();
        }

        // from http://wiki.unity3d.com/index.php/3d_Math_functions
        public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {

            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            //is coplanar, and not parrallel
            if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersection = linePoint1 + (lineVec1 * s);
                return true;
            }
            else
            {
                intersection = Vector3.zero;
                return false;
            }
        }
    }
}
