using System;
using MerkleAppendTree;

namespace MerkleAppendTree
{
    public class EventMerkleTree : MerkleTree
    {
        public static Action<string> OutputWriter = (s =>
        {
            if(false)
                Console.WriteLine(s);
        });

        public static void Output(string s)
        {
            OutputWriter?.Invoke(s);
        }
        
        protected override MerkleNode CreateNode(MerkleHash hash)
        {
            return new EventMerkleNode(hash);
        }

        protected override MerkleNode CreateNode(MerkleNode left, MerkleNode right)
        {
            var eLeft = (EventMerkleNode) left;
            var eRight = (EventMerkleNode) right;
            Output($"New node from EventMerkleTree.CreateNode - L:{eLeft.Text}, R:{eRight?.Text}");
            return new EventMerkleNode(eLeft, eRight);
        }
        
        public override MerkleNode AppendLeafNode(MerkleNode node)
        {
            if (CurrentLeaf == null) // Meaning we have an even number of nodes (or zero)
            {
                // There were an even number of Leaves, including zero
                if (CurrentParent == null)
                {
                    EventMerkleTree.OutputWriter($"New tree, first append - left leaf");
                    // This is the first node
                    CurrentParent = CreateNode(node, null);
                    CurrentLeaf = node;
                }
                else
                {
                    EventMerkleTree.OutputWriter($"Append left leaf");
                    CurrentParent = ((EventMerkleNode)CurrentParent).AppendMerkleNode(node);
                    CurrentLeaf = node;
                }    
            }
            else
            {
                EventMerkleTree.OutputWriter($"Append right leaf");
                CurrentParent = ((EventMerkleNode)CurrentParent).AppendMerkleNode(node);
                CurrentLeaf = null;
            }

            return node;
        }

    }
}