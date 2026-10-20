using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.ShellFolders;
using ManagedShell.ShellFolders.Enums;
using RetroBar.Utilities;

namespace RetroBar.Controls
{
    /// <summary>
    /// Interaction logic for Toolbar.xaml
    /// </summary>
    public partial class Toolbar : UserControl
    {
        private bool _ignoreNextUpdate;
        private bool _isLoaded;

        private enum MenuItem : uint
        {
            OpenParentFolder = CommonContextMenuItem.Paste + 1
        }

        public static DependencyProperty PathProperty = DependencyProperty.Register(nameof(Path), typeof(string), typeof(Toolbar), new PropertyMetadata(OnPathChanged));

        public string Path
        {
            get => (string)GetValue(PathProperty);
            // OnPathChanged sets up the folder (doing it here too would create it twice)
            set => SetValue(PathProperty, value);
        }

        private static DependencyProperty FolderProperty = DependencyProperty.Register(nameof(Folder), typeof(ShellFolder), typeof(Toolbar));

        // True when the toolbar has a row of its own: its icons then stay on one line.
        public static DependencyProperty SingleRowProperty = DependencyProperty.Register(nameof(SingleRow), typeof(bool), typeof(Toolbar), new PropertyMetadata(false));

        public bool SingleRow
        {
            get => (bool)GetValue(SingleRowProperty);
            set => SetValue(SingleRowProperty, value);
        }

        // Fill the other way than the default: rows first on horizontal taskbars, columns first on vertical ones.
        public static DependencyProperty FillAcrossProperty = DependencyProperty.Register(nameof(FillAcross), typeof(bool), typeof(Toolbar), new PropertyMetadata(false));

        public bool FillAcross
        {
            get => (bool)GetValue(FillAcrossProperty);
            set => SetValue(FillAcrossProperty, value);
        }

        // Size in pixels of the icons on this toolbar.
        public static DependencyProperty IconSizeProperty = DependencyProperty.Register(nameof(IconSize), typeof(int), typeof(Toolbar), new PropertyMetadata(ToolbarButton.DefaultIconSize));

        public int IconSize
        {
            get => (int)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public static DependencyProperty HostProperty = DependencyProperty.Register(nameof(Host), typeof(Taskbar), typeof(Toolbar), new PropertyMetadata(HostChangedCallback));

        public Taskbar Host
        {
            get { return (Taskbar)GetValue(HostProperty); }
            set { SetValue(HostProperty, value); }
        }

        public ToolbarDropHandler DropHandler { get; set; }

        private ShellFolder Folder
        {
            get => (ShellFolder)GetValue(FolderProperty);
            set
            {
                SetValue(FolderProperty, value);
                SetItemsSource();
            }
        }

        public Toolbar()
        {
            DropHandler = new ToolbarDropHandler(this);

            InitializeComponent();
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.QuickLaunchOrder))
            {
                if (_ignoreNextUpdate)
                {
                    _ignoreNextUpdate = false;
                    return;
                }

                Refresh();
            }
            else if (e.PropertyName == nameof(Settings.TaskbarScale))
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (Folder == null)
            {
                return;
            }

            ListCollectionView cvs = (ListCollectionView)CollectionViewSource.GetDefaultView(Folder.Files);
            cvs.Refresh();
        }

