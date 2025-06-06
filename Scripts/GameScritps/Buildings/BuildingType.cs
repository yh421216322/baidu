// 文件路径: Scripts/GameScritps/Buildings/BuildingType.cs
namespace YourGameNamespace.Buildings
{
    /// <summary>
    /// 建筑类型枚举
    /// </summary>
    public enum BuildingType
    {
        // --- 基础与住房 ---
        /// <summary>
        /// 帐篷 (提供少量住房)
        /// </summary>
        Tent,

        // --- 医疗 ---
        /// <summary>
        /// 医疗站 (基础治疗设施)
        /// </summary>
        MedicalPost,

        // --- 食物与水 ---
        /// <summary>
        /// 食堂 (提高食物利用率或允许烹饪)
        /// </summary>
        Cookhouse,

        // --- 资源生产 (对应 WorkstationType) ---
        /// <summary>
        /// 农场 (生产食物) - 对应 WorkstationType.Farm
        /// </summary>
        Farm,
        /// <summary>
        /// 发电厂 (生产电力) - 对应 WorkstationType.PowerPlant
        /// </summary>
        PowerPlant,
        /// <summary>
        /// 工坊 (生产弹药等) - 对应 WorkstationType.Workshop
        /// </summary>
        Workshop,
        /// <summary>
        /// 诊所 (生产药品) - 对应 WorkstationType.Clinic
        /// </summary>
        Clinic,
        /// <summary>
        /// 科研实验室 (产生研究点) - 对应 WorkstationType.ResearchLab
        /// </summary>
        ResearchLab,
    }
}
