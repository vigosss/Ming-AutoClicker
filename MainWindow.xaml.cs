using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Ming_AutoClicker.Models;
using Ming_AutoClicker.ViewModels;
using Ming_AutoClicker.Views;

namespace Ming_AutoClicker
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private MacroEditorView? _editorView;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            SourceInitialized += OnSourceInitialized;
            Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as MainViewModel;
            if (_viewModel == null) return;

            // 监听执行状态变化，更新状态指示灯颜色
            _viewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.IsExecuting))
                {
                    Dispatcher.Invoke(() =>
                    {
                        StatusIndicator.Fill = _viewModel.IsExecuting
                            ? new SolidColorBrush(Colors.LimeGreen)
                            : new SolidColorBrush(Colors.Gray);
                    });
                }
            };

            // 初始化宏列表视图事件
            MacroListView.RequestEdit += OnRequestEdit;
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            // 获取窗口句柄并注册热键
            var hwnd = new WindowInteropHelper(this).Handle;
            _viewModel?.RegisterHotkey(hwnd);
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _viewModel?.UnregisterHotkey();
            _viewModel?.Dispose();
        }

        /// <summary>
        /// 切换到编辑器视图
        /// </summary>
        private void OnRequestEdit(object? sender, MacroProfile macro)
        {
            if (_viewModel == null) return;

            // 创建编辑器 ViewModel（复用全局服务单例）
            var editorViewModel = new MacroEditorViewModel(
                App.StorageService!,
                App.ScreenCaptureService!,
                App.ImageMatchService!,
                macro);

            // 创建编辑器视图
            _editorView = new MacroEditorView
            {
                DataContext = editorViewModel
            };

            _editorView.RequestClose += OnEditorRequestClose;

            // 切换内容
            ContentArea.Content = _editorView;
        }

        /// <summary>
        /// 编辑器请求关闭，切回列表视图
        /// </summary>
        private void OnEditorRequestClose(object? sender, EventArgs e)
        {
            if (_editorView != null)
            {
                _editorView.RequestClose -= OnEditorRequestClose;
                _editorView = null;
            }

            // 切回列表视图
            ContentArea.Content = MacroListView;

            // 刷新宏列表
            _viewModel?.LoadMacros();
        }
    }
}