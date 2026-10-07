
namespace LazyScroll.Core.Interfaces;

public interface IMouseEventSource : IDisposable
{
    event EventHandler? MenuRequested;

    void Start();

    void Stop();
}
