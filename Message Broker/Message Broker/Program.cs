using Message_Broker.Data;
using Message_Broker.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source= MessageBroker.db"));

var app = builder.Build();

app.UseHttpsRedirection();

// Create Topic Endpoint
app.MapPost("api/topics", async(AppDbContext context, Topic topic) =>
{
    await context.Topics.AddAsync(topic);
    await context.SaveChangesAsync();

    return Results.Created($"api/topics/{topic.Id}",topic);
});

// Return all topics
app.MapGet("api/topics", async (AppDbContext context) =>
{
    var topics = await context.Topics.ToListAsync();

    return Results.Ok(topics);
});


//Publish Message

app.MapPost("api/topics/{id}/messages", async (AppDbContext context, int id, Message message) =>
{
    bool topics = await context.Topics.AnyAsync(t => t.Id == id);
    if (!topics)
    {
        return Results.NotFound("Topic Not Found");
    }

    var subs = context.Subscriptions.Where(s => s.TopicId == id);

    if(subs.Count() == 0)
    {
        return Results.NotFound("There are no subs for that topic");
    }

    foreach (var sub in subs)
    {
        Message msg = new Message
        {
            TopicMessage = message.TopicMessage,
            SubscriptionId = sub.Id,
            ExpiresAfter = message.ExpiresAfter,
            MessageStatus = message.MessageStatus
        };

        await context.Messages.AddAsync(msg);
    }
    await context.SaveChangesAsync();

    return Results.Ok("Message has been published");
});


app.Run();