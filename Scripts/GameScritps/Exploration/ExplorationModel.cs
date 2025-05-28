using QFramework;
using System.Collections.Generic;
using System.Linq;

using System; // Guid 所需

namespace YourGameNamespace.Exploration
{
    public class ExplorationModel : AbstractModel
    {
        private Dictionary<string, ExplorationPointOfInterest> mPointsOfInterest = new Dictionary<string, ExplorationPointOfInterest>();
        private List<Expedition> mActiveExpeditions = new List<Expedition>();

        protected override void OnInit()
        {
        
            PopulateInitialPOIs();
        }

        private void PopulateInitialPOIs()
        {
            // 示例 POI 1：废弃超市
            var supermarketRewards = new List<POIReward>
            {
                new POIReward(GameResourceType.Food, 10, 30, 0.8f),
                new POIReward(GameResourceType.Medicine, 1, 5, 0.3f)
            };
            mPointsOfInterest.Add("POI_SUPERMARKET_1", new ExplorationPointOfInterest("POI_SUPERMARKET_1", "Abandoned Supermarket", "Looks like it might still have some canned goods.", 1, 60f, 2, supermarketRewards));

            // 示例 POI 2：旧警察局
            var policeStationRewards = new List<POIReward>
            {
                new POIReward(GameResourceType.Ammo, 20, 50, 0.7f),
                new POIReward(GameResourceType.Power, 0, 0, 0.1f) // 特殊物品占位符
            };
            mPointsOfInterest.Add("POI_POLICE_1", new ExplorationPointOfInterest("POI_POLICE_1", "Old Police Station", "Might find weapons, ammo, or other useful gear.", 3, 120f, 3, policeStationRewards));
            
            // 示例 POI 3：废弃图书馆（更侧重于研究点）
            var libraryRewards = new List<POIReward>
            {
                new POIReward(GameResourceType.ResearchPoints, 10, 25, 0.6f), // 有几率找到研究笔记
                new POIReward(GameResourceType.Food, 5, 10, 0.2f) // 一些剩余的零食
            };
            mPointsOfInterest.Add("POI_LIBRARY_1", new ExplorationPointOfInterest("POI_LIBRARY_1", "Derelict Library", "Dusty shelves might hold forgotten knowledge.", 2, 90f, 1, libraryRewards));

            // 新增 POI_RADIO_TOWER
            var radioTowerRewards = new List<POIReward>
            {
                new POIReward(GameResourceType.ResearchPoints, 5, 15, 0.5f),
                new POIReward(GameResourceType.ElectronicParts, 5, 10, 0.7f) 
            };
            mPointsOfInterest.Add("POI_RADIO_TOWER", new ExplorationPointOfInterest(
                "POI_RADIO_TOWER",
                "Old Radio Tower",
                "A dilapidated radio tower. Might be repairable or contain useful parts for communication.",
                2, // 难度
                75f, // 基础探索时间
                1,   // 最大幸存者槽位
                radioTowerRewards
            ));
        }

        public ExplorationPointOfInterest GetPOI(string poiId)
        {
            mPointsOfInterest.TryGetValue(poiId, out var poi);
            return poi;
        }

        public List<ExplorationPointOfInterest> GetAllPOIs()
        {
            return new List<ExplorationPointOfInterest>(mPointsOfInterest.Values);
        }

        public List<ExplorationPointOfInterest> GetAvailablePOIs()
        {
            // 当前未被探索且未枯竭的POI
            return mPointsOfInterest.Values.Where(p => p.Status == POIStatus.Unexplored || p.Status == POIStatus.Scouted || p.Status == POIStatus.Explored).ToList();
        }

        public void AddExpedition(Expedition expedition)
        {
            if (expedition != null && !mActiveExpeditions.Contains(expedition))
            {
                mActiveExpeditions.Add(expedition);
            }
        }

        public void RemoveExpedition(Expedition expedition)
        {
            if (expedition != null)
            {
                mActiveExpeditions.Remove(expedition);
            }
        }
        
        public Expedition GetExpedition(Guid expeditionId)
        {
            return mActiveExpeditions.FirstOrDefault(e => e.ExpeditionId == expeditionId);
        }


        public List<Expedition> GetActiveExpeditions()
        {
            return new List<Expedition>(mActiveExpeditions); // 返回副本
        }

        public void UpdatePOIStatus(string poiId, POIStatus newStatus)
        {
            if (mPointsOfInterest.TryGetValue(poiId, out var poi))
            {
                poi.Status = newStatus;
            }
        }
    }
}
