﻿/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace MerkleAppendTree
{
    public class MerkleTree
    {
        public MerkleNode CurrentParent { get; protected set; }
        public MerkleNode CurrentLeaf { get; protected set; }

        private MerkleNode rootNode = null;
        public MerkleNode RootNode
        {
            get
            {
                if (rootNode == null)
                    rootNode = CurrentParent?.Root;
                if (rootNode?.Root != null)
                    rootNode = rootNode.Root;    // Recurse up to the root if needed
                return rootNode;
            }
        }
        
        public static void Contract(Func<bool> action, string msg)
        {
            if (!action())
            {
                throw new MerkleException(msg);
            }
        }

        public MerkleTree()
        {
        }

        public virtual MerkleNode AppendLeaf(MerkleNode node)
        {
            if (CurrentLeaf == null) // Meaning we have an even number of nodes (or zero)
            {
                // There were an even number of Leaves, including zero
                if (CurrentParent == null)
                {
                    // This is the first node
                    CurrentParent = CreateNode(node, null);
                    CurrentLeaf = node;
                }
                else
                {
                    // Append the leaf on the left, creating a new parent for it
                    CurrentParent = CurrentParent.AppendMerkleNode(node);
                    CurrentLeaf = node;
                }    
            }
            else
            {
                // Append the leaf on the right, creating a new parent for it
                CurrentParent = CurrentParent.AppendMerkleNode(node);
                CurrentLeaf = null;
            }

            return node;
        }

        public MerkleNode AppendLeaf(MerkleHash hash)
            => AppendLeaf(CreateNode(hash));

        public void AppendLeaves(MerkleNode[] nodes)
        {
            foreach (var n in nodes)
                AppendLeaf(n);
        }

        public List<MerkleNode> AppendLeaves(MerkleHash[] hashes)
        {
            List<MerkleNode> nodes = new List<MerkleNode>();
            foreach (var h in hashes)
                nodes.Add(AppendLeaf(h));

            return nodes;
        }
/*
        public MerkleHash AddTree(MerkleTree tree)
        {
            Contract(() => leaves.Count > 0, "Cannot add to a tree with no leaves.");
            tree.leaves.ForEach(l => AppendLeaf(l));

            return BuildTree();
        }

        /// <summary>
        /// If we have an odd number of leaves, add a leaf that
        /// is a duplicate of the last leaf hash so that when we add the leaves of the new tree,
        /// we don't change the root hash of the current tree.
        /// This method should only be used if you have a specific reason that you need to balance
        /// the last node with it's right branch, for example as a pre-step to computing an audit trail
        /// on the last leaf of an odd number of leaves in the tree.
        /// </summary>
        public void FixOddNumberLeaves()
        {
            if ((leaves.Count & 1) == 1)
            {
                var lastLeaf = leaves.Last();
                var l = AppendLeaf(lastLeaf.Hash);
                // l.Text = lastLeaf.Text;
            }
        }
*/
        /// <summary>
        /// Returns the audit proof hashes to reconstruct the root hash.
        /// </summary>
        /// <param name="leafHash">The leaf hash we want to verify exists in the tree.</param>
        /// <returns>The audit trail of hashes needed to create the root, or an empty list if the leaf hash doesn't exist.</returns>
        public List<MerkleProofHash> AuditProof(MerkleHash leafHash)
        {
            List<MerkleProofHash> auditTrail = new List<MerkleProofHash>();

            var leafNode = FindLeaf(leafHash);

            if (leafNode != null)
            {
                Contract(() => leafNode.Parent != null, "Expected leaf to have a parent.");
                var parent = leafNode.Parent;
                BuildAuditTrail(auditTrail, parent, leafNode);
            }

            return auditTrail;
        }

        /// <summary>
        /// Verifies ordering and consistency of the first n leaves, such that we reach the expected subroot.
        /// This verifies that the prior data has not been changed and that leaf order has been preserved.
        /// m is the number of leaves for which to do a consistency check.
        /// </summary>
        public List<MerkleProofHash> ConsistencyProof(int m)
        {
            // Rule 1:
            // Find the leftmost node of the tree from which we can start our consistency proof.
            // Set k, the number of leaves for this node.
            List<MerkleProofHash> hashNodes = new List<MerkleProofHash>();
            int idx = (int)Math.Log(m, 2);

            // Get the leftmost node.
            MerkleNode node = RootNode.Leaves().FirstOrDefault(leaf => true);

            // Traverse up the tree until we get to the node specified by idx.
            while (idx > 0)
            {
                node = node.Parent;
                --idx;
            }

            int k = node.Leaves().Count();
            hashNodes.Add(new MerkleProofHash(node.Hash, MerkleProofHash.Branch.OldRoot));

            if (m == k)
            {
                // Continue with Rule 3 -- the remainder is the audit proof
            }
            else
            {
                // Rule 2:
                // Set the initial sibling node (SN) to the sibling of the node acquired by Rule 1.
                // if m-k == # of SN's leaves, concatenate the hash of the sibling SN and exit Rule 2, as this represents the hash of the old root.
                // if m - k < # of SN's leaves, set SN to SN's left child node and repeat Rule 2.

                // sibling node:
                MerkleNode sn = node.Parent.RightNode;
                bool traverseTree = true;

                while (traverseTree)
                {
                    Contract(() => sn != null, "Sibling node must exist because m != k");
                    int sncount = sn.Leaves().Count();

                    if (m - k == sncount)
                    {
                        hashNodes.Add(new MerkleProofHash(sn.Hash, MerkleProofHash.Branch.OldRoot));
                        break;
                    }

                    if (m - k > sncount)
                    {
                        hashNodes.Add(new MerkleProofHash(sn.Hash, MerkleProofHash.Branch.OldRoot));
                        sn = sn.Parent.RightNode;
                        k += sncount;
                    }
                    else // (m - k < sncount)
                    {
                        sn = sn.LeftNode;
                    }
                }
            }

            // Rule 3: Apply ConsistencyAuditProof below.

            return hashNodes;
        }

        /// <summary>
        /// Completes the consistency proof with an audit proof using the last node in the consistency proof.
        /// </summary>
        public List<MerkleProofHash> ConsistencyAuditProof(MerkleHash nodeHash)
        {
            List<MerkleProofHash> auditTrail = new List<MerkleProofHash>();

            var node = RootNode.Single(n => n.Hash == nodeHash);
            var parent = node.Parent;
            BuildAuditTrail(auditTrail, parent, node);

            return auditTrail;
        }

        /// <summary>
        /// Verify that if we walk up the tree from a particular leaf, we encounter the expected root hash.
        /// </summary>
        public static bool VerifyAudit(MerkleHash rootHash, MerkleHash leafHash, List<MerkleProofHash> auditTrail)
        {
            MerkleHash testHash = leafHash;

            // TODO: Inefficient - compute hashes directly.
            foreach (MerkleProofHash auditHash in auditTrail)
            {
                testHash = auditHash.Direction == MerkleProofHash.Branch.Left ?
                    MerkleHash.Create(testHash.Value.Concat(auditHash.Hash.Value).ToArray()) :
                    MerkleHash.Create(auditHash.Hash.Value.Concat(testHash.Value).ToArray());
            }

            return rootHash == testHash;
        }
/*
        /// <summary>
        /// For demo / debugging purposes, we return the pairs of hashes used to verify the audit proof.
        /// </summary>
        public static List<Tuple<MerkleHash, MerkleHash>> AuditHashPairs(MerkleHash leafHash, List<MerkleProofHash> auditTrail)
        {
            Contract(() => auditTrail.Count > 0, "Audit trail cannot be empty.");
            var auditPairs = new List<Tuple<MerkleHash, MerkleHash>>();
            MerkleHash testHash = leafHash;

            // TODO: Inefficient - compute hashes directly.
            foreach (MerkleProofHash auditHash in auditTrail)
            {
                switch (auditHash.Direction)
                {
                    case MerkleProofHash.Branch.Left:
                        auditPairs.Add(new Tuple<MerkleHash, MerkleHash>(testHash, auditHash.Hash));
                        testHash = MerkleHash.Create(testHash.Value.Concat(auditHash.Hash.Value).ToArray());
                        break;

                    case MerkleProofHash.Branch.Right:
                        auditPairs.Add(new Tuple<MerkleHash, MerkleHash>(auditHash.Hash, testHash));
                        testHash = MerkleHash.Create(auditHash.Hash.Value.Concat(testHash.Value).ToArray());
                        break;
                }
            }

            return auditPairs;
        }

        public static bool VerifyConsistency(MerkleHash oldRootHash, List<MerkleProofHash> proof)
        {
            MerkleHash hash, lhash, rhash;

            if (proof.Count > 1)
            {
                lhash = proof[proof.Count - 2].Hash;
                int hidx = proof.Count - 1;
                hash = rhash = MerkleTree.ComputeHash(lhash, proof[hidx].Hash);
                hidx -= 2;

                // foreach (var nextHashNode in proof.Skip(1))
                while (hidx >= 0)
                {
                    lhash = proof[hidx].Hash;
                    hash = rhash = MerkleTree.ComputeHash(lhash, rhash);

                    --hidx;
                }
            }
            else
            {
                hash = proof[0].Hash;
            }

            return hash == oldRootHash;
        }
*/

        public static MerkleHash ComputeHash(MerkleHash left, MerkleHash right)
        {
            return MerkleHash.Create(left.Value.Concat(right.Value).ToArray());
        }
        
        protected void BuildAuditTrail(List<MerkleProofHash> auditTrail, MerkleNode parent, MerkleNode child)
        {
            if (parent != null)
            {
                Contract(() => child.Parent == parent, "Parent of child is not expected parent.");
                var nextChild = parent.LeftNode == child ? parent.RightNode : parent.LeftNode;
                var direction = parent.LeftNode == child ? MerkleProofHash.Branch.Left : MerkleProofHash.Branch.Right;

                // For the last leaf, the right node may not exist.  In that case, we ignore it because it's
                // the hash we are given to verify.
                if (nextChild != null)
                {
                    auditTrail.Add(new MerkleProofHash(nextChild.Hash, direction));
                }

                BuildAuditTrail(auditTrail, child.Parent.Parent, child.Parent);
            }
        }

        protected MerkleNode FindLeaf(MerkleHash leafHash)
        {
            return RootNode.Leaves().FirstOrDefault(l => l.Hash == leafHash);
        }

        // Override in derived class to extend the behavior.
        // Alternatively, we could implement a factory pattern.

        protected virtual MerkleNode CreateNode(MerkleHash hash)
        {
            return new MerkleNode(hash);
        }

        protected virtual MerkleNode CreateNode(MerkleNode left, MerkleNode right)
        {
            return new MerkleNode(left, right);
        }
    }
}
