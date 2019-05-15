using MerkleAppendTree;
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
        public void CompareCliftonMerkleToDebugEventMerkle_Test(int leafCount)
        {
            var demo = new Demo();
            var cliftonTree = new DemoMerkleTree();
            demo.CreateTree(cliftonTree, leafCount);
            
            var appendTree = new EventMerkleTree();
            for (int i = 0; i < leafCount; i++)
            {
                var newLeafNode = EventMerkleNode.Create(i.ToString()).SetText(i.ToString("X"));
                appendTree.AppendLeaf(newLeafNode);
            }
            
            Assert.Equal(cliftonTree.RootNode.Hash.ToString(), appendTree.RootNode.Hash.ToString());
            if (leafCount > 1)
                Assert.Equal(((DemoMerkleNode)cliftonTree.RootNode).Text, ((EventMerkleNode)appendTree.RootNode).Text);
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
        public void CompareCliftonMerkleToAppendMerkle_Test(int leafCount)
        {
            var cliftonTree = new Clifton.Blockchain.MerkleTree();
            for (int i = 0; i < leafCount; i++)
            {
                var newLeafNode = new Clifton.Blockchain.MerkleNode(Clifton.Blockchain.MerkleHash.Create(i.ToString()));
                cliftonTree.AppendLeaf(newLeafNode);
            }
            cliftonTree.BuildTree();

            var appendTree = new MerkleAppendTree.MerkleTree();
            for (int i = 0; i < leafCount; i++)
            {
                var newLeafNode = new MerkleAppendTree.MerkleNode(MerkleAppendTree.MerkleHash.Create(i.ToString()));
                appendTree.AppendLeaf(newLeafNode);
            }
            
            Assert.Equal(cliftonTree.RootNode.Hash.ToString(), appendTree.RootNode.Hash.ToString());
        }
        
    }
}