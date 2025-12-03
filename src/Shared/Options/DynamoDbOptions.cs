namespace Shared.Options
{
    public class DynamoDbOptions
    {
        public int TTLDays { get; set; } = 5;
        public string FullTableName { get; set; } = string.Empty;
    }
}
