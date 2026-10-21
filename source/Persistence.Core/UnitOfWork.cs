using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Core;

public class UnitOfWork(PointSquaresDbContext context)
{
    public async Task<DatabaseTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        new(await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken));
}
