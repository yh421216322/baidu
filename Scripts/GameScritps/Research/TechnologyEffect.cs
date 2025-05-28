
using YourGameNamespace.Workstations; // 用于 WorkstationType

namespace YourGameNamespace.Research
{
    public enum TechnologyEffectType
    {
        IncreaseProductionOutput,      // 特定工作站类型的总产量增加（例如，+1 单位）
        IncreaseProductionMultiplier,  // 特定工作站类型的百分比产量增加（例如，+0.1 表示 10%）
        ModifySurvivorStat,          // 例如，增加幸存者攻击力（Value = 乘数或固定加值）
        UnlockWorkstation,
        ReduceResourceConsumption,     // 用于消耗资源的工作站
        IncreaseResearchPointGeneration // 用于实验室或其他研究点来源
    }

    [System.Serializable] // 如果我们希望在Unity检视面板中使用它，则使其可序列化
    public class TechnologyEffectData
    {
        public TechnologyEffectType EffectType;
        public float Value; // 效果的大小。对于 UnlockWorkstation，将 WorkstationType 转换为 float。
                            // 对于 IncreaseProductionOutput，这是一个固定加值。
                            // 对于 IncreaseProductionMultiplier，这是一个百分比（0.1 = 10%）。

        // 可选：指定效果的目标。
        // 并非所有效果都需要这些，取决于 EffectType。
        public GameResourceType TargetResource;      // 例如，食物、弹药、电力（用于生产/消耗效果）
        public WorkstationType TargetWorkstationType; // 例如，农场、工坊（用于生产或解锁效果）
        // public string TargetSurvivorStat; // 如果我们有一个基于字符串的幸存者属性系统

        // 为方便起见提供的构造函数
        public TechnologyEffectData(TechnologyEffectType type, float val, WorkstationType targetStation = default, GameResourceType targetRes = default)
        {
            EffectType = type;
            Value = val;
            TargetWorkstationType = targetStation;
            TargetResource = targetRes;
        }
    }
}
