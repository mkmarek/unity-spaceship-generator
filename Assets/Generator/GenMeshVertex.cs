using UnityEngine;

namespace ProceduralSpaceShip
{
    public class GenMeshVertex
    {
        public Vector3 Coordinates { get; set; }
        public int Index { get; set; }

        public GenMeshVertex()
        {

        }

        public GenMeshVertex(Vector3 coordinates)
        {
            this.Coordinates = coordinates;
        }

        internal GenMeshVertex Clone()
        {
            return new GenMeshVertex(this.Coordinates);
        }
    }
}
