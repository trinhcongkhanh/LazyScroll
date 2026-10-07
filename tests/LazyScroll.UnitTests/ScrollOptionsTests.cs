
using LazyScroll.Core.Models;

namespace LazyScroll.UnitTests;

public class ScrollOptionsTests
{
    [Fact]
    public void ScrollUp_ReturnsPositiveDelta()
    {
        var options = new ScrollOptions(
            TimeSpan.FromSeconds(5),
            3,
            true);

        Assert.Equal(360, options.WheelDelta);
    }

    [Fact]
    public void ScrollDown_ReturnsNegativeDelta()
    {
        var options = new ScrollOptions(
            TimeSpan.FromSeconds(5),
            2,
            false);

        Assert.Equal(-240, options.WheelDelta);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3601)]
    public void RejectsInvalidInterval(int seconds)
    {
        var options = new ScrollOptions(
            TimeSpan.FromSeconds(seconds),
            2,
            true);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => options.Validate());
    }
}
