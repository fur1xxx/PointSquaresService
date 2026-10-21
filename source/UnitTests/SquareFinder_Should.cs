using Domain.Core;
using Xunit;

namespace UnitTests;

public class SquareFinder_Should
{
    [Fact]
    public void FindAll_NoPoints_ReturnsNoSquares()
    {
        Assert.Empty(new SquareFinder([]).FindAll(CancellationToken.None));
    }

    [Fact]
    public void FindAll_SquareGrid_ListsSquaresByBottomLeftThenSide()
    {
        var squares = new SquareFinder(Grid(3)).FindAll(CancellationToken.None);

        Square[] expected =
        [
            new(new Point(0, 0), 1),
            new(new Point(0, 0), 2),
            new(new Point(0, 1), 1),
            new(new Point(1, 0), 1),
            new(new Point(1, 1), 1),
        ];
        Assert.Equal(expected, squares);
    }

    [Fact]
    public void FindAll_GridTallerThanWide_ListsSquaresByBottomLeftThenSide()
    {
        var squares = new SquareFinder(Grid(3, 4)).FindAll(CancellationToken.None);

        Square[] expected =
        [
            new(new Point(0, 0), 1),
            new(new Point(0, 0), 2),
            new(new Point(0, 1), 1),
            new(new Point(0, 1), 2),
            new(new Point(0, 2), 1),
            new(new Point(1, 0), 1),
            new(new Point(1, 1), 1),
            new(new Point(1, 2), 1),
        ];
        Assert.Equal(expected, squares);
    }

    [Fact]
    public void FindAll_10By10Grid_FindsEachSquareOnce()
    {
        var squares = new SquareFinder(Grid(10)).FindAll(CancellationToken.None).ToList();

        Assert.Equal(SquaresInGrid(10), squares.Count);
        Assert.Distinct(squares);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(10, 20)]
    public void FindAll_ReversedInput_ListsSquaresInSameOrder(int width, int height)
    {
        var grid = Grid(width, height);

        var fromForwards = new SquareFinder(grid).FindAll(CancellationToken.None);
        var fromReversed = new SquareFinder(Enumerable.Reverse(grid)).FindAll(CancellationToken.None);

        Assert.Equal(fromForwards, fromReversed);
    }

    [Theory]
    [InlineData(20, 10)]
    [InlineData(10, 20)]
    public void FindAll_GridWithOnePointRemoved_SkipsSquaresUsingIt(int width, int height)
    {
        var removed = new Point(4, 4);
        var grid = Grid(width, height).Where(point => point != removed);

        var squares = new SquareFinder(grid).FindAll(CancellationToken.None);

        Assert.Equal(SquaresInGrid(width, height) - SquaresThroughPoint(width, height, removed), squares.LongCount());
    }

    [Fact]
    public void FindAll_NegativeCoordinates_FindsSquare()
    {
        Point[] corners = [new(-5, -5), new(-2, -5), new(-2, -2), new(-5, -2)];

        var square = Assert.Single(new SquareFinder(corners).FindAll(CancellationToken.None));

        Assert.Equal(new Square(new Point(-5, -5), 3), square);
    }

    [Fact]
    public void FindAll_CornersAtIntMinAndIntMax_FindsTheSquare()
    {
        Point[] corners =
        [
            new(int.MinValue, int.MinValue),
            new(int.MaxValue, int.MinValue),
            new(int.MaxValue, int.MaxValue),
            new(int.MinValue, int.MaxValue),
        ];

        var square = Assert.Single(new SquareFinder(corners).FindAll(CancellationToken.None));

        Assert.Equal(new Square(new Point(int.MinValue, int.MinValue), 4_294_967_295), square);
    }

    [Fact]
    public void FindAll_TallRectangleWhoseHeightOverflowsInt_ReturnsNoSquares()
    {
        Point[] rectangle = [new(-1, int.MinValue), new(0, int.MinValue), new(0, int.MaxValue), new(-1, int.MaxValue)];

        Assert.Empty(new SquareFinder(rectangle).FindAll(CancellationToken.None));
    }

    [Fact]
    public void FindAll_AllPointsInOneColumn_ReturnsNoSquares()
    {
        var column = Enumerable.Range(0, 20_000).Select(y => new Point(0, y));

        Assert.Empty(new SquareFinder(column).FindAll(CancellationToken.None));
    }

    [Fact]
    public void FindAll_CancelledWhileListing_Throws()
    {
        using var cancellation = new CancellationTokenSource();

        Assert.Throws<OperationCanceledException>(() =>
        {
            foreach (var _ in new SquareFinder(Grid(3)).FindAll(cancellation.Token))
                cancellation.Cancel();
        });
    }

