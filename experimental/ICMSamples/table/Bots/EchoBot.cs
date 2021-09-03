using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.BotBuilderSamples;
using Microsoft.BotBuilderSamples.Model;
using Newtonsoft.Json;
using Microsoft.Rest.Serialization;


// Represents a bot saves and echoes back user input.
public class EchoBot : ActivityHandler
{
    private CloudTable _table;
    private string _connectionString;
    private string _nameOfTable;
    public static readonly JsonSerializerSettings SerializationSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    public EchoBot(IConfiguration config)
    {
        _connectionString = config.GetSection("StorageConnectionString")?.Value;
        _nameOfTable = "<Name-Of-Your-Table>";
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new ArgumentException("The storage connection string cannot be null");
        }
    }

    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        await LogActivity(turnContext.Activity.Conversation.Id, turnContext.Activity.Id, (Activity)turnContext.Activity);

        await base.OnTurnAsync(turnContext, cancellationToken);
    }

    // Echo back user input.
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var activity = MessageFactory.Text($"Echo: {turnContext.Activity.Text}");

        var response = await turnContext.SendActivityAsync(activity, cancellationToken);
        activity.Id = response.Id;

        await LogActivity(turnContext.Activity.Conversation.Id, turnContext.Activity.Id, activity);
    }

    private async Task LogActivity(string conversationId, string activityId, Activity activity)
    {
        if (_table == null)
        {
            _table = await TableHelper.CreateTableAsync(_nameOfTable, _connectionString);
        }

        var serializedActivity = SafeJsonConvert.SerializeObject(activity, SerializationSettings);

        ActivityEntity data = new ActivityEntity(conversationId, activityId)
        {
            Activity = serializedActivity,
            Notes = "Some Note here"
        };

        await TableHelper.InsertOrMergeEntityAsync(_table, data);

    }
}
