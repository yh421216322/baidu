// 文件路径: Scripts/GameScritps/Buildings/BuildingSystem.cs
using QFramework;
using System;
using System.Collections.Generic;
using YourGameNamespace.Resources;
using YourGameNamespace.Events;
using YourGameNamespace.Workstations; // For IWorkstationSystem and WorkstationType
using YourGameNamespace.Framework;  // For IGameDataModel
using UnityEngine;                  // For Debug.Log

namespace YourGameNamespace.Buildings
{
    /// <summary>
    /// 建筑系统接口，负责建筑的建造逻辑和效果应用。
    /// </summary>
    public interface IBuildingSystem : QFISystem // Changed from ISystem
    {
        /// <summary>
        /// 尝试建造一个指定类型的建筑。
        /// </summary>
        /// <param name="type">要建造的建筑类型。</param>
        /// <param name="isPlayerAction">是否为玩家直接发起的动作 (用于区分系统内部调用，如初始放置，此时可能不消耗资源)。</param>
        /// <returns>如果建造成功则返回true，否则返回false。</returns>
        bool ConstructBuilding(BuildingType type, bool isPlayerAction = true);

        /// <summary>
        /// 获取指定建筑类型的建造成本。
        /// </summary>
        /// <param name="type">建筑类型。</param>
        /// <returns>成本列表 (资源类型, 数量)，如果未定义则返回null。</returns>
        List<(GameResourceType resource, int amount)> GetBuildingConstructionCosts(BuildingType type);
    }

    public class BuildingSystem : AbstractSystem, IBuildingSystem
    {
        private ResourceModel mResourceModel; // Changed from IResourceModel
        private BuildingModel mBuildingModel; // Changed from IBuildingModel
        private GameDataModel mGameDataModel; // Changed from IGameDataModel
        private WorkstationSystem mWorkstationSystem; // Changed from IWorkstationSystem, 主要用于获取成本信息

        // 建筑成本字典
        private Dictionary<BuildingType, List<(GameResourceType resource, int amount)>> mBuildingCosts;

        protected override void OnInit()
        {
            mResourceModel = this.GetModel<ResourceModel>(); // Use concrete class
            mBuildingModel = this.GetModel<BuildingModel>(); // Use concrete class
            mGameDataModel = this.GetModel<GameDataModel>(); // Use concrete class
            mWorkstationSystem = this.GetSystem<WorkstationSystem>(); // Use concrete class

            if (mResourceModel == null) Debug.LogError("建筑系统：资源模型 (ResourceModel) 未找到！");
            if (mBuildingModel == null) Debug.LogError("建筑系统：建筑模型 (BuildingModel) 未找到！");
            if (mGameDataModel == null) Debug.LogError("建筑系统：游戏数据模型 (GameDataModel) 未找到！");
            if (mWorkstationSystem == null) Debug.LogError("建筑系统：工作站系统 (WorkstationSystem) 未找到！");


            InitializeBuildingCosts();
            // Debug.Log("建筑系统 (BuildingSystem) 初始化完成。");
        }

        private void InitializeBuildingCosts()
        {
            mBuildingCosts = new Dictionary<BuildingType, List<(GameResourceType resource, int amount)>>
            {
                // 非工作站建筑的成本直接在此定义
                { BuildingType.Tent, new List<(GameResourceType resource, int amount)> { (GameResourceType.Wood, 10) } },
                { BuildingType.MedicalPost, new List<(GameResourceType resource, int amount)> { (GameResourceType.Wood, 20), (GameResourceType.ElectronicParts, 5) } },
                { BuildingType.Cookhouse, new List<(GameResourceType resource, int amount)> { (GameResourceType.Wood, 15), (GameResourceType.Scrap, 10) } },
            };

            // 对于与工作站等效的建筑类型，尝试从WorkstationSystem获取成本
            // 这依赖于IWorkstationSystem接口中添加一个 GetWorkstationBuildCosts(WorkstationType type) 方法
            foreach (BuildingType bt in Enum.GetValues(typeof(BuildingType)))
            {
                if (IsWorkstationEquivalent(bt, out WorkstationType wt))
                {
                    // 概念：List<(GameResourceType resource, int amount)> costs = mWorkstationSystem.GetWorkstationBuildCosts(wt);
                    // if (costs != null) {
                    //    mBuildingCosts[bt] = costs;
                    // } else {
                    //    Debug.LogError($"建筑系统：未能从工作站系统获取建筑类型 {bt} (对应工作站 {wt}) 的建造成本。");
                    //    // 可以选择设置一个默认空成本列表或高昂成本以示错误
                    //    mBuildingCosts[bt] = new List<(GameResourceType resource, int amount)>();
                    // }

                    // V1.1 临时占位符成本 (如果IWorkstationSystem.GetWorkstationBuildCosts未实现):
                     switch(bt) {
                        case BuildingType.Farm: mBuildingCosts[bt] = new List<(GameResourceType resource, int amount)> { (GameResourceType.Food, 50) }; break; // 假设建材为食物
                        case BuildingType.PowerPlant: mBuildingCosts[bt] = new List<(GameResourceType resource, int amount)> { (GameResourceType.ElectronicParts, 20) }; break;
                        case BuildingType.Workshop: mBuildingCosts[bt] = new List<(GameResourceType resource, int amount)> { (GameResourceType.ElectronicParts, 15) }; break;
                        case BuildingType.Clinic: mBuildingCosts[bt] = new List<(GameResourceType resource, int amount)> { (GameResourceType.Food, 30) }; break; // 假设建材为食物
                        case BuildingType.ResearchLab: mBuildingCosts[bt] = new List<(GameResourceType resource, int amount)> { (GameResourceType.ElectronicParts, 25) }; break;
                        default:
                            if (!mBuildingCosts.ContainsKey(bt)) // 避免覆盖已在上面定义的非工作站建筑
                                Debug.LogWarning($"建筑系统：建筑类型 {bt} (作为工作站) 的成本未在临时占位符中定义。");
                            break;
                     }
                }
            }
        }

