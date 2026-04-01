using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ming_AutoClicker.Helpers;

namespace Ming_AutoClicker.Views
{
    /// <summary>
    /// 坐标拾取窗口 - 全屏覆盖屏幕截图背景，按住鼠标拖动拾取屏幕坐标
    /// </summary>
    public partial class CoordinatePickWindow : Window
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly System.Drawing.Bitmap _screenBitmap;
        private bool _isDragging;

        /// <summary>
        /// 坐标拾取完成事件
        /// </summary>
        public event Action<int, int>? CoordinatePicked;

        public CoordinatePickWindow()
        {
            InitializeComponent();

            // 获取虚拟屏幕尺寸（支持多显示器）
            var (_, _, screenW, screenH) = Win32Api.GetVirtualScreenBounds();
            _screenWidth = screenW;
            _screenHeight = screenH;

            // 捕获当前屏幕作为背景
            _screenBitmap = Win32Api.CaptureVirtualScreen();

            Loaded += CoordinatePickWindow_Loaded;
            KeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    DialogResult = false;
                    Close();
                }
            };
        }

        private void CoordinatePickWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置背景截图
            BackgroundImage.Source = Win32Api.BitmapToBitmapSource(_screenBitmap);
            BackgroundImage.Width = _screenWidth;
            BackgroundImage.Height = _screenHeight;

            // 全屏半透明遮罩
            OverlayPath.Data = new RectangleGeometry(new Rect(0, 0, _screenWidth, _screenHeight));

            // 居中提示文字
            Canvas.SetLeft(TipText, (_screenWidth - TipText.ActualWidth) / 2);
            Canvas.SetTop(TipText, (_screenHeight - TipText.ActualHeight) / 2);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                TipText.Visibility = Visibility.Collapsed;
                UpdateCrosshair(e);
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging)
            {
                UpdateCrosshair(e);
                e.Handled = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Released)
            {
                _isDragging = false;

                // 获取屏幕坐标
                var pos = e.GetPosition(this);
                int screenX = (int)pos.X;
                int screenY = (int)pos.Y;

                // 触发事件
                CoordinatePicked?.Invoke(screenX, screenY);

                DialogResult = true;
                Close();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 更新十字准星和坐标信息面板
        /// </summary>
        private void UpdateCrosshair(MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            double x = pos.X;
            double y = pos.Y;

            // 更新竖线（全屏高度）
            CrossV.X1 = x;
            CrossV.Y1 = 0;
            CrossV.X2 = x;
            CrossV.Y2 = _screenHeight;

            // 更新横线（全屏宽度）
            CrossH.X1 = 0;
            CrossH.Y1 = y;
            CrossH.X2 = _screenWidth;
            CrossH.Y2 = y;

            // 更新坐标信息
            CoordText.Text = $"X: {(int)x}  Y: {(int)y}";
            InfoPanel.Visibility = Visibility.Visible;

            // 信息面板位置：跟随鼠标，偏移避免遮挡
            double panelX = x + 16;
            double panelY = y + 16;

            // 防止超出屏幕右侧
            if (panelX + InfoPanel.ActualWidth > _screenWidth)
            {
                panelX = x - InfoPanel.ActualWidth - 16;
            }
            // 防止超出屏幕底部
            if (panelY + InfoPanel.ActualHeight > _screenHeight)
            {
                panelY = y - InfoPanel.ActualHeight - 16;
            }

            Canvas.SetLeft(InfoPanel, panelX);
            Canvas.SetTop(InfoPanel, panelY);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _screenBitmap?.Dispose();
        }
    }
}