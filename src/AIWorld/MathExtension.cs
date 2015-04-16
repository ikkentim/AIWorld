using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AIWorld
{
    public static class Vector2Extension
    {

        public static Vector3 RotateAboutOriginY(this Vector3 point, Vector3 origin, float rotation)
        {
            return Vector3.Transform(point - origin, Matrix.CreateRotationY(rotation)) + origin;
        }

        public static float GetYAngle(this Vector3 point)
        {
            return (float) Math.Atan2(point.X, point.Z);
        }
    }
}
