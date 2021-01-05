using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;


using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
//using Ab3d.DXEngine.Wpf.Samples.Controls;
using Ab3d.Visuals;

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SkYaRk_Virtual_Desktop
{
    public partial class MainWindow : Window
    {
        public static MainWindow _mainWindow;

        bool _isComponents;

        public static int _processorCount = 0;

        public static int _workerThreadsTotal;
        public static int _portThreadsTotal;

        Thread _thread;

        public static int _totalThreads = 0;
        public static bool _mainFrameStarterItemz = true;

        public static string _oculusTouchMouseMove = null;

        public static bool _buttonChanged = false;

        public static string _indexTriggerTouchRight = null;
        public static string _indexTriggerTouchLeft = null;

        public MainWindow()
        {
            _mainWindow = this;
            //INITIALIZING COMPONENTS//
            for (int j = 0; j < 1; j++)
            {
                try
                {
                    InitializeComponent();
                    _isComponents = true;
                }
                catch
                {
                    _isComponents = false;
                    break;
                }
            }



            //string targetProcessName0 = "voidexpanse";
            //string targetProcessName1 = "SC_SkyArk";

            //IntPtr currentWindow = FindWindow(null, targetProcessName);

            //this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            //this.VerticalAlignment = VerticalAlignment.Center;

            //new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2);

           


            //////////ADDITIONAL WINDOWS////////////////////
            ////////////////////////////////////////////////
            ContentFrame2.Source = new Uri("Customizations/SC_AI_VR.xaml", UriKind.Relative);
            //ContentFrame2.NavigationUIVisibility = NavigationUIVisibility.Hidden;

            if (_isComponents == false)
            {

            }
            else
            {

            }
            //this.WindowState = WindowState.Minimized; it bugs the whole program.

            /*string targetProcessName = "Desktop";
            string targetProcessName0 = "dxengine";

            foreach (Process clsProcess in Process.GetProcesses())
            {
                //IntPtr handle = clsProcess.MainWindowHandle;
                //handle = GetWindow(handle, GW_HWNDFIRST);
                if (clsProcess.ProcessName.ToLower().Contains(targetProcessName.ToLower()) && !clsProcess.ProcessName.ToLower().Contains(targetProcessName0.ToLower()))
                {
                    IntPtr handle = clsProcess.MainWindowHandle;
                    //var sm = GetSystemMenu(handle, false);
                    //EnableMenuItem(sm, SC_CLOSE, MF_BYCOMMAND | MF_DISABLED0);
                    //Size newSize = new Size((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2);
                    //CentreWindow(handle, newSize);
                    var value = GetWindowLong(handle, GWL_STYLE);
                    SetWindowLong(handle, GWL_STYLE, (int)(value & WS_MINIMIZE));
                }
            }*/


            //this.WindowStyle = WindowStyle.ToolWindow;
            //this.WindowStyle = WindowStyle.ThreeDBorderWindow;
            //this.WindowState = WindowState.Minimized;


        

            //IntPtr currentWindow = FindWindow(null, targetProcessName);
        }



        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;



        [DllImport("user32")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        const int MF_BYCOMMAND = 0;
        const int MF_DISABLED0 = 1;
        const int MF_DISABLED = 2;
        const int SC_CLOSE = 0xF060;
        



        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZE = -131073;











        private void ContentFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            // Prevent navigation (for example clicking back button) because our ListBox is not updated when this navigation occurs
            // We prevent navigation with clearing the navigation history each time navigation item changes
            //ContentFrame0.NavigationService.RemoveBackEntry();
            //ContentFrame1.NavigationService.RemoveBackEntry();
        }

        private void comboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            /*string[] keysStringArray = new string[]
            {
                "NUMPAD0",
                "NUMPAD1",
                "NUMPAD2",
                "NUMPAD3",
                "NUMPAD4",
                "NUMPAD5",
                "NUMPAD6",
                "NUMPAD7",
                "NUMPAD8",
                "NUMPAD9",
                "F1",
                "F2",
                "F3",
                "F4",
                "F5",
                "F6",
                "F7",
                "F8",
                "F9",
                "F10",
                "F11",
                "F12",
                "KEY_A",
                "KEY_B",
                "KEY_C",
                "KEY_D",
                "KEY_E",
                "KEY_F",
                "KEY_G",
                "KEY_H",
                "KEY_I",
                "KEY_J",
                "KEY_K",
                "KEY_L",
                "KEY_M",
                "KEY_N",
                "KEY_O",
                "KEY_P",
                "KEY_Q",
                "KEY_R",
                "KEY_S",
                "KEY_T",
                "KEY_U",
                "KEY_V",
                "KEY_W",
                "KEY_X",
                "KEY_Y",
                "KEY_Z",
                "ENTER",
                "SPACE_BAR",
                "TAB",
            };

            changeKeyMapping.ItemsSource = keysStringArray;*/

           string[] keysStringArray = new string[]
           {
               "Oculus Touch Right Mouse Control",
               "Oculus Touch Left Mouse Control",
           };

            //_MouseLeft.ItemsSource = keysStringArray;
            changeKeyMapping.ItemsSource = keysStringArray;
        }



        private void _OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var test = sender as System.Windows.Controls.ComboBox;
            string tester = test.SelectedItem as string;
            _oculusTouchMouseMove = tester;
            _buttonChanged = true;



            //if (!this.IsLoaded)
            //    return;

            /*string currentText = "";
            switch (test.SelectedIndex)
            {
                case 1:
                    currentText = "test0"; // 33 ms (30 FPS)
                   
                    break;

                case 2:
                    currentText = "test1";
                    break;

                case 3:
                    currentText = "test2";
                    break;

                case 0:
                default:
                    currentText = "test3";
                    break;
            }*/
        }










        /*private void comboBox_MouseLeft_OnLoaded(object sender, RoutedEventArgs e)
        {
            string[] keysStringArray = new string[]
            {
                "mouseLeft",
                "mouseRight",
            };

            _MouseLeft.ItemsSource = keysStringArray;
        }*/

        /*private void _MouseLeft_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var test = sender as System.Windows.Controls.ComboBox;
            string tester = test.SelectedItem as string;
            _buttonA = tester;
            _buttonChanged = true;
        }*/

        /*private void comboBox_MouseRight_OnLoaded(object sender, RoutedEventArgs e)
        {
            string[] keysStringArray = new string[]
            {
                "mouseLeft",
                "mouseRight",
            };

            _MouseRight.ItemsSource = keysStringArray;
        }*/

        /*private void _MouseRight_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var test = sender as System.Windows.Controls.ComboBox;
            string tester = test.SelectedItem as string;
            _buttonA = tester;
            _buttonChanged = true;
        }*/


















        class keyboardKeySetup
        {
            public string[] keyboardKey
            {
                get;
                set;
            }
        }

        /*public ushort KeyCode
        {
            NUMPAD0 = 0x60,
            NUMPAD1 = 0x61,
            NUMPAD2 = 0x62,
            NUMPAD3 = 0x63,
            NUMPAD4 = 100,
            NUMPAD5 = 0x65,
            NUMPAD6 = 0x66,
            NUMPAD7 = 0x67,
            NUMPAD8 = 0x68,
            NUMPAD9 = 0x69,

            F1 = 0x70,
            F2 = 0x71,
            F3 = 0x72,
            F4 = 0x73,
            F5 = 0x74,
            F6 = 0x75,
            F7 = 0x76,
            F8 = 0x77,
            F9 = 120,
            F10 = 0x79,
            F11 = 0x7a,
            F12 = 0x7b,


            KEY_A = 0x41,
            KEY_B = 0x42,
            KEY_C = 0x43,
            KEY_D = 0x44,
            KEY_E = 0x45,
            KEY_F = 70,
            KEY_G = 0x47,
            KEY_H = 0x48,
            KEY_I = 0x49,
            KEY_J = 0x4a,
            KEY_K = 0x4b,
            KEY_L = 0x4c,
            KEY_M = 0x4d,
            KEY_N = 0x4e,
            KEY_O = 0x4f,
            KEY_P = 80,
            KEY_Q = 0x51,
            KEY_R = 0x52,
            KEY_S = 0x53,
            KEY_T = 0x54,
            KEY_U = 0x55,
            KEY_V = 0x56,
            KEY_W = 0x57,
            KEY_X = 0x58,
            KEY_Y = 0x59,
            KEY_Z = 90,

      
            ENTER = 13,
            SPACE_BAR = 0x20,
            TAB = 9,
        }*/
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);



        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(HandleRef hwnd, out RECT lpRect);


        private void CentreWindow(IntPtr handle, Size monitorDimensions)
        {
            RECT rect;
            GetWindowRect(new HandleRef(this, handle), out rect);

            int x1Pos = (int)(monitorDimensions.Width / 2 - (rect.Right - rect.Left) / 2);
            var x2Pos = rect.Right - rect.Left;
            int y1Pos = (int)(monitorDimensions.Height / 2 - (rect.Bottom - rect.Top) / 2);
            var y2Pos = rect.Bottom - rect.Top;

            SetWindowPos(handle, 0, x1Pos, y1Pos, x2Pos, y2Pos, 0);
        }

        /*private Size GetMonitorDimensions()
        {
            return SystemInformation.PrimaryMonitorSize;
        }*/
    }
}
