using MerkleAppendTree;

namespace MerkleAppendTree
{
    public class EventMerkleNode : MerkleNode
    {
        public string Text { get; set; } // Useful for diagramming.
        public EventMerkleNode(EventMerkleNode left, EventMerkleNode right = null) : base(left, right)
        {
            MergeText(left, right);
            EventMerkleTree.Output($"New node:{Text}, L:{left.Text}, R:{right?.Text}");
        }

        public EventMerkleNode(MerkleHash hash)
        {
            Hash = hash;
        }

        public override string ToString()
        {
            // Useful for debugging, we use the node text if it exists, otherwise return the hash as a string.
            return Text ?? Hash.ToString();
        }

        public static EventMerkleNode Create(string s)
        {
            return new EventMerkleNode(MerkleHash.Create(s));
        }

        public MerkleNode SetText(string text)
        {
            Text = text;

            return this;
        }

        protected void MergeText(MerkleNode left, MerkleNode right)
        {
            // Useful for debugging, we combine the text of the two nodes.
            string text = (((EventMerkleNode) left)?.Text ?? "") + (((EventMerkleNode) right)?.Text ?? "");

            if (!string.IsNullOrEmpty(text))
            {
                if (right == null)
                {
                    text = $"({text})";
                }

                Text = text;
            }
        }

        protected void MergeText()
        {
            EventMerkleTree.Output($"Updating Text from L:{((EventMerkleNode)LeftNode).Text}, R:{((EventMerkleNode)RightNode)?.Text}");
            MergeText(LeftNode, RightNode);
            ((EventMerkleNode)Parent)?.MergeText();
        }

        public override MerkleNode AppendMerkleNode(MerkleNode node)
        {
            if (RightNode == null)
            {
                SetRightNode(node);
                EventMerkleTree.Output($"Node {((EventMerkleNode)node).Text} set as right node on {this.Text}");
                MergeText();
                return this;
            }
            if (Parent == null)
            {
                EventMerkleTree.Output($"Parent of {this} was null on {((EventMerkleNode)node).Text}, will create new parent.");
                var newParent = new EventMerkleNode((EventMerkleNode) node, null);
                new EventMerkleNode(this, newParent);
                return newParent;
            }
            else // Parent exists
            {
                EventMerkleTree.Output($"Parent of {this} was NOT null:{((EventMerkleNode)Parent).Text} on {((EventMerkleNode)node).Text}"); 
                var newParent = new EventMerkleNode((EventMerkleNode) node, null);
                ((EventMerkleNode) Parent).AppendMerkleNode(newParent);
                return newParent;
            }
        }
    }
}