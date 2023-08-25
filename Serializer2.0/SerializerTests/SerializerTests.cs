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
            Assert.NotSame(node1, deepCopy);
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
        public async Task SerializeDeserialize_EmptyList_ShouldReturnNull()
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
                Assert.Null(await serializer.Deserialize(stream));
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

        [Fact]
        public async Task SerializeDeserialize_TwoNodes()
        {
            var node1 = new ListNode { Data = "Node 1" };
            var node2 = new ListNode { Data = "Node 2" };

            node1.Next = node2;
            node2.Previous = node1;

            var serializer = new ListSerializer();

            using (var memoryStream = new MemoryStream())
            {
                await serializer.Serialize(node1, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var deserializedNode = await serializer.Deserialize(memoryStream);

                AssertLinkedNodesEqual(node1, deserializedNode);
            }
        }

        [Fact]
        public async Task SerializeDeserialize_ThreeNodes()
        {
            var node1 = new ListNode { Data = "Node 1" };
            var node2 = new ListNode { Data = "Node 2" };
            var node3 = new ListNode { Data = "Node 3" };

            node1.Next = node2;
            node2.Next = node3;
            node2.Previous = node1;
            node3.Previous = node2;

            var serializer = new ListSerializer();

            using (var memoryStream = new MemoryStream())
            {
                await serializer.Serialize(node1, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var deserializedNode = await serializer.Deserialize(memoryStream);

                AssertLinkedNodesEqual(node1, deserializedNode);
            }
        }

        [Fact]
        public async Task SerializeDeserialize_CorrectRanadom()
        {
            var node1 = new ListNode { Data = "Node 1" };
            var node2 = new ListNode { Data = "Node 2" };
            var node3 = new ListNode { Data = "Node 3" };

            node1.Next = node2;
            node2.Next = node3;
            node2.Previous = node1;
            node3.Previous = node2;
            node1.Random = node3;
            node2.Random = node1;
            node3.Random = node2;

            var serializer = new ListSerializer();

            using (var memoryStream = new MemoryStream())
            {
                await serializer.Serialize(node1, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var deserializedNode = await serializer.Deserialize(memoryStream);

                AssertLinkedNodesEqual(node1, deserializedNode);
            }
        }

        private void AssertLinkedNodesEqual(ListNode expected, ListNode actual)
        {
            var nodeMap = new Dictionary<ListNode, ListNode>();

            var currentExpected = expected;
            var currentActual = actual;
            while (currentExpected != null && currentActual != null)
            {
                nodeMap[currentExpected] = currentActual;
                currentExpected = currentExpected.Next;
                currentActual = currentActual.Next;
            }

            while (expected != null && actual != null)
            {

                Assert.Equal(expected.Data, actual.Data);
                
                if (expected.Random != null)
                {
                    Assert.True(nodeMap.ContainsKey(expected.Random));
                    Assert.Same(nodeMap[expected.Random], actual.Random);
                }
                else
                {
                    Assert.Null(actual.Random);
                }


                if (expected.Previous != null)
                {
                    Assert.True(nodeMap.ContainsKey(expected.Previous));
                    Assert.Same(nodeMap[expected.Previous].Next, actual);
                }
                else
                {
                    Assert.Null(actual.Previous);
                }

                if (expected.Next != null)
                {
                    Assert.True(nodeMap.ContainsKey(expected.Next));
                    Assert.Same(nodeMap[expected.Next].Previous, actual);
                }
                else
                {
                    Assert.Null(actual.Next);
                }
                
                expected = expected.Next;
                actual = actual.Next;
            }
            Assert.Equal(expected, actual);
        }
    }
}