
using System.ComponentModel;
using System.Runtime.InteropServices;
using LazyScroll.Core.Interfaces;

namespace LazyScroll.Infrastructure.Windows;

public sealed class Win32ScrollExecutor : IScrollExecutor
{
    private const uint InputMouse = 0;
    private const uint MouseEventWheel = 0x0800;

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public MouseInput Mouse;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(
        uint inputCount,
        Input[] inputs,
        int inputSize);

    public void Scroll(int wheelDelta)
    {
        Input[] inputs =
        [
            new Input
            {
                Type = InputMouse,
                Mouse = new MouseInput
                {
                    MouseData = unchecked((uint)wheelDelta),
                    Flags = MouseEventWheel
                }
            }
        ];

        var sent = SendInput(
            1,
            inputs,
            Marshal.SizeOf<Input>());

        if (sent != 1)
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error());
        }
    }
}
