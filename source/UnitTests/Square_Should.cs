using Domain.Core;
using Xunit;

namespace UnitTests;

public class Square_Should
{
    [Fact]
    public void Corners_SquareFromIntMinToIntMax_AreAtIntMinAndIntMax()
    {
        var square = new Square(new Point(int.MinValue, int.MinValue), 4_294_967_295);

        Assert.Equal(new Point(int.MaxValue, int.MinValue), square.BottomRight);
        Assert.Equal(new Point(int.MaxValue, int.MaxValue), square.TopRight);
        Assert.Equal(new Point(int.MinValue, int.MaxValue), square.TopLeft);
    }
}
