using QFramework;
using YourGameNamespace.Research; // For IResearchSystem etc.

namespace YourGameNamespace.Commands
{
    public class StartResearchCommand : AbstractCommand
    {
        private readonly string technologyId;

        public StartResearchCommand(string techId)
        {
            this.technologyId = techId;
        }

        protected override void OnExecute()
        {
            var researchSystem = this.GetSystem<IResearchSystem>();

            bool success = researchSystem.StartResearch(this.technologyId);

            if (success)
            {
                // 研究成功开始。
                // UI 层将通过监听 ResearchSystem 的 CurrentlyResearching BindableProperty
                // 或 ResearchModel 的 Model_TechnologyStatusUpdatedEvent 来更新界面。
                UnityEngine.Debug.Log($"命令：开始研究技术 {this.technologyId} 的指令已执行，成功状态: {success}");
            }
            else
            {
                // 研究开始失败 (原因由ResearchSystem内部的Debug.Log或UI通过其他方式反馈)
                UnityEngine.Debug.LogWarning($"命令：开始研究技术 {this.technologyId} 的指令执行失败或条件不满足。");
            }
            // 可选：如果需要，可以在此发送一个 StartResearchFailedEvent(reason)
            // this.SendEvent(new StartResearchFailedEvent(this.technologyId, "some reason from StartResearch's new return type?"));
        }
    }
}
