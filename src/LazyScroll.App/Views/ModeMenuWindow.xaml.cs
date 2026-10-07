
using System.Windows;
using System.Windows.Input;

namespace LazyScroll.App.Views;

public partial class ModeMenuWindow : Window
{
    public event Action? NormalSelected;
    public event Action? TimerSelected;
    public event Action? StopSelected;

    public ModeMenuWindow()
    {
        InitializeComponent();
    }

    private void Normal_Click(
        object sender, RoutedEventArgs e)
    {
        NormalSelected?.Invoke();
    }

    private void Timer_Click(
        object sender, RoutedEventArgs e)
    {
        TimerSelected?.Invoke();
    }

    private void Stop_Click(
        object sender, RoutedEventArgs e)
    {
        StopSelected?.Invoke();
    }

    private void Window_KeyDown(
        object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
    }
}
