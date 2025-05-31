using QFramework;
using System.Collections.Generic;
using System; // 用于 Guid (全局唯一标识符)
using YourGameNamespace.Events; // 用于 Model_SurvivorAddedEvent

namespace YourGameNamespace.Survivors
{
    // 幸存者数据模型，负责存储和管理所有幸存者对象
    public class SurvivorModel : AbstractModel
    {
        // 存储所有幸存者对象的列表
        private List<Survivor> mSurvivors = new List<Survivor>();

        // 模型初始化时调用
        protected override void OnInit()
        {
            // 可选：在此处添加一些默认的幸存者用于游戏测试或初始设置。
            // 例如：
            // mSurvivors.Add(new Survivor("鲍勃", new SurvivorAttributes(5, 5, 5), SurvivorProfession.Unassigned)); // "鲍勃" 为示例名称
            // mSurvivors.Add(new Survivor("爱丽丝", new SurvivorAttributes(3, 7, 6), SurvivorProfession.Farmer));   // "爱丽丝" 为示例名称
            // 注意：实际游戏中，幸存者的创建可能由其他系统（如事件系统、任务奖励、或玩家操作）触发。
            // 为了与现有代码行为一致，保留原始的英文名添加：
            // 在OnInit中添加默认幸存者时，也应发送事件，如果这是期望行为。
            // 为保持与AddSurvivor方法的行为一致，此处也添加事件发送。
            Survivor bob = new Survivor("Bob", new SurvivorAttributes(5, 5, 5), SurvivorProfession.Unassigned);
            mSurvivors.Add(bob);
            this.SendEvent(new Model_SurvivorAddedEvent() { SurvivorData = bob });

            Survivor alice = new Survivor("Alice", new SurvivorAttributes(3, 7, 6), SurvivorProfession.Farmer);
            mSurvivors.Add(alice);
            this.SendEvent(new Model_SurvivorAddedEvent() { SurvivorData = alice });
        }

        // 添加一个新的幸存者到模型中
        public void AddSurvivor(Survivor newSurvivor) // 参数名改为 newSurvivor 以匹配事件中的使用
        {
            // 确保幸存者对象不为null且列表中不包含此幸存者（避免重复添加）
            if (newSurvivor != null && !mSurvivors.Contains(newSurvivor))
            {
                mSurvivors.Add(newSurvivor);
                // 发送幸存者添加事件
                this.SendEvent(new Model_SurvivorAddedEvent() { SurvivorData = newSurvivor });
            }
        }

        // 根据指定的ID获取幸存者对象
        public Survivor GetSurvivorById(Guid id)
        {
            // 使用List的Find方法查找具有匹配ID的幸存者
            return mSurvivors.Find(s => s.Id == id);
        }

        // 获取所有幸存者的列表
        public List<Survivor> GetAllSurvivors()
        {
            // 返回幸存者列表的一个新副本，以防止外部代码直接修改内部列表，从而保证数据的封装性。
            return new List<Survivor>(mSurvivors);
        }

        // 获取所有当前状态为空闲 (Idle) 的幸存者列表
        public List<Survivor> GetAvailableSurvivors()
        {
            // 使用List的FindAll方法筛选出状态为SurvivorStatus.Idle的幸存者
            return mSurvivors.FindAll(s => s.Status == SurvivorStatus.Idle);
        }
    }
}
