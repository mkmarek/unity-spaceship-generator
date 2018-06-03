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
        public GenMeshVertex[] Vertices { get { return Faces.SelectMany(e => e.Vertices).Distinct().ToArray(); } }
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
            mesh.Faces.Add(new GenMeshSquareFace(RTT, RTB, LTB, LTT));

            // left face
            mesh.Faces.Add(new GenMeshSquareFace(LTB, LBB, LBT, LTT));

            // right face
            mesh.Faces.Add(new GenMeshSquareFace(RTT, RBT, RBB, RTB));

            // front face
            mesh.Faces.Add(new GenMeshSquareFace(RTB, RBB, LBB, LTB));

            // back face
            mesh.Faces.Add(new GenMeshSquareFace(LTT, LBT, RBT, RTT));

            // bottom face
            mesh.Faces.Add(new GenMeshSquareFace(LBT, LBB, RBB, RBT));

            return mesh;
        }

        public static GenMesh CreateCyllinder(int numberOfSegments, float cylinderSize1, float cylinderSize2, float cylinderDepth)
        {
            var mesh = new GenMesh();
            var lowerCircle = new List<GenMeshVertex>();
            var upperCircle = new List<GenMeshVertex>();

            for (var i = 0; i < numberOfSegments; i++)
            {
                lowerCircle.Add(new GenMeshVertex(new Vector3(
                    Mathf.Cos((float)i / numberOfSegments * Mathf.PI * 2) * cylinderSize1 / 2,
                    Mathf.Sin((float)i / numberOfSegments * Mathf.PI * 2) * cylinderSize1 / 2,
                    - cylinderDepth / 2
                    )));

                upperCircle.Add(new GenMeshVertex(new Vector3(
                    Mathf.Cos((float)i / numberOfSegments * Mathf.PI * 2) * cylinderSize2 / 2,
                    Mathf.Sin((float)i / numberOfSegments * Mathf.PI * 2) * cylinderSize2 / 2,
                    cylinderDepth / 2)));
            }

            for (var i = 0; i < numberOfSegments; i++)
            {
                mesh.Faces.Add(new GenMeshSquareFace(
                    upperCircle[i],
                    upperCircle[(i + 1) % numberOfSegments],          
                    lowerCircle[(i + 1) % numberOfSegments],
                    lowerCircle[i]
                    ));
            }

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
            var faces = face.Extrude();

            foreach (var addedFace in faces)
            {
                this.Faces.Add(addedFace);
            }

            this.Faces.Remove(face);

            return faces;
        }

        public void Scale(Vector3 scaleVector, Matrix4x4 faceSpace, GenMeshVertex[] vertices)
        {
            // Not sure what Blender does with the space matrix, so I'm gonna skip it for now
            var center = Vector3.zero;

            foreach (var vert in vertices)
            {
                center += vert.Coordinates;
            }

            center = center / vertices.Count();

            foreach (var vert in vertices)
            {
                var transformedSpace = (vert.Coordinates - center);
                vert.Coordinates = (new Vector3(transformedSpace.x * scaleVector.x, transformedSpace.y * scaleVector.y, transformedSpace.z * scaleVector.z)) + center;
            }
        }

        public Mesh ToUnityMesh(bool smooth = false)
        {
            var mesh = new Mesh();

            var faces = smooth
                ? this.Faces.ToArray()
                : this.Faces.Select(e => e.Clone()).ToArray();

            var vertices = faces.SelectMany(f => f.Vertices);

            this.IndexAllVertexes(vertices);
            var coordinates = vertices.Select(e => e.Coordinates).ToArray();

            var centerOfMass = Vector3.zero;
            foreach (var vertex in coordinates) centerOfMass += vertex;
            centerOfMass /= coordinates.Length;

            mesh.vertices = coordinates.Select(v => v - centerOfMass).ToArray();
            mesh.triangles = faces.SelectMany(face => face.GetTriangles()).ToArray();

            Debug.Log("Vertex count: " + mesh.vertices.Length);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        private void IndexAllVertexes(IEnumerable<GenMeshVertex> vertices)
        {
            var index = 0;

            foreach (var vertex in vertices)
            {
                vertex.Index = index;
                index++;
            }
        }

        internal void Symmetrize(Axis axis)
        {
            // TODO
        }

        public GenMeshFace[] Subdivide(GenMeshFace face, int numberOfCuts)
        {
            var result = face.Subdivide(numberOfCuts);

            this.Faces.Remove(face);
            this.Faces.AddRange(result);

            return result;
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

        public void CreateCyllinder(int numberOfSegments, float cylinderSize1, float cylinderSize2, float cylinderDepth, Matrix4x4 cylinderMatrix)
        {
            var cyllinderMesh = CreateCyllinder(numberOfSegments, cylinderSize1, cylinderSize2, cylinderDepth);

            foreach (var vertex in cyllinderMesh.Vertices)
            {
                vertex.Coordinates = cylinderMatrix.MultiplyPoint3x4(vertex.Coordinates);
            }

            foreach (var face in cyllinderMesh.Faces)
            {
                this.Faces.Add(face);
            }
        }
    }
}
