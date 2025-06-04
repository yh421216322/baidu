using QFramework;
using YourGameNamespace.Research; // For Technology

namespace YourGameNamespace.Research
{
    public interface IResearchSystem : ISystem
    {
        BindableProperty<Technology> CurrentlyResearching { get; }
        BindableProperty<float> CurrentResearchProgressNormalized { get; }

        bool StartResearch(string techId);
        void UpdateResearchProcess(float deltaTime); // If driven externally
        bool IsResearching();
        Technology GetTechnologyById(string techId); // Added for TechDisplayItem prerequisite display
        string GetTechnologyNameById(string techId); // Added for convenience
        // Technology GetCurrentResearch(); // Can be replaced by CurrentlyResearching.Value
    }
}
