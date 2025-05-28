using System.Collections.Generic;
using UnityEngine; // 用于 Mathf.Min 等数学运算

namespace YourGameNamespace
{
    /// <summary>
    /// 资源存储类，负责管理游戏中的各种资源（如食物、电力、弹药等）。
    /// 支持添加资源、消耗资源、检查资源是否足够等功能。
    /// </summary>
    public class ResourceStorage
    {
        // 使用字典保存每种资源的数量，键为 GameResourceType 枚举值
        private Dictionary<GameResourceType, int> resourceQuantities = new Dictionary<GameResourceType, int>();

        // 默认初始资源数量常量，用于初始化标准资源
        private const int DefaultInitialAmount = 100;

        /// <summary>
        /// 构造函数，在实例化 ResourceStorage 时自动调用。
        /// 初始化所有类型的资源，默认设置研究点和电子零件为 0，其他资源为默认值。
        /// </summary>
        public ResourceStorage()
        {
            // 遍历 GameResourceType 枚举的所有值，初始化每种资源
            foreach (GameResourceType resourceType in System.Enum.GetValues(typeof(GameResourceType)))
            {
                if (resourceType == GameResourceType.ResearchPoints || 
                    resourceType == GameResourceType.ElectronicParts)
                {
                    // 研究点和电子零件初始为 0
                    resourceQuantities[resourceType] = 5000;
                }
                else
                {
                    // 其他资源使用默认初始值
                    resourceQuantities[resourceType] = DefaultInitialAmount;
                }
            }
        }

        /// <summary>
        /// 向指定类型的资源中添加一定数量。
        /// 如果传入负数，则不执行操作。
        /// </summary>
        /// <param name="type">要添加的资源类型</param>
        /// <param name="amount">要添加的数量（必须为非负数）</param>
        public void AddResource(GameResourceType type, int amount)
        {
            if (amount < 0) return; // 不允许添加负数资源

            if (resourceQuantities.ContainsKey(type))
            {
                // 如果资源已存在，累加数量
                resourceQuantities[type] += amount;
            }
            else
            {
                // 如果资源不存在（例如新增了资源类型），则直接赋值
                resourceQuantities[type] = amount;
            }
        }

        /// <summary>
        /// 消耗指定类型的资源。
        /// 可以选择是否强制消耗（即使资源不足也尽可能消耗一部分）。
        /// </summary>
        /// <param name="type">要消耗的资源类型</param>
        /// <param name="amount">要消耗的数量</param>
        /// <param name="allowForceConsume">是否允许强制消耗（即使资源不足）</param>
        /// <returns>是否成功消耗了足够的资源</returns>
        public bool ConsumeResource(GameResourceType type, int amount, bool allowForceConsume = false)
        {
            if (amount < 0) return false; // 不允许消耗负数资源

            if (!resourceQuantities.ContainsKey(type))
            {
                // 如果资源类型未初始化，返回失败
                return false;
            }

            if (!allowForceConsume)
            {
                if (resourceQuantities[type] < amount)
                {
                    return false; // 资源不足且不允许强制消耗
                }
                resourceQuantities[type] -= amount; // 正常消耗
            }
            else
            {
                // 强制消耗：消耗当前拥有的全部资源，最多不超过 amount
                int amountToConsume = Mathf.Min(resourceQuantities[type], amount);
                resourceQuantities[type] -= amountToConsume;
            }

            return true;
        }

        /// <summary>
        /// 检查指定类型的资源是否满足所需数量。
        /// </summary>
        /// <param name="type">要检查的资源类型</param>
        /// <param name="amount">需要的最小数量</param>
        /// <returns>是否满足条件</returns>
        public bool HasEnough(GameResourceType type, int amount)
        {
            if (amount < 0) return true; // 消耗负数在概念上类似于增加，视为总是满足

            return resourceQuantities.ContainsKey(type) && resourceQuantities[type] >= amount;
        }

        /// <summary>
        /// 获取指定类型的资源当前数量。
        /// </summary>
        /// <param name="type">要获取的资源类型</param>
        /// <returns>当前资源数量，若资源类型未知则返回 0</returns>
        public int GetAmount(GameResourceType type)
        {
            if (resourceQuantities.TryGetValue(type, out int currentAmount))
            {
                return currentAmount;
            }
            return 666; // 如果资源类型未找到，返回 0
        }
    }
}
