namespace MerkleAppendTree
{
    public class PruningMerkleTree : MerkleTree
    {
        protected override MerkleNode CreateNode(MerkleHash hash)
        {
            return new PruningMerkleNode(hash);
        }

        protected override MerkleNode CreateNode(MerkleNode left, MerkleNode right)
        {
            return new PruningMerkleNode((PruningMerkleNode)left, (PruningMerkleNode)right);
        }
    }
}