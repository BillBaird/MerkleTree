using EventTree;
using MerkleTreeDemo;
using Xunit;
using Xunit.Abstractions;

namespace MerkleTests.Tests
{
    public class ImplementationEqualityTests
    {
        protected readonly ITestOutputHelper Output;
        
        public ImplementationEqualityTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(63)]
        [InlineData(64)]
        [InlineData(65)]
        [InlineData(4096)]
        public void CompareCliftonMerkleToEventMerkleTest(int leafCount)
        {
            var demo = new Demo();
            var cliftonTree = new DemoMerkleTree();
            demo.CreateTree(cliftonTree, leafCount);
            
            var appendTree = new EventMerkleTree();
            for (int i = 0; i < leafCount; i++)
            {
                var newLeafNode = EventMerkleNode.Create(i.ToString()).SetText(i.ToString("X"));
                appendTree.AppendLeafNode(newLeafNode);
            }
            
            Assert.Equal(cliftonTree.RootNode.Hash.ToString(), appendTree.RootNode.Hash.ToString());
            if (leafCount > 1)
                Assert.Equal(((DemoMerkleNode)cliftonTree.RootNode).Text, ((EventMerkleNode)appendTree.RootNode).Text);
        }
        
    }
}