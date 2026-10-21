namespace Domain.Core;

public sealed class SquareFinder
{
    private readonly HashSet<Point> points;
    private readonly Dictionary<int, int[]> columns;
    private readonly Dictionary<int, int[]> rows;

    public SquareFinder(IEnumerable<Point> points)
    {
        var uniquePoints = points.ToHashSet();
        this.points = uniquePoints;

        columns = uniquePoints
            .GroupBy(point => point.X)
            .Where(column => column.Count() > 1)
            .ToDictionary(column => column.Key, column => column.Select(point => point.Y).Order().ToArray());
        rows = uniquePoints
            .GroupBy(point => point.Y)
            .Where(row => row.Count() > 1)
            .ToDictionary(row => row.Key, row => row.Select(point => point.X).Order().ToArray());
    }

    public IEnumerable<Square> FindAll(CancellationToken cancellationToken)
    {
        foreach (var x in columns.Keys.Order())
        {
            foreach (var y in columns[x])
            {
                cancellationToken.ThrowIfCancellationRequested();
                var bottomLeft = new Point(x, y);
                foreach (var opposite in OppositeCornersToTry(bottomLeft))
                    if (opposite.X > bottomLeft.X && opposite.Y > bottomLeft.Y && HasOtherCorners(bottomLeft, opposite))
                        yield return new Square(bottomLeft, opposite.X - bottomLeft.X);
            }
        }
    }

    public long CountSquaresWithAnyCornerIn(IReadOnlySet<Point> corners, CancellationToken cancellationToken)
    {
        if (!points.IsSupersetOf(corners))
            throw new ArgumentException("Every corner must be one of the points.", nameof(corners));

        var count = 0L;
        foreach (var corner in corners)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var square in FindSquaresWithCorner(corner))
                if (FirstCornerIn(square, corners) == corner)
                    count++;
        }
        return count;
    }

    public IReadOnlyList<Point> FindCornersThatCompleteSquares(Point corner)
    {
        if (!points.Contains(corner))
            throw new ArgumentException("The corner must be one of the points.", nameof(corner));

        return OppositeCornersToTry(corner)
            .Where(opposite => Contains(opposite.X, corner.Y) && Contains(corner.X, opposite.Y))
            .Select(opposite => new Point((int)opposite.X, (int)opposite.Y))
            .ToList();
    }

    private IEnumerable<(long X, long Y)> OppositeCornersToTry(Point corner)
    {
        var column = columns.GetValueOrDefault(corner.X, []);
        var row = rows.GetValueOrDefault(corner.Y, []);
        if (column.Length <= row.Length)
        {
            foreach (var y in column)
            {
                if (y == corner.Y)
                    continue;
                var side = Math.Abs((long)y - corner.Y);
                yield return (corner.X + side, y); // square to the right of the column
                yield return (corner.X - side, y); // square to the left of the column
            }
        }
        else
        {
            foreach (var x in row)
            {
                if (x == corner.X)
                    continue;
                var side = Math.Abs((long)x - corner.X);
                yield return (x, corner.Y + side); // square above the row
                yield return (x, corner.Y - side); // square below the row
            }
        }
    }

    private IEnumerable<Square> FindSquaresWithCorner(Point corner) =>
        OppositeCornersToTry(corner)
            .Where(opposite => HasOtherCorners(corner, opposite))
            .Select(opposite => new Square(
                new Point((int)Math.Min(corner.X, opposite.X), (int)Math.Min(corner.Y, opposite.Y)),
                Math.Abs(opposite.X - corner.X)
            ));

    private bool HasOtherCorners(Point corner, (long X, long Y) opposite) =>
        Contains(opposite.X, opposite.Y) && Contains(opposite.X, corner.Y) && Contains(corner.X, opposite.Y);

    private bool Contains(long x, long y) =>
        x is >= int.MinValue and <= int.MaxValue && y is >= int.MinValue and <= int.MaxValue && points.Contains(new Point((int)x, (int)y));

    private static Point FirstCornerIn(Square square, IReadOnlySet<Point> corners)
    {
        if (corners.Contains(square.BottomLeft))
            return square.BottomLeft;
        if (corners.Contains(square.BottomRight))
            return square.BottomRight;
        if (corners.Contains(square.TopRight))
            return square.TopRight;
        return square.TopLeft;
    }
}
