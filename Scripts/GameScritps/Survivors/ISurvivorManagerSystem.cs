using System; // For Guid
using YourGameNamespace.Workstations; // For WorkstationType

namespace YourGameNamespace.Survivors
{
    public interface ISurvivorManagerSystem : ISystem
    {
        Survivor CreateNewSurvivor(string name, SurvivorAttributes attributes, SurvivorProfession profession);
        void UpdateSurvivorNeeds(float deltaTime);
        bool SurvivorTryEat(Guid survivorId, int foodToEat);
        void SurvivorSetResting(Guid survivorId, bool isResting);
        void AssignSurvivorToWork(Guid survivorId, Guid workstationId, WorkstationType workstationType);
        void ClearSurvivorWorkAssignment(Guid survivorId);
        void SetSurvivorOnExpeditionStatus(Guid survivorId, bool isOnExpedition);
        void UpdateSurvivorStatus(Guid survivorId, SurvivorStatus newStatus); // 通用状态更新
    }
}
