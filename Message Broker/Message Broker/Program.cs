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


// Create Subscription

app.MapPost("api/topics/{id}/subscriptions", async (AppDbContext context, int id, Subscription sub) =>
{
    bool topic = await context.Topics.AnyAsync(t => t.Id == id);
    if (!topic)
    {
        return Results.NotFound("Topic Not Found");
    }

    sub.TopicId = id;

    await context.Subscriptions.AddAsync(sub);
    await context.SaveChangesAsync();

    return Results.Created($"api/topics/{id}/subscriptions/{sub.Id}", sub);
});

//Get Subscriber Messages

app.MapGet("api/subscriptions/{id}/messages", async (AppDbContext context, int id) =>
{
    bool subs = await context.Subscriptions.AnyAsync(s => s.Id == id);
    if (!subs)
    {
        return Results.NotFound("Subscription not found");
    }

    var messages = context.Messages.Where(m => m.SubscriptionId == id && m.MessageStatus != "SENT");

    if(messages.Count() == 0)
    {
        return Results.NotFound("No New Messages");
    }

    foreach (var msg in messages)
    {
        msg.MessageStatus = "REQUESTED";

    }

    await context.SaveChangesAsync();

    return Results.Ok(messages);
});


// Acknowledge messages for subscriber

app.MapPost("api/subscriptions/{id}/messages", async (AppDbContext context, int id, int[] confs) =>
{
    bool subs = await context.Subscriptions.AnyAsync(s => s.Id == id);
    if (!subs)
    {
        return Results.NotFound("Subscription not found");
    }

    if(confs.Length <= 0)
    {
        return Results.BadRequest();
    }

    int count = 0;

    foreach (var msgid in confs) 
    {
        var msg = context.Messages.FirstOrDefault( m => m.Id == msgid);
        if(msg != null)
        {
            msg.MessageStatus = "SENT";
            await context.SaveChangesAsync();
            count++;
        }
    }

    return Results.Ok($"Acknowledged {count}/{confs.Length} messages");

});

app.Run();