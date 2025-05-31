using System;
using QFramework;
using YourGameNamespace.Research; // For TechnologyEffectType
// GameResourceType is likely in YourGameNamespace, not YourGameNamespace.Workstations directly
// Adjust if GameResourceType is in a different specific sub-namespace like YourGameNamespace.Resources
using YourGameNamespace;


namespace YourGameNamespace.Workstations
{
    public interface IWorkstationSystem : ISystem
    {
        void ApplyResearchEffectToWorkstation(WorkstationType targetStationType, TechnologyEffectType effectType, float effectValue, GameResourceType targetAffectedResource);
        bool BuildWorkstation(WorkstationType type);
        void AssignSurvivorToWorkstation(Guid survivorId, Guid workstationId);
        void UnassignSurvivorFromWorkstation(Guid survivorId, Guid workstationId);
        void UpdateAllWorkstations(float deltaTime);
    }
}
