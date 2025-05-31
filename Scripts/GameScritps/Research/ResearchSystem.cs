using System;
using QFramework;
using UnityEngine; // 用于 Debug.Log 和 Time.deltaTime, Mathf.Clamp01
using YourGameNamespace.Events;   // 用于 TechnologyCompletedEvent, Model_TechnologyStatusUpdatedEvent
using YourGameNamespace.Workstations; // 用于 IWorkstationSystem 和 WorkstationType (在效果应用中)
using YourGameNamespace.Combat;     // 用于 ICombatSystem (在效果应用中)

namespace YourGameNamespace.Research
{
    // 研究系统，负责管理技术的研发过程和效果应用
    public class ResearchSystem : AbstractSystem, IResearchSystem, IController // 实现IController以便获取System
    {
        private ResearchModel mResearchModel; // 研究数据模型
        private ResourceModel mResourceModel; // 资源数据模型

        private Technology mCurrentResearch = null; // 当前正在研究的技术
        private float mCurrentResearchTimeAccumulated = 0f; // 当前研究已累积的时间（或理解为“研究点投入”）

        // 可观察的当前研究状态
        public BindableProperty<Technology> CurrentlyResearching { get; private set; }
        public BindableProperty<float> CurrentResearchProgressNormalized { get; private set; }

        public event Action<Technology> OnTechnologyStatusChanged; // 当技术状态（例如，从锁定变为可用）发生变化时触发的事件

        // IController 接口要求
        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        // 系统初始化
        protected override void OnInit()
        {
            mResearchModel = this.GetModel<ResearchModel>();
            mResourceModel = this.GetModel<ResourceModel>();

            CurrentlyResearching = new BindableProperty<Technology>(null);
            CurrentResearchProgressNormalized = new BindableProperty<float>(0f);

            if (mResearchModel == null) Debug.LogError("研究系统：未能找到研究数据模型 (ResearchModel)！初始化失败。");
            if (mResourceModel == null) Debug.LogError("研究系统：未能找到资源数据模型 (ResourceModel)！初始化失败。");

            // 注册监听来自ResearchModel的事件
            this.RegisterEvent<Model_TechnologyStatusUpdatedEvent>(OnModelTechnologyStatusUpdated);
        }

        private void OnModelTechnologyStatusUpdated(Model_TechnologyStatusUpdatedEvent e)
        {
            Technology tech = mResearchModel.GetTechnology(e.TechId); // 从Model获取更新后的Tech对象
            if (tech != null)
            {
                OnTechnologyStatusChanged?.Invoke(tech); // 触发自身的C#事件
            }
        }

        // 开始一项新的技术研究
        public bool StartResearch(string techId)
        {
            if (mCurrentResearch != null)
            {
                Debug.LogWarning($"无法开始研究技术 {techId}。因为 {mCurrentResearch.Name} 的研究已在进行中。");
                return false;
            }

            Technology techToResearch = mResearchModel.GetTechnology(techId);
            if (techToResearch == null)
            {
                Debug.LogError($"未能找到ID为 {techId} 的技术。");
                return false;
            }

            // 注意：techToResearch.Status 现在是 BindableProperty，需要访问 .Value
            if (techToResearch.Status.Value != ResearchStatus.Available)
            {
                Debug.LogWarning($"技术 {techToResearch.Name} (ID: {techId}) 当前不可研究。其状态为：{techToResearch.Status.Value}");
                return false;
            }

            if (!mResourceModel.ConsumeResource(GameResourceType.ResearchPoints, techToResearch.ResearchPointCost))
            {
                Debug.LogWarning($"研究点不足，无法开始研究 {techToResearch.Name}。需要研究点：{techToResearch.ResearchPointCost}，当前可用：{mResourceModel.GetAmount(GameResourceType.ResearchPoints)}");
                return false;
            }

            mCurrentResearch = techToResearch;
            mCurrentResearchTimeAccumulated = 0f;

            CurrentlyResearching.Value = mCurrentResearch; // 更新BindableProperty
            CurrentResearchProgressNormalized.Value = 0f;  // 更新BindableProperty

            // Model的UpdateTechnologyStatus方法现在会发送Model_TechnologyStatusUpdatedEvent
            mResearchModel.UpdateTechnologyStatus(techId, ResearchStatus.InProgress);
            Debug.Log($"开始研究：{mCurrentResearch.Name}。所需研究点/时间：{mCurrentResearch.ResearchPointCost}。");
            return true;
        }

