using QFramework;

namespace YourGameNamespace.Events
{
    public abstract class RandomEvent
    {
        public string Title { get; protected set; }
        public string Description { get; protected set; }
        public abstract void Execute(IArchitecture architecture);
    }
}
