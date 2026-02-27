using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace MCPForUnity.Editor.Helpers
{
    /// <summary>
    /// Recursively serializes a VisualElement hierarchy into a plain-object tree
    /// suitable for JSON output. Depth is capped at <c>maxDepth</c> to avoid
    /// runaway serialization on deeply nested trees.
    /// </summary>
    public static class UiDumpSerializer
    {
        /// <summary>
        /// Serializes <paramref name="element"/> and its descendants.
        /// </summary>
        /// <param name="element">Root element to serialize.</param>
        /// <param name="maxDepth">Maximum recursion depth (inclusive).</param>
        /// <param name="depth">Current depth (used during recursion).</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> node, or <c>null</c> if the element is null.</returns>
        public static object SerializeElement(VisualElement element, int maxDepth = 10, int depth = 0)
        {
            if (element == null) return null;

            var node = new Dictionary<string, object>
            {
                { "name", element.name },
                { "type", element.GetType().Name },
            };

            if (!string.IsNullOrEmpty(element.viewDataKey))
                node["viewDataKey"] = element.viewDataKey;

            var classes = element.GetClasses().ToList();
            if (classes.Count > 0)
                node["classes"] = classes;

            if (element.childCount > 0 && depth < maxDepth)
            {
                var children = new List<object>();
                foreach (var child in element.Children())
                {
                    var childNode = SerializeElement(child, maxDepth, depth + 1);
                    if (childNode != null) children.Add(childNode);
                }
                if (children.Count > 0)
                    node["children"] = children;
            }

            return node;
        }
    }
}
