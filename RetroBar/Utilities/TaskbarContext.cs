using ManagedShell.AppBar;
using System.Windows;

namespace RetroBar.Utilities
{
    /// <summary>
    /// Settings that can differ per taskbar, read from the taskbar that hosts an element.
    /// Outside a taskbar (e.g. the Properties preview) the global settings apply.
    /// </summary>
    public static class TaskbarContext
    {
        // The first element among multi-binding values (e.g. a tooltip's PlacementTarget), or null
        public static DependencyObject FindElement(object[] values)
        {
            foreach (object value in values)
            {
                if (value is DependencyObject element)
                {
                    return element;
                }
            }

            return null;
        }

        public static Taskbar GetTaskbar(DependencyObject element)
        {
            return element == null ? null : Window.GetWindow(element) as Taskbar;
        }

        public static AppBarEdge GetEdge(DependencyObject element)
        {
            return GetTaskbar(element)?.AppBarEdge ?? Settings.Instance.Edge;
        }

        public static bool IsVertical(DependencyObject element)
        {
            return GetEdge(element) is AppBarEdge.Left or AppBarEdge.Right;
        }

        public static double GetScale(DependencyObject element)
        {
            return GetTaskbar(element)?.Scale ?? Settings.Instance.TaskbarScale;
        }
    }
}
