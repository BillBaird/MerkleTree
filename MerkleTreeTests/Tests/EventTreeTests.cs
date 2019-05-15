using System.Collections.Generic;
using MerkleAppendTree;
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
            /************************************************************************************************
             * FOR DEBUGGING, uncomment the line below to get debug output.
             * This is necessary since it sets a static for writing to the output.  Alternatively, XUnit's
             * collection mechanism, which let you set mutually exclusive execution could be used so that
             * tests don't step on each other.
             ************************************************************************************************/
            //EventMerkleTree.OutputWriter = output.WriteLine;
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
            tree.AppendLeaf(newLeafNode);
            Output.WriteLine($"CP: {tree.CurrentParent}, CL: {tree.CurrentLeaf}");
            DrawNode(tree.RootNode);
            Output.WriteLine("");
        }

        // A Merkle audit path for a leaf in a Merkle Hash Tree is the shortest
        // list of additional nodes in the Merkle Tree required to compute the
        // Merkle Tree Hash for that tree.
        [Fact]
        public void AuditTest()
        {
            // Build a tree, and given the root node and a leaf hash, verify that the we can reconstruct the root hash.
            EventMerkleTree tree = new EventMerkleTree();
            MerkleHash l1 = MerkleHash.Create("abc");
            MerkleHash l2 = MerkleHash.Create("def");
            MerkleHash l3 = MerkleHash.Create("123");
            MerkleHash l4 = MerkleHash.Create("456");
            tree.AppendLeaves(new MerkleHash[] { l1, l2, l3, l4 });
            MerkleHash rootHash = tree.RootNode.Hash;

            foreach (var leaf in tree.RootNode.Leaves())
            {
                Output.WriteLine(leaf.ToString());
            }
            
            List<MerkleProofHash> auditTrail = tree.AuditProof(l1);
            Assert.True(MerkleTree.VerifyAudit(rootHash, l1, auditTrail));

            auditTrail = tree.AuditProof(l2);
            Assert.True(MerkleTree.VerifyAudit(rootHash, l2, auditTrail));

            auditTrail = tree.AuditProof(l3);
            Assert.True(MerkleTree.VerifyAudit(rootHash, l3, auditTrail));

            auditTrail = tree.AuditProof(l4);
            Assert.True(MerkleTree.VerifyAudit(rootHash, l4, auditTrail));
        }

        // A Merkle audit path for a leaf in a Merkle Hash Tree is the shortest
        // list of additional nodes in the Merkle Tree required to compute the
        // Merkle Tree Hash for that tree.
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(63)]
        [InlineData(64)]
        [InlineData(65)]
        public void AuditTest2(int leafCount)
        {
            MerkleHash hash = null;
            var tree = new MerkleAppendTree.MerkleTree();
            for (int i = 0; i < leafCount; i++)
            {
                hash = MerkleHash.Create(i.ToString());
                tree.AppendLeaf(hash);
            }

            MerkleHash rootHash = tree.RootNode.Hash;

            foreach (var leaf in tree.RootNode.Leaves())
            {
                Output.WriteLine(leaf.ToString());
            }
            
            List<MerkleProofHash> auditTrail = tree.AuditProof(hash);
            Output.WriteLine("Audit Trail:");
            foreach (var h in auditTrail)
            {
                Output.WriteLine(h.ToString());
            }
            Assert.True(MerkleTree.VerifyAudit(rootHash, hash, auditTrail));
        }

    }
}