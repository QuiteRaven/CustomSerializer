namespace SerializerTests.Nodes
{
    public class SerializedNode
    {
        public Guid Id { get; set; }

        public Guid Previous { get; set; }

        public Guid Next { get; set; }

        public Guid Random { get; set; }

        public string Data { get; set; }
    }
}
