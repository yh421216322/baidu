using System.Collections.Generic;
using UnityEngine; // 用于 Mathf.Min 等数学运算

namespace YourGameNamespace
{
    /// <summary>
    /// 资源存储类 (ResourceStorage)，负责管理游戏中的各种具体资源（如食物、电力、弹药等）。
    /// 此类提供添加资源、消耗资源、检查资源是否足够以及获取资源数量等核心功能。
    /// </summary>
    public class ResourceStorage
    {
        // 使用字典来存储每种资源的具体数量，键为 GameResourceType 枚举类型，值为整数数量。
        private Dictionary<GameResourceType, int> resourceQuantities = new Dictionary<GameResourceType, int>();

        // 定义一个常量，表示标准资源的默认初始数量。
        private const int DefaultInitialAmount = 100;

        /// <summary>
        /// ResourceStorage 类的构造函数。在创建 ResourceStorage 对象时自动调用。
        /// 此构造函数负责初始化所有在 GameResourceType 枚举中定义的资源类型。
        /// 对于研究点 (ResearchPoints) 和电子零件 (ElectronicParts)，初始数量设置为5000（可能是测试值或特定设计）。
        /// 其他所有资源类型则使用 DefaultInitialAmount (当前为100) 作为其初始数量。
        /// </summary>
        public ResourceStorage()
        {
            // 遍历 GameResourceType 枚举中的所有值，为每种资源设置初始数量。
            foreach (GameResourceType resourceType in System.Enum.GetValues(typeof(GameResourceType)))
            {
                if (resourceType == GameResourceType.ResearchPoints || 
                    resourceType == GameResourceType.ElectronicParts)
                {
                    // 特殊处理研究点和电子零件的初始数量。
                    resourceQuantities[resourceType] = 5000; // 注意：这里的5000可能是用于测试的较高初始值。
                }
                else
                {
                    // 其他所有标准资源使用统一的默认初始值。
                    resourceQuantities[resourceType] = DefaultInitialAmount;
                }
            }
        }

        /// <summary>
        /// 向指定的资源类型中添加一定数量的资源。
        /// 如果尝试添加的数量为负数或零，则此方法不执行任何操作。
        /// </summary>
        /// <param name="type">要添加资源的类型 (GameResourceType)。</param>
        /// <param name="amount">要添加的数量 (必须为正数)。</param>
        public void AddResource(GameResourceType type, int amount)
        {
            if (amount <= 0) return; // 不允许添加负数或零数量的资源。

            if (resourceQuantities.ContainsKey(type))
            {
                // 如果该资源类型已存在于字典中，则直接累加数量。
                resourceQuantities[type] += amount;
            }
            else
            {
                // 如果该资源类型在字典中尚不存在（例如，在枚举中新增了类型但未在此处初始化），
                // 则直接将数量赋值给新条目。这提供了一定的灵活性，但不应常规依赖此行为。
                resourceQuantities[type] = amount;
            }
        }

        /// <summary>
        /// 消耗指定类型的资源。
        /// 可以选择是否允许“强制消耗”。如果允许强制消耗，即使当前资源数量不足以支付请求的数量，
        /// 也会尽可能地消耗掉所有可用的该类型资源（即数量减至0）。
        /// </summary>
        /// <param name="type">要消耗的资源类型 (GameResourceType)。</param>
        /// <param name="amount">请求消耗的数量。</param>
        /// <param name="allowForceConsume">布尔值，指示是否允许强制消耗。默认为 false。</param>
        /// <returns>如果成功消耗了请求数量的资源（或在强制消耗模式下消耗了部分资源），则返回 true；
        /// 如果资源不足且不允许强制消耗，或者资源类型不存在，则返回 false。</returns>
        public bool ConsumeResource(GameResourceType type, int amount, bool allowForceConsume = false)
        {
            if (amount < 0) return false; // 不允许消耗负数数量的资源。

            if (!resourceQuantities.ContainsKey(type))
            {
                // 如果尝试消耗一个未初始化的资源类型，则操作失败。
                return false;
            }

            if (!allowForceConsume) // 如果不允许强制消耗
            {
                if (resourceQuantities[type] < amount)
                {
                    return false; // 资源不足，且不允许强制消耗，操作失败。
                }
                resourceQuantities[type] -= amount; // 正常消耗资源。
            }
            else // 如果允许强制消耗
            {
                // 实际消耗的数量是当前拥有的数量和请求数量中的较小者。
                int amountToConsume = Mathf.Min(resourceQuantities[type], amount);
                resourceQuantities[type] -= amountToConsume; // 消耗资源。
            }

            return true; // 消耗操作（全部或部分）成功。
        }

        /// <summary>
        /// 检查指定类型的资源是否满足（大于或等于）所需的数量。
        /// </summary>
        /// <param name="type">要检查的资源类型 (GameResourceType)。</param>
        /// <param name="amount">需要满足的最小数量。</param>
        /// <returns>如果当前资源数量满足要求，则返回 true；否则返回 false。
        /// 如果请求检查的数量为负数，则认为条件总是满足（消耗负数等同于增加）。</returns>
        public bool HasEnough(GameResourceType type, int amount)
        {
            if (amount < 0) return true; // 消耗负数资源在概念上类似于增加资源，因此视为总是“足够”。

            // 检查资源是否存在且数量是否大于或等于所需数量。
            return resourceQuantities.ContainsKey(type) && resourceQuantities[type] >= amount;
        }

        /// <summary>
        /// 获取指定类型的资源的当前存储数量。
        /// </summary>
        /// <param name="type">要查询的资源类型 (GameResourceType)。</param>
        /// <returns>指定类型资源的当前数量。如果该资源类型在存储中未找到（理论上不应发生，因为构造函数会初始化所有类型），
        /// 则返回一个特殊值 (当前为666，应考虑更改为0或抛出异常以指示错误状态)。</returns>
        public int GetAmount(GameResourceType type)
        {
            if (resourceQuantities.TryGetValue(type, out int currentAmount))
            {
                return currentAmount; // 返回找到的资源数量。
            }
            // 如果资源类型在字典中未找到，这通常表示一个逻辑错误或数据不一致。
            // 返回一个特殊值（如0或错误代码）或抛出异常可能是更合适的处理方式。
            // 当前返回666，这可能是一个临时的调试/占位符值。
            return 666; 
        }
    }
}
