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
            Assert.Equal(node1.Data, deepCopy.Data);
            Assert.Equal(node1.Next.Data, deepCopy.Next.Data);
        }

        [Fact]
        public async Task SerializeDeserialize_ShouldPreserveListStructure()
        {
            // Arrange
            var originalList = new ListNode { Data = "A" };
            originalList.Next = new ListNode { Data = "B" };
            originalList.Random = originalList.Next;

            var serializer = new ListSerializer();
            using (var stream = new MemoryStream())
            {
                // Act
                await serializer.Serialize(originalList, stream);
                stream.Seek(0, SeekOrigin.Begin);
                var deserializedList = await serializer.Deserialize(stream);

                // Assert
                Assert.Equal(originalList.Data, deserializedList.Data);
                Assert.Equal(originalList.Next.Data, deserializedList.Next.Data);
            }
        }

        [Fact]
        public async Task SerializeDeserialize_EmptyList_ShouldThrowException()
        {
            // Arrange
            var emptyList = null as ListNode;
            var serializer = new ListSerializer();
            using (var stream = new MemoryStream())
            {
                // Act
                await serializer.Serialize(emptyList, stream);
                stream.Seek(0, SeekOrigin.Begin);
                // Assert
                await Assert.ThrowsAsync<ArgumentException>(() => serializer.Deserialize(stream));
            }
        }
    }
}