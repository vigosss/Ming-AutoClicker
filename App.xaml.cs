using System.Windows;
using Ming_AutoClicker.Services;
using Ming_AutoClicker.ViewModels;

namespace Ming_AutoClicker
{
    public partial class App : Application
    {
        public static MainViewModel? MainViewModel { get; private set; }
        public static MacroStorageService? StorageService { get; private set; }
        public static ScreenCaptureService? ScreenCaptureService { get; private set; }
        public static ImageMatchService? ImageMatchService { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 按依赖顺序初始化服务
            StorageService = new MacroStorageService();
            ScreenCaptureService = new ScreenCaptureService();
            ImageMatchService = new ImageMatchService(ScreenCaptureService);
            var macroExecutor = new MacroExecutor(ImageMatchService, ScreenCaptureService);
            var hotkeyService = new HotkeyService();

            // 创建主 ViewModel
            MainViewModel = new MainViewModel(
                StorageService,
                ScreenCaptureService,
                ImageMatchService,
                macroExecutor,
                hotkeyService);

            // 设置主窗口 DataContext
            var mainWindow = (MainWindow)Current.MainWindow;
            mainWindow.DataContext = MainViewModel;
        }
    }
}