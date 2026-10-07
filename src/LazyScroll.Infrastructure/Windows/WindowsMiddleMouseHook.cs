
using System.ComponentModel;
using System.Runtime.InteropServices;
using LazyScroll.Core.Interfaces;
using LazyScroll.Core.Models;

namespace LazyScroll.Infrastructure.Windows;

public sealed class WindowsMiddleMouseHook
    : IMouseEventSource
{
    private const int WhMouseLl = 14;
    private const long WmMiddleDown = 0x0207;
    private const long WmMiddleUp = 0x0208;
    private const int VkShift = 0x10;

    private readonly SynchronizationContext _context;
    private readonly HookProc _callback;
    private readonly MiddleClickStateMachine _state = new();

    private nint _handle;
    private bool _disposed;

    public event EventHandler? MenuRequested;

    public WindowsMiddleMouseHook(
        SynchronizationContext context)
    {
        _context = context;
        _callback = HandleMouse;
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_handle != 0)
            return;

        nint module = GetModuleHandle(null);

        _handle = SetWindowsHookEx(
            WhMouseLl,
            _callback,
            module,
            0);

        if (_handle == 0)
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error());
        }
    }

    private nint HandleMouse(
        int code,
        nint message,
        nint data)
    {
        if (code < 0)
            return CallNextHookEx(
                _handle, code, message, data);

        MouseButtonEvent input =
            message.ToInt64() switch
            {
                WmMiddleDown => MouseButtonEvent.MiddleDown,
                WmMiddleUp => MouseButtonEvent.MiddleUp,
                _ => MouseButtonEvent.Other
            };

        if (input != MouseButtonEvent.Other)
        {
            bool shiftPressed =
                (GetAsyncKeyState(VkShift) & 0x8000) != 0;

            var decision = _state.Process(
                input, shiftPressed);

            if (decision.RequestMenu)
            {
                // Chuyển công việc sang UI message queue.
                _context.Post(
                    _ => MenuRequested?.Invoke(
                        this, EventArgs.Empty),
                    null);
            }

            if (decision.Consume)
                return 1;
        }

        return CallNextHookEx(
            _handle, code, message, data);
    }

    public void Stop()
    {
        if (_handle == 0)
            return;

        if (!UnhookWindowsHookEx(_handle))
            throw new Win32Exception(
                Marshal.GetLastWin32Error());

        _handle = 0;
        _state.Reset();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private delegate nint HookProc(
        int code,
        nint message,
        nint data);

    [DllImport(
        "user32.dll",
        EntryPoint = "SetWindowsHookExW",
        SetLastError = true)]
    private static extern nint SetWindowsHookEx(
        int idHook,
        HookProc callback,
        nint module,
        uint threadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(
        nint handle);

    [DllImport("user32.dll")]
    private static extern nint CallNextHookEx(
        nint handle,
        int code,
        nint message,
        nint data);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(
        int key);

    [DllImport(
        "kernel32.dll",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern nint GetModuleHandle(
        string? moduleName);
}
