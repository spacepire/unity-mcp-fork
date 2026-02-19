namespace MCPForUnity.Editor.Helpers
{
    /// <summary>
    /// Abstraction over different sources of UI Toolkit element trees.
    /// Implementations: RuntimeUiTreeProvider (Play Mode), AssetUiTreeProvider (Edit Mode).
    /// </summary>
    public interface IUiTreeProvider
    {
        /// <summary>Whether this provider can supply a tree right now.</summary>
        bool IsAvailable { get; }

        /// <summary>Returns the serialized element tree up to <paramref name="maxDepth"/> levels.</summary>
        object GetTree(int maxDepth);
    }
}
