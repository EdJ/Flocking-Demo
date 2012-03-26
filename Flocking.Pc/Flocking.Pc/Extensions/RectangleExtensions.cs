namespace Flocking.Pc.Extensions
{
    using Microsoft.Xna.Framework;

    internal static class RectangleExtensions
    {
        public static Point BottomLeft(this Rectangle r)
        {
            return new Point(r.Left, r.Bottom);
        }

        public static Point BottomRight(this Rectangle r)
        {
            return new Point(r.Right, r.Bottom);
        }

        public static Point TopLeft(this Rectangle r)
        {
            return new Point(r.Left, r.Top);
        }

        public static Point TopRight(this Rectangle r)
        {
            return new Point(r.Right, r.Top);
        }
    }
}
