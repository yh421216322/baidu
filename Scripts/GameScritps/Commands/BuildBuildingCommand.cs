// 文件路径: Scripts/GameScritps/Commands/BuildBuildingCommand.cs
using QFramework;
using YourGameNamespace.Buildings;
using YourGameNamespace.Events;
using UnityEngine;

namespace YourGameNamespace.Commands
{
    /// <summary>
    /// 请求建造一个新建筑的命令
    /// </summary>
    public class BuildBuildingCommand : AbstractCommand
    {
        private readonly BuildingType mBuildingType;
        private readonly bool mIsPlayerAction; // 新增：区分是否为玩家直接操作

        public BuildBuildingCommand(BuildingType buildingType, bool isPlayerAction = true)
        {
            mBuildingType = buildingType;
            mIsPlayerAction = isPlayerAction;
        }

        protected override void OnExecute()
        {
            var buildingSystem = this.GetSystem<BuildingSystem>(); // Changed from IBuildingSystem
            if (buildingSystem == null)
            {
                // Debug.LogError($"BuildBuildingCommand: 建筑系统 (BuildingSystem) 未找到！"); // Chinese Log
                this.SendEvent(new BuildBuildingResultEvent {
                    BuildingTypeAttempted = mBuildingType,
                    Success = false,
                    FailureReasonKey = "SYSTEM_NOT_FOUND"  // 系统未找到
                });
                return;
            }

            // 调用时传递 isPlayerAction
            bool success = buildingSystem.ConstructBuilding(mBuildingType, mIsPlayerAction);

            string failureKey = "";
            if (!success)
            {
                // BuildingSystem 内部会记录更具体的失败原因 (如资源不足)
                failureKey = "CONSTRUCTION_FAILED_GENERAL"; // 通用建造失败
                // Debug.LogWarning($"命令：请求建造建筑 {mBuildingType} 失败。"); // Chinese Log
            }
            // else
            // {
            //    Debug.Log($"命令：成功请求建造建筑 {mBuildingType}。"); // Chinese Log
            // }

            this.SendEvent(new BuildBuildingResultEvent {
                BuildingTypeAttempted = mBuildingType,
                Success = success,
                FailureReasonKey = failureKey
            });
        }
    }
}
