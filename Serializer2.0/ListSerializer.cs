using Newtonsoft.Json;
using SerializerTests.Interfaces;
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
        var index = 0;

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

            if (current.Previous != null) serializedNode.Previous = dict[current.Previous].Id;
            if (current.Next != null) serializedNode.Next = dict[current.Next].Id;
            if (current.Random != null) serializedNode.Random = dict[current.Random].Id;

            await sw.WriteLineAsync(JsonConvert.SerializeObject(serializedNode));
            current = current.Next;
        }
        await sw.FlushAsync();
    }

    private async Task<ListNode> DeserializeList(StreamReader streamReader)
    {
        string? line;
        var deserializeList = new List<SerializedNode>();
        var nodeMap = new Dictionary<int, ListNode>();
        try
        {
            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                var serializedNode = JsonConvert.DeserializeObject<SerializedNode>(line);
                var node = new ListNode { Data = serializedNode.Data };
                nodeMap.Add(serializedNode.Id, node);
                deserializeList.Add(serializedNode);
            }
        }
        catch (Exception)
        {
            throw new ArgumentException("Invalid serialized list data format.");
        }

        foreach (var node in deserializeList)
        {
            if (node.Previous >= 0 && node.Id != node.Previous)
            {
                nodeMap[node.Id].Previous = nodeMap[node.Previous];
            }
            if (node.Next > 0)
            {
                nodeMap[node.Id].Next = nodeMap[node.Next];
            }
            if (node.Random >= 0)
            {
                nodeMap[node.Id].Random = nodeMap[node.Random];
            }
        }

        return deserializeList.Count > 0 ? nodeMap[deserializeList[0].Id] : null;
    }
}