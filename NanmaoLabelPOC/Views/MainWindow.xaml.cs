using System.Windows;
using System.Windows.Input;
using NanmaoLabelPOC.ViewModels;

namespace NanmaoLabelPOC.Views;

/// <summary>
/// 主視窗
/// [ref: raw_spec 8.2, 8.3]
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;
    private WindowState _previousWindowState;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    /// <summary>
    /// 設定 ViewModel
    /// </summary>
    public void SetViewModel(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;

        // 設定 LabelPrintView 的 DataContext
        LabelPrintPage.DataContext = viewModel.LabelPrintViewModel;

        // 設定 DataManageView 的 DataContext [ref: raw_spec 8.5]
        DataManagePage.DataContext = viewModel.DataManageViewModel;
    }

    /// <summary>
    /// 視窗載入完成
    /// </summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel?.Initialize();
    }

    #region 標題列事件 [ref: raw_spec 8.3]

    /// <summary>
    /// 標題列拖曳移動視窗
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // 雙擊最大化/還原 [ref: raw_spec 8.3]
            ToggleMaximize();
        }
        else
        {
            // 拖曳移動 [ref: raw_spec 8.3]
            if (WindowState == WindowState.Maximized)
            {
                // 從最大化狀態拖曳時，先還原再移動
                var mousePosition = e.GetPosition(this);
                var screenPosition = PointToScreen(mousePosition);

                WindowState = WindowState.Normal;

                // 調整視窗位置讓游標在標題列中央
                Left = screenPosition.X - (Width / 2);
                Top = screenPosition.Y - 20;
            }

            DragMove();
        }
    }

    private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // 釋放滑鼠
    }

    #endregion

    #region 視窗控制按鈕 [ref: raw_spec 8.3]

    private void FullScreen_Click(object sender, RoutedEventArgs e)
    {
        EnterFullScreen();
    }

    private void ExitFullScreen_Click(object sender, RoutedEventArgs e)
    {
        ExitFullScreen();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    #endregion

    #region 全螢幕控制 [ref: raw_spec 8.3]

    /// <summary>
    /// 進入全螢幕模式
    /// [ref: raw_spec 8.3]
    /// </summary>
    private void EnterFullScreen()
    {
        if (_viewModel != null)
        {
            _previousWindowState = WindowState;
            _viewModel.IsFullScreen = true;
            WindowState = WindowState.Maximized;
            // Note: 實際隱藏工作列需要額外處理，POC 階段暫以最大化代替
        }
    }

    /// <summary>
    /// 離開全螢幕模式
    /// [ref: raw_spec 8.3]
    /// </summary>
    private void ExitFullScreen()
    {
        if (_viewModel != null)
        {
            _viewModel.IsFullScreen = false;
            WindowState = _previousWindowState == WindowState.Minimized
                ? WindowState.Normal
                : _previousWindowState;
        }
    }

    /// <summary>
    /// 切換最大化/還原
    /// </summary>
    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

        // 更新按鈕圖示
        MaximizeButton.Content = WindowState == WindowState.Maximized ? "❐" : "□";
    }

    #endregion

    #region 鍵盤事件 [ref: raw_spec 8.3]

    /// <summary>
    /// 處理鍵盤快捷鍵
    /// [ref: raw_spec 8.3]
    /// </summary>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F11:
                // F11: 切換全螢幕 [ref: raw_spec 8.3]
                if (_viewModel?.IsFullScreen == true)
                    ExitFullScreen();
                else
                    EnterFullScreen();
                e.Handled = true;
                break;

            case Key.Escape:
                // ESC: 離開全螢幕 [ref: raw_spec 8.3]
                if (_viewModel?.IsFullScreen == true)
                {
                    ExitFullScreen();
                    e.Handled = true;
                }
                break;
        }
    }

    #endregion

    #region 分頁切換 [ref: raw_spec 8.2]

    /// <summary>
    /// 記錄當前分頁（用於切換時判斷來源）
    /// </summary>
    private bool _isOnDataManagePage;

    private void Tab_Checked(object sender, RoutedEventArgs e)
    {
        if (LabelPrintPage == null || DataManagePage == null || _viewModel == null)
            return;

        if (TabLabelPrint.IsChecked == true)
        {
            // T056: 從資料管理切換到標籤列印時，檢查未儲存變更 [ref: raw_spec 8.9]
            if (_isOnDataManagePage && _viewModel.DataManageViewModel.IsDirty)
            {
                var result = MessageBox.Show(
                    "資料尚未儲存，是否要儲存變更？",
                    "確認",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    // 取消切換，恢復選取資料管理分頁
                    TabDataManage.IsChecked = true;
                    return;
                }

                if (result == MessageBoxResult.Yes)
                {
                    // 儲存變更
                    try
                    {
                        _viewModel.DataManageViewModel.SaveCommand.Execute(null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"儲存失敗：{ex.Message}",
                            "錯誤",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        TabDataManage.IsChecked = true;
                        return;
                    }
                }
            }

            LabelPrintPage.Visibility = Visibility.Visible;
            DataManagePage.Visibility = Visibility.Collapsed;
            _isOnDataManagePage = false;

            // 切換到標籤列印時刷新資料
            _viewModel.RefreshData();
        }
        else if (TabDataManage.IsChecked == true)
        {
            LabelPrintPage.Visibility = Visibility.Collapsed;
            DataManagePage.Visibility = Visibility.Visible;
            _isOnDataManagePage = true;

            // 切換到資料管理時載入資料
            _viewModel.DataManageViewModel.LoadDataCommand.Execute(null);
        }
    }

    #endregion
}
