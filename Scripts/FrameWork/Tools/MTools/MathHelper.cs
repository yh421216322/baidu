using System;

namespace MyGameNamespace
{
    // 2的N次方    => [1 << n]
    // N*2的E次方  => [n << e]
    // N/2的E次方  => [n >> e]
    // 1=-1取反    => [~n + 1] 
    // AND 判断o数 => [(n & 1) == 0]
    // 计算以中值两侧的索引 => [i - count/2] 如果是偶数 需要 +0.5f 来补充
    // 是否在0-Max范围内 => [(uint)n < max] 如果n是一个负数，转成uint越界，变成一个很大的正数
    // 使用 BoundsInt 结构通过循环的方式 构建了线性表的索引 可以使用下面的公式进行反推
    // int recalculatedIndex = (x - xMin) * (yMax - yMin) + (y - yMin)
    // cur / max 将 0-max 映射到 0 - 1;
    // EqualityComparer<T>.Default.Equals(a, b); 通常用于泛型比较

    public static class MathHelper
    {
        public readonly static Random random = new Random();
        public const float MIN = 0.001f;
        /// <summary>
        /// 角度转弧度
        /// </summary>
        public const double Deg2Rad = Math.PI / 180;
        /// <summary>
        /// 弧度转角度
        /// </summary>
        public const double Rad2Deg = 180 / Math.PI;
        /// <summary>
        /// 向目标点移动
        /// </summary>
        public static void MoveTowards(this ref float cur, float target, float step)
        {
            cur = (target - cur).Abs() > step ? cur + (target - cur).Sign() * step : target;
        }
        /// <summary>
        /// 简单线性插值
        /// </summary>
        public static float Linear(float a, float b, float t = 0.5f) => (a + b) * t;
        /// <summary>
        /// 求绝对值[位运算优化]
        /// </summary>
        public static int Abs(this int a) => (a ^ (a >> 31)) - (a >> 31);
        /// <summary>
        /// 求绝对值 float
        /// </summary>
        public static float Abs(this float a) => a >= 0f ? a : -a;
        /// <summary>
        /// 求绝对值 double
        /// </summary>
        public static double Abs(this double a) => a >= 0f ? a : -a;
        /// <summary>
        /// 获取符号
        /// </summary>
        public static int Sign(this float f) => f >= 0f ? 1 : -1;
        /// <summary>
        /// 获取符号 无输入 0
        /// </summary>
        public static int Sign(this int n) => n == 0 ? 0 : n > 0 ? 1 : -1;
        /// <summary>
        /// 四舍五入取整
        /// </summary>
        public static int RoundToInt(this float n) => (int)(n >= 0 ? n + 0.5f : n - 0.5f);
        /// <summary>
        /// 将值限定在 0-1 之间
        /// </summary>
        public static void Clamp01(this ref float t) => t = t < 0f ? 0f : t > 1f ? 1f : t;
        public static void Clamp(this ref float t, float min, float max) => t = t < min ? min : t > max ? max : t;
        /// <summary>
        /// 数据交换
        /// </summary>
        public static void Swap<T>(this ref T a, ref T b) where T : struct
        { T c = a; a = b; b = c; }
        public static T Swap<T>(this ref T a, T b) where T : struct
        { T c = a; a = b; return c; }
        /// <summary>
        /// 将一个范围映射到另一个范围
        /// </summary>
        public static float RangeToRange(this float cur, float min, float max, float minA, float maxA)
        {
            return minA + (maxA - minA) / (max - min) * (cur - min);
        }
        /// <summary>
        /// 将一个范围映射到另一个范围 最小值相同
        /// </summary>
        public static float RangeToRange(this float cur, float min, float max, float maxA)
        {
            return maxA / (max - min) * (cur - min);
        }
        /// <summary>
        /// 将一个范围映射到另一个范围[最小值都是0的情况]
        /// </summary>
        public static float RangeToRange(this float cur, float max, float maxA) => maxA / max * cur;
        /// <summary>
        /// 将最小值-最大值的范围 映射到 0 - 1
        /// </summary>
        public static float RangeTo01(this float cur, float min, float max) => cur / (max - min);
        /// <summary>
        /// 将最小值-最大值的范围 映射到 1 - 0
        /// </summary>
        public static float RangeTo10(this float cur, float min, float max) => (max - cur) / (max - min);
        /// <summary>
        /// 0-最大值的范围 映射到 1 - 0
        /// </summary>
        public static float RangeTo10(this float cur, float max) => (max - cur) / max;
        /// <summary>
        /// 检测是否在Min-Max范围内
        /// </summary>
        public static bool InRange(this float n, float min, float max) => n >= min && n <= max;
        /// <summary>
        /// 渐入
        /// </summary>
        public static double EaseIn(this double n, float k = 2f) => Math.Pow(n, k);
        /// <summary>
        /// 渐出
        /// </summary>
        public static double EaseOut(this double n, float k = 2f)
        {
            return (k / (k - 1)) * Math.Pow(n, 1 / k) - 1 / (k - 1) * n;
        }
        /// <summary>
        /// 缓动
        /// </summary>
        public static double EaseInAndOut(this double n, float k = 2f)
        {
            double pow = Math.Pow(n, k);
            return -k * pow * n + (k + 1) * pow;
        }
        /// <summary>
        /// 弹跳
        /// </summary>
        public static double Bounce(this double n, byte count)
        {
            return (1 - n) * Math.Sin(2 * n * Math.PI * count);
        }
    }
}