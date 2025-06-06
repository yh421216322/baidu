using System.Drawing;
using UnityEngine;

namespace MyGameNamespace
{
    public static class RectHelper
    {
        public static bool Contains(this Rect rect, float x, float y)
        {
            return x >= rect.xMin && x < rect.xMax && y >= rect.yMin && y < rect.yMax;
        }
        public static float CenterX(this Rect rect) => rect.x + rect.width * 0.5f;
        public static float CenterY(this Rect rect) => rect.y + rect.height * 0.5f;
        /// <summary>
        /// 根据Rect限制Vector2边界
        /// </summary>
        public static void Clamp(this Rect self, ref Vector2 pos)
        {
            pos.x = Mathf.Clamp(pos.x, self.xMin, self.xMax);
            pos.y = Mathf.Clamp(pos.y, self.yMin, self.yMax);
        }
    }
}