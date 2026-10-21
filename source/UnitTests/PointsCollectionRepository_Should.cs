using Microsoft.EntityFrameworkCore;
using Persistence.Core;
using Persistence.Core.Repositories;
using Xunit;

namespace UnitTests;

public class PointsCollectionRepository_Should
{
    [Fact]
    public async Task LockAsync_OutsideTransaction_Throws()
    {
        var options = new DbContextOptionsBuilder<PointSquaresDbContext>().UseMySQL("Server=localhost;Port=1").Options;
        await using var context = new PointSquaresDbContext(options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new PointsCollectionRepository(context).LockAsync(1, TestContext.Current.CancellationToken)
        );

        Assert.Equal("LockAsync must run inside a transaction, or MySQL releases the lock at once.", exception.Message);
    }
}
