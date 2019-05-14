using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using EventTree;
using TestingUtil;
using Xunit;
using Xunit.Abstractions;

namespace EventTreeTests
{
    public class EventTreeVisualizationTests
    {
        protected readonly ITestOutputHelper Output;
        
        public EventTreeVisualizationTests(ITestOutputHelper output)
        {
            Output = output;
            // Give the EventMerkleTree implementation a way of writing to the output log
            EventMerkleTree.OutputWriter = output.WriteLine;
        }
        
        public void DrawNode(MerkleNode node, int depth = 0)
        {
            if (node.IsLeaf)
            {
                //Output.WriteLine($"{new string(' ', depth * 3)}{node}: {node.Hash} P:{node.Parent}");
                Output.WriteLine($"{new string(' ', depth * 3)}{node}: {node.Hash}");
            }
            else
            {
                DrawNode(node.LeftNode, depth + 1);
                //Output.WriteLine($"{new string(' ', depth * 3)}{node}: {node.Hash} P:{node.Parent}, L:{node.LeftNode}, R:{node.RightNode}");
                Output.WriteLine($"{new string(' ', depth * 3)}{node}: {node.Hash}");
                if (node.RightNode != null)
                    DrawNode(node.RightNode, depth + 1);
            }
        }
        
        [Fact]
        public void TreeIncrementalPersistTest2()
        {
            var tree = new EventMerkleTree();
            for (int i = 0; i <= 64; i++)
                AppendLeafAndDraw(tree, i);
            Assert.True(true);
        }
        
        public void AppendLeafAndDraw(EventMerkleTree tree, int content)
        {
            Output.WriteLine($"Append {content}");
            var newLeafNode = EventMerkleNode.Create(content.ToString()).SetText(content.AsAlphaChar());
            tree.AppendLeafNode(newLeafNode);
            Output.WriteLine($"CP: {tree.CurrentParent}, CL: {tree.CurrentLeaf}");
            DrawNode(tree.RootNode);
            Output.WriteLine("");
        }
        
    }
}