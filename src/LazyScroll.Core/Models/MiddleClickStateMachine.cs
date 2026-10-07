
namespace LazyScroll.Core.Models;

public enum MouseButtonEvent
{
    Other,
    MiddleDown,
    MiddleUp
}

public readonly record struct MouseHookDecision(
    bool Consume,
    bool RequestMenu
);

public sealed class MiddleClickStateMachine
{
    private bool _waitingForRelease;

    public MouseHookDecision Process(
        MouseButtonEvent input,
        bool shiftPressed)
    {
        // Đang chờ nút giữa được thả.
        if (_waitingForRelease)
        {
            if (input == MouseButtonEvent.MiddleUp)
            {
                _waitingForRelease = false;
                return new(true, false);
            }

            if (input == MouseButtonEvent.MiddleDown)
                return new(true, false);
        }

        // Shift + Middle Down -> mở menu.
        if (input == MouseButtonEvent.MiddleDown
            && shiftPressed)
        {
            _waitingForRelease = true;
            return new(true, true);
        }

        // Các input còn lại được giữ nguyên.
        return new(false, false);
    }

    public void Reset()
    {
        _waitingForRelease = false;
    }
}