        /// <summary>
        /// 获取指定建筑类型的建造成本。
        /// </summary>
        public List<(GameResourceType resource, int amount)> GetBuildingConstructionCosts(BuildingType type)
        {
            if (mBuildingCosts.TryGetValue(type, out var costs))
            {
                return costs; // 返回成本列表副本可能更安全，但此处为简化直接返回引用
            }
            Debug.LogWarning($"未找到建筑类型 {type} 的建造成本定义。");
            return null; // 或返回空的List
        }

        /// <summary>
        /// 尝试建造一个指定类型的建筑。
        /// </summary>
        public bool ConstructBuilding(BuildingType type, bool isPlayerAction = true)
        {
            if (mResourceModel == null || mBuildingModel == null || mGameDataModel == null) {
                 Debug.LogError($"建造建筑 {type} 失败：核心模型未初始化。");
                 return false;
            }

            List<(GameResourceType resource, int amount)> costs = GetBuildingConstructionCosts(type);
            if (costs == null)
            {
                Debug.LogError($"建造建筑 {type} 失败：未定义其建造成本。");
                return false;
            }

            if (isPlayerAction) // 仅当是玩家操作时才检查并消耗资源
            {
                foreach (var cost in costs)
                {
                    if (!mResourceModel.HasEnough(cost.resource, cost.amount))
                    {
                        Debug.LogWarning($"建造建筑 {type} 失败：资源 {cost.resource} 不足。需要: {cost.amount}, 当前: {mResourceModel.GetAmount(cost.resource)}。");
                        return false;
                    }
                }
                // 再次遍历以消耗资源 (确保所有资源都足够后才统一消耗)
                foreach (var cost in costs)
                {
                    if (!mResourceModel.ConsumeResource(cost.resource, cost.amount))
                    {
                        Debug.LogError($"建造建筑 {type} 失败：消耗资源 {cost.resource} ({cost.amount}单位) 时发生错误。可能在检查后资源量发生变化。");
                        // 此处应考虑资源回滚逻辑，但V1简化处理
                        return false;
                    }
                }
                Debug.Log($"为建造 {type} 已消耗指定资源。");
            }

            Building newBuilding = new Building(type);
            mBuildingModel.AddBuilding(newBuilding); // BuildingModel内部会发送 Model_BuildingConstructedEvent

            ApplyImmediateBuildingEffects(newBuilding);

            // 工作站的创建逻辑将由 WorkstationSystem 监听 Model_BuildingConstructedEvent 来处理，
            // 以实现更好的解耦并避免双重扣费问题。

            Debug.Log($"建筑 {type} (ID: {newBuilding.Id.ToString().Substring(0,4)}) 已成功记录到建筑模型。");
            return true;
        }

        /// <summary>
        /// 应用建筑建成后的即时效果。
        /// </summary>
        private void ApplyImmediateBuildingEffects(Building building)
        {
            switch (building.Type)
            {
                case BuildingType.Tent:
                    if (mGameDataModel.MaxHousingCapacity != null)
                    {
                         mGameDataModel.MaxHousingCapacity.Value += building.HousingCapacityProvided;
                         Debug.Log($"总住房容量因建造 {building.Type} 已增加 {building.HousingCapacityProvided}。新上限: {mGameDataModel.MaxHousingCapacity.Value}");
                    }
                    else { Debug.LogError("GameDataModel.MaxHousingCapacity 未初始化! 无法更新住房容量。"); }
                    break;

                case BuildingType.MedicalPost:
                     Debug.Log($"医疗建筑 {building.Type} 已建造，提供 {building.PatientCapacity} 病人容量。具体效果由医疗系统处理。");
                     break;

                // 对于其他类型的建筑，如果它们有即时全局效果，在此处添加
                // 例如 Cookhouse 可能减少全局食物消耗率等 (需要GameDataModel支持此类属性)

                // Farm, PowerPlant 等作为工作站的特定逻辑由 WorkstationSystem 通过监听事件处理
                // BuildingSystem 本身不再直接调用 WorkstationSystem.BuildWorkstation
            }
        }

        /// <summary>
        /// 辅助方法，检查 BuildingType 是否对应一个 WorkstationType。
        /// </summary>
        public static bool IsWorkstationEquivalent(BuildingType buildingType, out WorkstationType workstationType)
        {
            // 依赖于 BuildingType 和 WorkstationType 中同名枚举成员的精确匹配
            string buildingTypeName = buildingType.ToString();
            return Enum.TryParse(buildingTypeName, out workstationType);
        }
    }
}
