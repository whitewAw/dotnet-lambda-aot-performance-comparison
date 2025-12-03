using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Models;
using Shared.Options;


namespace Shared.Services
{
    public class DynamoDbRepository : IDynamoDBRepository
    {
        private readonly IDynamoDBContext _dbContext;
        private readonly DynamoDbOptions _dynamoDbOptions;
        private readonly ILogger<DynamoDbRepository> _logger;

        public DynamoDbRepository(IDynamoDBContext dbContext,
                                       IOptions<DynamoDbOptions> options,
                                            ILogger<DynamoDbRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dynamoDbOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        private string TableName => _dynamoDbOptions.FullTableName;
        private SaveConfig GetSaveConfig() => string.IsNullOrWhiteSpace(TableName) ? new() : new() { OverrideTableName = TableName };
        public async Task<Guid> CreateAsync(CancellationToken cancellationToken)
        {
            var item = new DbItem
            {
                Id = Guid.NewGuid(),
                PostingDate = DateTime.Now,
                TTL = DateTime.UtcNow.AddDays(_dynamoDbOptions.TTLDays)
            };

            await _dbContext.SaveAsync(item, GetSaveConfig(), cancellationToken);

            _logger.LogInformation("Created DbItem: {Id}, PostingDate: {PostingDate}, TTL: {TTL}", item.Id, item.PostingDate, item.TTL);

            return item.Id;
        }
    }
}
