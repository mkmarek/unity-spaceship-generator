using UnityEditor;
using UnityEngine;

namespace ProceduralSpaceShip.Editor
{
    [CustomEditor(typeof(SpaceShipGenerator))]
    public class SpaceShipGeneratorInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var myTarget = (SpaceShipGenerator)target;

            if (GUILayout.Button("Generate"))
            {
                myTarget.GenerateMesh();
            }

            if (GUILayout.Button("Generate with new seed"))
            {
                myTarget.RandomizeSeed();
                myTarget.GenerateMesh();
            }
        }
    }
}
