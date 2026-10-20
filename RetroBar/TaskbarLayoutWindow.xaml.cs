using ManagedShell.AppBar;
using ManagedShell.Common.Helpers;
using RetroBar.Utilities;
using System;
using System.ComponentModel;
using System.Windows;

namespace RetroBar
{
    /// <summary>
    /// Layout of the taskbars (location, size, scale, auto hide and visible items): for all of them together,
    /// or for each monitor when they are configured separately.
    /// </summary>
    public partial class TaskbarLayoutWindow : Window
    {
        private static TaskbarLayoutWindow _instance;

        private bool _isLoading;

        private TaskbarLayoutWindow(AppBarScreen screen)
        {
            InitializeComponent();

            spnRowCount.Maximum = Settings.Instance.RowLimit;
            spnTaskbarWidth.Maximum = Settings.Instance.TaskbarWidthLimit;

            LoadMonitors(screen);

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
            LoadValues();
        }

        public static void Open(Window owner, AppBarScreen screen, Point position)
        {
            if (_instance == null)
            {
                _instance = new TaskbarLayoutWindow(screen);
                _instance.Owner = owner;
                _instance.Left = position.X + 10;
                _instance.Top = position.Y + 10;
                _instance.Show();
            }
            else
            {
                _instance.Activate();
            }
        }

        private class MonitorItem
        {
            public string Id;
            public bool Primary;
            public string Label;
            public override string ToString() => Label;
        }

        private void LoadMonitors(AppBarScreen current)
        {
            int number = 0;
            MonitorItem selected = null;

            foreach (AppBarScreen screen in AppBarScreen.FromAllScreens())
            {
                number++;
                MonitorIdentifier.MonitorInfo info = MonitorIdentifier.GetMonitor(screen.DeviceName);
                string label = $"{number} - {info.Name}";

                if (screen.Primary)
                {
                    label += " " + (string)FindResource("taskbar_layout_primary_suffix");
                }

                MonitorItem item = new MonitorItem { Id = info.Id, Primary = screen.Primary, Label = label };
                cboMonitor.Items.Add(item);

                if (current != null ? screen.DeviceName == current.DeviceName : screen.Primary)
                {
                    selected = item;
                }
            }

            // The global settings, used by monitors that have no settings of their own.
            cboMonitor.Items.Add(new MonitorItem { Id = null, Label = (string)FindResource("taskbar_layout_default") });
            cboMonitor.SelectedItem = selected ?? cboMonitor.Items[0];
        }

        private MonitorItem SelectedMonitor => cboMonitor.SelectedItem as MonitorItem;

        // True when editing one monitor's own settings; false when editing the global settings.
        private bool IsSeparate => Settings.Instance.UseMonitorOverrides && SelectedMonitor?.Id != null;

        // The monitor whose values are shown (null = the global settings)
        private string ShownMonitorId => IsSeparate ? SelectedMonitor.Id : null;

        // Applies a change to the selected monitor's own settings, or to the global settings.
        private void Change(Action<MonitorOverride> perMonitor, Action global)
        {
            if (_isLoading)
            {
                return;
            }

            if (IsSeparate)
            {
                Settings.Instance.SetMonitorOverride(SelectedMonitor.Id, perMonitor);
            }
            else
            {
                global();
            }
        }

