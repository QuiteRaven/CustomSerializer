using Newtonsoft.Json;
using SerializerTests.Interfaces;
using SerializerTests.Models;
using SerializerTests.Nodes;

public class ListSerializer : IListSerializer
{
    public async Task Serialize(ListNode head, Stream s)
    {
        var writer = new StreamWriter(s);
        await SerializeList(head, writer);
    }

    public async Task<ListNode> Deserialize(Stream s)
    {
        var reader = new StreamReader(s);
        return await DeserializeList(reader);
    }

    public async Task<ListNode> DeepCopy(ListNode head)
    {
        using (var stream = new MemoryStream())
        {
            await Serialize(head, stream);
            stream.Seek(0, SeekOrigin.Begin);
            return await Deserialize(stream);
        }
    }

    private async Task SerializeList(ListNode head, StreamWriter sw)
    {
        var dict = new Dictionary<ListNode, SerializedNode>();
        var current = head;
        var index = 1;

        while (current != null)
        {
            var serializedNode = new SerializedNode()
            {
                Id = index,
                Data = current.Data
            };

            dict.Add(current, serializedNode);
            current = current.Next;
            index++;
        }

        current = head;

        while (current != null)
        {
            var serializedNode = dict[current];

            if (current.Random != null) serializedNode.Random = dict[current.Random].Id;

            await sw.WriteLineAsync(JsonConvert.SerializeObject(serializedNode));
            current = current.Next;
        }
        await sw.FlushAsync();
    }

    private async Task<ListNode> DeserializeList(StreamReader streamReader)
    {
        string? line;
        var nodeMap = new List<DeserializedData>();
        nodeMap.Add(new DeserializedData
        {
            Target = null,
            Source = null,
        });

        try
        {
            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                var serializedNode = JsonConvert.DeserializeObject<SerializedNode>(line);

                var node = new ListNode { Data = serializedNode.Data };

                if (nodeMap.Count > 1)
                {
                    node.Previous = nodeMap.Last().Target;
                    node.Previous.Next = node;
                }

                nodeMap.Add(new DeserializedData
                {
                    Target = node,
                    Source = serializedNode,
                });
            }
        }
        catch (Exception)
        {
            throw new ArgumentException("Invalid serialized list data format.");
        }

        for (int i = 1; i < nodeMap.Count; i++)
        {
            var deNode = nodeMap[i];

            if (deNode.Source.Random != 0)
            {
                deNode.Target.Random = nodeMap[deNode.Source.Random].Target;
            }
        }

        return nodeMap.Count > 1 ? nodeMap[1].Target : null;
    }
}