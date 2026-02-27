using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MCPForUnity.Editor.Helpers
{
    /// <summary>
    /// Play-Mode UI tree provider. Walks all <see cref="UIDocument"/> components
    /// in the active scenes and serializes their live <c>rootVisualElement</c> trees.
    /// </summary>
    public class RuntimeUiTreeProvider : IUiTreeProvider
    {
        /// <inheritdoc/>
        public bool IsAvailable => Application.isPlaying;

        /// <inheritdoc/>
        public object GetTree(int maxDepth)
        {
            var results = new List<object>();
            var docs = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            foreach (var doc in docs)
            {
                if (doc.rootVisualElement != null)
                {
                    results.Add(new Dictionary<string, object>
                    {
                        { "gameObject", doc.gameObject.name },
                        { "tree", UiDumpSerializer.SerializeElement(doc.rootVisualElement, maxDepth) },
                    });
                }
            }
            return results;
        }
    }
}