        private void LoadValues()
        {
            Settings settings = Settings.Instance;
            string id = ShownMonitorId;
            bool primary = !IsSeparate || SelectedMonitor.Primary;
            _isLoading = true;

            MonitorPanel.Visibility = settings.UseMonitorOverrides ? Visibility.Visible : Visibility.Collapsed;

            // Without an own entry, the getters return the global settings
            AppBarEdge edge = settings.GetEdge(id);
            bool horizontal = edge is AppBarEdge.Top or AppBarEdge.Bottom;
            cboEdge.SelectedIndex = (int)edge;
            RowsPanel.Visibility = horizontal ? Visibility.Visible : Visibility.Collapsed;
            WidthPanel.Visibility = horizontal ? Visibility.Collapsed : Visibility.Visible;
            spnRowCount.Value = settings.GetRowCount(id);
            spnTaskbarWidth.Value = settings.GetTaskbarWidth(id);
            spnTaskbarScale.Value = (int)Math.Round(settings.GetTaskbarScale(id) * 100);
            cbAutoHide.IsChecked = settings.GetAutoHide(id);
            cboTaskButtonsAlignment.SelectedIndex = (int)settings.GetTaskButtonsAlignment(id);
            cbStaggerTaskButtons.IsChecked = settings.GetStaggerTaskButtons(id);

            cbShowStartButton.IsChecked = IsSeparate ? settings.GetShowStartButton(id, primary) : settings.ShowStartButton;
            cbShowQuickLaunch.IsChecked = settings.GetShowQuickLaunch(id);
            cbQuickLaunchFillRowsFirst.IsChecked = settings.GetQuickLaunchFillRowsFirst(id);
            lblQuickLaunchFill.SetResourceReference(ContentProperty, horizontal ? "quick_launch_fill_rows_first" : "quick_launch_fill_columns_first");
            cboQuickLaunchPosition.SelectedIndex = (int)settings.GetQuickLaunchPosition(id);
            spnQuickLaunchIconSize.Value = settings.GetQuickLaunchIconSize(id);
            btnQuickLaunchFolder.ToolTip = settings.GetQuickLaunchPath(id);
            cbShowNotifyIcons.IsChecked = settings.GetShowNotifyIcons(id);
            cbShowClock.IsChecked = settings.GetShowClock(id);
            cbShowDesktopButton.IsChecked = settings.GetShowDesktopButton(id);
            cbShowInputLanguage.IsChecked = settings.GetShowInputLanguage(id);

            if (IsSeparate)
            {
                cbShowStartButtonMultiMon.Visibility = Visibility.Collapsed;
                // Start button on secondary taskbars is supported on Windows 10 and later only.
                cbShowStartButton.IsEnabled = primary || EnvironmentHelper.IsWindows10OrBetter;
            }
            else
            {
                cbShowStartButtonMultiMon.IsChecked = settings.ShowStartButtonMultiMon;
                cbShowStartButton.IsEnabled = true;
                // Start button on secondary taskbars is supported on Windows 10 and later only.
                cbShowStartButtonMultiMon.Visibility = settings.ShowMultiMon && EnvironmentHelper.IsWindows10OrBetter ? Visibility.Visible : Visibility.Collapsed;
                cbShowStartButtonMultiMon.IsEnabled = settings.ShowStartButton;
            }

            _isLoading = false;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.RowCount):
                case nameof(Settings.TaskbarWidth):
                case nameof(Settings.TaskbarScale):
                case nameof(Settings.AutoHide):
                case nameof(Settings.Edge):
                case nameof(Settings.ShowStartButton):
                case nameof(Settings.ShowStartButtonMultiMon):
                case nameof(Settings.ShowQuickLaunch):
                case nameof(Settings.QuickLaunchPosition):
                case nameof(Settings.QuickLaunchIconSize):
                case nameof(Settings.QuickLaunchPath):
                case nameof(Settings.QuickLaunchFillRowsFirst):
                case nameof(Settings.TaskButtonsAlignment):
                case nameof(Settings.StaggerTaskButtons):
                case nameof(Settings.ShowNotifyIcons):
                case nameof(Settings.ShowClock):
                case nameof(Settings.ShowDesktopButton):
                case nameof(Settings.ShowInputLanguage):
                case nameof(Settings.ShowMultiMon):
                case nameof(Settings.ConfigureTaskbarsSeparately):
                case nameof(Settings.MonitorOverrides):
                    // Also covers changes made by dragging the taskbar
                    LoadValues();
                    break;
            }
        }

        private void CboMonitor_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_isLoading && IsLoaded)
            {
                LoadValues();
            }
        }

        private void CboEdge_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cboEdge.SelectedIndex < 0)
            {
                return;
            }

            AppBarEdge edge = (AppBarEdge)cboEdge.SelectedIndex;
            Change(o => o.Edge = edge, () => Settings.Instance.Edge = edge);
        }

        private void CboQuickLaunchPosition_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isLoading || cboQuickLaunchPosition.SelectedIndex < 0)
            {
                return;
            }

            Settings settings = Settings.Instance;
            QuickLaunchPositionOption position = (QuickLaunchPositionOption)cboQuickLaunchPosition.SelectedIndex;
            bool ownRow = position is QuickLaunchPositionOption.OwnRowScreenEdge or QuickLaunchPositionOption.OwnRowScreenCenter;
            bool horizontal = settings.GetEdge(ShownMonitorId) is AppBarEdge.Top or AppBarEdge.Bottom;

            // An own row needs a second row (or a second width unit on vertical taskbars): add it, so the constraint is visible
            Change(o =>
            {
                o.QuickLaunchPosition = position;

                if (ownRow && horizontal && (o.RowCount ?? settings.RowCount) < 2)
                {
                    o.RowCount = 2;
                }
                else if (ownRow && !horizontal && (o.TaskbarWidth ?? settings.TaskbarWidth) < 2)
                {
                    o.TaskbarWidth = 2;
                }
            }, () =>
            {
                if (ownRow && horizontal && settings.RowCount < 2)
                {
                    settings.RowCount = 2;
                }
                else if (ownRow && !horizontal && settings.TaskbarWidth < 2)
                {
                    settings.TaskbarWidth = 2;
                }

                settings.QuickLaunchPosition = position;
            });
        }

        private void CboTaskButtonsAlignment_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cboTaskButtonsAlignment.SelectedIndex < 0)
            {
                return;
            }

            TaskButtonsAlignmentOption alignment = (TaskButtonsAlignmentOption)cboTaskButtonsAlignment.SelectedIndex;
            Change(o => o.TaskButtonsAlignment = alignment, () => Settings.Instance.TaskButtonsAlignment = alignment);
        }

        private void SpnRowCount_ValueChangedByUser(object sender, EventArgs e)
        {
            int rows = spnRowCount.Value;
            Change(o => o.RowCount = rows, () => Settings.Instance.RowCount = rows);
        }

        private void SpnTaskbarWidth_ValueChangedByUser(object sender, EventArgs e)
        {
            int width = spnTaskbarWidth.Value;
            Change(o => o.TaskbarWidth = width, () => Settings.Instance.TaskbarWidth = width);
        }

        private void SpnTaskbarScale_ValueChangedByUser(object sender, EventArgs e)
        {
            double scale = spnTaskbarScale.Value / 100.0;
            Change(o => o.TaskbarScale = scale, () => Settings.Instance.TaskbarScale = scale);
        }

        private void SpnQuickLaunchIconSize_ValueChangedByUser(object sender, EventArgs e)
        {
            int size = spnQuickLaunchIconSize.Value;
            Change(o => o.QuickLaunchIconSize = size, () => Settings.Instance.QuickLaunchIconSize = size);
        }

        private void QuickLaunchFolder_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = (string)FindResource("quick_launch_folder");
