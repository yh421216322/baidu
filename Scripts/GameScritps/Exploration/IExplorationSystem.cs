using QFramework;
using System;
using System.Collections.Generic;

namespace YourGameNamespace.Exploration
{
    public interface IExplorationSystem : ISystem
    {
        bool CanStartExpeditionToPOI(string poiId, List<Guid> survivorIds, out string reason);
        bool StartExpedition(string poiId, List<Guid> survivorIds);
        void UpdateActiveExpeditions(float deltaTime);
        // Potentially other methods like ResolveExpeditionOutcome if they need to be called from outside,
        // but for now, it's private/internal to ExplorationSystem triggered by UpdateActiveExpeditions.
    }
}
