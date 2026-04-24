/// <summary>
/// Implement on any system that needs to participate in the global production tick.
/// Register with ProductionManager instead of using Update() directly.
/// </summary>
public interface ITickable
{
    void OnTick(float deltaTime);
}
