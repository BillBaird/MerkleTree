using System;
using System.Linq;
using MerkleAppendTree;

namespace MerkleAppendTree
{
    public class PruningMerkleNode : MerkleNode
    {
        public bool IsPruned { get; protected set; }

        public PruningMerkleNode(PruningMerkleNode left, PruningMerkleNode right = null) : base(left, right) { }

        public PruningMerkleNode(MerkleHash hash) : base(hash) { }

        public override bool IsLeaf
            => LeftNode == null && RightNode == null && !IsPruned;

        public override void SetRightNode(MerkleNode node)
        {
            MerkleTree.Contract(() => node.Hash != null, "Node hash must be initialized.");
            RightNode = node;
            RightNode.Parent = this;

            // Can't compute hash if the left node isn't set yet.
            if (LeftNode != null)
            {
                ComputeHash();
                if (((PruningMerkleNode) RightNode).IsFullOrPruned)
                {
                    // The hash has been calculated, so it cannot change.  Therefore we should never need the children again.
                    // Prune the nodes
                    IsPruned = true;
                    LeftNode = null;
                    RightNode = null;
                }
            }
        }

        public bool IsFullOrPruned
        {
            get
            {
                if (IsPruned || IsLeaf)
                    return true;
                return LeftNode != null &&
                       RightNode != null &&
                       ((PruningMerkleNode) LeftNode).IsFullOrPruned &&
                       ((PruningMerkleNode) RightNode).IsFullOrPruned;
            }
        }
        
        protected override void ComputeHash()
        {
            // Repeat the left node if the right node doesn't exist.
            // This process breaks the case of doing a consistency check on 3 leaves when there are only 3 leaves in the tree.
            //MerkleHash rightHash = RightNode == null ? LeftNode.Hash : RightNode.Hash;
            //Hash = MerkleHash.Create(LeftNode.Hash.Value.Concat(rightHash.Value).ToArray());

            // Alternativately, do not repeat the left node, but carry the left node's hash up.
            // This process does not break the edge case described above.
            // We're implementing this version because the consistency check unit tests pass when we don't simulate
            // a right-hand node.
            if (!IsPruned)
                Hash = RightNode == null 
                    ? LeftNode.Hash //MerkleHash.Create(LeftNode.Hash.Value.Concat(LeftNode.Hash.Value).ToArray()) : 
                    : MerkleHash.Create(LeftNode.Hash.Value.Concat(RightNode.Hash.Value).ToArray());
//            if (Parent != null && !IsPruned && !IsLeaf && RightNode != null && ((PruningMerkleNode)LeftNode).IsFullOrPruned && ((PruningMerkleNode)RightNode).IsFullOrPruned)
//            {
//                this.IsPruned = true;
//                this.LeftNode = null;
//                this.RightNode = null;
//            }
            ((PruningMerkleNode)Parent)?.ComputeHash();      // Recurse, because our hash has changed.
        }
        
        public override MerkleNode AppendMerkleNode(MerkleNode node)
        {
            if (RightNode == null && !IsPruned)
            {
                SetRightNode(node);
                return this;
            }
            if (Parent == null)
            {
                var newParent = new PruningMerkleNode((PruningMerkleNode) node, null);
                new PruningMerkleNode(this, newParent);
                return newParent;
            }
            else // Parent exists
            {
                var newParent = new PruningMerkleNode((PruningMerkleNode) node, null);
                ((PruningMerkleNode) Parent).AppendMerkleNode(newParent);
                return newParent;
            }
        }
    }
}