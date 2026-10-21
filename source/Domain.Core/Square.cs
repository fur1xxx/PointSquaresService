namespace Domain.Core;

public readonly record struct Square(Point BottomLeft, long Side)
{
    public Point BottomRight => new(checked((int)(BottomLeft.X + Side)), BottomLeft.Y);
    public Point TopRight => new(BottomRight.X, TopLeft.Y);
    public Point TopLeft => new(BottomLeft.X, checked((int)(BottomLeft.Y + Side)));
}
