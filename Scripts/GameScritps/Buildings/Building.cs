// 文件路径: Scripts/GameScritps/Buildings/Building.cs
using System;
using QFramework;
using UnityEngine;

namespace YourGameNamespace.Buildings
{
    /// <summary>
    /// 建筑基类，存储所有建筑的通用数据
    /// </summary>
    public class Building
    {
        /// <summary>
        /// 建筑的唯一ID
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 建筑类型
        /// </summary>
        public BuildingType Type { get; private set; }

        /// <summary>
        /// 建筑是否正在运作
        /// </summary>
        public BindableProperty<bool> IsOperational { get; private set; }

        // --- 类型特定属性 ---
        /// <summary>
        /// 此建筑提供的住房容量 (仅对住房类建筑有意义)
        /// </summary>
        public int HousingCapacityProvided { get; private set; }

        /// <summary>
        /// 此医疗建筑的病人容量 (仅对医疗类建筑有意义)
        /// </summary>
        public int PatientCapacity { get; private set; }

        public Building(BuildingType type)
        {
            Id = Guid.NewGuid();
            Type = type;
            IsOperational = new BindableProperty<bool>(true);
            InitializeTypeSpecificProperties(type);
        }

        private void InitializeTypeSpecificProperties(BuildingType buildingType)
        {
            HousingCapacityProvided = 0;
            PatientCapacity = 0;

            switch (buildingType)
            {
                case BuildingType.Tent:
                    HousingCapacityProvided = 5;
                    break;

                case BuildingType.MedicalPost:
                    PatientCapacity = 2;
                    break;

                case BuildingType.Cookhouse:
                    break;

                case BuildingType.Farm:
                case BuildingType.PowerPlant:
                case BuildingType.Workshop:
                case BuildingType.Clinic:
                case BuildingType.ResearchLab:
                    break;

                default:
                    Debug.LogWarning($"建筑类型 {buildingType} 在 InitializeTypeSpecificProperties 中没有特定的属性设置。");
                    break;
            }
        }
    }
}
