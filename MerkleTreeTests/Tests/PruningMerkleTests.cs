using MerkleAppendTree;
using Xunit;
using Xunit.Abstractions;

namespace MerkleTests.Tests
{
    public class PruningMerkleTests
    {
        protected readonly ITestOutputHelper Output;
        
        public PruningMerkleTests(ITestOutputHelper output)
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
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(63)]
        [InlineData(64)]
        [InlineData(65)]
        [InlineData(4096)]
        public void CompareCliftonMerkleToDebugEventMerkle_Test(int leafCount)
        {
            var appendTree = new EventMerkleTree();
            for (int i = 0; i < leafCount; i++)
            {
                var newLeafNode = EventMerkleNode.Create(i.ToString()).SetText(i.ToString("X"));
                appendTree.AppendLeaf(newLeafNode);
            }
            DrawNode(appendTree.RootNode);
            
            Output.WriteLine("----------------");
            
            var pruningTree = new PruningMerkleTree();
            for (int i = 0; i < leafCount; i++)
            {
                var newLeafNode = new PruningMerkleNode(MerkleHash.Create(i.ToString()));
                pruningTree.AppendLeaf(newLeafNode);
            }
            DrawPruningNode((PruningMerkleNode)pruningTree.RootNode);
            
            Assert.Equal(appendTree.RootNode.Hash.ToString(), pruningTree.RootNode.Hash.ToString());
        }
        
        public void DrawNode(MerkleNode node, int depth = 0)
        {
            if (node.IsLeaf)
                Output.WriteLine($"{new string(' ', depth * 3)}{node}: {node.Hash}");
            else
            {
                if (node.LeftNode != null)
                    DrawNode(node.LeftNode, depth + 1);
                Output.WriteLine($"{new string(' ', depth * 3)}{node}: {node.Hash}");
                if (node.RightNode != null)
                    DrawNode(node.RightNode, depth + 1);
            }
        }

        public void DrawPruningNode(PruningMerkleNode node, int depth = 0)
        {
            if (node.IsLeaf)
                Output.WriteLine($"{new string(' ', depth * 3)}{node}");
            else
            {
                if (node.LeftNode != null)
                    DrawPruningNode((PruningMerkleNode)node.LeftNode, depth + 1);
                Output.WriteLine($"{new string(' ', depth * 3)}{node} {(node.IsPruned ? "(P)" : "")} {node.IsFullOrPruned.ToString()}");
                if (node.RightNode != null)
                    DrawPruningNode((PruningMerkleNode)node.RightNode, depth + 1);
            }
        }

    }
}