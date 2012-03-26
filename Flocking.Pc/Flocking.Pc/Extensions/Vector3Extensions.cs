namespace Flocking.Extensions
{
    using Microsoft.Xna.Framework;

    public static class Vector3Extensions
    {
        public static void SetTo(this Vector3 toSet, Vector3 setTo)
        {
            toSet.X = setTo.X;
            toSet.Y = setTo.Y;
            toSet.Z = setTo.Z;
        }
    }
}