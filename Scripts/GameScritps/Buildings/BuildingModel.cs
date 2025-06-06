// 文件路径: Scripts/GameScritps/Buildings/BuildingModel.cs
using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using YourGameNamespace.Events; // For building events
using UnityEngine; // Required for Debug.Log in case it's uncommented

namespace YourGameNamespace.Buildings
{
    /// <summary>
    /// 建筑数据模型接口
    /// </summary>
    public interface IBuildingModel : IModel
    {
        /// <summary>
        /// 添加一个新建筑到模型中
        /// </summary>
        void AddBuilding(Building building);

        /// <summary>
        /// 根据ID移除一个建筑
        /// </summary>
        void RemoveBuilding(Guid buildingId);

        /// <summary>
        /// 根据ID获取建筑对象
        /// </summary>
        Building GetBuildingById(Guid buildingId);

        /// <summary>
        /// 获取所有已建造建筑的列表
        /// </summary>
        List<Building> GetAllBuildings();

        /// <summary>
        /// 获取指定类型的已建造建筑列表
        /// </summary>
        List<Building> GetBuildingsByType(BuildingType type);

        /// <summary>
        /// 获取当前总住房容量
        /// </summary>
        int GetTotalHousingCapacity();

        /// <summary>
        /// 获取当前总病人容量
        /// </summary>
        int GetTotalPatientCapacity();
    }

    public class BuildingModel : AbstractModel, IBuildingModel
    {
        private List<Building> mConstructedBuildings = new List<Building>();

        protected override void OnInit()
        {
            // Debug.Log("建筑模型 (BuildingModel) 初始化完成。"); // Chinese log
        }

        public void AddBuilding(Building building)
        {
            if (building != null && !mConstructedBuildings.Any(b => b.Id == building.Id))
            {
                mConstructedBuildings.Add(building);
                // Debug.Log($"新建筑已添加: 类型 {building.Type}, ID {building.Id.ToString().Substring(0,4)}"); // Chinese log
                this.SendEvent(new Model_BuildingConstructedEvent() { BuildingData = building });
            }
            // else
            // {
            //    Debug.LogWarning($"尝试添加建筑失败：建筑为空或已存在 (ID: {building?.Id.ToString().Substring(0,4)})。"); // Chinese log
            // }
        }

        public void RemoveBuilding(Guid buildingId)
        {
            Building buildingToRemove = mConstructedBuildings.FirstOrDefault(b => b.Id == buildingId);
            if (buildingToRemove != null)
            {
                mConstructedBuildings.Remove(buildingToRemove);
                // Debug.Log($"建筑已移除: 类型 {buildingToRemove.Type}, ID {buildingId.ToString().Substring(0,4)}"); // Chinese log
                this.SendEvent(new Model_BuildingDemolishedEvent() { BuildingId = buildingId, BuildingType = buildingToRemove.Type });
            }
            // else
            // {
            //    Debug.LogWarning($"尝试移除建筑失败：未找到ID为 {buildingId.ToString().Substring(0,4)} 的建筑。"); // Chinese log
            // }
        }

        public Building GetBuildingById(Guid buildingId)
        {
            return mConstructedBuildings.FirstOrDefault(b => b.Id == buildingId);
        }

        public List<Building> GetAllBuildings()
        {
            return new List<Building>(mConstructedBuildings);
        }

        public List<Building> GetBuildingsByType(BuildingType type)
        {
            return new List<Building>(mConstructedBuildings.Where(b => b.Type == type));
        }

        public int GetTotalHousingCapacity()
        {
            int totalCapacity = 0;
            foreach (var building in mConstructedBuildings)
            {
                if (building.IsOperational.Value)
                {
                    totalCapacity += building.HousingCapacityProvided;
                }
            }
            return totalCapacity;
        }

        public int GetTotalPatientCapacity()
        {
            int totalCapacity = 0;
            foreach (var building in mConstructedBuildings)
            {
                if (building.IsOperational.Value)
                {
                    totalCapacity += building.PatientCapacity;
                }
            }
            return totalCapacity;
        }
    }
}
