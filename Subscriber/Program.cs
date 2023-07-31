




using Subscriber.Dtos;
using System.Net.Http.Json;


Console.WriteLine("Press ESC to stop!!!");

do
{
    HttpClient client = new HttpClient();
    Console.WriteLine("Listning!!");

    while (!Console.KeyAvailable)
    {
        List<int> ackIds = await GetMessagesAsync(client);

        Thread.Sleep(2000);

        if(ackIds.Count > 0)
        {
            await AckMessagesAsync(client, ackIds);
        }

    }
} while (Console.ReadKey(true).Key != ConsoleKey.Escape);


static async Task<List<int>> GetMessagesAsync(HttpClient httpClient)
{
    List<int> ackIds = new List<int>();

    List<MessageReadDto>? newMessages = new List<MessageReadDto>();

    try
    {
        newMessages = await httpClient.GetFromJsonAsync<List<MessageReadDto>>("http://localhost:20034/api/subscriptions/2/messages");
    }
    catch
    {
        return new List<int>();
    }

    foreach(MessageReadDto msg in newMessages!)
    {
        Console.WriteLine($"{msg.Id} - {msg.TopicMessage} - {msg.MessageStatus}");
        ackIds.Add(msg.Id);

    }
    return ackIds;
}


static async Task AckMessagesAsync(HttpClient httpclient, List<int> ackId)
{
    var response = await httpclient.PostAsJsonAsync("http://localhost:20034/api/subscriptions/2/messages",ackId);

    var returnMessage = await response.Content.ReadAsStringAsync();

    Console.WriteLine(returnMessage);
}