        // 更新当前研究项目的进度
        public void UpdateResearchProcess(float deltaTime)
        {
            if (mCurrentResearch == null)
            {
                return;
            }

            mCurrentResearchTimeAccumulated += deltaTime;
            // 更新BindableProperty
            if(mCurrentResearch.ResearchPointCost > 0) // 避免除以零
                CurrentResearchProgressNormalized.Value = Mathf.Clamp01(mCurrentResearchTimeAccumulated / mCurrentResearch.ResearchPointCost);
            else
                CurrentResearchProgressNormalized.Value = 1f; // 如果成本为0，视为立即完成

            if (mCurrentResearchTimeAccumulated >= mCurrentResearch.ResearchPointCost)
            {
                CompleteResearch(mCurrentResearch.Id);
            }
        }
        
        // 完成指定ID的技术研究
        private void CompleteResearch(string techId)
        {
            Technology completedTech = mResearchModel.GetTechnology(techId);
            if (completedTech == null) return;

            Debug.LogWarning($"研究完成：{completedTech.Name}！");
            // Model的UpdateTechnologyStatus方法现在会发送Model_TechnologyStatusUpdatedEvent
            mResearchModel.UpdateTechnologyStatus(techId, ResearchStatus.Completed);

            Debug.Log($"正在应用技术 {completedTech.Name} 的效果：");

            // 获取System接口的引用
            var workstationSystem = this.GetSystem<IWorkstationSystem>();
            var combatSystem = this.GetSystem<ICombatSystem>();

            foreach (var effect in completedTech.Effects)
            {
                Debug.Log($"应用效果 -> 类型：{effect.EffectType}，值：{effect.Value}，目标工作站：{effect.TargetWorkstationType}，目标资源：{effect.TargetResource}");
                switch (effect.EffectType)
                {
                    case TechnologyEffectType.IncreaseProductionMultiplier:
                    case TechnologyEffectType.IncreaseProductionOutput:
                        if (workstationSystem != null)
                        {
                            // 调用WorkstationSystem的新接口方法 (假设已定义)
                            workstationSystem.ApplyResearchEffectToWorkstation(effect.TargetWorkstationType, effect.EffectType, effect.Value, effect.TargetResource);
                        }
                        else { Debug.LogError("研究系统：未能获取 IWorkstationSystem 实例！"); }
                        break;

                    case TechnologyEffectType.ModifySurvivorStat:
                        if (combatSystem != null)
                        {
                            // 调用CombatSystem的新接口方法 (假设已定义)
                            combatSystem.ApplyResearchEffectToCombat(effect.EffectType, effect.Value);
                        }
                        else { Debug.LogError("研究系统：未能获取 ICombatSystem 实例！"); }
                        break;

                    case TechnologyEffectType.UnlockWorkstation:
                        Debug.Log($"工作站类型 {(YourGameNamespace.Workstations.WorkstationType)effect.Value} 现在因完成研究 {completedTech.Name} 而被视为已解锁！实际解锁逻辑应由WorkstationSystem处理或通过事件通知。");
                        // 可选：发送一个更通用的事件，例如 WorkstationTypeUnlockedEvent，由 WorkstationSystem 监听并处理具体的解锁逻辑。
                        // this.SendEvent(new WorkstationTypeUnlockedEvent() { Type = (WorkstationType)effect.Value });
                        break;
            
                    default:
                        Debug.LogWarning($"技术 {completedTech.Name} 的效果类型 {effect.EffectType} 的应用逻辑尚未实现。");
                        break;
                }
            }

            mCurrentResearch = null;
            mCurrentResearchTimeAccumulated = 0f;

            CurrentlyResearching.Value = null; // 更新BindableProperty
            CurrentResearchProgressNormalized.Value = 0f; // 或 1f，取决于完成时的期望显示

            mResearchModel.UpdateAllTechnologyStatuses();
            
            this.SendEvent(new TechnologyCompletedEvent(completedTech.Id));
        }

        // 旧的 NotifyTechnologyStatusChanged 方法已移除，因为现在Model会发送事件，System监听这些事件。

        public Technology GetCurrentResearch()
        {
            return mCurrentResearch; // 或者 return CurrentlyResearching.Value;
        }

        // 这个方法可以被外部（如UI）直接访问BindableProperty替代
        // public float GetCurrentResearchProgressNormalized()
        // {
        //     return CurrentResearchProgressNormalized.Value;
        // }

        public bool IsResearching() => CurrentlyResearching.Value != null; // 通过BindableProperty判断
    }
}
