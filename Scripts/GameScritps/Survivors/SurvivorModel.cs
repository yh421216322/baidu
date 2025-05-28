using QFramework;
using System.Collections.Generic;
using System; // Guid 所需

namespace YourGameNamespace.Survivors
{
    public class SurvivorModel : AbstractModel
    {
        private List<Survivor> mSurvivors = new List<Survivor>();

        protected override void OnInit()
        {
      
            // 可选：添加一些默认幸存者用于测试：
            mSurvivors.Add(new Survivor("Bob", new SurvivorAttributes(5, 5, 5), SurvivorProfession.Unassigned));
            mSurvivors.Add(new Survivor("Alice", new SurvivorAttributes(3, 7, 6), SurvivorProfession.Farmer));
        }

        public void AddSurvivor(Survivor survivor)
        {
            if (survivor != null && !mSurvivors.Contains(survivor))
            {
                mSurvivors.Add(survivor);
            }
        }

        public Survivor GetSurvivorById(Guid id)
        {
            return mSurvivors.Find(s => s.Id == id);
        }

        public List<Survivor> GetAllSurvivors()
        {
            return new List<Survivor>(mSurvivors); // 返回一个新列表以防止外部修改
        }

        public List<Survivor> GetAvailableSurvivors()
        {
            return mSurvivors.FindAll(s => s.Status == SurvivorStatus.Idle);
        }
    }
}
