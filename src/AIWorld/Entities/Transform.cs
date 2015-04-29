using Microsoft.Xna.Framework;

namespace AIWorld.Entities
{
    public static class Transform
    {
        public static Vector3 ToLocalSpace(Vector3 position, Vector3 heading, Vector3 up, Vector3 side, Vector3 point)
        {
            float tx = -Vector3.Dot(position, heading);
            float ty = -Vector3.Dot(position, up);
            float tz = -Vector3.Dot(position, side);

            return Vector3.Transform(point,
                new Matrix(heading.X, 0, side.X, 0, heading.Y, 1, side.Y, 0, heading.Z, 0, side.Z, 0, tx, ty, tz, 0));
        }

        public static Vector3 VectorToWorldSpace(Vector3 heading, Vector3 up, Vector3 side, Vector3 vec)
        {
            return Vector3.Transform(vec,
                Matrix.Identity *
                new Matrix(heading.X, up.X, side.X, 0, heading.Y, up.Y, side.Y, 0, heading.Z, up.Z, side.Z, 0, 0, 0, 0, 0));
        }
    }
}