using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public class SpaceShipGenerator : MonoBehaviour
    {
        [SerializeField]
        private bool smoothShading = false;

        [SerializeField]
        private int seed = 844483692;

        [SerializeField]
        private int numHullSegmentsMin = 3;

        [SerializeField]
        private int numHullSegmentsMax = 6;

        [SerializeField]
        private bool createAsymmetrySegments = true;

        [SerializeField]
        private int numAsymmetrySegmentsMin = 1;

        [SerializeField]
        private int numAsymmetrySegmentsMax = 5;

        [SerializeField]
        private bool createFaceDetail = true;

        [SerializeField]
        private bool allowHorizontalSymmetry = true;

        [SerializeField]
        private bool allowVerticalSymmetry = true;

        [SerializeField]
        private bool applyBevelModifier = true;

        [SerializeField]
        private bool assignMaterials = true;

        public void RandomizeSeed()
        {
            seed = Random.Range(0, int.MaxValue);
        }

        public void GenerateMesh()
        {
            Random.InitState(this.seed);

            var mesh = GenMeshFactory.CreateCube();

            Generate(mesh);

            var meshFilter = GetComponent<MeshFilter>();

            meshFilter.sharedMesh = mesh.ToUnityMesh(smoothShading);
        }

        private void Generate(GenMesh genmesh)
        {
            var scaleFactor = Random.Range(0.75f, 2.0f);
            var scaleVector = Vector3.one * scaleFactor;

            genmesh.Scale(scaleVector, genmesh.Vertices);

            var faces = genmesh.Faces.ToArray();
            for (var f = 0; f < faces.Length; f++)
            {
                var face = faces[f];

                if (Mathf.Abs(face.Normal.x) > 0.5f)
                {
                    var hullSegmentLength = Random.Range(0.3f, 1.0f);
                    var numberOfHullSegments = Random.Range(numHullSegmentsMin, numHullSegmentsMax);

                    for (var i = 0; i < numberOfHullSegments; i++)
                    {
                        var isLastHullSegment = i == numberOfHullSegments - 1;
                        var val = Random.value;

                        if (val > 0.1f)
                        {
                            // Most of the time, extrude out the face with some random deviations
                            face = this.ExtrudeFace(genmesh, face, hullSegmentLength);

                            if (Random.value > 0.75f)
                            {
                                face = this.ExtrudeFace(genmesh, face, hullSegmentLength * 0.25f);
                            }

                            // Maybe apply some scaling
                            if (Random.value > 0.5f)
                            {
                                var sy = Random.Range(1.2f, 1.5f);
                                var sz = Random.Range(1.2f, 1.5f);

                                if (isLastHullSegment || Random.value > 0.5f)
                                {
                                    sy = 1f / sy;
                                    sz = 1f / sz;
                                }

                                this.ScaleFace(genmesh, face, 1f, sy, sz);
                            }

                            // Maybe apply some sideways translation
                            if (Random.value > 0.5f)
                            {
                                var sidewaysTranslation = new Vector3(0f, 0f, Random.Range(0.1f, 0.4f) * scaleVector.z * hullSegmentLength);

                                if (Random.value > 0.5f)
                                {
                                    sidewaysTranslation = -sidewaysTranslation;
                                }

                                genmesh.Translate(sidewaysTranslation, face.Vertices);
                            }

                            // Maybe add some rotation around Z axis
                            if (Random.value > 0.5f)
                            {
                                var angle = 5f;
                                if (Random.value > 0.5f)
                                {
                                    angle = -angle;
                                }

                                var quaterion = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));
                                genmesh.Rotate(face.Vertices, new Vector3(0, 0, 0), quaterion);
                            }
                        }
                        else
                        {
                            // Rarely, create a ribbed section of the hull
                            var ribScale = Random.Range(0.75f, 0.95f);
                            face = this.RibbedExtrudeFace(genmesh, face, hullSegmentLength, Random.Range(2, 4), ribScale);
                        }
                    }
                }
            }

            //Add some large asymmetrical sections of the hull that stick out
            if (createAsymmetrySegments)
            {
                var potentialFaces = genmesh.Faces.ToArray();
                for (var f = 0; f < potentialFaces.Length; f++)
                {
                    var face = potentialFaces[f];

                    if (face.AspectRatio > 4f)
                    {
                        continue;
                    }

                    if (Random.value > 0.85f)
                    {
                        var hullPieceLength = Random.Range(0.1f, 0.4f);
                        var totalSegments = Random.Range(numAsymmetrySegmentsMin, numAsymmetrySegmentsMax);

                        for (var i = 0; i < totalSegments; i++)
                        {
                            face = ExtrudeFace(genmesh, face, hullPieceLength);

                            // Maybe apply some scaling
                            if (Random.value > 0.25f)
                            {
                                var s = 1f / Random.Range(1.1f, 1.5f);
                                ScaleFace(genmesh, face, s, s, s);
                            }
                        }
                    }
                }
            }

            // Now the basic hull shape is built, let's categorize + add detail to all the faces
            if (createFaceDetail)
            {
                var engineFaces = new List<GenMeshFace>();
                var gridFaces = new List<GenMeshFace>();
                var antennaFaces = new List<GenMeshFace>();
                var weaponFaces = new List<GenMeshFace>();
                var sphereFaces = new List<GenMeshFace>();
                var discFaces = new List<GenMeshFace>();
                var cylinderFaces = new List<GenMeshFace>();

                faces = genmesh.Faces.ToArray();
                for (var f = 0; f < faces.Length; f++)
                {
                    var face = faces[f];

                    // Skip any long thin faces as it'll probably look stupid
                    if (face.AspectRatio > 3) continue;

                    // Spin the wheel! Let's categorize + assign some materials
                    var val = Random.value;

                    if (face.Normal.x < -0.9f)
                    {
                        if (!engineFaces.Any() || val > 0.75f)
                            engineFaces.Add(face);
                        else if (val > 0.5f)
                            cylinderFaces.Add(face);
                        else if (val > 0.25f)
                            gridFaces.Add(face);
                        else
                            face.MaterialIndex = 1;//Material.hull_lights
                    }
                    else if (face.Normal.x > 0.9f) // front face
                    {
                        if (Vector3.Dot(face.Normal, face.CalculateCenterBounds()) > 0 && val > 0.7f)
                        {
                            antennaFaces.Add(face);  // front facing antenna
                            face.MaterialIndex = 1;// Material.hull_lights
                        }
                        else if (val > 0.4f)
                            gridFaces.Add(face);
                        else
                            face.MaterialIndex = 1;// Material.hull_lights
                    }
                    else if (face.Normal.y > 0.9f) // top face
                    {
                        if (Vector3.Dot(face.Normal, face.CalculateCenterBounds()) > 0 && val > 0.7f)
                            antennaFaces.Add(face);  // top facing antenna
                        else if (val > 0.6f)
                            gridFaces.Add(face);
                        else if (val > 0.3f)
                            cylinderFaces.Add(face);
                    }
                    else if (face.Normal.y < -0.9f) // bottom face
                    {
                        if (val > 0.75f)
                            discFaces.Add(face);
                        else if (val > 0.5f)
                            gridFaces.Add(face);
                        else if (val > 0.25f)
                            weaponFaces.Add(face);
                    }
                    else if (Mathf.Abs(face.Normal.z) > 0.9f) // side face
                    {
                        if (!weaponFaces.Any() || val > 0.75f)
                            weaponFaces.Add(face);
                        else if (val > 0.6f)
                            gridFaces.Add(face);
                        else if (val > 0.4f)
                            sphereFaces.Add(face);
                        else
                            face.MaterialIndex = 1;// Material.hull_lights
                    }
                }

                // Now we've categorized, let's actually add the detail
                foreach (var fac in engineFaces)
                    AddExhaustToFace(genmesh, fac);

                foreach (var fac in gridFaces)
                    AddGridToFace(genmesh, fac);

                foreach (var fac in antennaFaces)
                    AddSurfaceAntennaToFace(genmesh, fac);

                foreach (var fac in weaponFaces)
                    AddWeaponsToFace(genmesh, fac);

                foreach (var fac in sphereFaces)
                    AddSphereToFace(genmesh, fac);

                foreach (var fac in discFaces)
                    AddDiscToFace(genmesh, fac);

                foreach (var fac in cylinderFaces)
                    AddCylindersToFace(genmesh, fac);
            }

            // Apply horizontal symmetry sometimes
            if (allowHorizontalSymmetry && Random.value > 0.5f)
                genmesh.Symmetrize(Axis.Horizontal);

            // Apply vertical symmetry sometimes - this can cause spaceship "islands", so disabled by default
            if (allowHorizontalSymmetry && Random.value > 0.5f)
                genmesh.Symmetrize(Axis.Vertical);

            if (applyBevelModifier)
            {
                // TODO
            }
        }

        private Matrix4x4 GetFaceMatrix(GenMeshFace face, Vector3? position = null)
        {
            var xAxis = (face.RightTop.Coordinates - face.LeftTop.Coordinates).normalized;
            var zAxis = -face.Normal;
            var yAxis = Vector3.Cross(zAxis, xAxis);

            if (!position.HasValue)
            {
                position = face.CalculateCenterBounds();
            }

            //return Matrix4x4.Translate(position.Value);

            // Construct a 4x4 matrix from axes + position:
            // http://i.stack.imgur.com/3TnQP.png

            var mat = new Matrix4x4();
            mat[0, 0] = xAxis.x;
            mat[1, 0] = xAxis.y;
            mat[2, 0] = xAxis.z;
            mat[3, 0] = 0;
            mat[0, 1] = yAxis.x;
            mat[1, 1] = yAxis.y;
            mat[2, 1] = yAxis.z;
            mat[3, 1] = 0;
            mat[0, 2] = zAxis.x;
            mat[1, 2] = zAxis.y;
            mat[2, 2] = zAxis.z;
            mat[3, 2] = 0;
            mat[0, 3] = position.Value.x;
            mat[1, 3] = position.Value.y;
            mat[2, 3] = position.Value.z;
            mat[3, 3] = 1;

            return mat;
        }

        private void AddCylindersToFace(GenMesh genmesh, GenMeshFace fac)
        {
            var horizontalStep = Random.Range(1, 3);
            var verticalStep = Random.Range(1, 3);
            var numberOfSegments = Random.Range(6, 12);
            var faceWidth = fac.Width;
            var faceHeight = fac.Height;
            var cylinderDepth = 1.3f * Mathf.Min(faceWidth / (horizontalStep + 2), faceHeight / (verticalStep + 2));
            var cylinderSize = cylinderDepth * 0.5f;

            for (var h = 0; h < horizontalStep; h++)
            {
                var top = Vector3.Lerp(fac.LeftTop.Coordinates, fac.RightTop.Coordinates, ((float)h + 1) / (horizontalStep + 1));
                var bottom = Vector3.Lerp(fac.LeftBottom.Coordinates, fac.RightBottom.Coordinates, ((float)h + 1) / (horizontalStep + 1));

                for (var v = 0; v < verticalStep; v++)
                {
                    var pos = Vector3.Lerp(top, bottom, ((float)v + 1) / (verticalStep + 1));
                    var cylinderMatrix = GetFaceMatrix(fac, pos) * Matrix4x4.Rotate(Quaternion.AngleAxis(90, new Vector3(0, 1, 0)));

                    genmesh.CreateCylinder(numberOfSegments, cylinderSize, cylinderSize, cylinderDepth, cylinderMatrix);
                }
            }

        }

        private void AddDiscToFace(GenMesh genmesh, GenMeshFace fac)
        {
            var faceWidth = fac.Width;
            var faceHeight = fac.Height;
            var depth = 0.125f * Mathf.Min(faceWidth, faceHeight);

            genmesh.CreateCylinder(
                32,
                depth * 3,
                depth * 4,
                depth,
                GetFaceMatrix(fac, fac.CalculateCenterBounds() + fac.Normal * depth * 0.5f));

            genmesh.CreateCylinder(
                32,
                depth * 1.25f,
                depth * 2.25f,
                0.0f,
                GetFaceMatrix(fac, fac.CalculateCenterBounds() + fac.Normal * depth * 1.05f));

            /*
             for vert in result['verts']:
        for face in vert.link_faces:
            face.material_index = Material.glow_disc
             
             */
        }

        private void AddSphereToFace(GenMesh genmesh, GenMeshFace fac)
        {
            var sphereSize = Random.Range(0.4f, 1f) * Mathf.Min(fac.Width, fac.Height);
            var sphereMatrix = GetFaceMatrix(fac, fac.CalculateCenterBounds() - fac.Normal * Random.Range(0f, sphereSize * 0.5f));
            genmesh.CreateIcosphere(3, sphereSize, sphereMatrix);

            /*
    for vert in result['verts']:
        for face in vert.link_faces:
            face.material_index = Material.hull
             */
        }

        private void AddWeaponsToFace(GenMesh genmesh, GenMeshFace fac)
        {
            var horizontalStep = Random.Range(1, 3);
            var verticalStep = Random.Range(1, 3);
            var numSegments = 16;

            var weaponSize = 0.5f * Mathf.Min(fac.Width / (horizontalStep + 2), fac.Height / (verticalStep + 2));
            var weaponDepth = weaponSize * 0.2f;

            for (var h = 0; h < horizontalStep; h++)
            {
                var top = Vector3.Lerp(fac.LeftTop.Coordinates, fac.RightTop.Coordinates, (float)(h + 1) / (horizontalStep + 1));
                var bottom = Vector3.Lerp(fac.LeftBottom.Coordinates, fac.RightBottom.Coordinates, (float)(h + 1) / (horizontalStep + 1));

                for (var v = 0; v < verticalStep; v++)
                {
                    var pos = Vector3.Lerp(top, bottom, (float)(v + 1) / (verticalStep + 1));
                    var faceMatrix = GetFaceMatrix(fac, pos + fac.Normal * weaponDepth * 0.5f) *
                        Matrix4x4.Rotate(Quaternion.AngleAxis(Random.Range(0, 90), new Vector3(0, 0, 1)));


                    // Turret foundation
                    genmesh.CreateCylinder(numSegments, weaponSize * 0.9f, weaponSize, weaponDepth, faceMatrix);

                    // Turret left guard
                    var leftGuardMat = faceMatrix *
                        Matrix4x4.Rotate(Quaternion.AngleAxis(90, new Vector3(0, 1, 0))) *
                        Matrix4x4.Translate(new Vector3(0, 0, weaponSize * 0.6f));
                    genmesh.CreateCylinder(numSegments, weaponSize * 0.6f, weaponSize * 0.5f, weaponDepth * 2, leftGuardMat);

                    // Turret right guard
                    var rightGuardMat = faceMatrix *
                        Matrix4x4.Rotate(Quaternion.AngleAxis(90, new Vector3(0, 1, 0))) *
                        Matrix4x4.Translate(new Vector3(0, 0, weaponSize * -0.6f));
                    genmesh.CreateCylinder(numSegments, weaponSize * 0.5f, weaponSize * 0.6f, weaponDepth * 2, rightGuardMat);

                    // Turret housing
                    var upwardAngle = Random.Range(0, 45);
                    var turretHouseMat = faceMatrix *
                        Matrix4x4.Rotate(Quaternion.AngleAxis(upwardAngle, new Vector3(1, 0, 0))) *
                        Matrix4x4.Translate(new Vector3(0, weaponSize * -0.4f, 0));
                    genmesh.CreateCylinder(8, weaponSize * 0.4f, weaponSize * 0.4f, weaponDepth * 5, turretHouseMat);

                    // Turret barrels L + R
                    genmesh.CreateCylinder(8, weaponSize * 0.1f, weaponSize * 0.1f, weaponDepth * 6, turretHouseMat *
                        Matrix4x4.Translate(new Vector3(weaponSize * 0.2f, 0, -weaponSize)));
                    genmesh.CreateCylinder(8, weaponSize * 0.1f, weaponSize * 0.1f, weaponDepth * 6, turretHouseMat *
                        Matrix4x4.Translate(new Vector3(weaponSize * -0.2f, 0, -weaponSize)));
                }
            }
        }

        private void AddSurfaceAntennaToFace(GenMesh genmesh, GenMeshFace fac)
        {
            var horizontalStep = Random.Range(4, 10);
            var verticalStep = Random.Range(4, 10);

            for (var h = 0; h < horizontalStep; h++)
            {
                var top = Vector3.Lerp(fac.LeftTop.Coordinates, fac.RightTop.Coordinates, ((float)h + 1) / (horizontalStep + 1));
                var bottom = Vector3.Lerp(fac.LeftBottom.Coordinates, fac.RightBottom.Coordinates, ((float)h + 1) / (horizontalStep + 1));

                for (var v = 0; v < verticalStep; v++)
                {
                    if (Random.value > 0.9f)
                    {
                        var pos = Vector3.Lerp(top, bottom, ((float)v + 1) / (verticalStep + 1));
                        var faceSize = Mathf.Sqrt(fac.Area());
                        var depth = Random.Range(0.1f, 1.5f) * faceSize;
                        var depthShort = depth * Random.Range(0.02f, 0.15f);
                        var baseDiameter = Random.Range(0.005f, 0.05f);
                        var materialIndex = Random.value > 0.5f ? 0 /*Material.hull*/ : 1;/*Material.hull_dark*/

                        // Spire
                        var numSegments = Random.Range(3, 6);
                        genmesh.CreateCylinder(numSegments, 0, baseDiameter, depth, GetFaceMatrix(fac, pos + fac.Normal * depth * 0.5f));

                        //for vert in result['verts']:
                        //    for vert_face in vert.link_faces:
                        //        vert_face.material_index = material_index
                        //    }

                        // Base
                        genmesh.CreateCylinder(
                            numSegments,
                            baseDiameter * Random.Range(1f, 1.5f),
                            baseDiameter * Random.Range(1.5f, 2f),
                            depthShort,
                            GetFaceMatrix(fac, pos + fac.Normal * depthShort * 0.45f));

                        //    for vert in result['verts']:
                        //for vert_face in vert.link_faces:
                        //    vert_face.material_index = material_index
                    }
                }
            }
        }

        private void AddGridToFace(GenMesh genmesh, GenMeshFace fac)
        {
            var result = genmesh.Subdivide(fac, Random.Range(2, 4));
            var gridLength = Random.Range(0.025f, 0.15f);
            var scale = 0.8f;

            for (var i = 0; i < result.Length; i++)
            {
                var face = result[i];
                var materialIndex = Random.value > 0.5f ? 1/*Material.hull_lights*/ : 4 /*Material.hull*/;
                var extrudedFaceList = new List<GenMeshFace>();

                face = ExtrudeFace(genmesh, face, gridLength, extrudedFaceList);

                foreach (var f in extrudedFaceList)
                {
                    if (Mathf.Abs(face.Normal.z) < 0.707) // # side face
                        f.MaterialIndex = materialIndex;
                }

                ScaleFace(genmesh, face, scale, scale, scale);
            }
        }

        // Given a face, splits it into a uniform grid and extrudes each grid face
        // out and back in again, making an exhaust shape.
        private void AddExhaustToFace(GenMesh genmesh, GenMeshFace faceForExhaust)
        {
            // The more square the face is, the more grid divisions it might have
            var num_cuts = Random.Range(1, (int)(4 - faceForExhaust.AspectRatio));
            var result = genmesh.Subdivide(faceForExhaust, num_cuts);

            var exhaust_length = Random.Range(0.1f, 0.2f);
            var scaleOuter = 1f / Random.Range(1.3f, 1.6f);
            var scale_inner = 1f / Random.Range(1.05f, 1.1f);

            for (var i = 0; i < result.Count(); i++)
            {
                var face = result[i];
                face.MaterialIndex = 2;// Material.hull_dark;

                face = ExtrudeFace(genmesh, face, exhaust_length);
                ScaleFace(genmesh, face, scaleOuter, scaleOuter, scaleOuter);

                face = ExtrudeFace(genmesh, face, 0);
                ScaleFace(genmesh, face, scaleOuter * 0.9f, scaleOuter * 0.9f, scaleOuter * 0.9f);

                var extruded_face_list = new List<GenMeshFace>();
                face = ExtrudeFace(genmesh, face, -exhaust_length * 0.9f, extruded_face_list);

                foreach (var extruded_face in extruded_face_list)
                    extruded_face.MaterialIndex = 3;// Material.exhaust_burn

                ScaleFace(genmesh, face, scale_inner, scale_inner, scale_inner);
            }
        }

        private GenMeshFace RibbedExtrudeFace(GenMesh genmesh, GenMeshFace face, float distance, int numberOfRibs = 3, float ribScale = 0.9f)
        {
            var distancePerRib = distance / numberOfRibs;
            var newFace = face;

            for (var i = 0; i < numberOfRibs; i++)
            {
                newFace = ExtrudeFace(genmesh, newFace, distancePerRib * 0.25f);
                newFace = ExtrudeFace(genmesh, newFace, 0.0f);
                ScaleFace(genmesh, newFace, ribScale, ribScale, ribScale);
                newFace = ExtrudeFace(genmesh, newFace, distancePerRib * 0.5f);
                newFace = ExtrudeFace(genmesh, newFace, 0.0f);
                ScaleFace(genmesh, newFace, 1 / ribScale, 1 / ribScale, 1 / ribScale);
                newFace = ExtrudeFace(genmesh, newFace, distancePerRib * 0.25f);
            }

            return newFace;
        }

        private void ScaleFace(GenMesh genmesh, GenMeshFace face, float sx, float sy, float sz)
        {
            genmesh.Scale(new Vector3(sx, sy, sz), GetFaceMatrix(face).inverse, face.Vertices);
        }

        private GenMeshFace ExtrudeFace(GenMesh genmesh, GenMeshFace face, float distance, List<GenMeshFace> extrudedFaces = null)
        {
            var newFaces = genmesh.ExtrudeDiscreetFace(face);

            if (extrudedFaces != null)
            {
                extrudedFaces.AddRange(newFaces);
            }

            var newFace = newFaces[0];
            genmesh.Translate(newFace.Normal * distance, newFace.Vertices);

            return newFace;
        }
    }
}
