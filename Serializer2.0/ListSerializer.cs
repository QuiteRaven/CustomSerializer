using Newtonsoft.Json;
using SerializerTests.Interfaces;
using SerializerTests.Nodes;

public class ListSerializer : IListSerializer
{
    public async Task Serialize(ListNode head, Stream s)
    {
        var writer = new StreamWriter(s);
        await writer.WriteAsync(SerializeList(head));
        writer.Flush();
    }

    public async Task<ListNode> Deserialize(Stream s)
    {
        var reader = new StreamReader(s);
        var json = await reader.ReadToEndAsync();
        return DeserializeList(json);
    }

    public async Task<ListNode> DeepCopy(ListNode head)
    {
        var serializedList = SerializeList(head);
        return DeserializeList(serializedList);
    }

    private string SerializeList(ListNode head)
    {
        var serializedNodes = new List<string>();
        var dict = new Dictionary<ListNode, SerializedNode>();
        var cycleNode = head;

        while (cycleNode != null)
        {
            var serializedNode = new SerializedNode()
            {
                Id = Guid.NewGuid(),
                Data = cycleNode.Data
            };

            dict.Add(cycleNode, serializedNode);

            cycleNode = cycleNode.Next;
        }

        cycleNode = head;

        while (cycleNode != null)
        {
            var serializedNode = dict[cycleNode];

            if (cycleNode.Previous != null) serializedNode.Previous = dict[cycleNode.Previous].Id;
            if (cycleNode.Next != null) serializedNode.Next = dict[cycleNode.Next].Id;
            if (cycleNode.Random != null) serializedNode.Random = dict[cycleNode.Random].Id;

            serializedNodes.Add(JsonConvert.SerializeObject(serializedNode));
            cycleNode = cycleNode.Next;
        }
        return string.Join(";", serializedNodes);
    }

    private ListNode DeserializeList(string serializedList)
    {
        if (string.IsNullOrEmpty(serializedList))
            throw new ArgumentException("Serialized list cannot be null or empty.");

        var serializedNodes = serializedList.Split(';');

        
        var resultList = new List<SerializedNode>();
        var nodeMap = new Dictionary<Guid, ListNode>();

        try
        {
            foreach (var serializedNode in serializedNodes)
            {
                resultList.Add(JsonConvert.DeserializeObject<SerializedNode>(serializedNode));
            }
        }
        catch (Exception)
        {

            throw new ArgumentException("Invalid serialized list data format.");
        }

        foreach (var serializedNode in resultList)
        {
            var node = new ListNode { Data = serializedNode.Data };
            nodeMap.Add(serializedNode.Id, node);
        }

        foreach (var serializedNode in resultList)
        {
            if (serializedNode.Previous != Guid.Empty)
            {
                nodeMap[serializedNode.Id].Previous = nodeMap[serializedNode.Previous];
            }
            if (serializedNode.Next != Guid.Empty)
            {
                nodeMap[serializedNode.Id].Next = nodeMap[serializedNode.Next];
            }
            if (serializedNode.Random != Guid.Empty)
            {
                nodeMap[serializedNode.Id].Random = nodeMap[serializedNode.Random];
            }
        }

        return resultList.Count > 0 ? nodeMap[resultList[0].Id] : null;
    }
}