using System;
using UnityEngine;

namespace MyGameNamespace
{
    public static class ShapeHelper
    {
        public static Mesh GetRectMesh(float w, float h, bool isAnchorCentered)
        {
            if (isAnchorCentered)
            {
                float subW = w * 0.5f;
                float subH = h * 0.5f;

                return new Mesh()
                {
                    vertices = new Vector3[]
                    {
                        new Vector2(-subW, subH),
                        new Vector2(subW, subH),
                        new Vector2(subW, -subH),
                        new Vector2(-subW, -subH)
                    },
                    uv = new Vector2[]
                    {
                        new Vector2(0,1),
                        new Vector2(1,1),
                        new Vector2(1,0),
                        new Vector2(0,0),
                    },
                    triangles = new int[]
                    {
                        0, 1, 2, 0, 2, 3
                    },
                };
            }
            return new Mesh()
            {
                vertices = new Vector3[]
                {
                    new Vector2(0,0),
                    new Vector2(0,h),
                    new Vector2(w,h),
                    new Vector2(w,0),
                },
                uv = new Vector2[]
                {
                    new Vector2(0,0),
                    new Vector2(0,1),
                    new Vector2(1,1),
                    new Vector2(1,0),
                },
                triangles = new int[]
                {
                    0, 1, 2, 0, 2, 3
                }
            };
        }
        // 三角形居中顶点 UV 还是以左下角
        public static Mesh GetTriangleMeshUp(float w, float h, bool isAnchorCentered)
        {
            if (isAnchorCentered)
            {
                float subW = w * 0.5f;
                float subH = h * 0.5f;

                return new Mesh()
                {
                    vertices = new Vector3[]
                    {
                        new Vector2(-subW, -subH),
                        new Vector2(0f, subH),
                        new Vector2(subW, -subH),
                    },
                    uv = new Vector2[]
                    {
                        new Vector2(0, 0),
                        new Vector2(0.5f, 1),
                        new Vector2(1, 0),
                    },
                    triangles = new int[] { 0, 1, 2 },
                };
            }
            return new Mesh()
            {
                vertices = new Vector3[]
                {
                    new Vector2(0, 0),
                    new Vector2(w * 0.5f, h),
                    new Vector2(h, 0),
                },
                uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(0.5f, 1),
                    new Vector2(1, 0),
                },
                triangles = new int[] { 0, 1, 2 },
            };
        }
        public static Mesh GetTriangleMeshDown(float w, float h, bool isAnchorCentered)
        {
            if (isAnchorCentered)
            {
                float subW = w * 0.5f;
                float subH = h * 0.5f;

                return new Mesh()
                {
                    vertices = new Vector3[]
                    {
                        new Vector2(-subW, subH),
                        new Vector2(subW, subH),
                        new Vector2(0, -subH),
                    },
                    uv = new Vector2[]
                    {
                        new Vector2(0, 1),
                        new Vector2(1, 1),
                        new Vector2(0.5f, 0),
                    },
                    triangles = new int[] { 0, 1, 2 },
                };
            }
            return new Mesh()
            {
                vertices = new Vector3[]
                {
                    new Vector2(0, h),
                    new Vector2(w, h),
                    new Vector2(w * 0.5f, 0),
                },
                uv = new Vector2[]
                {
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(0.5f, 0),
                 },
                triangles = new int[] { 0, 1, 2 },
            };
        }
        public static Mesh GetCubeMesh()
        {
            return new Mesh()
            {
                vertices = new Vector3[]
                {
                    new Vector3(0,0,0), // 0
                    new Vector3(0,0,1), // 1
                    new Vector3(0,1,0), // 2
                    new Vector3(0,1,1), // 3

                    new Vector3(1,0,0), // 4
                    new Vector3(1,0,1), // 5
                    new Vector3(1,1,0), // 6
                    new Vector3(1,1,1), // 7
                },
                triangles = new int[]
                {
                    0,2,6,0,6,4, // 后
                    4,6,7,4,7,5, // 右
                    2,3,7,2,7,6, // 顶
                    0,4,5,0,5,1, // 底
                    1,5,7,1,7,3, // 前
                    0,1,3,0,3,2  // 左
                },
            };
        }
        public static Mesh GetMC_CubeMesh()
        {
            return new Mesh()
            {
                vertices = new Vector3[]
                {
                    // 后
                    new Vector3(0,0,0),
                    new Vector3(0,1,0),
                    new Vector3(1,1,0),
                    new Vector3(1,0,0),
                    // 右
                    new Vector3(1,0,0),
                    new Vector3(1,1,0),
                    new Vector3(1,1,1),
                    new Vector3(1,0,1),
                    // 顶
                    new Vector3(0,1,0),
                    new Vector3(0,1,1),
                    new Vector3(1,1,1),
                    new Vector3(1,1,0),
                    // 底
                    new Vector3(0,0,0),
                    new Vector3(1,0,0),
                    new Vector3(1,0,1),
                    new Vector3(0,0,1),
                    // 前
                    new Vector3(0,0,1),
                    new Vector3(1,0,1),
                    new Vector3(1,1,1),
                    new Vector3(0,1,1),
                    // 左
                    new Vector3(0,0,0),
                    new Vector3(0,0,1),
                    new Vector3(0,1,1),
                    new Vector3(0,1,0),
                },
                triangles = new int[]
                {
                    0,1,2,0,2,3, // 后
                    4,5,6,4,6,7, // 右
                    8,9,10,8,10,11, // 顶
                    12,13,14,12,14,15, // 底
                    16,17,18,16,18,19, // 前
                    20,21,22,20,22,23  // 左
                },
                uv = new Vector2[]
                {
                    // 后
                    new Vector2(0f / 6f,0),
                    new Vector2(0f / 6f,1),
                    new Vector2(1f / 6f,1),
                    new Vector2(1f / 6f,0),
                    // 右        
                    new Vector2(1f / 6f,0),
                    new Vector2(1f / 6f,1),
                    new Vector2(2f / 6f,1),
                    new Vector2(2f / 6f,0),
                    // 顶        
                    new Vector2(4f / 6f,0),
                    new Vector2(4f / 6f,1),
                    new Vector2(5f / 6f,1),
                    new Vector2(5f / 6f,0),
                    // 底        
                    new Vector2(5f / 6f,0),
                    new Vector2(5f / 6f,1),
                    new Vector2(6f / 6f,1),
                    new Vector2(6f / 6f,0),
                    // 前        
                    new Vector2(3f / 6f,0),
                    new Vector2(2f / 6f,0),
                    new Vector2(2f / 6f,1),
                    new Vector2(3f / 6f,1),
                    // 左        
                    new Vector2(4f / 6f,0),
                    new Vector2(3f / 6f,0),
                    new Vector2(3f / 6f,1),
                    new Vector2(4f / 6f,1),
                }
            };
        }
        /// <summary>
        /// 根据中点创建矩形顶点
        /// </summary>
        /// <param name="centre">中心坐标</param>
        /// <param name="w">宽</param>
        /// <param name="h">高</param>
        public static Vector2[] CreateRectVertex(Vector2 centre, float w = 1f, float h = 1f)
        {
            w *= 0.5f; h *= 0.5f;

            float xl = centre.x - w;
            float xr = centre.x + w;
            float yl = centre.y - h;
            float yr = centre.y + h;

            var a = new Vector2(xl, yr);
            var b = new Vector2(xr, yr);
            var c = new Vector2(xr, yl);
            var d = new Vector2(xl, yl);

            return new Vector2[4] { a, b, c, d };
        }
        /// <summary>
        /// 创建等边多边形顶点
        /// </summary>
        /// <param name="centre">中心点</param>
        /// <param name="count">顶点数量</param>
        /// <param name="startAngle">起始角度</param>
        /// <param name="r">生成半径</param>
        public static Vector2[] CreateEquilateralPolyVertex(Vector2 centre, byte count, float startAngle, float r = 1)
        {
            if (count < 3) return null;
            var ver = new Vector2[count];
            // Math.PI * 0.5f; 从圆周的顶部
            double pi1 = MathHelper.Deg2Rad * startAngle;
            double pi2 = 2 * Math.PI / count;
            Vector2 temp; double t; r *= 0.5f;
            for (int i = 0; i < count; i++)
            {
                t = pi1 - i * pi2;
                temp.x = (float)(r * Math.Cos(t));
                temp.y = (float)(r * Math.Sin(t));
                ver[i] = temp + centre;
            }
            return ver;
        }
        /// <summary>
        /// 根据多边形顶点获取中心点
        /// </summary>
        /// <param name="vertexs">顶点集合</param>
        /// <returns>中心点</returns>
        public static Vector2 GetPolyCentre(this Vector2[] vertexs)
        {
            switch (vertexs.Length)
            {
                case 0: throw new Exception("没有顶点无法构建多边形");
                case 1: return vertexs[0];
                case 2: return Vector2.Lerp(vertexs[0], vertexs[1], 0.5f);
                default:
                    int count = vertexs.Length;
                    var v = vertexs[0];
                    float maxX, maxY;
                    float minX = maxX = v.x;
                    float minY = maxY = v.y;

                    for (int i = 1; i < count; i++)
                    {
                        v = vertexs[i];
                        if (v.x < minX) minX = v.x;
                        else if (v.x > maxX) maxX = v.x;

                        if (v.y < minY) minY = v.y;
                        else if (v.y > maxY) maxY = v.y;
                    }
                    v.x = (minX + maxX) * 0.5f;
                    v.y = (minY + maxY) * 0.5f;
                    return v;
            }
        }
    }
}