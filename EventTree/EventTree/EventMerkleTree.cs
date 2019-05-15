using System;

namespace EventTree
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
    }
}