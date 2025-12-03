using Amazon.DynamoDBv2.DataModel;

namespace Shared.Models
{
    [DynamoDBTable("lambda-demo-db")]
    public class DbItem
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }

        public DateTime PostingDate { get; set; }

        [DynamoDBProperty(StoreAsEpochLong = true)]
        public DateTime TTL { get; set; }
    }
}
