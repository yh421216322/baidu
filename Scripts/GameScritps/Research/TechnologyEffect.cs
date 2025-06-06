using YourGameNamespace.Workstations; // 用于 WorkstationType 枚举

namespace YourGameNamespace.Research
{
    // 技术效果的类型枚举
    public enum TechnologyEffectType
    {
        IncreaseProductionOutput,       // 增加特定工作站类型的固定总产量 (例如，每次生产循环 +1 单位)
        IncreaseProductionMultiplier,   // 增加特定工作站类型的产量百分比 (例如，产量 x (1 + 0.1)，即增加10%)
        ModifySurvivorStat,           // 修改幸存者属性 (例如，增加幸存者攻击力，Value 可以是乘数或固定加值)
        UnlockWorkstation,            // 解锁新的工作站类型，使其可建造
        ReduceResourceConsumption,      // 减少特定工作站消耗资源的数量
        IncreaseResearchPointGeneration // 提高研究点的产生速率或数量 (例如，用于研究实验室或其他研究点来源)
    }

    // 代表技术效果的具体数据
    // [System.Serializable] 属性允许该类的实例在Unity检视面板中被序列化和编辑（如果用作MonoBehaviour的公共字段或列表元素）
    [System.Serializable]
    public class TechnologyEffectData
    {
        public TechnologyEffectType EffectType; // 效果的类型
        public float Value;                     // 效果的数值大小。
                                                // 对于 UnlockWorkstation，Value 可以被强制转换为 WorkstationType 枚举的整数值。
                                                // 对于 IncreaseProductionOutput，Value 是一个固定的产量加成值。
                                                // 对于 IncreaseProductionMultiplier，Value 是一个百分比 (例如，0.1 代表 10%)。

        // 可选字段：用于指定效果的具体目标。
        // 并非所有 EffectType 都需要这些字段，具体取决于效果的性质。
        public GameResourceType TargetResource;      // 目标资源类型 (例如，食物、弹药、电力)，主要用于与生产或消耗相关的效果。
        public WorkstationType TargetWorkstationType; // 目标工作站类型 (例如，农场、工坊)，主要用于与生产、解锁或修改特定工作站相关的效果。
        // public string TargetSurvivorStat;        // 如果有一个基于字符串的幸存者属性系统，可以用此字段指定目标属性名称。

        // 为方便起见提供的构造函数
        public TechnologyEffectData(TechnologyEffectType type, float val, WorkstationType targetStation = default(WorkstationType), GameResourceType targetRes = default(GameResourceType))
        {
            EffectType = type;
            Value = val;
            TargetWorkstationType = targetStation;
            TargetResource = targetRes;
        }
    }
}
