using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ScreenDimmer
{
    public partial class MainWindow : Window
    {
        private const double MAX_OPACITY = 0.85;
        private const double STEP = 0.05;

        private const int HOTKEY_UP = 1;
        private const int HOTKEY_DOWN = 2;
        private const int HOTKEY_EXIT = 3;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MakeFullscreen();
            Opacity = 0.5;
            EnableClickThrough();
            RegisterHotkeys();
        }

        private void MakeFullscreen()
        {
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
        }

        private void RegisterHotkeys()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(hwnd);
            source.AddHook(WndProc);

            RegisterHotKey(hwnd, HOTKEY_UP, MOD_CTRL | MOD_ALT, VK_UP);
            RegisterHotKey(hwnd, HOTKEY_DOWN, MOD_CTRL | MOD_ALT, VK_DOWN);
            RegisterHotKey(hwnd, HOTKEY_EXIT, MOD_CTRL | MOD_ALT | MOD_SHIFT, VK_ESC);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                switch (wParam.ToInt32())
                {
                    case HOTKEY_UP:
                        Opacity = Math.Max(0, Opacity - STEP);
                        break;

                    case HOTKEY_DOWN:
                        Opacity = Math.Min(MAX_OPACITY, Opacity + STEP);
                        break;

                    case HOTKEY_EXIT:
                        Application.Current.Shutdown();
                        break;
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;

            UnregisterHotKey(hwnd, HOTKEY_UP);
            UnregisterHotKey(hwnd, HOTKEY_DOWN);
            UnregisterHotKey(hwnd, HOTKEY_EXIT);

            base.OnClosed(e);
        }

        private void EnableClickThrough()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(hwnd, GWL_EXSTYLE);

            SetWindowLong(
                hwnd,
                GWL_EXSTYLE,
                style | WS_EX_LAYERED | WS_EX_TRANSPARENT
            );
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WM_HOTKEY = 0x0312;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CTRL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;

        private const uint VK_UP = 0x26;
        private const uint VK_DOWN = 0x28;
        private const uint VK_ESC = 0x1B;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int index, int newStyle);
    }
}