using QFramework;
using System.Collections.Generic;
using System; // 用于 Guid (全局唯一标识符)

namespace YourGameNamespace.Workstations
{
    // 工作站数据模型，负责存储和管理游戏中所有已建造的工作站对象
    public class WorkstationModel : AbstractModel
    {
        // 存储所有工作站对象的列表
        private List<Workstation> mWorkstations = new List<Workstation>();

        // 模型初始化时调用
        protected override void OnInit()
        {
            // 可选：在此处添加一些默认的工作站用于游戏启动时的测试或初始设置。
            // 例如：
            // AddWorkstation(new Workstation(WorkstationType.Farm));       // 添加一个农场
            // AddWorkstation(new Workstation(WorkstationType.PowerPlant)); // 添加一个发电厂
            // 实际游戏中，工作站的创建通常由玩家操作或游戏逻辑（如GameInitializer）通过WorkstationSystem来完成。
        }

        // 添加一个新的工作站到模型中
        public void AddWorkstation(Workstation workstation)
        {
            // 确保工作站对象不为null，并且列表中不包含具有相同ID的工作站（避免重复添加）
            if (workstation != null && !mWorkstations.Exists(w => w.Id == workstation.Id))
            {
                mWorkstations.Add(workstation); // 将工作站添加到列表
            }
        }

        // 根据指定的ID获取工作站对象
        public Workstation GetWorkstationById(Guid id)
        {
            // 使用List的Find方法查找具有匹配ID的工作站
            return mWorkstations.Find(w => w.Id == id);
        }

        // 获取所有已建造工作站的列表
        public List<Workstation> GetAllWorkstations()
        {
            // 返回工作站列表的一个新副本，以防止外部代码直接修改内部列表，从而保证数据的封装性。
            return new List<Workstation>(mWorkstations); 
        }
    }
}
