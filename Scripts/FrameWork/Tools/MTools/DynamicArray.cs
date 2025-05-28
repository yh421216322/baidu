using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MyGameNamespace
{
    public class FuncComparer<T> : IComparer<T>
    {
        private Comparison<T> compareFunction;
        public FuncComparer(Comparison<T> comparison)
        {
            compareFunction = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }
        int IComparer<T>.Compare(T x, T y) => compareFunction(x, y);
    }
    /// <summary>
    /// 可自动扩容的动态数组 注意该结构需要手动释放未使用结构
    /// 当移除时 需手动调用缩容
    /// </summary>
    public class DynamicArray<T> : IEnumerable<T>
    {
        protected T[] arr;
        protected int N = 0;
        public int Count => N;
        public int Capacity => arr.Length;
        public bool IsEmpty => N == 0;
        public int MaxIndex => N - 1;
        public T[] SelfArray => arr;
        public T Last => arr[N - 1];
        public T this[int index]
        {
            get
            {
                IsLegal(index);
                return arr[index];
            }
            set
            {
                IsLegal(index);
                arr[index] = value;
            }
        }
        public DynamicArray(int capacity = 8)
        {
            if (capacity <= 0)
                throw new ArgumentException("无效的容量");
            arr = new T[capacity];
        }
        public DynamicArray(IEnumerable<T> items)
        {
            arr = items.ToArray();
            N = arr.Length;
        }
        /// <summary>
        /// 如果 N 等于 最大容量 说明装满了
        /// </summary>
        public bool IsFull(int maxCapacity) => N == maxCapacity;
        /// <summary>
        /// 往末尾添加元素 限制只能添加maxCapacity个
        /// </summary>
        public void Push(T e, int maxCapacity)
        {
            // 如果 N 小于最大容量 且 等于当前数组长度 启动扩容
            if (arr.Length < maxCapacity && N == arr.Length)
            {
                int newCapacity = N + N;
                // 如果下一个长度大于 最大容量 就直接返回最大容量
                Resize(newCapacity < maxCapacity ? newCapacity : maxCapacity);
            }
            Add(e);
        }
        /// <summary>
        /// 往末尾添加元素
        /// </summary>
        public void Push(T e)
        {
            if (N == arr.Length) Resize(N + N);
            Add(e);
        }
        // 往后添加元素
        private void Add(T e) => arr[N++] = e;
        /// <summary>
        /// 跟栈一样的弹出操作 需要手动调用缩容
        /// </summary>
        public T Pop() => arr[--N];
        /// <summary>
        /// 弹栈操作 需要手动调用缩容
        /// </summary>
        public bool TryPop(out T e)
        {
            if (N == 0)
            {
                e = default;
                return false;
            }
            e = Pop();
            return true;
        }
        /// <summary>
        /// 移除尾部元素 有缩容机制
        /// </summary>
        public void RemoveLast()
        {
            if (N == 0) return;
            N--; Shrinkage();
        }
        public void RemoveLastNoShrinkage()
        {
            if (N > 0) N--;
        }
        /// <summary>
        /// 交换式移除 处理静态数组 且没有顺序的情况下使用 有缩容机制
        /// </summary>
        public void SwapRemove(int index)
        {
            IsLegal(index);
            // 如果就是最后一位 直接移除无需交换
            if (index == --N) return;
            // 从有效区域移除后与最后一个做交换
            arr[index] = arr[N];
            Shrinkage();
        }
        /// <summary>
        /// 无缩容机制的移除操作
        /// </summary>
        public void SwapRemoveNoShrinkage(int index)
        {
            IsLegal(index);
            // 如果就是最后一位 直接移除无需交换
            if (index == --N) return;
            // 从有效区域移除后与最后一个做交换
            arr[index] = arr[N];
        }
        /// <summary>
        /// 尝试随机获取并移除 需要手动调用缩容
        /// </summary>
        public bool TryRandomDequeue(out T e)
        {
            if (N == 0)
            {
                e = default;
                return false;
            }
            int index = MathHelper.random.Next(0, N--);
            // 如果就是最后一位索引 就无需交换
            if (index == N)
                e = arr[index];
            else
            {
                e = arr[index];
                arr[index] = arr[N];
            }
            return true;
        }
        /// <summary>
        /// 两个位置交换元素
        /// </summary>
        public void Swap(int a, int b)
        {
            IsLegal(a);
            IsLegal(b);
            T item = arr[a];
            arr[a] = arr[b];
            arr[b] = item;
        }
        /// <summary>
        /// 从有效位置中随机获取一个元素
        /// </summary>
        public bool TryRandomGet(out T e)
        {
            e = N == 0 ? default : arr[MathHelper.random.Next(0, N)];
            return N > 0;
        }
        /// <summary>
        /// 移除当前索引元素 有缩容机制
        /// </summary>
        public void Remove(int i)
        {
            IsLegal(i);
            N--;
            while (i < N) arr[i++] = arr[i + 1];
            Shrinkage();
        }
        /// <summary>
        /// 获取最小值并移除 a > b 为成立 有缩容机制
        /// </summary>
        public T GetMinOrRemove(Func<T, T, bool> comparison)
        {
            if (N == 0 || comparison == null)
                throw new InvalidOperationException("比较函数为空或数组无元素");
            int index = 0;
            T min = arr[0];
            for (int i = 1; i < N; i++)
            {
                if (comparison(min, arr[i]))
                {
                    min = arr[i];
                    index = i;
                }
            }
            // 如果就是最后一位 直接移除无需交换
            if (index == --N) return min;
            // 从有效区域移除后与最后一个做交换
            arr[index] = arr[N];
            // 移除后进行缩容
            Shrinkage();
            return min;
        }
        public void Sort(IComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentException("比较器不能为空", nameof(comparer));
            if (N == 0) return;
            Array.Sort<T>(arr, 0, N, comparer);
        }
        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentException("比较函数不能为空", nameof(comparison));
            if (N == 0) return;
            Array.Sort<T>(arr, 0, N, new FuncComparer<T>(comparison));
        }
        public bool Contains(T e) => Array.IndexOf<T>(arr, e, 0, N) >= 0;
        public int IndexOf(T e) => Array.IndexOf<T>(arr, e, 0, N);
        public int IndexOf(Predicate<T> call)
        {
            if (call == null)
                throw new ArgumentException("回调不能为空");
            for (int i = 0; i < N; i++)
                if (call(arr[i])) return i;
            return -1;
        }
        // 检查索引是否合法
        private void IsLegal(int index)
        {
            if (index < 0 || index >= N)
                throw new ArgumentException("索引越界");
        }
        public DynamicArray<T> Clone()
        {
            return new DynamicArray<T>(arr.Take(N));
        }
        // 需要主动前置条件
        public void TryShrinkage()
        {
            if (N < arr.Length / 4)
                Resize(arr.Length / 2);
        }
        /// <summary>
        /// 先移除对象 然后缩容
        /// </summary>
        private void Shrinkage()
        {
            if (arr.Length < 8) return;
            TryShrinkage();
        }
        /// <summary>
        /// 清空所有元素引用 不进行缩容
        /// </summary>
        public void Clear()
        {
            if (N == 0) return;
            Array.Clear(arr, 0, arr.Length);
            ResetCursor();
        }
        private void Resize(int newSize)
        {
            // 如果长度相等 不需要变容量
            if (arr.Length == newSize) return;
            T[] newArr = new T[newSize];
            Array.Copy(arr, 0, newArr, 0, N);
            arr = newArr;
        }
        /// <summary>
        /// 重置容量到 N
        /// </summary>
        public void ResizeToN() => Resize(N);
        /// <summary>
        /// 只重置游标 不释放数据
        /// </summary>
        public void ResetCursor() => N = 0;
        /// <summary>
        /// 游标移动到数组的末端
        /// </summary>
        public void MoveToLast() => N = arr.Length;
        /// <summary>
        /// 迭代器
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < N; i++)
                yield return arr[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}