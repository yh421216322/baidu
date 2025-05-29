using QFramework;
using System.Collections.Generic;
using System.Linq;

using System; // 用于 Guid

namespace YourGameNamespace.Exploration
{
    // 探索模型，存储所有兴趣点(POI)和当前活动的远征信息
    public class ExplorationModel : AbstractModel
    {
        // 存储所有POI的字典，键为POI的ID
        private Dictionary<string, ExplorationPointOfInterest> mPointsOfInterest = new Dictionary<string, ExplorationPointOfInterest>();
        // 当前正在进行的远征列表
        private List<Expedition> mActiveExpeditions = new List<Expedition>();

        protected override void OnInit()
        {
            // 初始化时，填充预定义的兴趣点
            PopulateInitialPOIs();
        }

        // 填充初始的兴趣点数据
        private void PopulateInitialPOIs()
        {
            // 示例 POI 1：废弃超市
            var supermarketRewards = new List<POIReward> // 超市的潜在奖励
            {
                new POIReward(GameResourceType.Food, 10, 30, 0.8f), // 80%概率找到10-30单位食物
                new POIReward(GameResourceType.Medicine, 1, 5, 0.3f)  // 30%概率找到1-5单位药品
            };
            mPointsOfInterest.Add("POI_SUPERMARKET_1", new ExplorationPointOfInterest("POI_SUPERMARKET_1", "废弃超市", "看起来可能还有一些罐头食品。", 1, 60f, 2, supermarketRewards));

            // 示例 POI 2：旧警察局
            var policeStationRewards = new List<POIReward> // 警察局的潜在奖励
            {
                new POIReward(GameResourceType.Ammo, 20, 50, 0.7f),  // 70%概率找到20-50单位弹药
                new POIReward(GameResourceType.Power, 0, 0, 0.1f)   // 10%概率找到特殊物品（此处用电力作为占位符，实际应为特定物品ID或类型）
            };
            mPointsOfInterest.Add("POI_POLICE_1", new ExplorationPointOfInterest("POI_POLICE_1", "旧警察局", "可能会找到武器、弹药或其他有用的装备。", 3, 120f, 3, policeStationRewards));
            
            // 示例 POI 3：废弃图书馆（更侧重于研究点）
            var libraryRewards = new List<POIReward> // 图书馆的潜在奖励
            {
                new POIReward(GameResourceType.ResearchPoints, 10, 25, 0.6f), // 60%概率找到10-25研究点（可能代表研究笔记）
                new POIReward(GameResourceType.Food, 5, 10, 0.2f)             // 20%概率找到一些剩余的零食
            };
            mPointsOfInterest.Add("POI_LIBRARY_1", new ExplorationPointOfInterest("POI_LIBRARY_1", "废弃图书馆", "布满灰尘的书架上可能藏有被遗忘的知识。", 2, 90f, 1, libraryRewards));

            // 新增 POI：旧无线电塔
            var radioTowerRewards = new List<POIReward> // 无线电塔的潜在奖励
            {
                new POIReward(GameResourceType.ResearchPoints, 5, 15, 0.5f),   // 50%概率找到5-15研究点
                new POIReward(GameResourceType.ElectronicParts, 5, 10, 0.7f) // 70%概率找到5-10电子零件
            };
            mPointsOfInterest.Add("POI_RADIO_TOWER", new ExplorationPointOfInterest(
                "POI_RADIO_TOWER", // ID
                "旧无线电塔",        // 名称
                "一座摇摇欲坠的无线电塔。或许可以修复，或者包含有用的通讯部件。", // 描述
                2,   // 难度
                75f, // 基础探索时间 (秒)
                1,   // 最大幸存者槽位
                radioTowerRewards
            ));
        }

        // 根据ID获取POI信息
        public ExplorationPointOfInterest GetPOI(string poiId)
        {
            mPointsOfInterest.TryGetValue(poiId, out var poi);
            return poi;
        }

        // 获取所有POI的列表
        public List<ExplorationPointOfInterest> GetAllPOIs()
        {
            return new List<ExplorationPointOfInterest>(mPointsOfInterest.Values); // 返回值的副本以防止外部修改
        }

        // 获取所有当前可供探索的POI列表
        public List<ExplorationPointOfInterest> GetAvailablePOIs()
        {
            // 可用POI定义为：尚未被探索、或已被侦察、或已被探索但可能重置（当前逻辑简单化，已探索也算可用）
            return mPointsOfInterest.Values.Where(p => p.Status == POIStatus.Unexplored || p.Status == POIStatus.Scouted || p.Status == POIStatus.Explored).ToList();
        }

        // 添加一个新的远征到活动列表
        public void AddExpedition(Expedition expedition)
        {
            if (expedition != null && !mActiveExpeditions.Contains(expedition)) // 确保不为空且未重复添加
            {
                mActiveExpeditions.Add(expedition);
            }
        }

        // 从活动列表中移除一个远征（通常在远征完成或失败后）
        public void RemoveExpedition(Expedition expedition)
        {
            if (expedition != null)
            {
                mActiveExpeditions.Remove(expedition);
            }
        }
        
        // 根据远征ID获取活动中的远征信息
        public Expedition GetExpedition(Guid expeditionId)
        {
            return mActiveExpeditions.FirstOrDefault(e => e.ExpeditionId == expeditionId);
        }

        // 获取所有当前活动远征的列表
        public List<Expedition> GetActiveExpeditions()
        {
            return new List<Expedition>(mActiveExpeditions); // 返回列表的副本以防止外部修改
        }

        // 更新指定ID的POI的状态
        public void UpdatePOIStatus(string poiId, POIStatus newStatus)
        {
            if (mPointsOfInterest.TryGetValue(poiId, out var poi))
            {
                poi.Status = newStatus;
            }
        }
    }
}