#if NETCOREAPP3_0_OR_GREATER
            fbd.UseDescriptionForTitle = true;
#endif
            fbd.ShowNewFolderButton = false;
            fbd.SelectedPath = Environment.ExpandEnvironmentVariables(Settings.Instance.GetQuickLaunchPath(ShownMonitorId));

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                Change(o => o.QuickLaunchPath = path, () => Settings.Instance.QuickLaunchPath = path);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool value = (sender as System.Windows.Controls.CheckBox)?.IsChecked == true;
            Settings settings = Settings.Instance;

            if (sender == cbShowStartButton) Change(o => o.ShowStartButton = value, () => settings.ShowStartButton = value);
            else if (sender == cbShowStartButtonMultiMon) Change(o => { }, () => settings.ShowStartButtonMultiMon = value);
            else if (sender == cbShowQuickLaunch) Change(o => o.ShowQuickLaunch = value, () => settings.ShowQuickLaunch = value);
            else if (sender == cbQuickLaunchFillRowsFirst) Change(o => o.QuickLaunchFillRowsFirst = value, () => settings.QuickLaunchFillRowsFirst = value);
            else if (sender == cbShowNotifyIcons) Change(o => o.ShowNotifyIcons = value, () => settings.ShowNotifyIcons = value);
            else if (sender == cbShowClock) Change(o => o.ShowClock = value, () => settings.ShowClock = value);
            else if (sender == cbShowDesktopButton) Change(o => o.ShowDesktopButton = value, () => settings.ShowDesktopButton = value);
            else if (sender == cbShowInputLanguage) Change(o => o.ShowInputLanguage = value, () => settings.ShowInputLanguage = value);
            else if (sender == cbAutoHide) Change(o => o.AutoHide = value, () => settings.AutoHide = value);
            else if (sender == cbStaggerTaskButtons) Change(o => o.StaggerTaskButtons = value, () => settings.StaggerTaskButtons = value);
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Instance.PropertyChanged -= Settings_PropertyChanged;
            _instance = null;
        }
    }
}
