using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Clifton.Blockchain;

namespace MerkleTreeDemo
{
    public class Demo
    {
        public void CreateTree(MerkleTree tree, int numLeaves)
        {
            List<DemoMerkleNode> leaves = new List<DemoMerkleNode>();

            for (int i = 0; i < numLeaves; i++)
            {
                tree.AppendLeaf(DemoMerkleNode.Create(i.ToString()).SetText(i.ToString("X")));
            }

            tree.BuildTree();
        }
    }
}