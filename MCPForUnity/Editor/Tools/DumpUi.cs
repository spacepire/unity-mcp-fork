using MCPForUnity.Editor.Helpers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MCPForUnity.Editor.Tools
{
    /// <summary>
    /// MCP tool that dumps the UI Toolkit element tree for inspection.
    ///
    /// Edit Mode: supply <c>asset_path</c> pointing to a .uxml file.
    ///   The UXML is cloned into a detached element and serialized without
    ///   entering Play Mode.
    ///
    /// Play Mode: <c>asset_path</c> is optional. All active UIDocument
    ///   components are serialized from their live rootVisualElement.
    /// </summary>
    [McpForUnityTool("dump_ui")]
    public static class DumpUi
    {
        public static object HandleCommand(JObject @params)
        {
            if (@params == null)
                return new ErrorResponse("Parameters cannot be null.");

            var p = new ToolParams(@params);
            int maxDepth = p.GetInt("max_depth") ?? 10;

            if (Application.isPlaying)
            {
                var provider = new RuntimeUiTreeProvider();
                return new SuccessResponse("UI tree dumped (runtime).", new
                {
                    mode = "runtime",
                    trees = provider.GetTree(maxDepth),
                });
            }

            string assetPath = p.Get("asset_path");
            if (string.IsNullOrEmpty(assetPath))
                return new ErrorResponse("'asset_path' is required in Edit Mode.");

            var assetProvider = new AssetUiTreeProvider(assetPath);
            if (!assetProvider.IsAvailable)
                return new ErrorResponse($"UXML asset not found: {assetPath}");

            return new SuccessResponse("UI tree dumped (asset).", new
            {
                mode = "asset",
                assetPath,
                tree = assetProvider.GetTree(maxDepth),
            });
        }
    }
}
