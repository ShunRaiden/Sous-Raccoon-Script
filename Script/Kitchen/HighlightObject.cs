using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon
{
    public class HighlightObject : MonoBehaviour
    {
        [Header("Mesh Renderer")]
        public List<MeshRenderer> meshRenderer = new();
        public List<Material> defaultMat = new();
        public List<Material> highlightMat = new();

        [Space(3)]
        [Header("Skinned Mesh Renderer")]
        public List<SkinnedMeshRenderer> skinnedMeshRenderers;
        public List<Material> defaultskinMeshMat;
        public List<Material> highlightskinMeshMat;

        public void ShowHighlight()
        {
            SetMaterials(meshRenderer, highlightMat);
            SetMaterials(skinnedMeshRenderers, highlightskinMeshMat);
        }

        public void HideHighlight()
        {
            SetMaterials(meshRenderer, defaultMat);
            SetMaterials(skinnedMeshRenderers, defaultskinMeshMat);
        }

        private void SetMaterials<T>(List<T> renderers, List<Material> materials) where T : Renderer
        {
            if (renderers.Count == 0 || materials.Count == 0) return;

            for (int i = 0; i < renderers.Count && i < materials.Count; i++)
            {
                renderers[i].material = materials[i];
            }
        }
    }
}
