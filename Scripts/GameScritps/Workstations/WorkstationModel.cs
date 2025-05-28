using QFramework;
using System.Collections.Generic;
using System; // 用于 Guid

namespace YourGameNamespace.Workstations
{
    public class WorkstationModel : AbstractModel
    {
        private List<Workstation> mWorkstations = new List<Workstation>();

        protected override void OnInit()
        {
          
            // 可选：添加默认工作站用于测试
            // AddWorkstation(new Workstation(WorkstationType.Farm));
            // AddWorkstation(new Workstation(WorkstationType.PowerPlant));
        }

        public void AddWorkstation(Workstation workstation)
        {
            if (workstation != null && !mWorkstations.Exists(w => w.Id == workstation.Id))
            {
                mWorkstations.Add(workstation);
            }
        }

        public Workstation GetWorkstationById(Guid id)
        {
            return mWorkstations.Find(w => w.Id == id);
        }

        public List<Workstation> GetAllWorkstations()
        {
            return new List<Workstation>(mWorkstations); // 返回一个新列表以防止外部修改
        }
    }
}
