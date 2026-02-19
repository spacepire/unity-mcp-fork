using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;
using Newtonsoft.Json.Linq;

namespace MCPForUnityTests.Editor.Tools
{
    /// <summary>
    /// Tests for the UI Toolkit inspection system:
    /// UiDumpSerializer, AssetUiTreeProvider, and the DumpUi MCP tool.
    /// </summary>
    public class DumpUiTests
    {
        private const string TestUxmlPath = "Assets/Tests/EditMode/Tools/TestDumpUi.uxml";

        [SetUp]
        public void SetUp()
        {
            string uxmlContent =
                "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\">\n" +
                "  <ui:VisualElement name=\"container\" class=\"root-panel\">\n" +
                "    <ui:Label name=\"title\" text=\"Hello\" />\n" +
                "    <ui:Button name=\"btn\" text=\"Click\" />\n" +
                "  </ui:VisualElement>\n" +
                "</ui:UXML>";

            string dir = System.IO.Path.GetDirectoryName(TestUxmlPath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            System.IO.File.WriteAllText(TestUxmlPath, uxmlContent);
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            if (System.IO.File.Exists(TestUxmlPath))
                AssetDatabase.DeleteAsset(TestUxmlPath);
        }

        // ── UiDumpSerializer ────────────────────────────────────────────────

        [Test]
        public void SerializeElement_NullElement_ReturnsNull()
        {
            Assert.IsNull(UiDumpSerializer.SerializeElement(null));
        }

        [Test]
        public void SerializeElement_SimpleElement_ReturnsNameAndType()
        {
            var el = new VisualElement { name = "myEl" };

            var result = UiDumpSerializer.SerializeElement(el) as Dictionary<string, object>;

            Assert.IsNotNull(result);
            Assert.AreEqual("myEl", result["name"]);
            Assert.AreEqual("VisualElement", result["type"]);
            Assert.IsFalse(result.ContainsKey("children"), "Leaf element should have no children key");
        }

        [Test]
        public void SerializeElement_WithChildren_IncludesChildrenList()
        {
            var parent = new VisualElement { name = "parent" };
            parent.Add(new Label { name = "child1" });
            parent.Add(new Button { name = "child2" });

            var result = UiDumpSerializer.SerializeElement(parent) as Dictionary<string, object>;

            Assert.IsNotNull(result);
            var children = result["children"] as List<object>;
            Assert.IsNotNull(children);
            Assert.AreEqual(2, children.Count);
        }

        [Test]
        public void SerializeElement_MaxDepth_TruncatesRecursion()
        {
            var root = new VisualElement { name = "root" };
            var child = new VisualElement { name = "child" };
            var grandchild = new VisualElement { name = "grandchild" };
            root.Add(child);
            child.Add(grandchild);

            var result = UiDumpSerializer.SerializeElement(root, maxDepth: 1) as Dictionary<string, object>;

            Assert.IsNotNull(result);
            var children = result["children"] as List<object>;
            Assert.IsNotNull(children);
            // child is present but its grandchild should be absent (depth == maxDepth)
            var childNode = children[0] as Dictionary<string, object>;
            Assert.IsNotNull(childNode);
            Assert.IsFalse(childNode.ContainsKey("children"), "Grandchild should be truncated at maxDepth=1");
        }

        [Test]
        public void SerializeElement_WithClasses_IncludesClassList()
        {
            var el = new VisualElement { name = "styled" };
            el.AddToClassList("foo");
            el.AddToClassList("bar");

            var result = UiDumpSerializer.SerializeElement(el) as Dictionary<string, object>;

            Assert.IsNotNull(result);
            var classes = result["classes"] as List<string>;
            Assert.IsNotNull(classes);
            Assert.IsTrue(classes.Contains("foo"));
            Assert.IsTrue(classes.Contains("bar"));
        }

        // ── AssetUiTreeProvider ─────────────────────────────────────────────

        [Test]
        public void AssetUiTreeProvider_MissingAsset_IsNotAvailable()
        {
            var provider = new AssetUiTreeProvider("Assets/NonExistent.uxml");
            Assert.IsFalse(provider.IsAvailable);
        }

        [Test]
        public void AssetUiTreeProvider_ValidAsset_IsAvailable()
        {
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TestUxmlPath);
            if (vta == null) Assert.Inconclusive("Test UXML asset not created.");

            var provider = new AssetUiTreeProvider(TestUxmlPath);
            Assert.IsTrue(provider.IsAvailable);
        }

        [Test]
        [Timeout(10000)]
        public void AssetUiTreeProvider_GetTree_ReturnsSerializedHierarchy()
        {
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TestUxmlPath);
            if (vta == null) Assert.Inconclusive("Test UXML asset not created.");

            var provider = new AssetUiTreeProvider(TestUxmlPath);
            var tree = provider.GetTree(maxDepth: 10) as Dictionary<string, object>;

            Assert.IsNotNull(tree, "GetTree should return a non-null node");
            // Root is a VisualElement wrapper; its children come from the UXML
            var children = tree["children"] as List<object>;
            Assert.IsNotNull(children, "Root wrapper should have children from UXML");
        }

        // ── DumpUi tool ─────────────────────────────────────────────────────

        [Test]
        public void DumpUi_NullParams_ReturnsError()
        {
            var result = DumpUi.HandleCommand(null);
            Assert.IsInstanceOf<ErrorResponse>(result);
        }

        [Test]
        public void DumpUi_EditMode_MissingAssetPath_ReturnsError()
        {
            // Edit Mode is the default when not playing
            var result = DumpUi.HandleCommand(new JObject());
            Assert.IsInstanceOf<ErrorResponse>(result);
        }

        [Test]
        public void DumpUi_EditMode_NonExistentAsset_ReturnsError()
        {
            var result = DumpUi.HandleCommand(new JObject
            {
                ["asset_path"] = "Assets/DoesNotExist.uxml"
            });
            Assert.IsInstanceOf<ErrorResponse>(result);
        }

        [Test]
        [Timeout(10000)]
        public void DumpUi_EditMode_ValidAsset_ReturnsSuccess()
        {
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TestUxmlPath);
            if (vta == null) Assert.Inconclusive("Test UXML asset not created.");

            var result = DumpUi.HandleCommand(new JObject
            {
                ["asset_path"] = TestUxmlPath
            });

            Assert.IsInstanceOf<SuccessResponse>(result);
            var success = (SuccessResponse)result;
            Assert.IsNotNull(success.Data);
        }
    }
}
