using System;
using UnityEngine;

namespace MyGameNamespace
{
    public static class MonoHelper
    {
        /// <summary>
        /// 找到面板父节点下所有对应控件
        /// </summary>
        /// <typeparam name="T">控件类型</typeparam>
        public static void FindChildrenControl<T>(this Component mono, Action<string, T> callback = null) where T : Component
        {
            // 得到所有子控件
            T[] controls = mono.GetComponentsInChildren<T>(true);
            // 如果没有找到组件 直接 return
            if (controls.Length == 0) return;
            // 遍历所有子控件
            foreach (T control in controls)
            {
                string objName = control.gameObject.name;
                callback?.Invoke(objName, control);
            }
        }
        public static void Log<T>(this T msg)
        {
#if UNITY_EDITOR
            Debug.unityLogger.Log(LogType.Log, msg);
#endif
        }
        public static void DrawBox(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color, float duration = 0)
        {
            Debug.DrawLine(a, b, color, duration);
            Debug.DrawLine(b, c, color, duration);
            Debug.DrawLine(c, d, color, duration);
            Debug.DrawLine(d, a, color, duration);
        }
        
        public static void DrawLine(Vector2 a, Vector2 b, Color color, float duration = 0)
        {
            Debug.DrawLine(a, b, color, duration);//
         
        }
        public static void DrawBox(Vector2 origin, Vector2 size, Color color, float duration = 0)
        {
            //Debug.Log("画线");
            Vector2 _half = size * 0.5f;

            float x1 = origin.x - _half.x;
            float x2 = origin.x + _half.x;
            float y1 = origin.y - _half.y;
            float y2 = origin.y + _half.y;

            var a = new Vector2(x1, y2);
            var b = new Vector2(x2, y2);
            var c = new Vector2(x2, y1);
            var d = new Vector2(x1, y1);

            DrawBox(a, b, c, d, color, duration);
        }
    }
}