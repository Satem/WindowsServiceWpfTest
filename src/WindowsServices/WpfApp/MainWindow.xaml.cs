namespace WpfApp
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow(MainViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void WindowsServiceList_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = (DependencyObject)e.OriginalSource;
            while (source != null && source is DataGridRow == false) source = VisualTreeHelper.GetParent(source);
            if (source == null) return;
            var row = (DataGridRow)source;

            if (row.IsSelected == false)
                WindowsServiceList.SelectedItem = ((DataGridRow)source).Item;
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    MoveSelectedElementDown();
                    break;
                case Key.Up:
                    MoveSelectedElementUp();
                    break;
                case Key.Right:
                    MoveFocusRight();
                    break;
                case Key.Left:
                    MoveFocusLeft();
                    break;
            }
        }

        private void MoveSelectedElementDown()
        {
            if (WindowsServiceList.Items.Count == 0)
                return;

            if (WindowsServiceList.SelectedItem == null
                || WindowsServiceList.SelectedIndex == WindowsServiceList.Items.Count - 1)
                WindowsServiceList.SelectedItem = WindowsServiceList.Items[0];
            else
            {
                var nextItemIndex = WindowsServiceList.SelectedIndex + 1;
                WindowsServiceList.SelectedItem = WindowsServiceList.Items[nextItemIndex];
            }

            WindowsServiceList.ScrollIntoView(WindowsServiceList.SelectedItem);
        }

        private void MoveSelectedElementUp()
        {
            if (WindowsServiceList.Items.Count == 0)
                return;

            if (WindowsServiceList.SelectedItem == null
                || WindowsServiceList.SelectedIndex == 0)
                WindowsServiceList.SelectedItem = WindowsServiceList.Items[WindowsServiceList.Items.Count - 1];
            else
            {
                var previous = WindowsServiceList.SelectedIndex - 1;
                WindowsServiceList.SelectedItem = WindowsServiceList.Items[previous];
            }

            WindowsServiceList.ScrollIntoView(WindowsServiceList.SelectedItem);
        }

        private void MoveFocusRight()
        {
            CommandPanel.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
        }

        private void MoveFocusLeft()
        {
            CommandPanel.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
        }
    }
}