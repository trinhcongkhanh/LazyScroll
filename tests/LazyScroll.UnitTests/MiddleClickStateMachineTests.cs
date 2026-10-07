
using LazyScroll.Core.Models;
using Xunit;

namespace LazyScroll.UnitTests;

public class MiddleClickStateMachineTests
{
    [Fact]
    public void NormalMiddleClick_PassesThrough()
    {
        var machine = new MiddleClickStateMachine();

        var down = machine.Process(
            MouseButtonEvent.MiddleDown, false);

        var up = machine.Process(
            MouseButtonEvent.MiddleUp, false);

        Assert.False(down.Consume);
        Assert.False(up.Consume);
        Assert.False(down.RequestMenu);
    }

    [Fact]
    public void ShiftMiddleClick_OpensMenu()
    {
        var machine = new MiddleClickStateMachine();

        var result = machine.Process(
            MouseButtonEvent.MiddleDown, true);

        Assert.True(result.Consume);
        Assert.True(result.RequestMenu);
    }

    [Fact]
    public void ReleaseIsConsumedEvenAfterShiftReleased()
    {
        var machine = new MiddleClickStateMachine();

        machine.Process(
            MouseButtonEvent.MiddleDown, true);

        var result = machine.Process(
            MouseButtonEvent.MiddleUp, false);

        Assert.True(result.Consume);
        Assert.False(result.RequestMenu);
    }

    [Fact]
    public void NextNormalClickIsNotBlocked()
    {
        var machine = new MiddleClickStateMachine();

        machine.Process(
            MouseButtonEvent.MiddleDown, true);

        machine.Process(
            MouseButtonEvent.MiddleUp, false);

        var next = machine.Process(
            MouseButtonEvent.MiddleDown, false);

        Assert.False(next.Consume);
    }

    [Fact]
    public void RepeatedDownDoesNotOpenAnotherMenu()
    {
        var machine = new MiddleClickStateMachine();

        machine.Process(
            MouseButtonEvent.MiddleDown, true);

        var repeated = machine.Process(
            MouseButtonEvent.MiddleDown, true);

        Assert.True(repeated.Consume);
        Assert.False(repeated.RequestMenu);
    }
}
