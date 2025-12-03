namespace Shared.Services
{
    public interface IDynamoDBRepository
    {
        Task<Guid> CreateAsync(CancellationToken cancellationToken);
    }
}
