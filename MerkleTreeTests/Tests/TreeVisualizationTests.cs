using System.Linq.Expressions;
using Clifton.Blockchain;
using MerkleTreeDemo;
using TestingUtil;
using Xunit;
using Xunit.Abstractions;

namespace MerkleTests
{
    public class TreeVisualizationTests
    {
        protected readonly ITestOutputHelper Output;

        public TreeVisualizationTests(ITestOutputHelper output)
        {
            Output = output;
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        public void TreeTest(int nodeCount)
        {
            var demo = new Demo();
            var tree = new DemoMerkleTree();
            demo.CreateTree(tree, nodeCount);
            DrawNode(tree.RootNode);
            Assert.True(true);
        }

        public void DrawNode(MerkleNode node, int depth = 0)
        {
            if (node.IsLeaf)
                Output.WriteLine($"{new string(' ', depth * 3)}{node}: {node.Hash}");
            else
            {
                DrawNode(node.LeftNode, depth + 1);
                Output.WriteLine($"{new string(' ', depth * 3)}{node}: {node.Hash}");
                if (node.RightNode != null)
                    DrawNode(node.RightNode, depth + 1);
            }
        }

        public void AppendLeafAndDraw(DemoMerkleTree tree, int content)
        {
            Output.WriteLine($"Append {content}");
            tree.AppendLeaf(DemoMerkleNode.Create(content.ToString()).SetText(content.AsAlphaChar()));
            tree.BuildTree();
            DrawNode(tree.RootNode);
            Output.WriteLine("");
        }
        
        [Fact]
        public void TreeIncrementalPersistTest()
        {
            var tree = new DemoMerkleTree();
            for (int i = 0; i <= 64; i++)
                AppendLeafAndDraw(tree, i);
            Assert.True(true);
        }
    }
}