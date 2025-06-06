using System;
using UnityEngine;

namespace MyGameNamespace
{
    /// <summary>
    /// 坐标模式
    /// </summary>
    public enum COOR_Mode : byte { Screen, World, Viewport, UILocal }
    public static class GLHelper
    {
        /// <summary>
        /// 渲染 GL 图形
        /// </summary>
        /// <param name="callback">渲染回调</param>
        /// <param name="mode">渲染方式</param>
        public static void Render(COOR_Mode mode, Action callback)
        {
            GL.PushMatrix();
            switch (mode)
            {
                case COOR_Mode.Screen: GL.LoadPixelMatrix(); break;
                case COOR_Mode.Viewport: GL.LoadOrtho(); break;
            }
            callback?.Invoke();
            GL.PopMatrix();
        }
        /// <summary>
        /// 根据两个对角点绘制矩形
        /// </summary>
        /// <param name="start">开始点</param>
        /// <param name="end">结束点</param>
        /// <param name="isWire">是否为线框渲染</param>
        public static void DrawRect(Vector2 start, Vector2 end, bool isWire = true)
        {
            GL.Begin(isWire ? GL.LINE_STRIP : GL.QUADS);
            // 如果不是线框 并且没有反向绘制顶点
            if (!isWire && ((start.x > end.x && start.y < end.y) ||
                (start.x < end.x && start.y > end.y))) start.Swap(ref end);

            GL.Vertex3(start.x, start.y, 0f);
            GL.Vertex3(start.x, end.y, 0f);
            GL.Vertex3(end.x, end.y, 0f);
            GL.Vertex3(end.x, start.y, 0f);
            GL.Vertex3(start.x, start.y, 0f);

            GL.End();
        }
        /// <summary>
        /// 根据中心点绘制矩形
        /// </summary>
        /// <param name="centre">中心点</param>
        /// <param name="w">矩形宽度</param>
        /// <param name="h">矩形高度</param>
        /// <param name="isWire">是否为线框渲染</param>
        public static void DrawRect(Vector2 centre, float w = 1, float h = 1, bool isWire = true)
        {
            GL.Begin(isWire ? GL.LINE_STRIP : GL.QUADS);

            w *= 0.5f; h *= 0.5f;

            float xl = centre.x - w;
            float xr = centre.x + w;
            float yl = centre.y - h;
            float yr = centre.y + h;

            GL.Vertex3(xl, yr, 0f);
            GL.Vertex3(xr, yr, 0f);
            GL.Vertex3(xr, yl, 0f);
            GL.Vertex3(xl, yl, 0f);
            GL.Vertex3(xl, yr, 0f);

            GL.End();
        }
        public static void DrawRectByAngle(Vector2 centre, float angle, float w = 1, float h = 1, bool isWire = true)
        {
            GL.Begin(isWire ? GL.LINE_STRIP : GL.QUADS);

            w *= 0.5f; h *= 0.5f;

            double r = angle * MathHelper.Deg2Rad;
            double c = Math.Cos(r);
            double s = Math.Sin(r);

            Vector2 a = default; // -w +h
            a.x = centre.x + (float)(-w * c - h * s);
            a.y = centre.y + (float)(h * c - w * s);
            GL.Vertex(a);
            Vector2 b = default; // +w +h
            b.x = centre.x + (float)(w * c - h * s);
            b.y = centre.y + (float)(w * s + h * c);
            GL.Vertex(b);       // +w -h
            b.x = centre.x + (float)(w * c + h * s);
            b.y = centre.y + (float)(w * s - h * c);
            GL.Vertex(b);
            b.x = centre.x + (float)(h * s - w * c);
            b.y = centre.y + (float)(-w * s - h * c);
            GL.Vertex(b);
            GL.Vertex(a);

            GL.End();
        }
        /// <summary>
        /// 绘制单元格
        /// </summary>
        /// <param name="origin">原点坐标 通常在左下角</param>
        /// <param name="xNum">横轴数量</param>
        /// <param name="yNum">纵轴数量</param>
        public static void DrawCells(Vector2 origin, int xNum, int yNum)
        {
            float tmp, a = origin.y;
            float b = a + yNum;
            int i = xNum;
            GL.Begin(GL.LINES);
            while (i >= 0)
            {
                tmp = origin.x + i--;
                GL.Vertex3(tmp, a, 0f);
                GL.Vertex3(tmp, b, 0f);
            }
            b = origin.x;
            a = b + xNum;
            while (yNum >= 0)
            {
                tmp = origin.y + yNum--;
                GL.Vertex3(a, tmp, 0f);
                GL.Vertex3(b, tmp, 0f);
            }
            GL.End();
        }
        // /// <summary>
        // /// 在向量尾端绘制箭头
        // /// </summary>
        // public static void DrawArrow(Vector2 s, Vector2 e, float sideLength, float angle, bool isWire = false)
        // {
        //     var vec = (s - e).normalized * sideLength;
        //     GL.Begin(isWire ? GL.LINE_STRIP : GL.TRIANGLES);
        //     GL.Vertex(e);
        //     GL.Vertex(vec.RotateTo(e, angle));
        //     GL.Vertex(vec.RotateTo(e, -angle));
        //     GL.Vertex(e);
        //     GL.End();
        // }
        /// <summary>
        /// 节点模式绘制线条 线段之间有断层
        /// </summary>
        public static void DrawLine(Vector2[] points)
        {
            int len = points.Length;
            if (len < 2) return;
            GL.Begin(GL.LINES);
            for (int i = 0; i < len; i++) GL.Vertex(points[i]);
            GL.End();
        }
        /// <summary>
        /// 根据顶点绘制连续线条
        /// </summary>
        public static void DrawLine(Vector2[] points, bool isClosed)
        {
            int len = points.Length;
            if (len < 2) return;
            GL.Begin(GL.LINE_STRIP);
            for (int i = 0; i < len; i++) GL.Vertex(points[i]);
            if (isClosed) GL.Vertex(points[0]);
            GL.End();
        }
        /// <summary>
        /// 绘制一根线段
        /// </summary>
        public static void DrawLine(Vector2 p1, Vector2 p2)
        {
            GL.Begin(GL.LINES);
            GL.Vertex3(p1.x, p1.y, 0f);
            GL.Vertex3(p2.x, p2.y, 0f);
            GL.End();
        }
    }
}