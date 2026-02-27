using UnityEditor;
using UnityEngine.UIElements;

namespace MCPForUnity.Editor.Helpers
{
    /// <summary>
    /// Edit-Mode UI tree provider. Loads a <see cref="VisualTreeAsset"/> (UXML) from the
    /// Asset Database, instantiates it into a detached element, and serializes the result.
    /// No Play Mode is required.
    /// </summary>
    public class AssetUiTreeProvider : IUiTreeProvider
    {
        private readonly string _assetPath;

        /// <param name="assetPath">Project-relative path to the UXML file, e.g. <c>Assets/UI/HUD.uxml</c>.</param>
        public AssetUiTreeProvider(string assetPath)
        {
            _assetPath = assetPath;
        }

        /// <inheritdoc/>
        public bool IsAvailable =>
            !string.IsNullOrEmpty(_assetPath) &&
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_assetPath) != null;

        /// <inheritdoc/>
        public object GetTree(int maxDepth)
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_assetPath);
            if (asset == null) return null;

            var root = new VisualElement();
            asset.CloneTree(root);
            return UiDumpSerializer.SerializeElement(root, maxDepth);
        }
    }
}
