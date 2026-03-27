using System;
using System.ComponentModel;
using System.Windows.Controls;
using Ming_AutoClicker.ViewModels;

namespace Ming_AutoClicker.Views
{
    public partial class MacroEditorView : UserControl
    {
        /// <summary>
        /// 请求关闭编辑器事件（返回列表视图）
        /// </summary>
        public event EventHandler? RequestClose;

        private MacroEditorViewModel? _viewModel;

        public MacroEditorView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // 取消旧订阅
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            _viewModel = e.NewValue as MacroEditorViewModel;

            // 订阅新 ViewModel
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MacroEditorViewModel.StatusMessage))
            {
                var msg = _viewModel?.StatusMessage;
                if (msg == "保存成功" || msg == "已取消更改")
                {
                    // 延迟关闭，让用户看到状态消息
                    Dispatcher.BeginInvoke(() =>
                    {
                        RequestClose?.Invoke(this, EventArgs.Empty);
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }
    }
}