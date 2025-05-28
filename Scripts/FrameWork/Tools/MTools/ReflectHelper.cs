using System;
using System.Linq;
using System.Reflection;

namespace MyGameNamespace
{
    public static class ReflectHelper
    {
        public static Assembly GetAssembly(string assName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == assName);
        }
        /// <summary>
        /// 反射创建实例
        /// </summary>
        /// <param name="fullName">带命名空间的完整类名</param>
        public static object CreateInstance(string fullName)
        {
            var type = Type.GetType(fullName);
            return Activator.CreateInstance(type);
        }
        /// <summary>
        /// 利用【反射】给类的字段赋值 bool值 使用 0 1进行配置
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="obj">被赋值的对象</param>
        public static void FiSetValue(string value, FieldInfo fi, object obj)
        {
            if (fi.FieldType == typeof(bool))
            {
                if (sbyte.TryParse(value, out var r))
                {
                    fi.SetValue(obj, r == 1);
                }
            }
            else
            {
                var type = Nullable.GetUnderlyingType(fi.FieldType) ?? fi.FieldType;
                var o = Convert.ChangeType(value, type);
                if (o == null) return;
                fi.SetValue(obj, o);
            }
        }
    }
}