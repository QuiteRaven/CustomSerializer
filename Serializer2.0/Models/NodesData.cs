namespace SerializerTests.Nodes
{
    public class NodesData
    {
        public int NextIndex { get; set; }
        public int RandomIndex { get; set; }
        public string Data { get; set; }
        public ListNode Original { get; }

        public NodesData(ListNode original)
        {
            Original = original;
        }
    }
}
