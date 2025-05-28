using QFramework;

namespace YourGameNamespace.Framework
{
    public class GameDataModel : AbstractModel
    {
        public int CurrentDay { get; set; } = 1;
        public float BaseHealth { get; set; } = 100f; // 使用 float 表示生命值

        protected override void OnInit()
        {
           
            CurrentDay = 1;
            BaseHealth = 100f;
        }
    }
}
