using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MyGameNamespace
{
    public enum E_Axis : byte { X, Y, Z }
    /// 屏幕坐标转换为 UGUI 的 anchoredPosition 坐标 对应的API如下
    /// RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenPoint, uiCamera, out localPos);
    public static class VecHelper
    {
        // [DllImport("User32")]
        // public static extern bool SetCursorPos(int x, int y);
        // [DllImport("User32")]
        // public static extern bool GetCursorPos(out System.Drawing.Point pt);
        /// <summary>
        /// 获取某个轴向的四元数
        /// </summary>
        public static Quaternion ToQUAT(E_Axis axis, float angle)
        {
            double rad = angle * MathHelper.Deg2Rad * 0.5;
            var q = new Quaternion(0, 0, 0, (float)Math.Cos(rad));
            switch (axis)
            {
                case E_Axis.X: q.x = (float)Math.Sin(rad); break;
                case E_Axis.Y: q.y = (float)Math.Sin(rad); break;
                case E_Axis.Z: q.z = (float)Math.Sin(rad); break;
            }
            return q;
        }
        public static int DirToIndex(this Vector2 dir, int count)
        {
            // 计算每个步幅的角度
            float step = 360f / count;
            // 计算向量角并对角度进行步幅偏移
            float angle = Vector2.SignedAngle(Vector2.up, dir.normalized) + step * 0.5f;
            // 避免负数 让他保持在360度内
            if (angle < 0) angle += 360f;
            // 向下取索引
            return (int)(angle / step);
        }
        public static Vector2 Normalize(this Vector2 vec, float magnitude)
        {
            return magnitude > 1E-05f ? vec / magnitude : Vector2.zero;
        }
        /// <summary>
        /// 判断坐标是否在视口内
        /// </summary>
        public static bool InView3D(this Vector3 worldPos)
        {
            // 利用点乘计算投影角度 判断物体是否在相机前面
            Transform cam = Camera.main.transform;
            return Vector3.Dot(cam.forward, (worldPos - cam.position).normalized) > 0 && InView2DByWorld(worldPos);
        }
        public static bool InView2DByWorld(this Vector2 worldPos)
        {
            return InView2D(Camera.main.WorldToViewportPoint(worldPos));
        }
        /// <summary>
        /// 当前2D坐标是否在视口中
        /// </summary>
        public static bool InView2DByScreen(this Vector2 screenPos)
        {
            return screenPos.x >= 0 && screenPos.x <= Screen.width && screenPos.y >= 0 && screenPos.y <= Screen.height;
        }
        public static bool InView2D(this Vector2 viewPos)
        {
            return viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1;
        }
        public static Vector2Int Sign(this Vector2Int point)
        {
            point.x = point.x.Sign();
            point.y = point.y.Sign();
            return point;
        }
        public static Vector2 RotateTo(this Vector2 cur, Vector2 axis, float angle)
        {
            double r = angle * MathHelper.Deg2Rad;
            double c = Math.Cos(r);
            double s = Math.Sin(r);
            axis.x += (float)(cur.x * c - cur.y * s);
            axis.y += (float)(cur.x * s + cur.y * c);
            return axis;
        }
        public static void Rotate(this Quaternion q, Vector2 center, Vector2 size, out Vector2 a, out Vector2 b, out Vector2 c, out Vector2 d)
        {
            float sw = size.x * 0.5f;
            float sh = size.y * 0.5f;
            // 得到一个锚点为中心的矩形四个顶点 从左下角开始
            a = new Vector2(center.x - sw, center.y - sh);
            b = new Vector2(center.x - sw, center.y + sh);
            c = new Vector2(center.x + sw, center.y + sh);
            d = new Vector2(center.x + sw, center.y - sh);
            q.RotatePoint(ref a, center);
            q.RotatePoint(ref b, center);
            q.RotatePoint(ref c, center);
            q.RotatePoint(ref d, center);

            MonoHelper.DrawBox(a, b, c, d, Color.black);
        }
        // 旋转点的函数
        public static void RotatePoint(this Quaternion q, ref Vector2 p, Vector2 center)
        {
            // 将点坐标转换为相对于旋转中心的坐标
            p -= center;
            p = q * p;
            // 更新传入的引用参数
            p += center;
        }
        /// <summary>
        /// 旋转顶点
        /// </summary>
        public static void RotateTo(this Vector2[] vertexs, Vector2 axis, double angle, Func<int, Vector2> call = null)
        {
            double r = angle * MathHelper.Deg2Rad;
            double c = Math.Cos(r);
            double s = Math.Sin(r);
            Vector2 v, pos = Vector2.zero;
            for (int i = vertexs.Length - 1; i >= 0; i--)
            {
                v = call == null ? vertexs[i] - axis : call(i);
                pos.x = axis.x + (float)(v.x * c - v.y * s);
                pos.y = axis.y + (float)(v.x * s + v.y * c);
                vertexs[i] = pos;
            }
        }
        /// <summary>
        /// 获取两点的朝向角度 正负180度
        /// </summary>
        public static float GetAngle180ByPoints(Vector2 from, Vector2 to) =>
            (float)(Math.Atan2(from.y - to.y, from.x - to.x) * MathHelper.Rad2Deg);
        /// <summary>
        /// 获取Y轴为0角度的欧拉角
        /// </summary>
        public static float GetEulerAngleByPoints(this Vector2 from, Vector2 to)
        {
            float angle = GetAngle180ByPoints(from, to) + 90;
            if (angle < 0) angle += 360;
            return angle % 360;
        }
        /// <summary>
        /// 随机获取圆边的任意点
        /// </summary>
        public static Vector2 GetCircularRandomEdgePoint(float radius)
        {
            int angle = MathHelper.random.Next(0, 360);
            return GetVec(angle) * radius;
        }
        /// <summary>  
        /// 根据角度获取向量角 [角度为0时 方向为正右]
        /// </summary>
        public static Vector2 GetVec(float angle)
        {
            double r = MathHelper.Deg2Rad * angle;
            return new Vector2((float)Math.Cos(r), (float)Math.Sin(r));
        }
        public static void GetVec(float angle, out double x, out double y)
        {
            double r = MathHelper.Deg2Rad * angle;
            x = Math.Cos(r);
            y = Math.Sin(r);
        }
        /// <summary>
        /// 判断当前点是否在线段附近
        /// </summary>
        public static bool OnLine(Vector2 p, Vector2 s, Vector2 e, float min, out Vector2 projectionPos)
        {
            float esx = e.x - s.x;
            float esy = e.y - s.y;

            float abac = esx * (p.x - s.x) + esy * (p.y - s.y);
            if (abac < 0) // 内积小于0，夹角大于90度，c在ab线段外面靠近a的一侧
            {
                projectionPos = s;
                return (s - p).sqrMagnitude + min < min;
            }
            float denominator = esx * esx + esy * esy;
            if (abac > denominator) // 内积大于ab模的平方，ac在ab方向的投影大于ab，c在ab线段外面靠近b的一侧
            {
                projectionPos = e;
                return (e - p).sqrMagnitude + min < min;
            }
            projectionPos = s + (e - s) * (abac / denominator);
            return (p - projectionPos).sqrMagnitude < min;
        }
        /// <summary>
        /// 获取一个点在(无限延申)直线上的投影
        /// </summary>
        /// <param name="p">直线外一点</param>
        /// <param name="a">直线端点1</param>
        /// <param name="b">直线端点2</param>
        /// <returns>投影点坐标</returns>
        public static Vector2 Projection(this Vector2 p, Vector2 a, Vector2 b)
        {
            float x = b.x - a.x;
            float y = b.y - a.y;

            float xp = x * x;
            float yp = y * y;

            float denominator = xp + yp;
            // 说明两条线段平行
            if (denominator == 0) return p;

            float axby = a.x * b.y;
            float bxay = b.x * a.y;

            float xy = x * y;

            float moleculey = yp * p.y + xy * p.x - x * axby + x * bxay;
            float moleculex = xp * p.x + xy * p.y - y * bxay + y * axby;

            return new Vector2(moleculex / denominator, moleculey / denominator);
        }
        /// <summary>
        /// 判断两个向量是否碰到(近似)
        /// </summary>
        public static bool Approximate(this Vector2 a, Vector2 b)
        {
            return (a - b).sqrMagnitude <= MathHelper.MIN;
        }
        /// <summary>
        /// 一条斜线定义矩形的面积尺寸
        /// </summary>
        public static void GetArea(Vector2 a, Vector2 b, out float w, out float h)
        {
            w = (a.x - b.x).Abs();
            h = (a.y - b.y).Abs();
        }
        /// <summary>
        /// 得到两点的插值点 简化
        /// </summary>
        public static Vector2 Linear(Vector2 a, Vector2 b, float t = 0.5f) => (a + b) * t;
        /// <summary>
        /// 得到一根向量的垂线 XNA [-x => -pi/2] [-y => +pi/2]
        /// </summary>
        public static Vector2 Perpendicular(this Vector2 vec) => new Vector2(vec.y, -vec.x);
        /// <summary>
        /// 获取2维二次贝塞尔曲线
        /// </summary>
        /// <param name="s">起始点</param>
        /// <param name="m">中间点</param>
        /// <param name="e">结束点</param>
        /// <param name="t">时间系数 通常在[0-1]</param>
        /// <returns>插值点</returns>
        public static Vector2 GetBezier2(Vector2 s, Vector2 m, Vector2 e, float t)
        {
            t.Clamp01();
            float omt = 1f - t;
            return s * (omt * omt) + m * (2f * t * omt) + e * (t * t);
        }
        /// <summary>
        /// 屏幕坐标转GUI坐标 
        /// GUI原点在左上角 X轴向右为正方向，Y轴向下为正方向
        /// 屏幕坐标[鼠标]的原点在左下角 X轴向右是正方向，Y轴向上是正方向
        /// </summary>
        public static void ScreenToGUIPoint(this ref Vector2 screenPos)
        {
            screenPos.y = Screen.height - screenPos.y;
        }
        public static void ToGUIPoint(this ref Vector2 pos, COOR_Mode mode)
        {
            switch (mode)
            {
                case COOR_Mode.World: pos = Camera.main.WorldToScreenPoint(pos); break;
                case COOR_Mode.Viewport: pos = Camera.main.ViewportToScreenPoint(pos); break;
            }
            if (mode == COOR_Mode.UILocal) return;
            pos.ScreenToGUIPoint();
        }
        /// <summary>
        /// 屏幕坐标的 XY 转GUI坐标 XY
        /// </summary>
        public static Vector2 ScreenToGUIPoint(float x, float y) => new Vector2(x, Screen.height - y);
        /// <summary>
        /// 世界坐标转到单位坐标
        /// </summary>
        public static Vector2Int WorldPosToCellPos(this Vector2 worldPos) => new Vector2Int(worldPos.x.RoundToInt(), worldPos.y.RoundToInt());
        /// <summary>
        /// 屏幕坐标转换到世界单位坐标
        /// </summary>
        public static Vector2Int ScreenToWorldCellPos(this Vector2 screenPos) => WorldPosToCellPos(Camera.main.ScreenToWorldPoint(screenPos));
    }
}