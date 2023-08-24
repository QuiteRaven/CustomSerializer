using SerializerTests.Nodes;

namespace UnitTests
{
    public class SerializerTests
    {
        [Fact]
        public async Task DeepCopy_ShouldCreateIdenticalCopy()
        {
            // Arrange
            ListNode node1 = new ListNode { Data = "Node 1" };
            ListNode node2 = new ListNode { Data = "Node 2" };
            ListNode node3 = new ListNode { Data = "Node 3" };

            node1.Next = node2;
            node2.Next = node3;
            node2.Previous = node1;
            node3.Previous = node2;
            node1.Random = node3;
            node2.Random = node1;
            node3.Random = node2;

            var serializer = new ListSerializer();

            // Act
            var deepCopy = await serializer.DeepCopy(node1);

            // Assert
            AssertLinkedNodesEqual(node1, deepCopy);
        }

        [Fact]
        public async Task Deserialize_StreamHasInvalidData_ShouldThrowException()
        {
            // Arrange
            var emptyList = null as ListNode;
            var serializer = new ListSerializer();
            using (var stream = new MemoryStream())
            {
                // Act
                var writer = new StreamWriter(stream);
                await writer.WriteLineAsync("sdfsdfdsg");
                await writer.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);
                // Assert
                await Assert.ThrowsAsync<ArgumentException>(() => serializer.Deserialize(stream));
            }
        }

        [Fact]
        public async Task SerializeDeserialize_SingleNode()
        {
            var node = new ListNode { Data = "Node 1" };
            var serializer = new ListSerializer();

            using (var memoryStream = new MemoryStream())
            {
                await serializer.Serialize(node, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var deserializedNode = await serializer.Deserialize(memoryStream);

                AssertLinkedNodesEqual(node, deserializedNode);
            }
        }

        private void AssertLinkedNodesEqual(ListNode expected, ListNode actual)
        {
            var expectedNodes = new List<ListNode>();
            var actualNodes = new List<ListNode>();

            while (expected != null && actual != null)
            {
                expectedNodes.Add(expected);
                actualNodes.Add(actual);
                Assert.Equal(expected.Data, actual.Data);
                expected = expected.Next;
                actual = actual.Next;
            }

            Assert.Equal(expected, actual);

            for (int i = 0; i < expectedNodes.Count; i++)
            {
                Assert.Equal(expectedNodes[i].Previous?.Data, actualNodes[i].Previous?.Data);
                Assert.Equal(expectedNodes[i].Next?.Data, actualNodes[i].Next?.Data);
            }
        }
    }
}