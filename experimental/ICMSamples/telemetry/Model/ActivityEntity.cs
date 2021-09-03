namespace CosmosTableSamples.Model
{
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Bot.Schema;

    public class ActivityEntity : TableEntity
    {
        public ActivityEntity()
        {
        }

        public ActivityEntity(string conversationId, string activityId)
        {
            PartitionKey = conversationId;
            RowKey = activityId;
        }

        public Activity Activity { get; set; }

        public string Notes { get; set; }
    }
}
