namespace YourGameNamespace.Survivors
{
    // 幸存者的当前状态枚举
    public enum SurvivorStatus
    {
        Idle,           // 空闲：幸存者当前没有特定任务，可以被分配新任务
        Working,        // 工作中：幸存者当前正在某个工作站工作或执行特定任务
        Resting,        // 休息中：幸存者正在恢复休息值
        Injured,        // 受伤：幸存者因战斗或事件受伤，可能无法工作或效率降低，需要治疗
        NeedsAttention, // 需要关注：幸存者的基本需求（如食物、休息）过低，需要玩家干预
        OnExpedition    // 远征中：幸存者当前正在参与一次外出远征 (已添加)
    }
}
