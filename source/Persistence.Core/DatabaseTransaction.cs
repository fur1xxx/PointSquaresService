using Microsoft.EntityFrameworkCore.Storage;

namespace Persistence.Core;

public sealed class DatabaseTransaction(IDbContextTransaction transaction) : IAsyncDisposable
{
    public Task CommitAsync(CancellationToken cancellationToken) => transaction.CommitAsync(cancellationToken);

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
