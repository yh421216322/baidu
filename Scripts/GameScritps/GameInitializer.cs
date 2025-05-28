using UnityEngine;
using YourGameNamespace;
using YourGameNamespace.Combat;
using YourGameNamespace.Survivors;
using YourGameNamespace.Workstations;

public class GameInitializer : MonoBehaviour
    {
        void Awake()
        {
            // 这将调用 QFramework 的 Architecture<T> 中的 MakeSureArchitecture
            // 并随后调用你的 GameArchitecture 的 Init()
            var architecture = GameArchitecture.Interface;

            // GameInitializer.Awake() 中的临时测试代码
            var workstationSystem = GameArchitecture.Interface.GetSystem<WorkstationSystem>();
            var survivorModel = GameArchitecture.Interface.GetModel<SurvivorModel>();
            var workstationModel = GameArchitecture.Interface.GetModel<WorkstationModel>();
            var combatSystem = GameArchitecture.Interface.GetSystem<CombatSystem>();
            var survivorManager = GameArchitecture.Interface.GetSystem<SurvivorManagerSystem>();


            if (workstationSystem != null && survivorModel != null && workstationModel != null)
            {
                // 如果不存在，则构建默认工作站
                if (workstationModel.GetAllWorkstations().Count == 0) 
                {
                     workstationSystem.BuildWorkstation(WorkstationType.Farm);
                     workstationSystem.BuildWorkstation(WorkstationType.PowerPlant);
                     workstationSystem.BuildWorkstation(WorkstationType.Workshop);
                }

                // 如果有可用幸存者且农场为空，则分配幸存者到农场
                var farm = workstationModel.GetAllWorkstations().Find(w => w.Type == WorkstationType.Farm);
                var availableSurvivors = survivorModel.GetAvailableSurvivors();
                if (farm != null && availableSurvivors.Count > 0 && farm.AssignedSurvivorIds.Count == 0)
                {
                    workstationSystem.AssignSurvivorToWorkstation(availableSurvivors[0].Id, farm.Id);
                }
                
                // 如果有可用幸存者且工坊为空，则分配幸存者到工坊
                var workshop = workstationModel.GetAllWorkstations().Find(w => w.Type == WorkstationType.Workshop);
                availableSurvivors = survivorModel.GetAvailableSurvivors(); // 重新获取，第一个可能已被分配
                if (workshop != null && availableSurvivors.Count > 0 && workshop.AssignedSurvivorIds.Count == 0)
                {
                    workstationSystem.AssignSurvivorToWorkstation(availableSurvivors[0].Id, workshop.Id);
                }
            }
            
            // 设置 CombatSystem 基地位置
            if (combatSystem != null)
            {
                 combatSystem.BasePosition = new Vector2(0,0); 
                 // Debug.Log("游戏初始化器：已为战斗系统设置基地位置。"); // 减少日志 spam
            }
            else
            {
                Debug.LogError("游戏初始化器：战斗系统 (CombatSystem) 为空！无法设置基地位置。");
            }
            
            // 幸存者需求测试代码 (可选 - 取消注释以测试)
            if (survivorManager != null)
            {
                // ... (现有的幸存者需求测试代码可以保留，如果需要) ...
            }

            // ResearchLab 测试代码
            if (workstationSystem != null && survivorModel != null && workstationModel != null) 
            {
                bool labExists = false;
                foreach (var ws in workstationModel.GetAllWorkstations()) 
                {
                    if (ws.Type == WorkstationType.ResearchLab)
                    {
                        labExists = true;
                        break;
                    }
                }

                if (!labExists)
                {
                    Debug.Log("游戏初始化器：正在为测试构建研究实验室 (ResearchLab)。");
                    workstationSystem.BuildWorkstation(WorkstationType.ResearchLab);
                }

                var researchLab = workstationModel.GetAllWorkstations().Find(w => w.Type == WorkstationType.ResearchLab);
                if (researchLab != null && researchLab.AssignedSurvivorIds.Count == 0)
                {
                    var availableSurvivors = survivorModel.GetAvailableSurvivors(); 
                    if (availableSurvivors.Count > 0)
                    {
                        Debug.Log($"游戏初始化器：正在分配 {availableSurvivors[0].Name} 到研究实验室。");
                        workstationSystem.AssignSurvivorToWorkstation(availableSurvivors[0].Id, researchLab.Id);
                    }
                    else
                    {
                        Debug.Log("游戏初始化器：没有可用的幸存者分配到研究实验室。");
                    }
                }
                else if (researchLab != null && researchLab.AssignedSurvivorIds.Count > 0)
                {
                    Debug.Log("游戏初始化器：研究实验室已有分配的幸存者。");
                }
                // 此处移除了“未找到ResearchLab”的else情况，因为它可能刚被构建而引起混淆
            }
            
            Debug.Log("游戏初始化器：设置完成。昼夜系统 (DayNightSystem) 将处理初始僵尸潮和事件。已尝试研究实验室的测试设置。");

        }
    }
