using MyGameNamespace;
using UnityEngine; // Ensure this is present
using YourGameNamespace;
using YourGameNamespace.Combat;
using YourGameNamespace.Survivors;
using YourGameNamespace.Workstations;

// 游戏初始化器，负责在游戏启动时进行必要的设置和初始化工作
public class GameInitializer : MonoBehaviour
    {
        public Transform baseMarker; // 用于在Inspector中拖拽一个场景中的对象来标记基地位置

        void Awake() // Unity生命周期方法，在对象实例化后立即调用
        {
            // 这行代码会调用 QFramework 框架中 Architecture<T> 类的 MakeSureArchitecture 方法，
            // 以确保 GameArchitecture 的单例存在，并随后调用 GameArchitecture 的 Init() 方法进行初始化。
            var architecture = RegisterManager.Interface;

            // --- 以下为 GameInitializer.Awake() 方法中的临时测试与设置代码 ---
            var workstationSystem = RegisterManager.Interface.GetSystem<WorkstationSystem>(); // 获取工作站系统
            var survivorModel = RegisterManager.Interface.GetModel<SurvivorModel>();         // 获取幸存者模型
            var workstationModel = RegisterManager.Interface.GetModel<WorkstationModel>();   // 获取工作站模型
            var combatSystem = RegisterManager.Interface.GetSystem<CombatSystem>();         // 获取战斗系统
            var survivorManager = RegisterManager.Interface.GetSystem<SurvivorManagerSystem>(); // 获取幸存者管理器系统


            if (workstationSystem != null && survivorModel != null && workstationModel != null)
            {
                // 如果当前没有任何工作站，则构建一组默认工作站
                if (workstationModel.GetAllWorkstations().Count == 0) 
                {
                     workstationSystem.BuildWorkstation(WorkstationType.Farm);       // 建造农场
                     workstationSystem.BuildWorkstation(WorkstationType.PowerPlant); // 建造发电厂
                     workstationSystem.BuildWorkstation(WorkstationType.Workshop);   // 建造工坊
                }

                // 如果有可用的幸存者且农场当前没有分配幸存者，则分配一个幸存者到农场
                var farm = workstationModel.GetAllWorkstations().Find(w => w.Type == WorkstationType.Farm);
                var availableSurvivors = survivorModel.GetAvailableSurvivors();
                if (farm != null && availableSurvivors.Count > 0 && farm.AssignedSurvivorIds.Count == 0)
                {
                    workstationSystem.AssignSurvivorToWorkstation(availableSurvivors[0].Id, farm.Id);
                }
                
                // 如果有可用的幸存者且工坊当前没有分配幸存者，则分配一个幸存者到工坊
                var workshop = workstationModel.GetAllWorkstations().Find(w => w.Type == WorkstationType.Workshop);
                availableSurvivors = survivorModel.GetAvailableSurvivors(); // 重新获取可用幸存者列表，因为第一个可能已被分配到农场
                if (workshop != null && availableSurvivors.Count > 0 && workshop.AssignedSurvivorIds.Count == 0)
                {
                    workstationSystem.AssignSurvivorToWorkstation(availableSurvivors[0].Id, workshop.Id);
                }
            }
            
            // 设置战斗系统 (CombatSystem) 的基地位置
            if (combatSystem != null)
            {
                if (baseMarker != null)
                {
                    combatSystem.BasePosition = baseMarker.position;
                    Debug.Log($"游戏初始化器：已从 baseMarker 设置战斗系统的基地位置为: {baseMarker.position}");
                }
                else
                {
                    combatSystem.BasePosition = Vector2.zero; // 保留默认值或设置一个明确的默认
                    Debug.LogWarning("游戏初始化器：场景中未配置 baseMarker Transform。战斗系统基地位置将使用默认值 (Vector2.zero)。");
                }
            }
            else
            {
                Debug.LogError("游戏初始化器：战斗系统 (CombatSystem) 为空！无法设置基地位置。");
            }
            
            // 幸存者需求相关的测试代码 (可选 - 如果需要测试特定需求逻辑，可以取消注释相关代码)
            if (survivorManager != null)
            {
                // ... (任何现有的幸存者需求测试代码可以保留在此处，如果需要的话) ...
            }

            // 研究实验室 (ResearchLab) 相关的测试与设置代码
            if (workstationSystem != null && survivorModel != null && workstationModel != null) 
            {
                bool labExists = false; // 标记研究实验室是否已存在
                foreach (var ws in workstationModel.GetAllWorkstations()) 
                {
                    if (ws.Type == WorkstationType.ResearchLab)
                    {
                        labExists = true;
                        break;
                    }
                }

                if (!labExists) // 如果研究实验室不存在
                {
                    Debug.Log("游戏初始化器：当前未发现研究实验室 (ResearchLab)，正在为测试目的构建一个新的研究实验室。");
                    workstationSystem.BuildWorkstation(WorkstationType.ResearchLab); // 构建研究实验室
                }

                var researchLab = workstationModel.GetAllWorkstations().Find(w => w.Type == WorkstationType.ResearchLab);
                if (researchLab != null && researchLab.AssignedSurvivorIds.Count == 0) // 如果研究实验室存在且没有分配幸存者
                {
                    var availableSurvivors = survivorModel.GetAvailableSurvivors(); // 获取可用的幸存者
                    if (availableSurvivors.Count > 0) // 如果有可用的幸存者
                    {
                        Debug.Log($"游戏初始化器：正在尝试分配幸存者 {availableSurvivors[0].Name} 到研究实验室。");
                        workstationSystem.AssignSurvivorToWorkstation(availableSurvivors[0].Id, researchLab.Id); // 分配第一个可用的幸存者
                    }
                    else
                    {
                        Debug.Log("游戏初始化器：没有可用的幸存者可以分配到研究实验室。");
                    }
                }
                else if (researchLab != null && researchLab.AssignedSurvivorIds.Count > 0) // 如果研究实验室已存在且已有幸存者
                {
                    Debug.Log("游戏初始化器：研究实验室已存在并且已有幸存者被分配。");
                }
                // 此处移除了“未找到ResearchLab”的else情况，因为它可能在上一段代码中刚被构建，记录日志可能会引起混淆。
            }
            
            Debug.Log("游戏初始化器：所有初始化设置已完成。昼夜系统 (DayNightSystem) 将负责处理初始的僵尸潮和随机事件。研究实验室相关的测试设置也已尝试执行。");
        }
    }