    [Theory]
    [InlineData(0, 0, 9)]
    [InlineData(4, 4, 17)]
    [InlineData(0, 5, 9)]
    public void CountSquaresWithAnyCornerIn_OneNewPoint_CountsSquaresThroughIt(int x, int y, long expected)
    {
        var finder = new SquareFinder(Grid(10));

        var count = finder.CountSquaresWithAnyCornerIn(new HashSet<Point> { new(x, y) }, CancellationToken.None);

        Assert.Equal(expected, count);
    }

    [Fact]
    public void CountSquaresWithAnyCornerIn_AllPointsNew_CountsEachSquareOnce()
    {
        var grid = Grid(10);

        var count = new SquareFinder(grid).CountSquaresWithAnyCornerIn(grid.ToHashSet(), CancellationToken.None);

        Assert.Equal(SquaresInGrid(10), count);
    }

    [Fact]
    public void CountSquaresWithAnyCornerIn_LastColumnNew_CountsEachSquareTouchingItOnce()
    {
        var grid = Grid(10);
        var lastColumn = grid.Where(point => point.X == 9).ToHashSet();

        var count = new SquareFinder(grid).CountSquaresWithAnyCornerIn(lastColumn, CancellationToken.None);

        Assert.Equal(9 + 8 + 7 + 6 + 5 + 4 + 3 + 2 + 1, count);
    }

    [Fact]
    public void CountSquaresWithAnyCornerIn_CancelledToken_Throws()
    {
        var grid = Grid(3);

        Assert.Throws<OperationCanceledException>(() =>
            new SquareFinder(grid).CountSquaresWithAnyCornerIn(grid.ToHashSet(), new CancellationToken(canceled: true))
        );
    }

    [Fact]
    public void CountSquaresWithAnyCornerIn_PointNotInFinder_Throws()
    {
        var finder = new SquareFinder(Grid(3));

        Assert.Throws<ArgumentException>(() => finder.CountSquaresWithAnyCornerIn(new HashSet<Point> { new(5, 5) }, CancellationToken.None));
    }

    [Fact]
    public void FindCornersThatCompleteSquares_AddedPointCompletesOneSquare_ReturnsCornerToCheck()
    {
        Point[] collection = [new(0, 0), new(0, 2), new(2, 0), new(5, 7)];
        var point = new Point(2, 2);
        var columnAndRow = collection.Where(stored => stored.X == point.X || stored.Y == point.Y);

        var oppositeCorners = new SquareFinder(columnAndRow.Append(point)).FindCornersThatCompleteSquares(point);

        Assert.Equal(new Point(0, 0), Assert.Single(oppositeCorners));
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(10, 20)]
    public void FindCornersThatCompleteSquares_EveryGridPoint_OneExistingCornerPerSquareThroughIt(int width, int height)
    {
        var grid = Grid(width, height).ToHashSet();

        foreach (var point in grid)
        {
            var columnAndRow = grid.Where(stored => stored.X == point.X || stored.Y == point.Y);
            var oppositeCorners = new SquareFinder(columnAndRow).FindCornersThatCompleteSquares(point);

            Assert.Equal(SquaresThroughPoint(width, height, point), oppositeCorners.Count(grid.Contains));
        }
    }

    [Fact]
    public void FindCornersThatCompleteSquares_PointNotInFinder_Throws()
    {
        var finder = new SquareFinder(Grid(3));

        Assert.Throws<ArgumentException>(() => finder.FindCornersThatCompleteSquares(new Point(5, 5)));
    }

    private static List<Point> Grid(int size) => Grid(size, size);

    private static List<Point> Grid(int width, int height) =>
        [.. Enumerable.Range(0, width).SelectMany(x => Enumerable.Range(0, height).Select(y => new Point(x, y)))];

    private static long SquaresInGrid(int size) => (long)(size - 1) * size * (2 * size - 1) / 6;

    private static long SquaresInGrid(int width, int height) =>
        Enumerable.Range(1, Math.Min(width, height) - 1).Sum(side => (long)(width - side) * (height - side));

    private static int SquaresThroughPoint(int width, int height, Point point)
    {
        var spaceLeft = point.X;
        var spaceRight = width - 1 - point.X;
        var spaceBelow = point.Y;
        var spaceAbove = height - 1 - point.Y;
        return Math.Min(spaceLeft, spaceBelow)
            + Math.Min(spaceLeft, spaceAbove)
            + Math.Min(spaceRight, spaceBelow)
            + Math.Min(spaceRight, spaceAbove);
    }
}
