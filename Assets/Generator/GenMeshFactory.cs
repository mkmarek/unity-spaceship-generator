using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public static class GenMeshFactory
    {
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

        public static GenMesh CreateCylinder(int numberOfSegments, float cylinderSize1, float cylinderSize2, float cylinderDepth)
        {
            var mesh = new GenMesh();
            var lowerCircle = new List<GenMeshVertex>();
            var upperCircle = new List<GenMeshVertex>();

            for (var i = 0; i < numberOfSegments; i++)
            {
                lowerCircle.Add(new GenMeshVertex(new Vector3(
                    Mathf.Cos((float)i / numberOfSegments * Mathf.PI * 2) * cylinderSize1 / 2,
                    Mathf.Sin((float)i / numberOfSegments * Mathf.PI * 2) * cylinderSize1 / 2,
                    -cylinderDepth / 2
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

            mesh.Faces.Add(new GenMeshComplexFace(lowerCircle.ToArray(),
                BowyerWatson.GetTris(lowerCircle.Select(e => (Vector2)e.Coordinates * 100))));

            mesh.Faces.Add(new GenMeshComplexFace(upperCircle.ToArray(),
                BowyerWatson.GetTris(lowerCircle.Select(e => (Vector2)e.Coordinates * 100), true)));

            return mesh;
        }


        // Inspired by http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html
        public static GenMesh CreateIcosphere(int numberOfSubdivisions, float size)
        {
            var t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

            var verticies = new Vector3[]
            {
                new Vector3(-1, t, 0),
                new Vector3(1, t, 0),
                new Vector3(-1, -t, 0),
                new Vector3(1, -t, 0),

                new Vector3(0, -1, t),
                new Vector3(0, 1, t),
                new Vector3(0, -1, -t),
                new Vector3(0, 1, -t),

                new Vector3(t, 0, -1),
                new Vector3(t, 0, 1),
                new Vector3(-t, 0, -1),
                new Vector3(-t, 0, 1)
            }
            .Select(p => {
                var length = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
                return new GenMeshVertex(new Vector3(p.x / length, p.y / length, p.z / length));
            })
            .ToArray();

            var faces = new List<GenMeshFace>()
            {
                new GenMeshTriangleFace(verticies[0], verticies[11], verticies[5]),
                new GenMeshTriangleFace(verticies[0], verticies[5], verticies[1]),
                new GenMeshTriangleFace(verticies[0], verticies[1], verticies[7]),
                new GenMeshTriangleFace(verticies[0], verticies[7], verticies[10]),
                new GenMeshTriangleFace(verticies[0], verticies[10], verticies[11]),

                new GenMeshTriangleFace(verticies[1], verticies[5], verticies[9]),
                new GenMeshTriangleFace(verticies[5], verticies[11], verticies[4]),
                new GenMeshTriangleFace(verticies[11], verticies[10], verticies[2]),
                new GenMeshTriangleFace(verticies[10], verticies[7], verticies[6]),
                new GenMeshTriangleFace(verticies[7], verticies[1], verticies[8]),

                new GenMeshTriangleFace(verticies[3], verticies[9], verticies[4]),
                new GenMeshTriangleFace(verticies[3], verticies[4], verticies[2]),
                new GenMeshTriangleFace(verticies[3], verticies[2], verticies[6]),
                new GenMeshTriangleFace(verticies[3], verticies[6], verticies[8]),
                new GenMeshTriangleFace(verticies[3], verticies[8], verticies[9]),

                new GenMeshTriangleFace(verticies[4], verticies[9], verticies[5]),
                new GenMeshTriangleFace(verticies[2], verticies[4], verticies[11]),
                new GenMeshTriangleFace(verticies[6], verticies[2], verticies[10]),
                new GenMeshTriangleFace(verticies[8], verticies[6], verticies[7]),
                new GenMeshTriangleFace(verticies[9], verticies[8], verticies[1])
            };

            var afterSubdivision = new List<GenMeshFace>();

            foreach (var face in faces)
            {
                afterSubdivision.AddRange(face.Subdivide(numberOfSubdivisions));
            }

            var allVerticies = afterSubdivision.SelectMany(e => e.Vertices).Distinct();

            foreach (var v in allVerticies)
            {
                v.Coordinates = v.Coordinates * size / 2;
            }

            return new GenMesh()
            {
                Faces = afterSubdivision
            };
        }
    }
}
