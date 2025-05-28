using UnityEngine;

namespace MyGameNamespace
{
    public static class MatrixHelper
    {
        public static void Translate(this ref Matrix4x4 matrix, Vector2 p) => matrix.Translate(p.x, p.y, 0f);
        public static void Translate(this ref Matrix4x4 matrix, float x, float y, float z = 0f)
        {
            matrix.m03 = x;
            matrix.m13 = y;
            matrix.m23 = z;
        }
        public static Vector2 Pos2D(this Matrix4x4 m) => new Vector2(m.m03, m.m13);
        public static void Scale(this ref Matrix4x4 matrix, float newSize)
        {
            matrix.m00 = newSize;
            matrix.m11 = newSize;
            matrix.m22 = newSize;
        }
        /// <summary>
        /// 只比较位置相等
        /// </summary>
        public static bool EqualsPos(this Matrix4x4 matrix, Vector2 p)
        {
            return matrix.m03 == p.x && matrix.m13 == p.y;
        }
    }
}