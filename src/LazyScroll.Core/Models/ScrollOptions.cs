
namespace LazyScroll.Core.Models;

public sealed record ScrollOptions(
    TimeSpan Interval,
    int Notches,
    bool ScrollUp)
{
    public int WheelDelta =>
        (ScrollUp ? 1 : -1) * Notches * 120;

    public void Validate()
    {
        if (Interval < TimeSpan.FromSeconds(1) ||
            Interval > TimeSpan.FromHours(1))
        {
            throw new ArgumentOutOfRangeException(
                nameof(Interval));
        }

        if (Notches < 1 || Notches > 20)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Notches));
        }
    }
}
