using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralSpaceShip
{
    public class SpaceShipGenerator : MonoBehaviour
    {
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

            var mesh = GenMesh.CreateCube();

            Generate(mesh);

            var meshFilter = GetComponent<MeshFilter>();

            meshFilter.sharedMesh = mesh.ToUnityMesh();
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

                            // Maybe add some rotation around Y axis
                            if (Random.value > 0.5f)
                            {
                                var angle = 5f;
                                if (Random.value > 0.5f)
                                {
                                    angle = -angle;
                                }

                                var quaterion = Quaternion.AngleAxis(angle, new Vector3(0, 1, 0));
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

                    if (face.Normal.x < -0.95f)
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
                    else if (face.Normal.z > 0.9f) // top face
                    {
                        if (Vector3.Dot(face.Normal, face.CalculateCenterBounds()) > 0 && val > 0.7f)
                            antennaFaces.Add(face);  // top facing antenna
                        else if (val > 0.6f)
                            gridFaces.Add(face);
                        else if (val > 0.3f)
                            cylinderFaces.Add(face);
                    }
                    else if (face.Normal.z < -0.9f) // bottom face
                    {
                        if (val > 0.75f)
                            discFaces.Add(face);
                        else if (val > 0.5f)
                            gridFaces.Add(face);
                        else if (val > 0.25f)
                            weaponFaces.Add(face);
                    }
                    else if (Mathf.Abs(face.Normal.y) > 0.9f) // side face
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
                    AddCyllindersToFace(genmesh, fac);
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

        private void AddCyllindersToFace(GenMesh genmesh, GenMeshFace fac)
        {
            
        }

        private void AddDiscToFace(GenMesh genmesh, GenMeshFace fac)
        {
            
        }

        private void AddSphereToFace(GenMesh genmesh, GenMeshFace fac)
        {
            
        }

        private void AddWeaponsToFace(GenMesh genmesh, GenMeshFace fac)
        {
            
        }

        private void AddSurfaceAntennaToFace(GenMesh genmesh, GenMeshFace fac)
        {
            
        }

        private void AddGridToFace(GenMesh genmesh, GenMeshFace fac)
        {
           
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
            genmesh.Scale(new Vector3(sx, sy, sz), face.Vertices);
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
