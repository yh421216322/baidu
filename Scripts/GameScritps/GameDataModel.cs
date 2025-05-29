using QFramework;

namespace YourGameNamespace.Framework
{
    // 核心游戏数据模型，存储游戏全局状态信息
    public class GameDataModel : AbstractModel
    {
        // 当前游戏进行到的天数，默认为第1天
        public int CurrentDay { get; set; } = 1;
        // 玩家基地的当前健康值，使用浮点数表示以便更精确地处理伤害
        public float BaseHealth { get; set; } = 100f; 

        // 模型初始化时调用
        protected override void OnInit()
        {
            // 设置初始游戏天数为1
            CurrentDay = 1;
            // 设置初始基地健康值为100
            BaseHealth = 100f;
        }
    }
}