        private void SetupFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                // The taskbar sets the folder (it can differ per taskbar)
                UnloadFolder();
                return;
            }

            Folder?.Dispose();
            Folder = new ShellFolder(Environment.ExpandEnvironmentVariables(path), IntPtr.Zero, true);
        }

        private void UnloadFolder()
        {
            Folder?.Dispose();
            Folder = null;
        }

        private void SetItemsSource()
        {
            if (Folder != null)
            {
                ToolbarItems.ItemsSource = Folder.Files;
                ListCollectionView cvs = (ListCollectionView)CollectionViewSource.GetDefaultView(Folder.Files);
                cvs.CustomSort = new ToolbarSorter(this);
            }
        }

        // Whether a saved Quick Launch path belongs to this toolbar's folder (taskbars can use different folders)
        private bool IsInThisFolder(string itemPath)
        {
            try
            {
                string folder = System.IO.Path.GetFullPath(Environment.ExpandEnvironmentVariables(Path ?? "")).TrimEnd('\\');
                string itemFolder = System.IO.Path.GetDirectoryName(itemPath)?.TrimEnd('\\');
                return string.Equals(folder, itemFolder, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        // The saved order of the items of this toolbar's folder (a copy)
        public List<string> GetDesiredOrder()
        {
            return Settings.Instance.QuickLaunchOrder.Where(IsInThisFolder).ToList();
        }

        public void SaveItemOrder()
        {
            // Keep the order saved for other folders
            List<string> itemPaths = Settings.Instance.QuickLaunchOrder.Where(p => !IsInThisFolder(p)).ToList();

            foreach (ShellFile file in ((ListCollectionView)CollectionViewSource.GetDefaultView(Folder.Files)).OfType<ShellFile>())
            {
                itemPaths.Add(file.Path);
            }

            // small optimization, only other toolbars with this folder need to reload when the setting is saved.
            _ignoreNextUpdate = true;

            Settings.Instance.QuickLaunchOrder = itemPaths;
        }

        public void AddToSource(StringCollection filesToAdd)
        {
            string sourcePath = Environment.ExpandEnvironmentVariables(Path);

            foreach (string itemPath in filesToAdd)
            {
                // Create shortcut to each dragged file
                try
                {
                    string destinationFileName = System.IO.Path.GetFileNameWithoutExtension(itemPath);
                    string destinationPath = System.IO.Path.Combine(sourcePath, destinationFileName + ".lnk");
                    int dupCount = 0;

                    while (ShellHelper.Exists(destinationPath))
                    {
                        dupCount++;

                        destinationPath = System.IO.Path.Combine(sourcePath, $"{destinationFileName} ({dupCount}).lnk");
                    }

                    ShellLinkHelper.CreateAndSave(itemPath, destinationPath);
                }
                catch (Exception e)
                {
                    ShellLogger.Error($"Toolbar: Unable to save shortcut to {itemPath}", e);
                }
            }
        }

        #region Events
        private static void OnPathChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Toolbar toolbar)
            {
                toolbar.SetupFolder((string)e.NewValue);
            }
        }

        private void ToolbarIcon_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToolbarButton icon = sender as ToolbarButton;
            if (icon == null)
            {
                return;
            }

            Mouse.Capture(null);
            ShellFile file = icon.DataContext as ShellFile;

            if (file == null || string.IsNullOrWhiteSpace(file.Path))
            {
                return;
            }

            if (InvokeContextMenu(file, false))
            {
                e.Handled = true;
            }
        }

        private void ToolbarIcon_OnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToolbarButton icon = sender as ToolbarButton;
            if (icon == null)
            {
                return;
            }
            
            ShellFile file = icon.DataContext as ShellFile;

            if (InvokeContextMenu(file, true))
            {
                e.Handled = true;
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool visible)
            {
                if (visible)
                {
                    if (Folder != null)
                    {
                        return;
                    }

                    SetupFolder(Path);
                }
                else
                {
                    UnloadFolder();
                }
            }
        }

        private void Toolbar_TaskbarHotkeyPressed(object sender, HotkeyManager.TaskbarHotkeyEventArgs e)
        {
            if (Settings.Instance.WinNumHotkeysAction == WinNumHotkeysOption.InvokeQuickLaunch && Host.Screen.Primary)
            {
                try
                {
                    ListCollectionView items = (ListCollectionView)CollectionViewSource.GetDefaultView(Folder.Files);

                    bool exists = items.MoveCurrentToPosition(e.index);
                    
                    if (exists) InvokeContextMenu((ShellFile)items.CurrentItem, false);

                }
                catch (ArgumentOutOfRangeException) { }
            }
        }
        #endregion

        #region Context menu
        private ShellMenuCommandBuilder GetFileCommandBuilder(ShellFile file)
        {
            if (file == null)
            {
                return new ShellMenuCommandBuilder();
            }

            ShellMenuCommandBuilder builder = new ShellMenuCommandBuilder();

            builder.AddSeparator();
            builder.AddCommand(new ShellMenuCommand
            {
                Flags = MFT.BYCOMMAND,
                Label = (string)FindResource("open_folder"),
                UID = (uint)MenuItem.OpenParentFolder
            });

            return builder;
        }

        private bool InvokeContextMenu(ShellFile file, bool isInteractive)
        {
            if (file == null)
            {
                return false;
            }
            
            var _ = new ShellItemContextMenu(new ShellItem[] { file }, Folder, IntPtr.Zero, HandleFileAction, isInteractive, false, new ShellMenuCommandBuilder(), GetFileCommandBuilder(file));
            return true;
        }

        private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Folder != null)
            {
                ShellHelper.StartProcess(Folder.Path);
            }
        }

        private bool HandleFileAction(string action, ShellItem[] items, bool allFolders)
        {
            if (action == ((uint)MenuItem.OpenParentFolder).ToString())
            {
                ShellHelper.StartProcess(Folder.Path);
                return true;
            }

            return false;
        }
        #endregion

        private void Initialize()
        {
            if (!_isLoaded && Host != null)
            {
                Settings.Instance.PropertyChanged += Settings_PropertyChanged;
                Host.hotkeyManager.TaskbarHotkeyPressed += Toolbar_TaskbarHotkeyPressed;

                _isLoaded = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Settings.Instance.PropertyChanged -= Settings_PropertyChanged;
            if (Host != null)
            {
                Host.hotkeyManager.TaskbarHotkeyPressed -= Toolbar_TaskbarHotkeyPressed;
            }

            _isLoaded = false;
        }

        private static void HostChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Toolbar toolbar && e.OldValue == null && e.NewValue != null)
            {
                toolbar.Initialize();
            }
        }
    }
}