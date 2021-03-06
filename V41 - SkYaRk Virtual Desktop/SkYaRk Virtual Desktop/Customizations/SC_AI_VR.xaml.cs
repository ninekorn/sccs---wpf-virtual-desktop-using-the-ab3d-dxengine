﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Ab3d.Cameras;
using Ab3d.Common;
using Ab3d.Common.Cameras;
using Ab3d.Controls;
using Ab3d.DirectX;
using Ab3d.DirectX.Controls;
using Ab3d.OculusWrap;
using Ab3d.Visuals;
using Ab3d.DXEngine.OculusWrap;

using System.Diagnostics;
using System.Drawing.Imaging;
using Ab3d.DirectX.Materials;
using System.Drawing;

using Ab3d;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System.Windows.Media.Imaging;

using System.Windows.Input;
using System.Reflection;
//using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Ab3d.DirectX.Models;

using System.Numerics;

namespace SkYaRk_Virtual_Desktop.Customizations
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SC_AI_VR : Page
    {
        /// <summary>
        /// VIRTUAL REALITY VARIABLES BUT MOSTLY COMING FROM AB4D VIRTUAL REALITY SAMPLE
        /// </summary>
        public const bool RenderAt90Fps = true; // When true, a background worker is used to force rendering at 90 FPS; when false a standard WPF's Rendering event is used to render at app. 60 FPS
        public const bool UseOculusRift = true; // When false, no Oculus device is initialized and we have standard DXEngine 3D rendering
        private OvrWrap _ovr;
        private DXDevice _dxDevice;
        private Viewport3D _viewport3D;
        private DXViewportView _dxViewportView;
        private FirstPersonCamera _camera;
        private XInputCameraController _xInputCameraController;
        private volatile OculusWrapVirtualRealityProvider _oculusRiftVirtualRealityProvider;
        private VarianceShadowRenderingProvider _varianceShadowRenderingProvider;
        private int _framesCount;
        private double _renderTime;
        private int _lastFpsMeterSecond = -1;
        private bool _isFirstSecond = true;
        private TimeSpan _lastRenderTime;
        private string _originalWindowTitle;
        //private MeshGeometry3D _leftControllerMesh;
        //private ModelVisual3D _leftControllerVisual3D;
        private QuaternionRotation3D _leftControllerQuaternionRotation3D;
        private RotateTransform3D _leftControllerBodyRotateTransform3D;
        private TranslateTransform3D _leftControllerTranslateTransform3D;
        //private ModelVisual3D _rightControllerVisual3D;
        private QuaternionRotation3D _rightControllerQuaternionRotation3D;
        private RotateTransform3D _rightControllerBodyRotateTransform3D;
        private TranslateTransform3D _rightControllerTranslateTransform3D;
        public static SharpDX.DXGI.Device _device;
        private DisposeList _disposables;
        ModelVisual3D _rootVisual3D;
        SessionStatus _ovrHeadset;
        IntPtr sessionPtr;

        /// <summary>
        /// NEW VARIABLES FOR THE DESKTOPSCREEN
        /// </summary>
        BoxVisual3D _floorBox;
        ImageBrush _brush;
        Ab3d.Visuals.VerticalPlaneVisual3D _desktopScreen;
        public static SC_SharpDX_ScreenCapture _desktopDupe;
        SC_SharpDX_ScreenFrame _desktopFrame;
        DateTime startTime;
        DiffuseMaterial vertexColorDiffuseMaterial;       
        StandardMaterial _standardMaterial;
        StandardMaterial _lastStandardMaterial;
        ShaderResourceView _shaderResourceView;
        ShaderResourceView _lastShaderResourceView;
        Bitmap _bitmap = null;
        ControllerType connectedControllerTypes;

        QuaternionRotation3D _desktopScreenQuaternionRotation3D;
        TranslateTransform3D _desktopScreenTranslateTransform3D;
        RotateTransform3D _desktopScreenBodyRotateTransform3D;
        BoxVisual3D rightControllerVisual3D;
        BoxVisual3D leftControllerVisual3D;
        //BoxVisual3D hitPointRight;
        SphereVisual3D hitPointRight;
        //BoxVisual3D hitPointLeft;
        SphereVisual3D hitPointLeft;
        MeshGeometry3D meshGeometry;
        Ab3d.Utilities.Plane plane;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        Ab3d.OculusWrap.InputState someState;
        Ab3d.OculusWrap.InputState inputStateRTouch;
        Ab3d.OculusWrap.InputState inputStateLTouch;
        ControllerType lTouch = ControllerType.LTouch;
        ControllerType rTouch = ControllerType.RTouch;
        Point3DCollection _point3DCollection = new Point3DCollection();
        Point3D[] point3DCollection;
        Point3D[] point3DCollectionTemp;

        Point3D pointOnPlane;
        System.Windows.Media.Media3D.Vector3D planeNormal;
        Point3D intersectionRight;
        Point3D intersectionLeft;
        TranslateTransform3D leftHitPoint = new TranslateTransform3D();
        TranslateTransform3D rightHitPoint = new TranslateTransform3D();

        Vector3D rayDirectionRight = new Vector3D(0, 0, 0);
        Vector3D rayDirectionLeft = new Vector3D(0, 0, 0);

        ControllerType controllerTypeRTouch;
        ControllerType controllerTypeLTouch;
        System.Windows.Media.Media3D.Quaternion _rightControllerQuaternion;
        System.Windows.Media.Media3D.Quaternion _leftControllerQuaternion;



        //HERE IS THE MOUSE STABILIZER ARRAYS - THE BIGGER THE ARRAYS THE SLOWER AND MORE STABLE THE MOUSE IS ON THE SCREEN.
        Point3D[] arrayOfStabilizerPosRight = new Point3D[50];
        double[] arrayOfStabilizerPosXRight = new double[50];
        double[] arrayOfStabilizerPosDifferenceXRight = new double[49];
        double[] arrayOfStabilizerPosYRight = new double[50];
        double[] arrayOfStabilizerPosDifferenceYRight = new double[49];

        double[] arrayOfStabilizerPosZRight = new double[50];
        double[] arrayOfStabilizerPosDifferenceZRight = new double[49];


        Point3D[] arrayOfStabilizerPosLeft = new Point3D[50];
        double[] arrayOfStabilizerPosXLeft = new double[50];
        double[] arrayOfStabilizerPosDifferenceXLeft = new double[49];
        double[] arrayOfStabilizerPosYLeft = new double[50];
        double[] arrayOfStabilizerPosDifferenceYLeft = new double[49];



        //
        Point3D[] _arrayOfStabilizerPosRight = new Point3D[40];
        double[] _arrayOfStabilizerPosXRight = new double[40];
        double[] _arrayOfStabilizerPosDifferenceXRight = new double[39];
        double[] _arrayOfStabilizerPosYRight = new double[40];
        double[] _arrayOfStabilizerPosDifferenceYRight = new double[39];

        Point3D[] _arrayOfStabilizerPosLeft = new Point3D[40];
        double[] _arrayOfStabilizerPosXLeft = new double[40];
        double[] _arrayOfStabilizerPosDifferenceXLeft = new double[39];
        double[] _arrayOfStabilizerPosYLeft = new double[40];
        double[] _arrayOfStabilizerPosDifferenceYLeft = new double[39];

        double[] _arrayOfStabilizerPosZLeft = new double[40];
        double[] _arrayOfStabilizerPosDifferenceZLeft = new double[39];


        double[] _arrayOfStabilizerPosZRight = new double[40];
        double[] _arrayOfStabilizerPosDifferenceZRight = new double[39];






        public static int _mainThreadFrameCounter = 0;
        private static int _frameCounter = 0;
        int strider;
        int _updateSceneFrameCounter = 0;
        int currentFrameRight = 0;
        int currentFrameLeft = 0;
        public static int _indexMouseMove = 0;
        uint _lastMousePosXRight = 9999;
        uint _lastMousePosYRight = 9999;
        uint _lastMousePosXLeft = 9999;
        uint _lastMousePosYLeft = 9999;
        uint lastbuttonPressedOculusTouchRight = 0;
        uint buttonPressedOculusTouchRight = 0;
        uint lastbuttonPressedOculusTouchLeft = 0;
        uint buttonPressedOculusTouchLeft = 0;
        int _frameOneCounter = 0;
        int _frameTwoCounter = 0;
        int _frameThreeCounter = 0;
        int _frameFourCounter = 0;
        int _currentFrameRight = 0;
        const uint ENABLE_QUICK_EDIT = 0x0040;
        const int STD_INPUT_HANDLE = -10;

        Stopwatch _updateFunctionStopwatchLeftHandTrigger;
        Stopwatch _updateFunctionStopwatchRightHandTrigger;
        Stopwatch _updateFunctionStopwatchLeftThumbstickGoRight;
        Stopwatch _updateFunctionStopwatchLeftThumbstickGoLeft;
        Stopwatch _updateFunctionStopwatchRightThumbstickGoRight;
        Stopwatch _updateFunctionStopwatchRightThumbstickGoLeft;
        Stopwatch _updateFunctionStopwatchRightIndexTrigger;
        Stopwatch _updateFunctionStopwatchLeftIndexTrigger;
        Stopwatch _updateFunctionStopwatchRight;
        Stopwatch _updateFunctionStopwatchLeft;
        Stopwatch _updateFunctionStopwatchTouchRightButtonA;
        Stopwatch _newStopWatch = new Stopwatch();
        Stopwatch _mainThreadStopWatch = new Stopwatch();
        Stopwatch _stopWatch;
        Stopwatch _stopWatchForScreenControl = new Stopwatch();




        double averageXRight = 0;
        double averageYRight = 0;
        double averageZRight = 0;
        double lastRightHitPointXFrameOne = 0;
        double lastRightHitPointYFrameOne = 0;
        double lastRightHitPointZFrameOne = 0;
        double positionXRight = 0;
        double positionYRight = 0;
        double positionZRight = 0;

        double averageXLeft = 0;
        double averageYLeft = 0;
        double lastLeftHitPointXFrameOne = 0;
        double lastLeftHitPointYFrameOne = 0;
        double lastLeftHitPointZFrameOne = 0;
        double positionXLeft = 0;
        double positionYLeft = 0;
        double _lastRightHitPointXFrameOne = 0;
        double _lastRightHitPointYFrameOne = 0;
        double _lastRightHitPointZFrameOne = 0;
        double _lastAbsRightX = 0;
        double _lastAbsRightY = 0;
        double _lastAbsRightZ = 0;
        double _lastLeftHitPointXFrameOne = 0;
        double _lastLeftHitPointYFrameOne = 0;
        double _lastLeftHitPointZFrameOne = 0;
        double _averageXRight = 0;
        double _averageYRight = 0;
        double _averageZRight = 0;
        double _averageXLeft = 0;
        double _averageYLeft = 0;
        double _averageZLeft = 0;
        double _XDOUBLErightControllerTranslateTransform3D = 0;
        double _YDOUBLErightControllerTranslateTransform3D = 0;
        double _ZDOUBLErightControllerTranslateTransform3D = 0;
        double _XDOUBLErightControllerQuaternionRotation3D = 0;
        double _YDOUBLErightControllerQuaternionRotation3D = 0;
        double _ZDOUBLErightControllerQuaternionRotation3D = 0;
        double _WDOUBLErightControllerQuaternionRotation3D = 0;
        double _XDOUBLEleftControllerTranslateTransform3D = 0;
        double _YDOUBLEleftControllerTranslateTransform3D = 0;
        double _ZDOUBLEleftControllerTranslateTransform3D = 0;
        double _XDOUBLEleftControllerQuaternionRotation3D = 0;
        double _YDOUBLEleftControllerQuaternionRotation3D = 0;
        double _ZDOUBLEleftControllerQuaternionRotation3D = 0;
        double _WDOUBLEleftControllerQuaternionRotation3D = 0;
        double totalWidthX = 0;
        double totalHeightY = 0;

        //float speed = 0.1f;

        int _currentFrameLeft = 0;
        int UpdateSceneFrameCounter = 0;  
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;
        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZE = -131073;
        int _updateFunctionFrameCounter = 0;
        uint _lastabsoluteMoveX = 0;
        uint _lastabsoluteMoveY = 0;
        int _frameCounterTouchRight = 0;

        Point3D theCenterLeft = new Point3D(0, 0, 0);
        Point3D theCenterRight = new Point3D(0, 0, 0);
        Point3D _theCenterRight = new Point3D(0, 0, 0);
        Point3D _theCenterLeft = new Point3D(0, 0, 0);
        Point3D lowestX;
        Point3D highestX;
        Point3D lowestY;
        Point3D highestY;
        Point3D lowestZ;
        Point3D highestZ;
        
        bool _reachedFrameFour = false;
        bool _createStopWatch = true;
        bool _restartStopWatch = true;
        bool _restartFrameCounterRight = false;
        bool _restartFrameCounterLeft = false;
        bool _canResetCounterTouchRightButtonA = false;
        bool _canResetCounterTouchRightButtonB = false;
        int _frameCounterTouchLeft = 0;
        bool _canResetCounterTouchLeftButtonA = false;
        bool _canResetCounterTouchLeftButtonB = false;
        bool _canResetCounterTouchLeftButtonX = false;
        bool _canResetCounterTouchLeftButtonY = false;
        bool hasUsedThumbStickLeftW = false;
        bool hasUsedThumbStickLeftS = false;
        bool hasUsedThumbStickLeftA = false;
        bool hasUsedThumbStickRightD = false;
        bool hasUsedThumbStickRightQ = false;
        bool hasUsedThumbStickRightE = false;
        bool lastHasUsedHandTriggerLeft = false;
        bool hasUsedHandTriggerLeft = false;
        bool hasUsedHandTriggerRight = false;
        bool hasUsedIndexTriggerRight = false;
        bool hasUsedIndexTriggerLeft = false;
        bool _oncer = true;
        bool _failed = false;
        bool _createdSceneObjects = false;
        static bool _shaderQuality = true;
        public bool _stopWatchSwitch = true;
        static bool _startOnce = true;
        static bool _startOnce0 = true;
        bool restartFrameCounterRight = false;
        bool hasClickedHomeButtonTouchLeft = false;
        bool isHoldingBUTTONA = false;
        bool hasClickedBUTTONA = false;
        bool hasClickedBUTTONB = false;
        bool hasClickedBUTTONX = false;
        bool hasClickedBUTTONY = false;
        bool restartFrameCounterLeft = false;
        bool _startOnce02 = true;
        bool _updateFunctionBoolRight = true;
        bool _updateFunctionBoolLeft = true;
        bool _updateFunctionBoolLeftThumbStickGoLeft = true;
        bool _updateFunctionBoolLeftThumbStickGoRight = true;
        bool _updateFunctionBoolRightThumbStickGoLeft = true;
        bool _updateFunctionBoolRightThumbStickGoRight = true;
        bool _updateFunctionBoolLeftHandTrigger = true;
        bool _updateFunctionBoolRightHandTrigger = true;
        bool _updateFunctionBoolLeftIndexTrigger = true;
        bool _updateFunctionBoolRightIndexTrigger = true;
        bool _updateFunctionBoolTouchRightButtonA = true;
        bool _activateScreenModControl = false;
        bool _screenControlStopWatchBool = true;

        /// <summary>
        /// KEYS FOR MOUSE INPUTS AND KEYBOARD INPUT. NOT ALL OF THEM HERE FOR THE KEYBOARD YET.
        /// </summary>
        public const int KEY_W = 0x57;
        public const int KEY_A = 0x41;
        public const int KEY_S = 0x53;
        public const int KEY_D = 0x44;
        public const int KEY_SPACE = 0x20; //0x39
        public const int KEY_E = 0x45;
        public const int KEY_Q = 0x51;

        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001;

        const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        const uint MOUSEEVENTF_XDOWN = 0x0080;
        const uint MOUSEEVENTF_XUP = 0x0100;
        const uint MOUSEEVENTF_WHEEL = 0x0800;
        const uint MOUSEEVENTF_HWHEEL = 0x01000;

        /// <summary>
        /// DLLs - SOME ARE UNUSED BECAUSE I STILL WANT TO IMPROVE SOME STUFF WITH THE OSK.EXE. Right now, when the cursor is over the osk.exe, we see the keyboard keys white ONLY when the 
        /// OCULUS TOUCH THAT CONTROLS THE MOUSE HOVERS OVER THEM... The other oculus touch doesn't have this effect and it's a bit annoying.
        /// </summary>
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "keybd_event")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "mouse_event")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, long dwData, uint dwExtraInfo);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetCursorPos")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        private static extern void SetCursorPos(int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hwnd, uint wCmd);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);


        int _width = 0;
        int _height = 0;


        bool _startingOnce = true;


        //FOUND THAT SOMEWHERE ON STACKOVERFLOW TO TURN OFF INTERACTION BECAUSE IT WAS INTERFERING WITH THE CONSOLE MESSAGES WHEN I WAS CLICKING ON THE CONSOLE.
        internal static bool removeConsoleInteraction()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // get current console mode
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }

            // Clear the quick edit bit in the mode flags
            consoleMode &= ~ENABLE_QUICK_EDIT;

            // set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return false;
            }

            return true;
        }





        bool res;
        public SC_AI_VR()
        {
            InitializeComponent();

            startTime = DateTime.Now;
            _disposables = new DisposeList();
            removeConsoleInteraction();

            this.Loaded += delegate
            {
                if (UseOculusRift)
                {
                    // Create Oculus OVR Wrapper
                    _ovr = OvrWrap.Create();

                    // Check if OVR service is running
                    var detectResult = _ovr.Detect(0);

                    if (!detectResult.IsOculusServiceRunning)
                    {
                        System.Windows.MessageBox.Show("Oculus service is not running", "Oculus error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Check if Head Mounter Display is connected
                    if (!detectResult.IsOculusHMDConnected)
                    {
                        System.Windows.MessageBox.Show("Oculus HMD (Head Mounter Display) is not connected", "Oculus error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                InitializeOvrAndDirectX();
            };

            this.Unloaded += delegate
            {
                if (!RenderAt90Fps)
                    CompositionTarget.Rendering -= CompositionTargetOnRendering; // Stop rendering at 60 FPS

                if (_xInputCameraController != null)
                    _xInputCameraController.StopCheckingController();

                Dispose();
            };

            /*this.Closing += delegate (object sender, CancelEventArgs args)
            {
                if (!RenderAt90Fps)
                    CompositionTarget.Rendering -= CompositionTargetOnRendering; // Stop rendering at 60 FPS

                if (_xInputCameraController != null)
                    _xInputCameraController.StopCheckingController();

                Dispose();
            };*/
        }

        #region InitializeOvrAndDirectX
        private void InitializeOvrAndDirectX()
        {
            if (UseOculusRift)
            {
                // Initializing Oculus VR is very simple when using OculusWrapVirtualRealityProvider
                // First we create an instance of OculusWrapVirtualRealityProvider
                _oculusRiftVirtualRealityProvider = new OculusWrapVirtualRealityProvider(_ovr, multisamplingCount: 8); // originally 4 

                try
                {
                    // Then we initialize Oculus OVR and create a new DXDevice that uses the same adapter (graphic card) as Oculus Rift
                    _dxDevice = _oculusRiftVirtualRealityProvider.InitializeOvrAndDXDevice(requestedOculusSdkMinorVersion: 17);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Failed to initialize the Oculus runtime library.\r\nError: " + ex.Message, "Oculus error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //return;
                }

                string ovrVersionString = _ovr.GetVersionString();
                _originalWindowTitle = string.Format("DXEngine OculusWrap Sample (OVR v{0})", ovrVersionString);
                this.Title = _originalWindowTitle;


                // Reset tracking origin at startup
                _ovr.RecenterTrackingOrigin(_oculusRiftVirtualRealityProvider.SessionPtr);
            }
            else
            {
                //Create DXDevice that will be used to create DXViewportView
                DXDeviceConfiguration dxDeviceConfiguration = new DXDeviceConfiguration();
                dxDeviceConfiguration.DriverType = DriverType.Hardware;
                dxDeviceConfiguration.SupportedFeatureLevels = new FeatureLevel[] { FeatureLevel.Level_11_0 }; // Oculus requires at least feature level 11.0

                _dxDevice = new DXDevice(dxDeviceConfiguration);
                _dxDevice.InitializeDevice();

                _originalWindowTitle = this.Title;
            }

            


            //_device = _dxDevice.Device;
            // Create WPF's Viewport3D
            _viewport3D = new Viewport3D();

            // Create DXViewportView - a control that will enable DirectX 11 rendering of standard WPF 3D content defined in Viewport3D.
            // We use a specified DXDevice that was created by the _oculusRiftVirtualRealityProvider.InitializeOvrAndDXDevice (this way the same adapter is used by Oculus and DXEngine).
            _dxViewportView = new DXViewportView(_dxDevice, _viewport3D);

            _dxViewportView.BackgroundColor = Colors.Gray;

            // Currently DXEngine support showing Oculus mirror texture only with DirectXOverlay presentation type (not with DirectXImage)
            _dxViewportView.PresentationType = DXView.PresentationTypes.DirectXOverlay; //DirectXOverlay

            if (UseOculusRift)
            {
                // The _dxViewportView will show Oculus mirrow window.
                // The mirror window can be any size, for this sample we use 1/2 the HMD resolution.
                _dxViewportView.Width = 40;// _oculusRiftVirtualRealityProvider.HmdDescription.Resolution.Width / 2.0;
                _dxViewportView.Height = 40;// _oculusRiftVirtualRealityProvider.HmdDescription.Resolution.Height / 2.0;
            }
  
            // When the DXViewportView is initialized, we set the _oculusRiftVirtualRealityProvider to the DXScene object
            _dxViewportView.DXSceneInitialized += delegate (object sender, EventArgs args)
            {
                if (_dxViewportView.UsedGraphicsProfile.DriverType != GraphicsProfile.DriverTypes.Wpf3D &&
                    _dxViewportView.DXScene != null &&
                    _oculusRiftVirtualRealityProvider != null)
                {
                    // Initialize Virtual reality rendering
                    _dxViewportView.DXScene.InitializeVirtualRealityRendering(_oculusRiftVirtualRealityProvider);

                    // Initialized shadow rendering (see Ab3d.DXEngine.Wpf.Samples project - DXEngine/ShadowRenderingSample for more info
                    _varianceShadowRenderingProvider = new VarianceShadowRenderingProvider()
                    {
                        ShadowMapSize = 1024,
                        ShadowDepthBluringSize = 2,
                        ShadowTreshold = 0.2f
                    };

                    _dxViewportView.DXScene.InitializeShadowRendering(_varianceShadowRenderingProvider);

                    // Called after after SceneNodes are created and before RenderingQueus are filled (before they are rendered for the first time).
                    if (MainDXViewportView.DXScene == null) // DXScene can be null in case of WPF rendering
                        return;
                }
            };

            // Enable collecting rendering statistics (see _dxViewportView.DXScene.Statistics class)
            DXDiagnostics.IsCollectingStatistics = true;

            // Subscribe to SceneRendered to collect FPS statistics
            _dxViewportView.SceneRendered += DXViewportViewOnSceneRendered;
            //_dxViewportView.SceneRendered += updateMethodForVr;

            // Add _dxViewportView to the RootGrid
            // Before that we resize the window to be big enough to show the mirrored texture
            this.Width = 40; //40 //_dxViewportView.Width + 30
            this.Height = 40; //40 //_dxViewportView.Height + 50

            RootGrid.Children.Add(_dxViewportView);

            // Create FirstPersonCamera
            _camera = new FirstPersonCamera()
            {
                TargetViewport3D = _viewport3D,
                Position = new Point3D(0, 0, 0),
                Heading = 0,
                Attitude = 0,
                ShowCameraLight = ShowCameraLightType.Never               
            };

            RootGrid.Children.Add(_camera);

            // Initialize XBOX controller that will control the FirstPersonCamera
            _xInputCameraController = new XInputCameraController();
            _xInputCameraController.TargetCamera = _camera;
            _xInputCameraController.MovementSpeed = 0.02;
            _xInputCameraController.MoveVerticallyWithDPadButtons = true;

            // We handle the rotation by ourself to prevent rotating the camera up and down - this is done only by HMD
            _xInputCameraController.RightThumbChanged += delegate (object sender, XInputControllerThumbChangedEventArgs e)
            {
                // Apply only horizontal rotation
                _camera.Heading += e.NormalizedX * _xInputCameraController.RotationSpeed;
                // Mark the event as handled
                e.IsHandled = true;
            };

            _xInputCameraController.StartCheckingController();






            var _factory = new Factory1();
            var _adapter = _factory.GetAdapter1(0);

            using (var _output = _adapter.GetOutput(0))
            {
                _width = ((SharpDX.Rectangle)_output.Description.DesktopBounds).Width;
                _height = ((SharpDX.Rectangle)_output.Description.DesktopBounds).Height;
            }

            _adapter = null;
            _factory = null;





            resourceViewDescription = new ShaderResourceViewDescription
            {
                Format = Format.B8G8R8A8_UNorm,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource
                {
                    MipLevels = -1,
                    MostDetailedMip = 0
                }
            };



            //ADDED THOSE TWO VARIABLES HERE SO THAT I CAN USE THEM WITH THE VIRTUAL DESKTOP CONTROLLERS.
            sessionPtr = _oculusRiftVirtualRealityProvider.SessionPtr;
            connectedControllerTypes = _ovr.GetConnectedControllerTypes(sessionPtr);

            _standardMaterial = new Ab3d.DirectX.Materials.StandardMaterial()
            {
                DiffuseColor = new SharpDX.Color3(1f, 1f, 1f),
                HasTransparency = false, //true for Void Expanse tests
            };
            _standardMaterial.PreferedShaderQuality = ShaderQuality.Low;

            _bitmap = new Bitmap(_width, _height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            BitmapSource _bitSource01 = CreateBitmapSource(_bitmap);
            _shaderResourceView = Ab3d.DirectX.Materials.WpfMaterial.CreateTexture2D(_dxDevice, _bitSource01);

            if (_shaderResourceView != null)
            {
                _standardMaterial.DiffuseTextures = new ShaderResourceView[]
                {
                    _shaderResourceView,
                };
            }

            vertexColorDiffuseMaterial = new DiffuseMaterial(System.Windows.Media.Brushes.Black);
            vertexColorDiffuseMaterial.SetUsedDXMaterial(_standardMaterial);

            var _currentSize = new SharpDX.Size2(512, 512);
            vertexColorDiffuseMaterial.SetDXAttribute(DXAttributeType.CachedBitmapSize, _currentSize);
            vertexColorDiffuseMaterial.Freeze();

            controllerTypeRTouch = ControllerType.RTouch;
            controllerTypeLTouch = ControllerType.LTouch;

            CreateSceneObjects();           
          
            //CREATING THE DESKTOP DUPLICATOR HERE, AFTER THE MAIN SCREEN HAS BEEN CREATED IN CREATESCENEOBJECT();
            _desktopDupe = new SC_SharpDX_ScreenCapture(0, 0); // SharpDX desktop duplicator               

            var backgroundWorker00 = new BackgroundWorker();
            backgroundWorker00.DoWork += (object sender, DoWorkEventArgs args) =>
            {
            _threadLoop:

            if (SkYaRk_Virtual_Desktop.MainWindow._buttonChanged)
            {
                if (SkYaRk_Virtual_Desktop.MainWindow._oculusTouchMouseMove != null)
                {
                    int index = Array.IndexOf(keysStringArray, SkYaRk_Virtual_Desktop.MainWindow._oculusTouchMouseMove);
                    if (index != -1)
                    {
                        _indexMouseMove = index;
                        Console.WriteLine(index);
                    }
                    SkYaRk_Virtual_Desktop.MainWindow._buttonChanged = false;
                }
            }

            if (_startOnce)
            {
                /*foreach (Process clsProcess in Process.GetProcesses())
                {
                    //IntPtr handle = clsProcess.MainWindowHandle;
                    //handle = GetWindow(handle, GW_HWNDFIRST);
                    if (clsProcess.ProcessName.ToLower().Contains(targetProcessName.ToLower()))
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

                _updateFunctionStopwatchRight = new Stopwatch();
                _updateFunctionStopwatchLeft = new Stopwatch();

                _updateFunctionStopwatchTouchRightButtonA = new Stopwatch();

                _updateFunctionStopwatchLeftHandTrigger = new Stopwatch();
                _updateFunctionStopwatchRightHandTrigger = new Stopwatch();

                _updateFunctionStopwatchLeftThumbstickGoRight = new Stopwatch();
                _updateFunctionStopwatchLeftThumbstickGoLeft = new Stopwatch();

                _updateFunctionStopwatchRightIndexTrigger = new Stopwatch();
                _updateFunctionStopwatchLeftIndexTrigger = new Stopwatch();

                _updateFunctionStopwatchRightThumbstickGoLeft = new Stopwatch();
                _updateFunctionStopwatchRightThumbstickGoRight = new Stopwatch();

                _stopWatch = new Stopwatch();
                _bitmap = new Bitmap(_width, _height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                _startOnce = false;
            }

            if (_stopWatchSwitch)
            {
                _mainThreadStopWatch.Stop();
                _mainThreadStopWatch.Reset();
                _mainThreadStopWatch.Start();
                _stopWatchSwitch = false;
            }

            if (_updateFunctionBoolRight)
            {
                _updateFunctionStopwatchRight.Stop();
                _updateFunctionStopwatchRight.Reset();
                _updateFunctionStopwatchRight.Start();
                _updateFunctionBoolRight = false;
            }

            if (_updateFunctionBoolLeft)
            {
                _updateFunctionStopwatchLeft.Stop();
                _updateFunctionStopwatchLeft.Reset();
                _updateFunctionStopwatchLeft.Start();
                _updateFunctionBoolLeft = false;
            }

            if (_updateFunctionBoolLeftHandTrigger)
            {
                _updateFunctionStopwatchLeftHandTrigger.Stop();
                _updateFunctionStopwatchLeftHandTrigger.Reset();
                _updateFunctionStopwatchLeftHandTrigger.Start();
                _updateFunctionBoolLeftHandTrigger = false;
            }

            if (_updateFunctionBoolRightHandTrigger)
            {
                _updateFunctionStopwatchRightHandTrigger.Stop();
                _updateFunctionStopwatchRightHandTrigger.Reset();
                _updateFunctionStopwatchRightHandTrigger.Start();
                _updateFunctionBoolRightHandTrigger = false;
            }

            if (_updateFunctionBoolLeftThumbStickGoRight)
            {
                _updateFunctionStopwatchLeftThumbstickGoRight.Stop();
                _updateFunctionStopwatchLeftThumbstickGoRight.Reset();
                _updateFunctionStopwatchLeftThumbstickGoRight.Start();
                _updateFunctionBoolLeftThumbStickGoRight = false;
            }

            if (_updateFunctionBoolLeftThumbStickGoLeft)
            {
                _updateFunctionStopwatchLeftThumbstickGoLeft.Stop();
                _updateFunctionStopwatchLeftThumbstickGoLeft.Reset();
                _updateFunctionStopwatchLeftThumbstickGoLeft.Start();
                _updateFunctionBoolLeftThumbStickGoLeft = false;
            }

            if (_updateFunctionBoolRightIndexTrigger)
            {
                _updateFunctionStopwatchRightIndexTrigger.Stop();
                _updateFunctionStopwatchRightIndexTrigger.Reset();
                _updateFunctionStopwatchRightIndexTrigger.Start();
                _updateFunctionBoolRightIndexTrigger = false;
            }

            if (_updateFunctionBoolLeftIndexTrigger)
            {
                _updateFunctionStopwatchLeftIndexTrigger.Stop();
                _updateFunctionStopwatchLeftIndexTrigger.Reset();
                _updateFunctionStopwatchLeftIndexTrigger.Start();
                _updateFunctionBoolLeftIndexTrigger = false;
            }

            if (_updateFunctionBoolTouchRightButtonA)
            {
                _updateFunctionStopwatchTouchRightButtonA.Stop();
                _updateFunctionStopwatchTouchRightButtonA.Reset();
                _updateFunctionStopwatchTouchRightButtonA.Start();
                _updateFunctionBoolTouchRightButtonA = false;
            }

            if (_updateFunctionBoolRightThumbStickGoLeft)
            {
                _updateFunctionStopwatchRightThumbstickGoLeft.Stop();
                _updateFunctionStopwatchRightThumbstickGoLeft.Reset();
                _updateFunctionStopwatchRightThumbstickGoLeft.Start();
                _updateFunctionBoolRightThumbStickGoLeft = false;
            }

            if (_updateFunctionBoolRightThumbStickGoRight)
            {
                _updateFunctionStopwatchRightThumbstickGoRight.Stop();
                _updateFunctionStopwatchRightThumbstickGoRight.Reset();
                _updateFunctionStopwatchRightThumbstickGoRight.Start();
                _updateFunctionBoolRightThumbStickGoRight = false;
            }


                if (_screenControlStopWatchBool)
                {
                    _stopWatchForScreenControl.Stop();
                    _stopWatchForScreenControl.Reset();
                    _stopWatchForScreenControl.Start();
                    _screenControlStopWatchBool = false;
                }


            //if (_desktopFrame == null)
            {
                //Thread.Sleep(1);
                //goto _threadLoop;
            }
            for (int i = 0; i < 3;)
            {
                _failed = false;
                try
                {
                    _desktopFrame = _desktopDupe.ScreenCapture(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    _desktopDupe = new SC_SharpDX_ScreenCapture(0, 0);
                    _failed = true;
                }
                i++;
                if (!_failed)
                {
                    break;
                }
            }
            Thread.Sleep(1);

            _frameCounter = _mainThreadStopWatch.Elapsed.Seconds;
            if (_frameCounter >= 1)
            {
                _mainThreadStopWatch.Stop();
                Console.WriteLine(_updateSceneFrameCounter + " fps");
                _stopWatchSwitch = true;
                _updateSceneFrameCounter = 0;
            }

            //Thread.Sleep(1);

            ///if (_canRun)
            {
                if (_startOnce0)
                {
                    if (_bitmap != null)
                    {
                        var imageBoundaries00 = new System.Drawing.Rectangle(0, 0, _desktopFrame.width, _desktopFrame.height);
                        var imageData0 = _bitmap.LockBits(imageBoundaries00, ImageLockMode.ReadOnly, _bitmap.PixelFormat);
                        strider = imageData0.Stride;
                        _bitmap.UnlockBits(imageData0);
                        _startOnce0 = false;
                    }
                }

                    if (_createdSceneObjects)
                    {
                        if (_oncer)
                        {
                            /*point3DCollectionTemp = point3DCollection;

                            lowestX = point3DCollectionTemp.OrderBy(x => x.X).FirstOrDefault();
                            highestX = point3DCollectionTemp.OrderBy(x => x.X).Last();

                            lowestY = point3DCollectionTemp.OrderBy(y => y.X).FirstOrDefault();
                            highestY = point3DCollectionTemp.OrderBy(y => y.X).Last();

                            lowestZ = point3DCollectionTemp.OrderBy(z => z.X).FirstOrDefault();
                            highestZ = point3DCollectionTemp.OrderBy(z => z.X).Last();

                            totalWidthX = highestX.X - lowestX.X;
                            totalHeightY = highestY.Y - lowestY.Y;*/

                            _oncer = false;
                        }

                        point3DCollectionTemp = point3DCollection;

                        /*lowestX = point3DCollectionTemp.OrderBy(x => x.X).FirstOrDefault();
                        highestX = point3DCollectionTemp.OrderBy(x => x.X).Last();

                        lowestY = point3DCollectionTemp.OrderBy(y => y.X).FirstOrDefault();
                        highestY = point3DCollectionTemp.OrderBy(y => y.X).Last();

                        lowestZ = point3DCollectionTemp.OrderBy(z => z.X).FirstOrDefault();
                        highestZ = point3DCollectionTemp.OrderBy(z => z.X).Last();

                        totalWidthX = highestX.X - lowestX.X;
                        totalHeightY = highestY.Y - lowestY.Y;*/

                        var someTest = _ovr.GetSessionStatus(sessionPtr, ref _ovrHeadset);

                        if (_ovrHeadset.HmdMounted)
                        {
                            //var connectedControllerTypes = _ovr.GetConnectedControllerTypes(sessionPtr);
                            var resultsRight = _ovr.GetInputState(sessionPtr, controllerTypeRTouch, ref inputStateRTouch);
                            buttonPressedOculusTouchRight = inputStateRTouch.Buttons;
                            var thumbStickRight = inputStateRTouch.Thumbstick;
                            var handTriggerRight = inputStateRTouch.HandTrigger;
                            var indexTriggerRight = inputStateRTouch.IndexTrigger;

                            var resultsLeft = _ovr.GetInputState(sessionPtr, controllerTypeLTouch, ref inputStateLTouch);
                            buttonPressedOculusTouchLeft = inputStateLTouch.Buttons;
                            var thumbStickLeft = inputStateLTouch.Thumbstick;
                            var handTriggerLeft = inputStateLTouch.HandTrigger;
                            var indexTriggerLeft = inputStateLTouch.IndexTrigger;

                            //RIGHT CONTROLLER HITPOINT
                            //////////////////////////////////////////////////////////////////////////
                            _rightControllerQuaternion.X = _XDOUBLErightControllerQuaternionRotation3D;
                            _rightControllerQuaternion.Y = _YDOUBLErightControllerQuaternionRotation3D;
                            _rightControllerQuaternion.Z = _ZDOUBLErightControllerQuaternionRotation3D;
                            _rightControllerQuaternion.W = _WDOUBLErightControllerQuaternionRotation3D;

                            var centerPosRight = new Point3D(_XDOUBLErightControllerTranslateTransform3D, _YDOUBLErightControllerTranslateTransform3D, _ZDOUBLErightControllerTranslateTransform3D);    //rightControllerVisual3D 
                            var rayoriginRight = centerPosRight;

                            var rayDirRight = _getDirection(Vector3.ForwardRH, _rightControllerQuaternion);

                            //var rayDirRight = getDirection(_rightControllerQuaternion, Vector3.ForwardLH);
                            rayDirectionRight.X = rayDirRight.X;
                            rayDirectionRight.Y = rayDirRight.Y;
                            rayDirectionRight.Z = rayDirRight.Z;

                            Ab3d.Utilities.Plane.RayPlaneIntersection(rayoriginRight, rayDirectionRight, pointOnPlane, planeNormal, out intersectionRight); //bool resultsRighter

                            /*if (currentFrameRight < arrayOfStabilizerPosRight.Length)
                            {
                                arrayOfStabilizerPosXRight[currentFrameRight] = intersectionRight.X;
                                arrayOfStabilizerPosYRight[currentFrameRight] = intersectionRight.Y;
                                arrayOfStabilizerPosZRight[currentFrameRight] = intersectionRight.Z;
                                arrayOfStabilizerPosRight[currentFrameRight] = intersectionRight;
                            }
                            else
                            {
                                double differenceX = 0;
                                double differenceY = 0;
                                double differenceZ = 0;

                                int j = 1;

                                for (int i = 0; i < arrayOfStabilizerPosXRight.Length - 1; i++, j++)
                                {
                                    var currentX = arrayOfStabilizerPosXRight[j];
                                    var currentY = arrayOfStabilizerPosYRight[j];
                                    var currentZ = arrayOfStabilizerPosZRight[j];
                                    lastRightHitPointXFrameOne = arrayOfStabilizerPosXRight[i];
                                    lastRightHitPointYFrameOne = arrayOfStabilizerPosYRight[i];
                                    lastRightHitPointZFrameOne = arrayOfStabilizerPosZRight[i];

                                    if (lastRightHitPointXFrameOne >= currentX)
                                    {
                                        differenceX = lastRightHitPointXFrameOne - currentX;
                                    }
                                    else
                                    {
                                        differenceX = currentX - lastRightHitPointXFrameOne;
                                    }

                                    arrayOfStabilizerPosDifferenceXRight[i] = differenceX;

                                    if (lastRightHitPointYFrameOne >= currentY)
                                    {
                                        differenceY = lastRightHitPointYFrameOne - currentY;
                                    }
                                    else
                                    {
                                        differenceY = currentY - lastRightHitPointYFrameOne;
                                    }
                                    arrayOfStabilizerPosDifferenceYRight[i] = differenceY;

                                    if (lastRightHitPointZFrameOne >= currentZ)
                                    {
                                        differenceZ = lastRightHitPointZFrameOne - currentZ;
                                    }
                                    else
                                    {
                                        differenceZ = currentZ - lastRightHitPointZFrameOne;
                                    }
                                    arrayOfStabilizerPosDifferenceZRight[i] = differenceZ;
                                }

                                averageXRight = 0;
                                averageYRight = 0;
                                averageZRight = 0;

                                for (int i = 0; i < arrayOfStabilizerPosDifferenceXRight.Length; i++)
                                {
                                    averageXRight += arrayOfStabilizerPosDifferenceXRight[i];
                                    averageYRight += arrayOfStabilizerPosDifferenceYRight[i];
                                    averageZRight += arrayOfStabilizerPosDifferenceZRight[i];
                                }

                                averageXRight = averageXRight / arrayOfStabilizerPosDifferenceXRight.Length;
                                averageYRight = averageYRight / arrayOfStabilizerPosDifferenceYRight.Length;
                                averageZRight = averageZRight / arrayOfStabilizerPosDifferenceZRight.Length;

                                arrayOfStabilizerPosXRight = new double[arrayOfStabilizerPosRight.Length];
                                arrayOfStabilizerPosDifferenceXRight = new double[arrayOfStabilizerPosXRight.Length - 1];

                                arrayOfStabilizerPosYRight = new double[arrayOfStabilizerPosRight.Length];
                                arrayOfStabilizerPosDifferenceYRight = new double[arrayOfStabilizerPosYRight.Length - 1];

                                arrayOfStabilizerPosZRight = new double[arrayOfStabilizerPosRight.Length];
                                arrayOfStabilizerPosDifferenceZRight = new double[arrayOfStabilizerPosZRight.Length - 1];


                                restartFrameCounterRight = true;
                            }

                            if (!restartFrameCounterRight)
                            {
                                currentFrameRight++;
                            }
                            else
                            {
                                currentFrameRight = 0;
                                restartFrameCounterRight = false;
                            }

                            Point3D centerRight = new Point3D(0, 0, 0);

                            float count = 0;
                            for (int i = 0; i < arrayOfStabilizerPosRight.Length; i++)
                            {
                                centerRight.X += arrayOfStabilizerPosRight[i].X;
                                centerRight.Y += arrayOfStabilizerPosRight[i].Y;
                                centerRight.Z += arrayOfStabilizerPosRight[i].Z;
                                count++;
                            }

                            theCenterRight.X = centerRight.X / count;
                            theCenterRight.Y = centerRight.Y / count;
                            theCenterRight.Z = centerRight.Z / count;

                            positionXRight = theCenterRight.X;
                            positionYRight = theCenterRight.Y;*/

                            positionXRight = intersectionRight.X;
                            positionYRight = intersectionRight.Y;
                            positionZRight = intersectionRight.Z;

                            /////////////////////////////////////////////////////////////////////////////////////////////////////

                            //cosC = a2+b2-c2/2ab
                            //cosA = b2 +c2 - a2/2bc
                            //cosB = c2+a2-b2/2ca

                            //sin = hyp/opp
                            //cos = adj/hyp
                            //tan = opp/adj

                            double deltaX = intersectionRight.X - point3DCollectionTemp[0].X;
                            double deltaY = intersectionRight.Y - point3DCollectionTemp[0].Y;
                            double deltaZ = intersectionRight.Z - point3DCollectionTemp[0].Z;
                            var sideA = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));

                            double deltaXX = intersectionRight.X - point3DCollectionTemp[2].X;
                            double deltaYY = intersectionRight.Y - point3DCollectionTemp[2].Y;
                            double deltaZZ = intersectionRight.Z - point3DCollectionTemp[2].Z;
                            var sideB = Math.Sqrt((deltaXX * deltaXX) + (deltaYY * deltaYY) + (deltaZZ * deltaZZ));

                            double deltaXXX = point3DCollectionTemp[2].X - point3DCollectionTemp[0].X;
                            double deltaYYY = point3DCollectionTemp[2].Y - point3DCollectionTemp[0].Y;
                            double deltaZZZ = point3DCollectionTemp[2].Z - point3DCollectionTemp[0].Z;
                            var sideC = Math.Sqrt((deltaXXX * deltaXXX) + (deltaYYY * deltaYYY) + (deltaZZZ * deltaZZZ));

                            //LAW OF COSANUS
                            var angleA = ((sideB * sideB) + (sideC * sideC) - (sideA * sideA)) / (2 * sideB * sideC);
                            var angleRad = Math.Acos(angleA);
                            ////////////////

                            var oppositeSide = sideB * Math.Sin(angleRad);

                            var adjacentSide = Math.Cos(angleRad) * sideB;

                            double deltaXTotal = point3DCollectionTemp[1].X - point3DCollectionTemp[0].X;
                            double deltaYTotal = point3DCollectionTemp[1].Y - point3DCollectionTemp[0].Y;
                            double deltaZTotal = point3DCollectionTemp[1].Z - point3DCollectionTemp[0].Z;
                            var distanceTotalX = Math.Sqrt((deltaXTotal * deltaXTotal) + (deltaYTotal * deltaYTotal) + (deltaZTotal * deltaZTotal));

                            var percentXRight = (oppositeSide * _width) / distanceTotalX;

                            var percentYRight = (adjacentSide * _height) / sideC;




































                            /*//LEFT CONTROLLER HITPOINT
                            //////////////////////////////////////////////////////////////////////////
                            _leftControllerQuaternion.X = _XDOUBLEleftControllerQuaternionRotation3D;
                            _leftControllerQuaternion.Y = _YDOUBLEleftControllerQuaternionRotation3D;
                            _leftControllerQuaternion.Z = _ZDOUBLEleftControllerQuaternionRotation3D;
                            _leftControllerQuaternion.W = _WDOUBLEleftControllerQuaternionRotation3D;

                            var centerPosLeft = new Point3D(-_XDOUBLEleftControllerTranslateTransform3D, -_YDOUBLEleftControllerTranslateTransform3D, -_ZDOUBLEleftControllerTranslateTransform3D);
                            var rayoriginLeft = centerPosLeft;



                            var rayDirLeft = getDirection(_leftControllerQuaternion, Vector3.ForwardLH);
                            rayDirectionLeft.X = rayDirLeft.X;
                            rayDirectionLeft.Y = rayDirLeft.Y;
                            rayDirectionLeft.Z = rayDirLeft.Z;

                            Ab3d.Utilities.Plane.RayPlaneIntersection(rayoriginLeft, rayDirectionLeft, pointOnPlane, planeNormal, out intersectionLeft); //bool resultsLefter

                            if (currentFrameLeft < arrayOfStabilizerPosLeft.Length)
                            {
                                arrayOfStabilizerPosXLeft[currentFrameLeft] = intersectionLeft.X;
                                arrayOfStabilizerPosYLeft[currentFrameLeft] = intersectionLeft.Y;
                                arrayOfStabilizerPosLeft[currentFrameLeft] = intersectionLeft;
                            }
                            else
                            {
                                double differenceX = 0;
                                double differenceY = 0;
                                int j = 1;
                                for (int i = 0; i < arrayOfStabilizerPosXLeft.Length - 1; i++, j++)
                                {
                                    var currentX = arrayOfStabilizerPosXLeft[j];
                                    var currentY = arrayOfStabilizerPosYLeft[j];
                                    lastLeftHitPointXFrameOne = arrayOfStabilizerPosXLeft[i];
                                    lastLeftHitPointYFrameOne = arrayOfStabilizerPosYLeft[i];

                                    if (lastLeftHitPointXFrameOne >= currentX)
                                    {
                                        differenceX = lastLeftHitPointXFrameOne - currentX;
                                    }
                                    else
                                    {
                                        differenceX = currentX - lastLeftHitPointXFrameOne;
                                    }

                                    arrayOfStabilizerPosDifferenceXLeft[i] = differenceX;

                                    if (lastLeftHitPointYFrameOne >= currentY)
                                    {
                                        differenceY = lastLeftHitPointYFrameOne - currentY;
                                    }
                                    else
                                    {
                                        differenceY = currentY - lastLeftHitPointYFrameOne;
                                    }

                                    arrayOfStabilizerPosDifferenceYLeft[i] = differenceY;
                                }

                                averageXLeft = 0;
                                averageYLeft = 0;

                                for (int i = 0; i < arrayOfStabilizerPosDifferenceXLeft.Length; i++)
                                {
                                    averageXLeft += arrayOfStabilizerPosDifferenceXLeft[i];
                                    averageYLeft += arrayOfStabilizerPosDifferenceYLeft[i];
                                }

                                averageXLeft = averageXLeft / arrayOfStabilizerPosDifferenceXLeft.Length;
                                averageYLeft = averageYLeft / arrayOfStabilizerPosDifferenceYLeft.Length;

                                arrayOfStabilizerPosXLeft = new double[arrayOfStabilizerPosLeft.Length];
                                arrayOfStabilizerPosDifferenceXLeft = new double[arrayOfStabilizerPosXLeft.Length - 1];

                                arrayOfStabilizerPosYLeft = new double[arrayOfStabilizerPosLeft.Length];
                                arrayOfStabilizerPosDifferenceYLeft = new double[arrayOfStabilizerPosYLeft.Length - 1];
                                restartFrameCounterLeft = true;
                            }

                            if (!restartFrameCounterLeft)
                            {
                                currentFrameLeft++;
                            }
                            else
                            {
                                currentFrameLeft = 0;
                                restartFrameCounterLeft = false;
                            }

                            Point3D centerLeft = new Point3D(0, 0, 0);

                            float countLeft = 0;
                            for (int i = 0; i < arrayOfStabilizerPosLeft.Length; i++)
                            {
                                centerLeft.X += arrayOfStabilizerPosLeft[i].X;
                                centerLeft.Y += arrayOfStabilizerPosLeft[i].Y;
                                centerLeft.Z += arrayOfStabilizerPosLeft[i].Z;
                                countLeft++;
                            }

                            theCenterLeft.X = centerLeft.X / countLeft;
                            theCenterLeft.Y = centerLeft.Y / countLeft;
                            theCenterLeft.Z = centerLeft.Z / countLeft;

                            positionXLeft = -theCenterLeft.X;
                            positionYLeft = -theCenterLeft.Y;

                            var positionFromLeftToHitPointXLeft = positionXLeft - (lowestX.X + 100);
                            var percentXLeft = positionFromLeftToHitPointXLeft / totalWidthX;

                            percentXLeft *= _width;

                            var positionFromBottomToHitPointYLeft = positionYLeft - (highestY.Y + 100);
                            var percentYLeft = positionFromBottomToHitPointYLeft / totalHeightY;

                            percentYLeft *= -_height;
                            /////////////////////////////////////////////////////////////////////////////////////////////////////*/

                            var percentXLeft = 0;
                            var percentYLeft = 0;

                             _MicrosoftWindowsMouse(percentXRight, percentYRight, thumbStickRight, percentXLeft, percentYLeft, thumbStickLeft);

                             lastHasUsedHandTriggerLeft = hasUsedHandTriggerLeft;
                            lastbuttonPressedOculusTouchRight = buttonPressedOculusTouchRight;
                            lastbuttonPressedOculusTouchLeft = buttonPressedOculusTouchLeft;
                        }
                    }

                    /*_frameCounter = _mainThreadStopWatch.Elapsed.Seconds;
                    if (_frameCounter >= 1)
                    {
                        _mainThreadStopWatch.Stop();
                        Console.WriteLine(_updateSceneFrameCounter + " fps");
                        _stopWatchSwitch = true;
                        _updateSceneFrameCounter = 0;
                    }*/

                    if (_mainThreadFrameCounter > 1000)
                    {
                        GC.Collect();
                        _mainThreadFrameCounter = 0;
                    }
                }


                //lastClickButtonA = hasClickedBUTTONA;
                //hasResetedMouse = canResetMouse;

                _updateFunctionFrameCounter++;
                _mainThreadFrameCounter++;
                _updateSceneFrameCounter++;

                Thread.Sleep(1);
                goto _threadLoop;
            };
            backgroundWorker00.RunWorkerAsync();

            // Now we can create our sample 3D scene
            // Add lights

            var lightsVisual3D = new ModelVisual3D();
            var lightsGroup = new Model3DGroup();

            var directionalLight = new DirectionalLight(Colors.White, new Vector3D(0.5, -0.3, -0.3));
            directionalLight.SetDXAttribute(DXAttributeType.IsCastingShadow, false); // Set this light to cast shadow
            lightsGroup.Children.Add(directionalLight);

            var ambientLight = new AmbientLight(System.Windows.Media.Color.FromRgb(30, 30, 30));
            lightsGroup.Children.Add(ambientLight);

            lightsVisual3D.Content = lightsGroup;
            _viewport3D.Children.Add(lightsVisual3D);

            // Start rendering
            if (RenderAt90Fps)
            {
                // WPF do not support rendering at more the 60 FPS.
                // But with a trick where a rendering loop is created in a background thread, it is possible to achieve more than 60 FPS.
                // In case of sumbiting frames to Oculus Rift, the ovr.SubmitFrame method will limit rendering to 90 FPS.
                // 
                // NOTE:
                // When using DXEngine, it is also possible to render the scene in a background thread. 
                // This requires that the 3D scene is also created in the background thread and that the events and other messages are 
                // passed between UI and background thread in a thread safe way. This is too complicated for this simple sample project.
                // To see one possible implementation of background rendering, see the BackgroundRenderingSample in the Ab3d.DXEngine.Wpf.Samples project.
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (object sender, DoWorkEventArgs args) =>
                {
                    var refreshDXEngineAction = new Action(delegate
                    {
                        UpdateScene();

                        // Render DXEngine's 3D scene again
                        if (_dxViewportView != null)
                            _dxViewportView.Refresh();
                    });

                    while (_dxViewportView != null && !_dxViewportView.IsDisposed) // Render until window is closed
                    {
                        if (_oculusRiftVirtualRealityProvider != null && _oculusRiftVirtualRealityProvider.LastSessionStatus.ShouldQuit) // Stop rendering - this will call RunWorkerCompleted where we can quit the application
                            break;

                        // Sleep for 1 ms to allow WPF tasks to complete (for example handling XBOX controller events)
                        System.Threading.Thread.Sleep(1);

                        // Call Refresh to render the DXEngine's scene
                        // This is a synchronous call and will wait until the scene is rendered. 
                        // Because Oculus is limited to 90 fps, the call to ovr.SubmitFrame will limit rendering to 90 FPS.

                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, refreshDXEngineAction); //System.Windows.Threading.DispatcherPriority.Normal
                    }
                };

                backgroundWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
                {
                    if (_oculusRiftVirtualRealityProvider != null && _oculusRiftVirtualRealityProvider.LastSessionStatus.ShouldQuit)
                    {

                    }
                    //this.Close(); // Exit the application
                };

                backgroundWorker.RunWorkerAsync();


            }
            else
            {
                // Subscribe to WPF rendering event (called approximately 60 times per second)
                CompositionTarget.Rendering += CompositionTargetOnRendering;
            }
        }
        #endregion




        private void _MicrosoftWindowsMouse(double percentXRight, double percentYRight, Vector2f[] thumbStickRight, double percentXLeft, double percentYLeft, Vector2f[] thumbStickLeft)
        {
            if (_indexMouseMove == 0)
            {
                /////////////RIGHT OCULUS TOUCH/////////////////////////////////////////////////////////////////////////////////////
                if (percentXRight >= 0 && percentXRight <= _width && percentYRight >= 0 && percentYRight <= _height)
                {
                    var absoluteMoveX = Convert.ToUInt32((percentXRight * 65535) / _width);
                    var absoluteMoveY = Convert.ToUInt32((percentYRight * 65535) / _height);
                    //var absoluteMoveX = Convert.ToUInt32((percentXRight ));
                    //var absoluteMoveY = Convert.ToUInt32((percentYRight ));

                    var yo = _updateFunctionStopwatchRight.Elapsed.Milliseconds;

                    if (yo >= 10)
                    {
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, absoluteMoveX, absoluteMoveY, 0, 0);
                        //mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, absoluteMoveX, absoluteMoveY, 0, 0);
                        _updateFunctionStopwatchRight.Stop();
                        _updateFunctionBoolRight = true;
                    }

                    //MOUSE DOUBLE CLICK LOGIC. IF the PLAYER clicked at one location then it stores the location, and if the player re-clicks inside of 20 frames, then click at the first click location.
                    //It's very basic, and at least I should implement also a certain "radius" of distance from the first click and the second click... If the second click is too far from the first click,
                    //then disregard the first click location.

                    //Console.WriteLine(buttonPressedOculusTouchRight + "***" + buttonPressedOculusTouchLeft);

                    if (buttonPressedOculusTouchRight != 0)
                    {
                        if (_activateScreenModControl == false)
                        {
                            if (buttonPressedOculusTouchRight == 1)
                            {
                                if (hasClickedBUTTONA == false)
                                {
                                    if (_frameCounterTouchRight <= 20 && _canResetCounterTouchRightButtonA == true)
                                    {
                                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, _lastMousePosXRight, _lastMousePosYRight, 0, 0);
                                        _frameCounterTouchRight = 0;
                                    }

                                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                                    _lastMousePosXRight = absoluteMoveX;
                                    _lastMousePosYRight = absoluteMoveY;
                                    _canResetCounterTouchRightButtonA = true;
                                    hasClickedBUTTONA = true;
                                }
                            }
                            else if (buttonPressedOculusTouchRight == 2)
                            {
                                if (hasClickedBUTTONB == false)
                                {
                                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                                    hasClickedBUTTONB = true;
                                }
                            }
                            else if (buttonPressedOculusTouchRight == 4)
                            {
                                int _milliScreenControl = _stopWatchForScreenControl.Elapsed.Milliseconds;

                                if (_milliScreenControl >= 200)
                                {
                                    _activateScreenModControl = true;
                                    //_stopWatchForScreenControl.Stop();
                                    _screenControlStopWatchBool = true;
                                }
                            }
                        }
                        /*else
                        {
                            if (buttonPressedOculusTouchRight == 1)
                            {
                                var transform3DGroup00 = new Transform3DGroup();

                                System.Windows.Media.Media3D.Quaternion _newQuat = new System.Windows.Media.Media3D.Quaternion(new Vector3D(0, 1, 0), _rotateSpeed);
                                _desktopScreenQuaternionRotation3D.Quaternion = new System.Windows.Media.Media3D.Quaternion(_newQuat.X, _newQuat.Y, _newQuat.Z, _newQuat.W);

                                //Vector3 _dir = getDirection(_desktopScreenQuaternionRotation3D.Quaternion,Vector3.ForwardLH);

                                Matrix3D m = Matrix3D.Identity;
                                //System.Windows.Media.Media3D.Quaternion q = new System.Windows.Media.Media3D.Quaternion(new Vector3D(0, 1, 0), 0.01f);
                                m.Rotate(_newQuat);

                                Point3D[] _oldPoint3dCollection = point3DCollection;
                                point3DCollection = new Point3D[_oldPoint3dCollection.Length];
                                _point3DCollection = new Point3DCollection();

                                for (int i = 0; i < _oldPoint3dCollection.Length; i++)
                                {
                                    Vector3D myVectorToRotate = new Vector3D(0, 0, 0);

                                    myVectorToRotate.X = _oldPoint3dCollection[i].X;
                                    myVectorToRotate.Y = _oldPoint3dCollection[i].Y;
                                    myVectorToRotate.Z = _oldPoint3dCollection[i].Z;

                                    Vector3D myVectorResult = m.Transform(myVectorToRotate);
                                    point3DCollection[i] = new Point3D(myVectorResult.X, myVectorResult.Y, myVectorResult.Z);
                                    _point3DCollection.Add(point3DCollection[i]);
                                    _screenCornersTotal[i].CenterPosition = point3DCollection[i];
                                }

                                meshGeometry.Positions = _point3DCollection;

                                plane = new Ab3d.Utilities.Plane(_point3DCollection[0], _point3DCollection[1], _point3DCollection[2]);
                                pointOnPlane = plane.GetPointOnPlane();
                                planeNormal = plane.Normal;

                                _standardMaterial = new Ab3d.DirectX.Materials.StandardMaterial()
                                {
                                    DiffuseColor = new SharpDX.Color3(1f, 1f, 1f),
                                    HasTransparency = false,
                                };

                                _standardMaterial.PreferedShaderQuality = ShaderQuality.Low;

                                vertexColorDiffuseMaterial = new DiffuseMaterial(System.Windows.Media.Brushes.Black);
                                vertexColorDiffuseMaterial.SetUsedDXMaterial(_standardMaterial);

                                _desktopScreen.Material = vertexColorDiffuseMaterial;

                                _shaderResourceView = new ShaderResourceView(_dxViewportView.DXScene.DXDevice.Device, _desktopFrame._texture2d, resourceViewDescription);

                                if (_shaderResourceView != null)
                                {
                                    _standardMaterial.DiffuseTextures = new ShaderResourceView[]
                                    {
                                        _shaderResourceView,
                                    };
                                }
                            }
                            else if (buttonPressedOculusTouchRight == 2)
                            {
                                var transform3DGroup00 = new Transform3DGroup();

                                System.Windows.Media.Media3D.Quaternion _newQuat = new System.Windows.Media.Media3D.Quaternion(new Vector3D(0, 1, 0), -_rotateSpeed);
                                _desktopScreenQuaternionRotation3D.Quaternion = new System.Windows.Media.Media3D.Quaternion(_newQuat.X, _newQuat.Y, _newQuat.Z, _newQuat.W);

                                //Vector3 _dir = getDirection(_desktopScreenQuaternionRotation3D.Quaternion,Vector3.ForwardLH);

                                Matrix3D m = Matrix3D.Identity;
                                //System.Windows.Media.Media3D.Quaternion q = new System.Windows.Media.Media3D.Quaternion(new Vector3D(0, 1, 0), 0.01f);
                                m.Rotate(_newQuat);

                                Point3D[] _oldPoint3dCollection = point3DCollection;
                                point3DCollection = new Point3D[_oldPoint3dCollection.Length];
                                _point3DCollection = new Point3DCollection();

                                for (int i = 0; i < _oldPoint3dCollection.Length; i++)
                                {
                                    Vector3D myVectorToRotate = new Vector3D(0, 0, 0);

                                    myVectorToRotate.X = _oldPoint3dCollection[i].X;
                                    myVectorToRotate.Y = _oldPoint3dCollection[i].Y;
                                    myVectorToRotate.Z = _oldPoint3dCollection[i].Z;

                                    Vector3D myVectorResult = m.Transform(myVectorToRotate);
                                    point3DCollection[i] = new Point3D(myVectorResult.X, myVectorResult.Y, myVectorResult.Z);
                                    _point3DCollection.Add(point3DCollection[i]);
                                    _screenCornersTotal[i].CenterPosition = point3DCollection[i];
                                }

                                meshGeometry.Positions = _point3DCollection;

                                plane = new Ab3d.Utilities.Plane(_point3DCollection[0], _point3DCollection[1], _point3DCollection[2]);
                                pointOnPlane = plane.GetPointOnPlane();
                                planeNormal = plane.Normal;

                                _standardMaterial = new Ab3d.DirectX.Materials.StandardMaterial()
                                {
                                    DiffuseColor = new SharpDX.Color3(1f, 1f, 1f),
                                    HasTransparency = false,
                                };

                                _standardMaterial.PreferedShaderQuality = ShaderQuality.Low;

                                vertexColorDiffuseMaterial = new DiffuseMaterial(System.Windows.Media.Brushes.Black);
                                vertexColorDiffuseMaterial.SetUsedDXMaterial(_standardMaterial);

                                _desktopScreen.Material = vertexColorDiffuseMaterial;

                                _shaderResourceView = new ShaderResourceView(_dxViewportView.DXScene.DXDevice.Device, _desktopFrame._texture2d, resourceViewDescription);

                                if (_shaderResourceView != null)
                                {
                                    _standardMaterial.DiffuseTextures = new ShaderResourceView[]
                                    {
                                    _shaderResourceView,
                                    };
                                }
                            }
                            else if (buttonPressedOculusTouchRight == 4)
                            {
                                int _milliScreenControl = _stopWatchForScreenControl.Elapsed.Milliseconds;

                                if (_milliScreenControl >= 200)
                                {
                                    _activateScreenModControl = false;
                                    //_stopWatchForScreenControl.Stop();
                                    _screenControlStopWatchBool = true;
                                }
                            }
                        }*/
                    }
                }

                if (_activateScreenModControl == false)
                {
                    //////////OCULUS TOUCH BUTTONS PRESSED////////////////////////////////////////
                    if (hasClickedBUTTONA && buttonPressedOculusTouchRight == 0 || hasClickedBUTTONB && buttonPressedOculusTouchRight == 0)
                    {
                        if (hasClickedBUTTONA && buttonPressedOculusTouchRight == 0)
                        {
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            hasClickedBUTTONA = false;
                        }
                        else if (hasClickedBUTTONB && buttonPressedOculusTouchRight == 0)
                        {
                            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                            hasClickedBUTTONB = false;
                        }
                    }

                    if (_canResetCounterTouchRightButtonA)
                    {
                        _frameCounterTouchRight++;
                    }

                    if (_frameCounterTouchRight >= 30)
                    {
                        _frameCounterTouchRight = 0;
                        _canResetCounterTouchRightButtonA = false;
                    }

                    if (buttonPressedOculusTouchLeft != 0)
                    {
                        if (buttonPressedOculusTouchLeft == 1024 && hasClickedBUTTONX == false)
                        {
                            string windowsKeyboard = "osk";

                            foreach (Process clsProcess in Process.GetProcesses())
                            {
                                if (clsProcess.ProcessName.ToLower().Contains(windowsKeyboard.ToLower()))
                                {
                                    break;
                                }
                                else
                                {
                                    Process proc = new Process();
                                    proc.StartInfo.FileName = windowsKeyboard + ".exe";
                                    proc.Start();
                                    break;
                                }
                            }
                            hasClickedBUTTONX = true;
                        }

                        else if (buttonPressedOculusTouchLeft == 1048576 && buttonPressedOculusTouchLeft != lastbuttonPressedOculusTouchLeft)
                        {
                            if (hasClickedHomeButtonTouchLeft == false)
                            {
                                _ovr.RecenterTrackingOrigin(_oculusRiftVirtualRealityProvider.SessionPtr);
                                hasClickedHomeButtonTouchLeft = true;
                            }
                        }
                    }
                    else if (buttonPressedOculusTouchLeft == 0 && hasClickedHomeButtonTouchLeft || buttonPressedOculusTouchLeft == 0 && hasClickedBUTTONX)
                    {
                        if (buttonPressedOculusTouchLeft == 0 && hasClickedHomeButtonTouchLeft)
                        {
                            hasClickedHomeButtonTouchLeft = false;
                        }
                        else if (buttonPressedOculusTouchLeft == 0 && hasClickedBUTTONX)
                        {
                            hasClickedBUTTONX = false;
                        }
                    }

                    /*//////////OCULUS TOUCH BUTTONS NOT PRESSED////////////////////////////////////////
               long test = 80;
               /////////RIGHT THUMBSTICK///////////
               var yo6 = _updateFunctionStopwatchRightThumbstickGoLeft.Elapsed.Milliseconds;
               if (yo6 >= 75)
               {
                   if (thumbStickRight[1].Y <= -0.1f && hasUsedThumbStickRightE == false)
                   {
                       //Console.WriteLine("test");
                       mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -test, 0);
                       hasUsedThumbStickRightE = true;
                   }
                   else if (hasUsedThumbStickRightE)
                   {
                       hasUsedThumbStickRightE = false;
                   }
                   _updateFunctionStopwatchRightThumbstickGoLeft.Stop();
                   _updateFunctionBoolRightThumbStickGoLeft = true;
               }
               ///////////////////////////////////////////////////////////////////////////

               /////////RIGHT THUMBSTICK/////////////////////////////////////////////////////
               var yo7 = _updateFunctionStopwatchRightThumbstickGoRight.Elapsed.Milliseconds;
               if (yo7 >= 75)
               {
                   if (thumbStickRight[1].Y >= 0.1f && hasUsedThumbStickRightQ == false)
                   {
                       mouse_event(MOUSEEVENTF_WHEEL, 0, 0, test, 0);
                       //hasUsedThumbStickRightQ = true;
                   }
                   else if (hasUsedThumbStickRightQ)
                   {
                       hasUsedThumbStickRightQ = false;
                   }
                   _updateFunctionStopwatchRightThumbstickGoRight.Stop();
                   _updateFunctionBoolRightThumbStickGoRight = true;
               }
               /////////////RIGHT OCULUS TOUCH////////////////////////////////////////////

               /////////////LEFT OCULUS TOUCH/////////////////////////////////////////////////////////////////////////////////////
               if (percentXLeft >= 0 && percentXLeft <= _width && percentYLeft >= 0 && percentYLeft <= _height)
               {
                   var absoluteMoveX = Convert.ToUInt32((percentXLeft * 65535) / _width);
                   var absoluteMoveY = Convert.ToUInt32((percentYLeft * 65535) / _height);

                   //var yo = _updateFunctionStopwatchLeft.Elapsed.Milliseconds;

                   //if (yo >= 10)
                   //{
                       //mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, absoluteMoveX, absoluteMoveY, 0, 0);
                   //    _updateFunctionStopwatchLeft.Stop();
                   //    _updateFunctionBoolLeft = true;
                   //}

                   //MOUSE DOUBLE CLICK LOGIC. IF the PLAYER clicked at one location then it stores the location, and if the player re-clicks inside of 20 frames, then click at the first click location.
                   //It's very basic, and at least I should implement also a certain "radius" of distance from the first click and the second click... If the second click is too far from the first click,
                   //then disregard the first click location.

                   if (buttonPressedOculusTouchLeft != 0)
                   {
                       if (buttonPressedOculusTouchLeft == 256)
                       {
                           if (hasClickedBUTTONX == false)
                           {
                               //if (_frameCounterTouchLeft <= 20 && _canResetCounterTouchLeftButtonX == true)
                               //{
                               //    mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, _lastMousePosXLeft, _lastMousePosYLeft, 0, 0);
                               //    _frameCounterTouchLeft = 0;
                               //}
                               mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, absoluteMoveX, absoluteMoveY, 0, 0);
                               mouse_event(MOUSEEVENTF_LEFTDOWN| MOUSEEVENTF_LEFTUP, absoluteMoveX, absoluteMoveY, 0, 0);
                               _lastMousePosXLeft = absoluteMoveX;
                               _lastMousePosYLeft = absoluteMoveY;
                               _canResetCounterTouchLeftButtonX = true;
                               hasClickedBUTTONX = true;
                           }
                       }
                       else if (buttonPressedOculusTouchLeft == 512)
                       {
                           if (hasClickedBUTTONY == false)
                           {
                               mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                               //_lastMousePosX = absoluteMoveX;
                               //_lastMousePosY = absoluteMoveY;
                               //_canResetCounterTouchRight = true;
                               hasClickedBUTTONY = true;
                           }
                       }
                   }
               }

               //////////OCULUS TOUCH BUTTONS PRESSED////////////////////////////////////////
               if (hasClickedBUTTONX && buttonPressedOculusTouchLeft == 0 || hasClickedBUTTONY && buttonPressedOculusTouchLeft == 0)
               {
                   if (hasClickedBUTTONX && buttonPressedOculusTouchLeft == 0)
                   {
                       //mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                       hasClickedBUTTONX = false;
                   }
                   else if (hasClickedBUTTONY && buttonPressedOculusTouchLeft == 0)
                   {
                       mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                       hasClickedBUTTONY = false;
                   }
               }

               if (_canResetCounterTouchLeftButtonX)
               {
                   _frameCounterTouchLeft++;
               }

               if (_frameCounterTouchLeft >= 30)
               {
                   _frameCounterTouchLeft = 0;
                   _canResetCounterTouchLeftButtonX = false;
               }


               //if (buttonPressedOculusTouchLeft == 0 && hasClickedHomeButtonTouchLeft)
               //{
               //    hasClickedHomeButtonTouchLeft = false;
               //}*/
                }
            }
            /*else if (_indexMouseMove == 1)
            {
                /////////////LEFT OCULUS TOUCH/////////////////////////////////////////////////////////////////////////////////////
                if (percentXLeft >= 0 && percentXLeft <= _width && percentYLeft >= 0 && percentYLeft <= _height)
                {
                    var absoluteMoveX = Convert.ToUInt32((percentXLeft * 65535) / _width);
                    var absoluteMoveY = Convert.ToUInt32((percentYLeft * 65535) / _height);

                    var yo = _updateFunctionStopwatchLeft.Elapsed.Milliseconds;

                    if (yo >= 10)
                    {
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, absoluteMoveX, absoluteMoveY, 0, 0);
                        _updateFunctionStopwatchLeft.Stop();
                        _updateFunctionBoolLeft = true;
                    }

                    //MOUSE DOUBLE CLICK LOGIC. IF the PLAYER clicked at one location then it stores the location, and if the player re-clicks inside of 20 frames, then click at the first click location.
                    //It's very basic, and at least I should implement also a certain "radius" of distance from the first click and the second click... If the second click is too far from the first click,
                    //then disregard the first click location.

                    if (buttonPressedOculusTouchLeft != 0)
                    {
                        if (buttonPressedOculusTouchLeft == 256)
                        {
                            if (hasClickedBUTTONX == false)
                            {
                                if (_frameCounterTouchLeft <= 20 && _canResetCounterTouchLeftButtonX == true)
                                {
                                    mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, _lastMousePosXLeft, _lastMousePosYLeft, 0, 0);
                                    _frameCounterTouchLeft = 0;
                                }

                                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);

                                _lastMousePosXLeft = absoluteMoveX;
                                _lastMousePosYLeft = absoluteMoveY;
                                _canResetCounterTouchLeftButtonX = true;
                                hasClickedBUTTONX = true;
                            }
                        }
                        else if (buttonPressedOculusTouchLeft == 512)
                        {
                            if (hasClickedBUTTONY == false)
                            {
                                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                                //_lastMousePosX = absoluteMoveX;
                                //_lastMousePosY = absoluteMoveY;
                                //_canResetCounterTouchRight = true;
                                hasClickedBUTTONY = true;
                            }
                        }
                    }
                }

                //////////OCULUS TOUCH BUTTONS PRESSED////////////////////////////////////////
                if (hasClickedBUTTONX && buttonPressedOculusTouchLeft == 0 || hasClickedBUTTONY && buttonPressedOculusTouchLeft == 0)
                {
                    if (hasClickedBUTTONX && buttonPressedOculusTouchLeft == 0)
                    {
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        hasClickedBUTTONX = false;
                    }
                    else if (hasClickedBUTTONY && buttonPressedOculusTouchLeft == 0)
                    {
                        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        hasClickedBUTTONY = false;
                    }
                }

                if (_canResetCounterTouchLeftButtonX)
                {
                    _frameCounterTouchLeft++;
                }

                if (_frameCounterTouchLeft >= 30)
                {
                    _frameCounterTouchLeft = 0;
                    _canResetCounterTouchLeftButtonX = false;
                }

                /////////////RIGHT OCULUS TOUCH/////////////////////////////////////////////////////////////////////////////////////
                if (percentXRight >= 0 && percentXRight <= _width && percentYRight >= 0 && percentYRight <= _height)
                {
                    var absoluteMoveX = Convert.ToUInt32((percentXRight * 65535) / _width);
                    var absoluteMoveY = Convert.ToUInt32((percentYRight * 65535) / _height);

                    //var yo = _updateFunctionStopwatchRight.Elapsed.Milliseconds;

                    //if (yo >= 10)
                    //{
                        //mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, absoluteMoveX, absoluteMoveY, 0, 0);
                     //   _updateFunctionStopwatchRight.Stop();
                    //    _updateFunctionBoolRight = true;
                    //}

                    //MOUSE DOUBLE CLICK LOGIC. IF the PLAYER clicked at one location then it stores the location, and if the player re-clicks inside of 20 frames, then click at the first click location.
                    //It's very basic, and at least I should implement also a certain "radius" of distance from the first click and the second click... If the second click is too far from the first click,
                    //then disregard the first click location.
                    if (buttonPressedOculusTouchRight != 0)
                    {
                        if (buttonPressedOculusTouchRight == 1)
                        {
                            if (hasClickedBUTTONA == false)
                            {
                                //if (_frameCounterTouchRight <= 20 && _canResetCounterTouchRightButtonA == true)
                                //{
                                //    mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, _lastMousePosXRight, _lastMousePosYRight, 0, 0);
                                //    _frameCounterTouchRight = 0;
                                //}

                                mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, absoluteMoveX, absoluteMoveY, 0, 0);
                                mouse_event(MOUSEEVENTF_LEFTDOWN| MOUSEEVENTF_LEFTUP, absoluteMoveX, absoluteMoveY, 0, 0);


                                _lastMousePosXRight = absoluteMoveX;
                                _lastMousePosYRight = absoluteMoveY;
                                _canResetCounterTouchRightButtonA = true;
                                hasClickedBUTTONA = true;
                            }
                        }
                        else if (buttonPressedOculusTouchRight == 2)
                        {
                            if (hasClickedBUTTONB == false)
                            {
                                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                                hasClickedBUTTONB = true;
                            }
                        }
                    }
                }

                //////////OCULUS TOUCH BUTTONS PRESSED////////////////////////////////////////
                if (hasClickedBUTTONA && buttonPressedOculusTouchRight == 0 || hasClickedBUTTONB && buttonPressedOculusTouchRight == 0)
                {
                    if (hasClickedBUTTONA && buttonPressedOculusTouchRight == 0)
                    {
                        //mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        hasClickedBUTTONA = false;
                    }
                    else if (hasClickedBUTTONB && buttonPressedOculusTouchRight == 0)
                    {
                        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        hasClickedBUTTONB = false;
                    }
                }

                if (_canResetCounterTouchRightButtonA)
                {
                    _frameCounterTouchRight++;
                }

                if (_frameCounterTouchRight >= 50)
                {
                    _frameCounterTouchRight = 0;
                    _canResetCounterTouchRightButtonA = false;
                }

                if (buttonPressedOculusTouchLeft != 0)
                {
                    if (buttonPressedOculusTouchLeft == 1024 && hasClickedBUTTONX == false)
                    {
                        string windowsKeyboard = "osk";

                        foreach (Process clsProcess in Process.GetProcesses())
                        {
                            if (clsProcess.ProcessName.ToLower().Contains(windowsKeyboard.ToLower()))
                            {
                                break;
                            }
                            else
                            {
                                Process proc = new Process();
                                proc.StartInfo.FileName = windowsKeyboard + ".exe";
                                proc.Start();
                                break;
                            }
                        }
                        hasClickedBUTTONX = true;
                    }

                    else if (buttonPressedOculusTouchLeft == 1048576 && buttonPressedOculusTouchLeft != lastbuttonPressedOculusTouchLeft)
                    {
                        if (hasClickedHomeButtonTouchLeft == false)
                        {
                            _ovr.RecenterTrackingOrigin(_oculusRiftVirtualRealityProvider.SessionPtr);
                            hasClickedHomeButtonTouchLeft = true;
                        }
                    }
                }
                else if (buttonPressedOculusTouchLeft == 0 && hasClickedHomeButtonTouchLeft || buttonPressedOculusTouchLeft == 0 && hasClickedBUTTONX)
                {
                    if (buttonPressedOculusTouchLeft == 0 && hasClickedHomeButtonTouchLeft)
                    {
                        hasClickedHomeButtonTouchLeft = false;
                    }
                    else if (buttonPressedOculusTouchLeft == 0 && hasClickedBUTTONX)
                    {
                        hasClickedBUTTONX = false;
                    }
                }

                //////////OCULUS TOUCH BUTTONS NOT PRESSED////////////////////////////////////////
                long test = 80;
                /////////RIGHT THUMBSTICK///////////
                var yo6 = _updateFunctionStopwatchRightThumbstickGoLeft.Elapsed.Milliseconds;
                if (yo6 >= 75)
                {
                    if (thumbStickRight[1].Y <= -0.1f && hasUsedThumbStickRightE == false)
                    {
                        //Console.WriteLine("test");
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -test, 0);
                        hasUsedThumbStickRightE = true;
                    }
                    else if (hasUsedThumbStickRightE)
                    {
                        hasUsedThumbStickRightE = false;
                    }
                    _updateFunctionStopwatchRightThumbstickGoLeft.Stop();
                    _updateFunctionBoolRightThumbStickGoLeft = true;
                }
                ///////////////////////////////////////////////////////////////////////////

                /////////RIGHT THUMBSTICK/////////////////////////////////////////////////////
                var yo7 = _updateFunctionStopwatchRightThumbstickGoRight.Elapsed.Milliseconds;
                if (yo7 >= 75)
                {
                    if (thumbStickRight[1].Y >= 0.1f && hasUsedThumbStickRightQ == false)
                    {
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, test, 0);
                        //hasUsedThumbStickRightQ = true;
                    }
                    else if (hasUsedThumbStickRightQ)
                    {
                        hasUsedThumbStickRightQ = false;
                    }
                    _updateFunctionStopwatchRightThumbstickGoRight.Stop();
                    _updateFunctionBoolRightThumbStickGoRight = true;
                }
                /////////////RIGHT OCULUS TOUCH////////////////////////////////////////////
            }*/
        }


        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        private void updateMethodForVr(object sender, EventArgs eventArgs)
        {
            //nothing yet in here. I gotta ask Andrej Benedik what the difference is between an update method created from _dxViewportView.SceneRendered += updateMethodForVr;
            //and the one that is INSIDE of the backgroundworker... At first thought Id say the one in backgroundworker is faster
        }

        ShaderResourceViewDescription resourceViewDescription;


        //ALDONATELLO Unity3D forums
        public static SharpDX.Vector3 getDirection(System.Windows.Media.Media3D.Quaternion quat, Vector3 vec)
        {
            double num = quat.X * 2f;
            double num2 = quat.Y * 2f;
            double num3 = quat.Z * 2f;
            double num4 = quat.X * num;
            double num5 = quat.Y * num2;
            double num6 = quat.Z * num3;
            double num7 = quat.X * num2;
            double num8 = quat.X * num3;
            double num9 = quat.Y * num3;
            double num10 = quat.W * num;
            double num11 = quat.W * num2;
            double num12 = quat.W * num3;
            Vector3 result;
            result.X = (float)((1f - (num5 + num6)) * vec.X + (num7 - num12) * vec.Y + (num8 + num11) * vec.Z);
            result.Y = (float)((num7 + num12) * vec.X + (1f - (num4 + num6)) * vec.Y + (num9 - num10) * vec.Z);
            result.Z = (float)((num8 - num11) * vec.X + (num9 + num10) * vec.Y + (1f - (num4 + num5)) * vec.Z);
            return result;
        }



        public static SharpDX.Vector3 setDirection(System.Windows.Media.Media3D.Quaternion quat, Vector3 vec)
        {
            double num = quat.X * 2f;
            double num2 = quat.Y * 2f;
            double num3 = quat.Z * 2f;
            double num4 = quat.X * num;
            double num5 = quat.Y * num2;
            double num6 = quat.Z * num3;
            double num7 = quat.X * num2;
            double num8 = quat.X * num3;
            double num9 = quat.Y * num3;
            double num10 = quat.W * num;
            double num11 = quat.W * num2;
            double num12 = quat.W * num3;

            Vector3 result;
            result.X = (float)((1f - (num5 + num6)) * vec.X + (num7 - num12) * vec.Y + (num8 + num11) * vec.Z);
            result.Y = (float)((num7 + num12) * vec.X + (1f - (num4 + num6)) * vec.Y + (num9 - num10) * vec.Z);
            result.Z = (float)((num8 - num11) * vec.X + (num9 + num10) * vec.Y + (1f - (num4 + num5)) * vec.Z);
            return result;
        }



        //https://pastebin.com/fAFp6NnN
        public static Vector3 _getDirection(Vector3 value, System.Windows.Media.Media3D.Quaternion rotation)
        {
            Vector3 vector;
            double num12 = rotation.X + rotation.X;
            double num2 = rotation.Y + rotation.Y;
            double num = rotation.Z + rotation.Z;
            double num11 = rotation.W * num12;
            double num10 = rotation.W * num2;
            double num9 = rotation.W * num;
            double num8 = rotation.X * num12;
            double num7 = rotation.X * num2;
            double num6 = rotation.X * num;
            double num5 = rotation.Y * num2;
            double num4 = rotation.Y * num;
            double num3 = rotation.Z * num;
            double num15 = ((value.X * ((1f - num5) - num3)) + (value.Y * (num7 - num9))) + (value.Z * (num6 + num10));
            double num14 = ((value.X * (num7 + num9)) + (value.Y * ((1f - num8) - num3))) + (value.Z * (num4 - num11));
            double num13 = ((value.X * (num6 - num10)) + (value.Y * (num4 + num11))) + (value.Z * ((1f - num8) - num5));
            vector.X = (float)num15;
            vector.Y = (float)num14;
            vector.Z = (float)num13;
            return vector;
        }




        /*//https://stackoverflow.com/questions/4436764/rotating-a-quaternion-on-1-axis?rq=1
        System.Windows.Media.Media3D.Quaternion create_from_axis_angle(double xx, double yy, double zz, double a)
        {
            // Here we calculate the sin( theta / 2) once for optimization
            double factor = Math.Sin(a / 2.0);

            // Calculate the x, y and z of the quaternion
            double x = xx * factor;
            double y = yy * factor;
            double z = zz * factor;

            // Calcualte the w value by cos( theta / 2 )
            double w = Math.Cos(a / 2.0);

            return new System.Windows.Media.Media3D.Quaternion(x, y, z, w);//.normalize();
        }*/

        //Matrix3D m = Matrix3D.Identity;
        //System.Windows.Media.Media3D.Quaternion q = new System.Windows.Media.Media3D.Quaternion(new Vector3D(0, 1, 0), 45);
        //m.Rotate(q);
        //Vector3D myVectorToRotate = new Vector3D(1, 2, 3);
        //Vector3D myVectorResult = m.Transform(myVectorToRotate);

        public System.Windows.Media.Media3D.Quaternion RotateQuaternion(System.Windows.Media.Media3D.Quaternion _initialQuat, Point3D pt)
        {
            _initialQuat.Normalize();
            System.Windows.Media.Media3D.Quaternion q1 = _initialQuat;
            q1.Conjugate();

            System.Windows.Media.Media3D.Quaternion qNode = new System.Windows.Media.Media3D.Quaternion(0, pt.X, pt.Y, pt.Z);
            qNode = _initialQuat * qNode * q1;
            //pt.X = qNode.X;
            //pt.Y = qNode.Y;
            //pt.Z = qNode.Z;
            return qNode;
        }


        /*public void Roll(int degree) // rotate around Z axis
        {
            SharpDX.Quaternion.from
            System.Windows.Media.Media3D.Quaternion q = new System.Windows.Media.Media3D.Quaternion();
            q.FromAxisAngle(new Vector3D(0, 0, 1), degree * Math.PI / 180.0);
            quan = q * quan;
        }

        public void Yaw(int degree)  // rotate around Y axis
        {
            Quaternion q = new Quaternion();
            q.FromAxisAngle(new Vector3d(0, 1, 0), degree * Math.PI / 180.0);
            quan = q * quan;
        }

        public void Pitch(int degree) // rotate around X axis
        {
            Quaternion q = new Quaternion();
            q.FromAxisAngle(new Vector3d(1, 0, 0), degree * Math.PI / 180.0);
            quan = q * quan;
        }*/



        System.Windows.Media.Media3D.Quaternion toQuaternion(double roll, double pitch, double yaw)
        {
            System.Windows.Media.Media3D.Quaternion q = new System.Windows.Media.Media3D.Quaternion();
            // Abbreviations for the various angular functions
            double cy = Math.Cos(yaw * 0.5);
            double sy = Math.Sin(yaw * 0.5);
            double cr = Math.Cos(roll * 0.5);
            double sr = Math.Sin(roll * 0.5);
            double cp = Math.Cos(pitch * 0.5);
            double sp = Math.Sin(pitch * 0.5);

            q.W = cy * cr * cp + sy * sr * sp;
            q.X = cy * sr * cp - sy * cr * sp;
            q.Y = cy * cr * sp + sy * sr * cp;
            q.Z = sy * cr * cp - cy * sr * sp;
            return q;
        }


        /*public static System.Windows.Media.Media3D.Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float num = roll * 0.5f;
            float num2 = (float)Math.Sin((double)num);
            float num3 = (float)Math.Cos((double)num);
            float num4 = pitch * 0.5f;
            float num5 = (float)Math.Sin((double)num4);
            float num6 = (float)Math.Cos((double)num4);
            float num7 = yaw * 0.5f;
            float num8 = (float)Math.Sin((double)num7);
            float num9 = (float)Math.Cos((double)num7);
            System.Windows.Media.Media3D.Quaternion result = new System.Windows.Media.Media3D.Quaternion();
            result.X = num9 * num5 * num3 + num8 * num6 * num2;
            result.Y = num8 * num6 * num3 - num9 * num5 * num2;
            result.Z = num9 * num6 * num2 - num8 * num5 * num3;
            result.W = num9 * num6 * num3 + num8 * num5 * num2;
            return result;
        }*/


        /*static void toEulerAngle(System.Windows.Media.Media3D.Quaternion q, out double roll, out double pitch, out double yaw)
        {
            // roll (x-axis rotation)
            double sinr = +2.0 * (q.W * q.X + q.Y * q.Z);
            double cosr = +1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
            roll = Math.Atan2(sinr, cosr);

            // pitch (y-axis rotation)
            double sinp = +2.0 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                pitch = copysign(Math.PI / 2, sinp); // use 90 degrees if out of range
            else
                pitch = Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny = +2.0 * (q.W * q.Z + q.X * q.Y);
            double cosy = +1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
            yaw = Math.Atan2(siny, cosy);
        }*/




        void quaternionToEulerAngle(double w, double x, double y, double z, out double _X, out double _Y, out double _Z)
        {

            var t0 = 2.0 * (w * x + y * z);
            var t1 = 1.0 - 2.0 * (x * x + y * y);
            _X = Math.Atan2(t0, t1); // Roll

            var t2 = 2.0 * (w * y - z * x);
            if (t2 > 1.0)
            {
                t2 = 1.0;
            }
            else if (t2 < -1.0)
            {
                t2 = -1.0;
            }
            _Y = Math.Asin(t2); // pitch

            var t3 = 2.0 * (w * z + x * y);
            var t4 = 1.0 - 2.0 * (y * y + z * z);
            _Z = Math.Atan2(t3, t4); // yaw

            //return X, Y, Z
        }


        System.Windows.Media.Media3D.Quaternion _initialQuater = new System.Windows.Media.Media3D.Quaternion();
        SharpDX.Vector3 _originalForward = SharpDX.Vector3.ForwardLH;
        SharpDX.Vector3 _newForward = SharpDX.Vector3.ForwardLH;

        TranslateTransform3D _desktopScreenPos = new TranslateTransform3D();


        float _rotateSpeed =0.00175f;//0.175f;

        public void UpdateScene()
        {
            //var currentTime = DateTime.Now;



            if (Keyboard.IsKeyDown(Key.F))
            {
                _dxViewportView.DXScene.InitializeVirtualRealityRendering(null);
            }

            else if (Keyboard.IsKeyDown(Key.T)) 
            {
                _dxViewportView.DXScene.InitializeVirtualRealityRendering(_oculusRiftVirtualRealityProvider);
            }










            if (_createStopWatch)
            {
                _newStopWatch = new Stopwatch();
                _createStopWatch = false;
            }
            if (_restartStopWatch)
            {
                _newStopWatch.Stop();
                _newStopWatch.Reset();
                _newStopWatch.Start();
                _restartStopWatch = false;
            }
            if (_createdSceneObjects)
            {
                if (_startOnce02)
                {
                    //_camera.Position = new Point3D(100, 100, 0.45f);
                    _camera.Position = new Point3D(0, 0, 0.45f);
                    _initialQuater = _desktopScreenQuaternionRotation3D.Quaternion;
                    _startOnce02 = false;
                }





                /*var centerPosLeft = new Point3D(-_leftControllerTranslateTransform3D.OffsetX, -_leftControllerTranslateTransform3D.OffsetY, -_leftControllerTranslateTransform3D.OffsetZ);    //rightControllerVisual3D 
                var rayoriginLeft = centerPosLeft;
                var rayDirLeft = getDirection(_leftControllerQuaternionRotation3D.Quaternion, Vector3.ForwardLH);

                Vector3D _rayDirectionLeft = new Vector3D(0, 0, 0);
                _rayDirectionLeft.X = rayDirLeft.X;
                _rayDirectionLeft.Y = rayDirLeft.Y;
                _rayDirectionLeft.Z = rayDirLeft.Z;
                Point3D _intersectionLeft;

                bool resultsLeft = Ab3d.Utilities.Plane.RayPlaneIntersection(rayoriginLeft, _rayDirectionLeft, pointOnPlane, planeNormal, out _intersectionLeft);
                if (_currentFrameLeft < _arrayOfStabilizerPosLeft.Length)
                {
                    _arrayOfStabilizerPosXLeft[_currentFrameLeft] = _intersectionLeft.X;
                    _arrayOfStabilizerPosYLeft[_currentFrameLeft] = _intersectionLeft.Y;
                    _arrayOfStabilizerPosLeft[_currentFrameLeft] = _intersectionLeft;
                }
                else
                {
                    double _differenceX = 0;
                    double _differenceY = 0;

                    int j = 1;

                    for (int i = 0; i < _arrayOfStabilizerPosXLeft.Length - 1; i++, j++)
                    {
                        var currentX = _arrayOfStabilizerPosXLeft[j];
                        var currentY = _arrayOfStabilizerPosYLeft[j];
                        _lastLeftHitPointXFrameOne = _arrayOfStabilizerPosXLeft[i];
                        _lastLeftHitPointYFrameOne = _arrayOfStabilizerPosYLeft[i];

                        if (_lastLeftHitPointXFrameOne >= currentX)
                        {
                            _differenceX = _lastLeftHitPointXFrameOne - currentX;
                        }
                        else
                        {
                            _differenceX = currentX - _lastLeftHitPointXFrameOne;
                        }

                        _arrayOfStabilizerPosDifferenceXLeft[i] = _differenceX;

                        if (_lastLeftHitPointYFrameOne >= currentY)
                        {
                            _differenceY = _lastLeftHitPointYFrameOne - currentY;
                        }
                        else
                        {
                            _differenceY = currentY - _lastLeftHitPointYFrameOne;
                        }

                        _arrayOfStabilizerPosDifferenceYLeft[i] = _differenceY;
                    }

                    _averageXLeft = 0;
                    _averageYLeft = 0;

                    for (int i = 0; i < _arrayOfStabilizerPosDifferenceXLeft.Length; i++)
                    {
                        _averageXLeft += _arrayOfStabilizerPosDifferenceXLeft[i];
                        _averageYLeft += _arrayOfStabilizerPosDifferenceYLeft[i];
                    }

                    _averageXLeft = _averageXLeft / _arrayOfStabilizerPosDifferenceXLeft.Length;
                    _averageYLeft = _averageYLeft / _arrayOfStabilizerPosDifferenceYLeft.Length;


                    _arrayOfStabilizerPosXLeft = new double[_arrayOfStabilizerPosLeft.Length];
                    _arrayOfStabilizerPosDifferenceXLeft = new double[_arrayOfStabilizerPosXLeft.Length - 1];

                    _arrayOfStabilizerPosYLeft = new double[_arrayOfStabilizerPosLeft.Length];
                    _arrayOfStabilizerPosDifferenceYLeft = new double[_arrayOfStabilizerPosYLeft.Length - 1];
                    _restartFrameCounterLeft = true;
                }

                if (!_restartFrameCounterLeft)
                {
                    _currentFrameLeft++;
                }
                else
                {
                    _currentFrameLeft = 0;
                    _restartFrameCounterLeft = false;
                }

                Point3D centerLeft = new Point3D(0, 0, 0);

                float countLeft = 0;
                for (int i = 0; i < _arrayOfStabilizerPosLeft.Length; i++)
                {
                    centerLeft.X += _arrayOfStabilizerPosLeft[i].X;
                    centerLeft.Y += _arrayOfStabilizerPosLeft[i].Y;
                    centerLeft.Z += _arrayOfStabilizerPosLeft[i].Z;
                    countLeft++;
                }

                _theCenterLeft.X = centerLeft.X / countLeft;
                _theCenterLeft.Y = centerLeft.Y / countLeft;
                _theCenterLeft.Z = centerLeft.Z / countLeft;

                leftHitPoint.OffsetX = -_theCenterLeft.X;
                leftHitPoint.OffsetY = -_theCenterLeft.Y;
                leftHitPoint.OffsetZ = _theCenterLeft.Z;

                hitPointLeft.Transform = leftHitPoint;

                _XDOUBLEleftControllerTranslateTransform3D = _leftControllerTranslateTransform3D.OffsetX;
                _YDOUBLEleftControllerTranslateTransform3D = _leftControllerTranslateTransform3D.OffsetY;
                _ZDOUBLEleftControllerTranslateTransform3D = _leftControllerTranslateTransform3D.OffsetZ;

                _XDOUBLEleftControllerQuaternionRotation3D = _leftControllerQuaternionRotation3D.Quaternion.X;
                _YDOUBLEleftControllerQuaternionRotation3D = _leftControllerQuaternionRotation3D.Quaternion.Y;
                _ZDOUBLEleftControllerQuaternionRotation3D = _leftControllerQuaternionRotation3D.Quaternion.Z;
                _WDOUBLEleftControllerQuaternionRotation3D = _leftControllerQuaternionRotation3D.Quaternion.W;
                */
                //Console.WriteLine(_desktopScreenQuaternionRotation3D.Quaternion.X + "***" + _desktopScreenQuaternionRotation3D.Quaternion.Y + "***" + _desktopScreenQuaternionRotation3D.Quaternion.Z + "***" + _desktopScreenQuaternionRotation3D.Quaternion.W);

                if (_activateScreenModControl)
                {
                    if (buttonPressedOculusTouchRight == 1)
                    {

                        Vector3D _refDir = point3DCollection[2] - point3DCollection[0];

                        Vector3 _refer = new Vector3((float)_refDir.X, (float)_refDir.Y, (float)_refDir.Z);

                        Vector3 _up = Vector3.Up;
                        Vector3 _dir = _getDirection(_up, _desktopScreenQuaternionRotation3D.Quaternion);

                        SharpDX.Quaternion _newQuater = new SharpDX.Quaternion();

                        SharpDX.Quaternion.RotationAxis(ref _dir,_rotateSpeed,out _newQuater);

                        System.Windows.Media.Media3D.Quaternion _newQuat = new System.Windows.Media.Media3D.Quaternion(_newQuater.X, _newQuater.Y, _newQuater.Z, _newQuater.W);
                       _desktopScreenQuaternionRotation3D.Quaternion = _newQuat;

                        /*System.Windows.Media.Media3D.Quaternion _quat = _initialQuater;
                        System.Windows.Media.Media3D.Quaternion _quater = _desktopScreenQuaternionRotation3D.Quaternion;

                        double XX = 0;
                        double YY = 0;
                        double ZZ = 0;
                        quaternionToEulerAngle(_quat.W, _quat.X, _quat.Y, _quat.Z, out XX, out YY, out ZZ);

                        YY += _rotateSpeed;

                        //double XXX = 0;
                        //double YYY = 0;
                        //double ZZZ = 0;
                        //quaternionToEulerAngle(_quater.W, _quater.X, _quater.Y, _quater.Z, out XXX, out YYY, out ZZZ);

                        System.Windows.Media.Media3D.Quaternion _newQuat = toQuaternion((float)XX, (float)YY, (float)ZZ);

                        _desktopScreenQuaternionRotation3D.Quaternion = _newQuat;*/


                        /*System.Windows.Media.Media3D.Quaternion _quat = _desktopScreenQuaternionRotation3D.Quaternion;

                        //var _yawer = (float)Math.Atan2(2f * _quat.X * _quat.W + 2f * _quat.Y * _quat.Z, 1 - 2f * ((_quat.Z * _quat.Z) + (_quat.W * _quat.W)));  // Yaw 
                        //var _pitcher = (float)Math.Asin(2f * (_quat.X * _quat.Z - _quat.W * _quat.Y));                                                          // Pitch 
                        //var _roller = (float)Math.Atan2(2f * _quat.X * _quat.Y + 2f * _quat.Z * _quat.W, 1 - 2f * ((_quat.Y * _quat.Y) + (_quat.Z * _quat.Z))); // ROLL


                        var _roll = Math.Atan2(2 * _quat.Y * _quat.W + 2 * _quat.X * _quat.Z, 1 - 2 * _quat.Y * _quat.Y - 2 * _quat.Z * _quat.Z);
                        var _pitch = Math.Atan2(2 * _quat.X * _quat.W + 2 * _quat.Y * _quat.Z, 1 - 2 * _quat.X * _quat.X - 2 * _quat.Z * _quat.Z);
                        var _yaw = Math.Asin(2 * _quat.X * _quat.Y + 2 * _quat.Z * _quat.W);


                        System.Windows.Media.Media3D.Quaternion _newQuat = toQuaternion((float)_roll, (float)_pitch + _rotateSpeed, (float)_yaw);

                        _desktopScreenQuaternionRotation3D.Quaternion = _newQuat;*/
                        //System.Windows.Media.Media3D.Quaternion _newQuat = new System.Windows.Media.Media3D.Quaternion(new Vector3D(0, 1, 0), _rotateSpeed * Math.PI / 180.0); //new Vector3D(0, 1, 0)
                        //_desktopScreenQuaternionRotation3D.Quaternion = new System.Windows.Media.Media3D.Quaternion(_newQuat.X, _newQuat.Y, _newQuat.Z, _newQuat.W);

                        Matrix3D m = Matrix3D.Identity;
                        m.Rotate(_newQuat);


                        Point3D[] _oldPoint3dCollection = point3DCollection;
                        point3DCollection = new Point3D[_oldPoint3dCollection.Length];
                        _point3DCollection = new Point3DCollection();

                        for (int i = 0; i < _oldPoint3dCollection.Length; i++)
                        {
                            Vector3D myVectorToRotate = new Vector3D(0, 0, 0);

                            myVectorToRotate.X = _oldPoint3dCollection[i].X;
                            myVectorToRotate.Y = _oldPoint3dCollection[i].Y;
                            myVectorToRotate.Z = _oldPoint3dCollection[i].Z;

                            Vector3D myVectorResult = m.Transform(myVectorToRotate);
                            point3DCollection[i] = new Point3D(myVectorResult.X, myVectorResult.Y, myVectorResult.Z);
                            _point3DCollection.Add(point3DCollection[i]);
                            _screenCornersTotal[i].CenterPosition = point3DCollection[i];


                            /*if (i < 2)
                            {
                                _screenCornersTotal[i].CenterPosition = point3DCollection[i];
                            }
                            else
                            {
                                _screenCornersTotal[i].CenterPosition = ;
                            }*/
                        }

                        meshGeometry.Positions = _point3DCollection;

                        plane = new Ab3d.Utilities.Plane(_point3DCollection[0], _point3DCollection[1], _point3DCollection[2]);
                        pointOnPlane = plane.GetPointOnPlane();
                        planeNormal = plane.Normal;

                        _standardMaterial = new Ab3d.DirectX.Materials.StandardMaterial()
                        {
                            DiffuseColor = new SharpDX.Color3(1f, 1f, 1f),
                            HasTransparency = false,
                        };

                        _standardMaterial.PreferedShaderQuality = ShaderQuality.Low;

                        vertexColorDiffuseMaterial = new DiffuseMaterial(System.Windows.Media.Brushes.Black);
                        vertexColorDiffuseMaterial.SetUsedDXMaterial(_standardMaterial);

                        _desktopScreen.Material = vertexColorDiffuseMaterial;

                        _shaderResourceView = new ShaderResourceView(_dxViewportView.DXScene.DXDevice.Device, _desktopFrame._texture2d, resourceViewDescription);

                        if (_shaderResourceView != null)
                        {
                            _standardMaterial.DiffuseTextures = new ShaderResourceView[]
                            {
                                _shaderResourceView,
                            };
                        }
                    }
                    else if (buttonPressedOculusTouchRight == 2)
                    {

                        /*System.Windows.Media.Media3D.Quaternion _quat = _initialQuater;
                        System.Windows.Media.Media3D.Quaternion _quater = _desktopScreenQuaternionRotation3D.Quaternion;

                        double XX = 0;
                        double YY = 0;
                        double ZZ = 0;
                        quaternionToEulerAngle(_quat.W, _quat.X, _quat.Y, _quat.Z, out XX, out YY, out ZZ);

                        YY -= _rotateSpeed;


                        double XXX = 0;
                        double YYY = 0;
                        double ZZZ = 0;
                        quaternionToEulerAngle(_quater.W, _quater.X, _quater.Y, _quater.Z, out XXX, out YYY, out ZZZ);

                        System.Windows.Media.Media3D.Quaternion _newQuat = toQuaternion((float)XX, (float)YY, (float)ZZ);
                        _desktopScreenQuaternionRotation3D.Quaternion = _newQuat;*/

                        Vector3D _refDir = point3DCollection[2] - point3DCollection[0];

                        Vector3 _refer = new Vector3((float)_refDir.X, (float)_refDir.Y, (float)_refDir.Z);

                        Vector3 _up = Vector3.Up;
                        Vector3 _dir = _getDirection(_up, _desktopScreenQuaternionRotation3D.Quaternion);

                        SharpDX.Quaternion _newQuater = new SharpDX.Quaternion();

                        SharpDX.Quaternion.RotationAxis(ref _dir, -_rotateSpeed, out _newQuater);

                        System.Windows.Media.Media3D.Quaternion _newQuat = new System.Windows.Media.Media3D.Quaternion(_newQuater.X, _newQuater.Y, _newQuater.Z, _newQuater.W);
                        _desktopScreenQuaternionRotation3D.Quaternion = _newQuat;

                        Matrix3D m = Matrix3D.Identity;
                        m.Rotate(_newQuat);

                        Point3D[] _oldPoint3dCollection = point3DCollection;
                        point3DCollection = new Point3D[_oldPoint3dCollection.Length];
                        _point3DCollection = new Point3DCollection();

                        for (int i = 0; i < _oldPoint3dCollection.Length; i++)
                        {
                            Vector3D myVectorToRotate = new Vector3D(0, 0, 0);

                            myVectorToRotate.X = _oldPoint3dCollection[i].X;
                            myVectorToRotate.Y = _oldPoint3dCollection[i].Y;
                            myVectorToRotate.Z = _oldPoint3dCollection[i].Z;

                            Vector3D myVectorResult = m.Transform(myVectorToRotate);
                            point3DCollection[i] = new Point3D(myVectorResult.X, myVectorResult.Y, myVectorResult.Z);
                            _point3DCollection.Add(point3DCollection[i]);
                            _screenCornersTotal[i].CenterPosition = point3DCollection[i];
                        }

                        meshGeometry.Positions = _point3DCollection;

                        plane = new Ab3d.Utilities.Plane(_point3DCollection[0], _point3DCollection[1], _point3DCollection[2]);
                        pointOnPlane = plane.GetPointOnPlane();
                        planeNormal = plane.Normal;

                        _standardMaterial = new Ab3d.DirectX.Materials.StandardMaterial()
                        {
                            DiffuseColor = new SharpDX.Color3(1f, 1f, 1f),
                            HasTransparency = false,
                        };

                        _standardMaterial.PreferedShaderQuality = ShaderQuality.Low;

                        vertexColorDiffuseMaterial = new DiffuseMaterial(System.Windows.Media.Brushes.Black);
                        vertexColorDiffuseMaterial.SetUsedDXMaterial(_standardMaterial);

                        _desktopScreen.Material = vertexColorDiffuseMaterial;

                        _shaderResourceView = new ShaderResourceView(_dxViewportView.DXScene.DXDevice.Device, _desktopFrame._texture2d, resourceViewDescription);

                        if (_shaderResourceView != null)
                        {
                            _standardMaterial.DiffuseTextures = new ShaderResourceView[]
                            {
                                    _shaderResourceView,
                            };
                        }
                    }
                    else if (buttonPressedOculusTouchLeft == 256)
                    {
                        /*System.Windows.Media.Media3D.Quaternion _quat = _initialQuater;
                        System.Windows.Media.Media3D.Quaternion _quater = _desktopScreenQuaternionRotation3D.Quaternion;

                        double XX = 0;
                        double YY = 0;
                        double ZZ = 0;
                        quaternionToEulerAngle(_quat.W, _quat.X, _quat.Y, _quat.Z, out XX, out YY, out ZZ);

                        XX += _rotateSpeed;

                        double XXX = 0;
                        double YYY = 0;
                        double ZZZ = 0;
                        quaternionToEulerAngle(_quater.W, _quater.X, _quater.Y, _quater.Z, out XXX, out YYY, out ZZZ);

                        System.Windows.Media.Media3D.Quaternion _newQuat = toQuaternion((float)XX, (float)YY, (float)ZZ);

                        _desktopScreenQuaternionRotation3D.Quaternion = _newQuat;*/

                        //var _dirX = (1 * Math.Cos(0 * Math.PI / 180)) + _desktopScreen.CenterPosition.X;
                        //var _dirY = (1 * Math.Sin(0 * Math.PI / 180)) + _desktopScreen.CenterPosition.Y;
                        //var _dirZ = (1 * Math.Tan(0 * Math.PI / 180)) + _desktopScreen.CenterPosition.Z;

                        Vector3D _refDir = point3DCollection[1] - point3DCollection[0];

                        Vector3 _refer = new Vector3((float)_refDir.X, (float)_refDir.Y, (float)_refDir.Z);

                        Vector3 _up = Vector3.Left;
                        Vector3 _dir = _getDirection(_refer, _desktopScreenQuaternionRotation3D.Quaternion);

                        SharpDX.Quaternion _newQuater = new SharpDX.Quaternion();

                        SharpDX.Quaternion.RotationAxis(ref _dir, _rotateSpeed, out _newQuater);

                        System.Windows.Media.Media3D.Quaternion _newQuat = new System.Windows.Media.Media3D.Quaternion(_newQuater.X, _newQuater.Y, _newQuater.Z, _newQuater.W);
                        _desktopScreenQuaternionRotation3D.Quaternion = _newQuat;

                        Matrix3D m = Matrix3D.Identity;
                        m.Rotate(_newQuat);

                        Point3D[] _oldPoint3dCollection = point3DCollection;
                        point3DCollection = new Point3D[_oldPoint3dCollection.Length];
                        _point3DCollection = new Point3DCollection();

                        for (int i = 0; i < _oldPoint3dCollection.Length; i++)
                        {
                            Vector3D myVectorToRotate = new Vector3D(0, 0, 0);

                            myVectorToRotate.X = _oldPoint3dCollection[i].X;
                            myVectorToRotate.Y = _oldPoint3dCollection[i].Y;
                            myVectorToRotate.Z = _oldPoint3dCollection[i].Z;

                            Vector3D myVectorResult = m.Transform(myVectorToRotate);
                            point3DCollection[i] = new Point3D(myVectorResult.X, myVectorResult.Y, myVectorResult.Z);
                            _point3DCollection.Add(point3DCollection[i]);
                            _screenCornersTotal[i].CenterPosition = point3DCollection[i];
                        }

                        meshGeometry.Positions = _point3DCollection;

                        plane = new Ab3d.Utilities.Plane(_point3DCollection[0], _point3DCollection[1], _point3DCollection[2]);
                        pointOnPlane = plane.GetPointOnPlane();
                        planeNormal = plane.Normal;

                        _standardMaterial = new Ab3d.DirectX.Materials.StandardMaterial()
                        {
                            DiffuseColor = new SharpDX.Color3(1f, 1f, 1f),
                            HasTransparency = false,
                        };

                        _standardMaterial.PreferedShaderQuality = ShaderQuality.Low;

                        vertexColorDiffuseMaterial = new DiffuseMaterial(System.Windows.Media.Brushes.Black);
                        vertexColorDiffuseMaterial.SetUsedDXMaterial(_standardMaterial);

                        _desktopScreen.Material = vertexColorDiffuseMaterial;

                        _shaderResourceView = new ShaderResourceView(_dxViewportView.DXScene.DXDevice.Device, _desktopFrame._texture2d, resourceViewDescription);

                        if (_shaderResourceView != null)
                        {
                            _standardMaterial.DiffuseTextures = new ShaderResourceView[]
                            {
                                    _shaderResourceView,
                            };
                        }
                    }
                    else if (buttonPressedOculusTouchLeft == 512)
                    {
                        /*System.Windows.Media.Media3D.Quaternion _quat = _initialQuater;
                        System.Windows.Media.Media3D.Quaternion _quater = _desktopScreenQuaternionRotation3D.Quaternion;

                        double XX = 0;
                        double YY = 0;
                        double ZZ = 0;
                        quaternionToEulerAngle(_quat.W, _quat.X, _quat.Y, _quat.Z, out XX, out YY, out ZZ);

                        XX -= _rotateSpeed;


                        double XXX = 0;
                        double YYY = 0;
                        double ZZZ = 0;
                        quaternionToEulerAngle(_quater.W, _quater.X, _quater.Y, _quater.Z, out XXX, out YYY, out ZZZ);

                        System.Windows.Media.Media3D.Quaternion _newQuat = toQuaternion((float)XX, (float)YY, (float)ZZ);


                        _desktopScreenQuaternionRotation3D.Quaternion = _newQuat;*/


                        Vector3D _refDir = point3DCollection[1] - point3DCollection[0];


                        Vector3 _refer = new Vector3((float)_refDir.X, (float)_refDir.Y, (float)_refDir.Z);

                        Vector3 _up = Vector3.Left;
                        Vector3 _dir = _getDirection(_refer, _desktopScreenQuaternionRotation3D.Quaternion);

                        SharpDX.Quaternion _newQuater = new SharpDX.Quaternion();

                        SharpDX.Quaternion.RotationAxis(ref _dir, -_rotateSpeed, out _newQuater);

                        System.Windows.Media.Media3D.Quaternion _newQuat = new System.Windows.Media.Media3D.Quaternion(_newQuater.X, _newQuater.Y, _newQuater.Z, _newQuater.W);
                        _desktopScreenQuaternionRotation3D.Quaternion = _newQuat;

                        Matrix3D m = Matrix3D.Identity;
                        m.Rotate(_newQuat);

                        Point3D[] _oldPoint3dCollection = point3DCollection;
                        point3DCollection = new Point3D[_oldPoint3dCollection.Length];
                        _point3DCollection = new Point3DCollection();

                        for (int i = 0; i < _oldPoint3dCollection.Length; i++)
                        {
                            Vector3D myVectorToRotate = new Vector3D(0, 0, 0);

                            myVectorToRotate.X = _oldPoint3dCollection[i].X;
                            myVectorToRotate.Y = _oldPoint3dCollection[i].Y;
                            myVectorToRotate.Z = _oldPoint3dCollection[i].Z;

                            Vector3D myVectorResult = m.Transform(myVectorToRotate);
                            point3DCollection[i] = new Point3D(myVectorResult.X, myVectorResult.Y, myVectorResult.Z);
                            _point3DCollection.Add(point3DCollection[i]);
                            _screenCornersTotal[i].CenterPosition = point3DCollection[i];
                        }

                        meshGeometry.Positions = _point3DCollection;

                        plane = new Ab3d.Utilities.Plane(_point3DCollection[0], _point3DCollection[1], _point3DCollection[2]);
                        pointOnPlane = plane.GetPointOnPlane();
                        planeNormal = plane.Normal;

                        _standardMaterial = new Ab3d.DirectX.Materials.StandardMaterial()
                        {
                            DiffuseColor = new SharpDX.Color3(1f, 1f, 1f),
                            HasTransparency = false,
                        };

                        _standardMaterial.PreferedShaderQuality = ShaderQuality.Low;

                        vertexColorDiffuseMaterial = new DiffuseMaterial(System.Windows.Media.Brushes.Black);
                        vertexColorDiffuseMaterial.SetUsedDXMaterial(_standardMaterial);

                        _desktopScreen.Material = vertexColorDiffuseMaterial;

                        _shaderResourceView = new ShaderResourceView(_dxViewportView.DXScene.DXDevice.Device, _desktopFrame._texture2d, resourceViewDescription);

                        if (_shaderResourceView != null)
                        {
                            _standardMaterial.DiffuseTextures = new ShaderResourceView[]
                            {
                                    _shaderResourceView,
                            };
                        }
                    }

                    else if (buttonPressedOculusTouchRight == 4)
                    {
                        int _milliScreenControl = _stopWatchForScreenControl.Elapsed.Milliseconds;

                        if (_milliScreenControl >= 200)
                        {
                            _activateScreenModControl = false;
                            //_stopWatchForScreenControl.Stop();
                            _screenControlStopWatchBool = true;
                        }
                    }
                }


























                var centerPosRight = new Point3D(_rightControllerTranslateTransform3D.OffsetX, _rightControllerTranslateTransform3D.OffsetY, _rightControllerTranslateTransform3D.OffsetZ);    //rightControllerVisual3D 
                var rayoriginRight = centerPosRight;

                //var rayDirRight = getDirection(_rightControllerQuaternionRotation3D.Quaternion, Vector3.ForwardLH);
                var rayDirRight = _getDirection(Vector3.ForwardRH, _rightControllerQuaternionRotation3D.Quaternion);

                Vector3D _rayDirectionRight = new Vector3D(0, 0, 0);

                _rayDirectionRight.X = rayDirRight.X;
                _rayDirectionRight.Y = rayDirRight.Y;
                _rayDirectionRight.Z = rayDirRight.Z;

                Point3D _intersectionRight;
                bool resultsRight = Ab3d.Utilities.Plane.RayPlaneIntersection(rayoriginRight, _rayDirectionRight, pointOnPlane, planeNormal, out _intersectionRight);

                if (_currentFrameRight < _arrayOfStabilizerPosRight.Length)
                {
                    _arrayOfStabilizerPosXRight[_currentFrameRight] = _intersectionRight.X;
                    _arrayOfStabilizerPosYRight[_currentFrameRight] = _intersectionRight.Y;
                    _arrayOfStabilizerPosZRight[_currentFrameRight] = _intersectionRight.Z;

                    _arrayOfStabilizerPosRight[_currentFrameRight] = _intersectionRight;
                }
                else
                {
                    double _differenceX = 0;
                    double _differenceY = 0;
                    double _differenceZ = 0;
                    int j = 1;

                    for (int i = 0; i < _arrayOfStabilizerPosXRight.Length - 1; i++, j++)
                    {
                        var currentX = _arrayOfStabilizerPosXRight[j];
                        var currentY = _arrayOfStabilizerPosYRight[j];
                        var currentZ = _arrayOfStabilizerPosZRight[j];

                        _lastRightHitPointXFrameOne = _arrayOfStabilizerPosXRight[i];
                        _lastRightHitPointYFrameOne = _arrayOfStabilizerPosYRight[i];
                        _lastRightHitPointZFrameOne = _arrayOfStabilizerPosZRight[i];

                        if (_lastRightHitPointXFrameOne >= currentX)
                        {
                            _differenceX = _lastRightHitPointXFrameOne - currentX;
                        }
                        else
                        {
                            _differenceX = currentX - _lastRightHitPointXFrameOne;
                        }

                        _arrayOfStabilizerPosDifferenceXRight[i] = _differenceX;



                        if (_lastRightHitPointYFrameOne >= currentY)
                        {
                            _differenceY = _lastRightHitPointYFrameOne - currentY;
                        }
                        else
                        {
                            _differenceY = currentY - _lastRightHitPointYFrameOne;
                        }

                        _arrayOfStabilizerPosDifferenceYRight[i] = _differenceY;


                        if (_lastRightHitPointZFrameOne >= currentZ)
                        {
                            _differenceZ = _lastRightHitPointZFrameOne - currentZ;
                        }
                        else
                        {
                            _differenceZ = currentZ - _lastRightHitPointZFrameOne;
                        }

                        _arrayOfStabilizerPosDifferenceZRight[i] = _differenceZ;
                    }


                    _averageXRight = 0;
                    _averageYRight = 0;
                    _averageZRight = 0;

                    for (int i = 0; i < _arrayOfStabilizerPosDifferenceXRight.Length; i++)
                    {
                        _averageXRight += _arrayOfStabilizerPosDifferenceXRight[i];
                        _averageYRight += _arrayOfStabilizerPosDifferenceYRight[i];
                        _averageZRight += _arrayOfStabilizerPosDifferenceZRight[i];
                    }

                    _averageXRight = _averageXRight / _arrayOfStabilizerPosDifferenceXRight.Length;
                    _averageYRight = _averageYRight / _arrayOfStabilizerPosDifferenceYRight.Length;
                    _averageZRight = _averageZRight / _arrayOfStabilizerPosDifferenceZRight.Length;

                    _arrayOfStabilizerPosXRight = new double[_arrayOfStabilizerPosRight.Length];
                    _arrayOfStabilizerPosDifferenceXRight = new double[_arrayOfStabilizerPosXRight.Length - 1];

                    _arrayOfStabilizerPosYRight = new double[_arrayOfStabilizerPosRight.Length];
                    _arrayOfStabilizerPosDifferenceYRight = new double[_arrayOfStabilizerPosYRight.Length - 1];

                    _arrayOfStabilizerPosZRight = new double[_arrayOfStabilizerPosRight.Length];
                    _arrayOfStabilizerPosDifferenceZRight = new double[_arrayOfStabilizerPosZRight.Length - 1];

                    _restartFrameCounterRight = true;
                }

                if (!_restartFrameCounterRight)
                {
                    _currentFrameRight++;
                }
                else
                {
                    _currentFrameRight = 0;
                    _restartFrameCounterRight = false;
                }

                Point3D centerRight = new Point3D(0, 0, 0);

                float countRight = 0;
                for (int i = 0; i < _arrayOfStabilizerPosRight.Length; i++)
                {
                    centerRight.X += _arrayOfStabilizerPosRight[i].X;
                    centerRight.Y += _arrayOfStabilizerPosRight[i].Y;
                    centerRight.Z += _arrayOfStabilizerPosRight[i].Z;
                    countRight++;
                }

                _theCenterRight.X = centerRight.X / countRight;
                _theCenterRight.Y = centerRight.Y / countRight;
                _theCenterRight.Z = centerRight.Z / countRight;
                
                rightHitPoint.OffsetX = _theCenterRight.X;
                rightHitPoint.OffsetY = _theCenterRight.Y;
                rightHitPoint.OffsetZ = _theCenterRight.Z;

                //rightHitPoint.OffsetX = -intersectionRight.X;
                //rightHitPoint.OffsetY = -intersectionRight.Y;
                //rightHitPoint.OffsetZ = -intersectionRight.Z;

                hitPointRight.Transform = rightHitPoint;

                _XDOUBLErightControllerTranslateTransform3D = _rightControllerTranslateTransform3D.OffsetX;
                _YDOUBLErightControllerTranslateTransform3D = _rightControllerTranslateTransform3D.OffsetY;
                _ZDOUBLErightControllerTranslateTransform3D = _rightControllerTranslateTransform3D.OffsetZ;

                _XDOUBLErightControllerQuaternionRotation3D = _rightControllerQuaternionRotation3D.Quaternion.X;
                _YDOUBLErightControllerQuaternionRotation3D = _rightControllerQuaternionRotation3D.Quaternion.Y;
                _ZDOUBLErightControllerQuaternionRotation3D = _rightControllerQuaternionRotation3D.Quaternion.Z;
                _WDOUBLErightControllerQuaternionRotation3D = _rightControllerQuaternionRotation3D.Quaternion.W;













               



                if (_desktopFrame._texture2d != null) //_desktopFrame != null && 
                {
                    /*resourceViewDescription = new ShaderResourceViewDescription
                    {
                        Format = Format.B8G8R8A8_UNorm,
                        Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription.Texture2DResource
                        {
                            MipLevels = -1,
                            MostDetailedMip = 0
                        }
                    };*/

                    _shaderResourceView = new ShaderResourceView(_dxViewportView.DXScene.DXDevice.Device, _desktopFrame._texture2d, resourceViewDescription);

                    //_shaderResourceView = Ab3d.DirectX.TextureLoader.CreateShaderResourceView(_dxViewportView.DXScene.DXDevice.Device, _desktopFrame.bitmapByteArray, _width, _height, strider, Format.B8G8R8A8_UNorm, true);

                    if (_shaderResourceView != null)
                    {
                        _standardMaterial.DiffuseTextures[0] = _shaderResourceView;
                    }

                    if (_lastShaderResourceView != null)
                    {
                        _lastShaderResourceView.Dispose();
                    }

                    if (_lastStandardMaterial != null)
                    {
                        _lastStandardMaterial.Dispose();
                    }

                    _lastShaderResourceView = _shaderResourceView;
                    _lastStandardMaterial = _standardMaterial;
                }
                _frameOneCounter++;
                _frameTwoCounter++;
                _frameThreeCounter++;
                _frameFourCounter++;
            }

            if (_shaderQuality)
            {
                if (_dxViewportView.DXScene != null)
                {
                    _dxViewportView.DXScene.ShaderQuality = ShaderQuality.Low;
                    _shaderQuality = false;
                }
            }

            int _seconds = _newStopWatch.Elapsed.Seconds;

            if (_seconds >= 1)
            {
                _newStopWatch.Stop();
                _restartStopWatch = true;
                //Console.WriteLine(UpdateSceneFrameCounter);              
                UpdateSceneFrameCounter = 0;
            }

            ProcessTouchControllers();
            UpdateSceneFrameCounter++;
        }



        BoxVisual3D _screenCorners;
        BoxVisual3D[] _screenCornersTotal;

        #region CreateSceneObjects
        private void CreateSceneObjects()
        {
            _rootVisual3D = new ModelVisual3D();

            hitPointRight = new SphereVisual3D()
            {
                CenterPosition = new Point3D(0, 0, 0),
                Radius = 0.0025f,
                Material = new DiffuseMaterial(System.Windows.Media.Brushes.Red)
            };
            _rootVisual3D.Children.Add(hitPointRight);

            hitPointLeft = new SphereVisual3D()
            {
                CenterPosition = new Point3D(0, 0, 0),
                Radius = 0.0025f,
                Material = new DiffuseMaterial(System.Windows.Media.Brushes.Green)
            };
            _rootVisual3D.Children.Add(hitPointLeft);

            /*_floorBox = new BoxVisual3D()
            {
                CenterPosition = new Point3D(100, 99f, 0),
                Size = new Size3D(10, 1, 10),
                Material = new DiffuseMaterial(System.Windows.Media.Brushes.Gray)
            };
            _rootVisual3D.Children.Add(_floorBox);*/

            _desktopScreen = new Ab3d.Visuals.VerticalPlaneVisual3D()
            {
                CenterPosition = new Point3D(0, 0, 0),
                Size = new System.Windows.Size(_width * 0.0005f, _height * 0.0005f),
                Material = vertexColorDiffuseMaterial,
            };



            meshGeometry = (MeshGeometry3D)_desktopScreen.Geometry;
            //meshGeometry.Freeze();
            _rootVisual3D.Children.Add(_desktopScreen);


            rightControllerVisual3D = new BoxVisual3D()
            {
                CenterPosition = new Point3D(0, 0, 0),
                Size = new Size3D(0.1f, 0.1f, 0.1f), // 10 x 1 x 10 meters
                Material = new DiffuseMaterial(System.Windows.Media.Brushes.Gray)
            };
            _rootVisual3D.Children.Add(rightControllerVisual3D);

            leftControllerVisual3D = new BoxVisual3D()
            {
                CenterPosition = new Point3D(0, 0, 0),
                Size = new Size3D(0.1f, 0.1f, 0.1f), // 10 x 1 x 10 meters
                Material = new DiffuseMaterial(System.Windows.Media.Brushes.Gray)
            };
            _rootVisual3D.Children.Add(leftControllerVisual3D);




            /*for (int i = 0;i < 4;i++)
            {
                _screenCorners = new BoxVisual3D()
                {
                    CenterPosition = new Point3D(0, 0, 0),
                    Size = new Size3D(0.01f, 0.01f, 0.01f), // 10 x 1 x 10 meters
                    Material = new DiffuseMaterial(System.Windows.Media.Brushes.Green)
                };
                _rootVisual3D.Children.Add(_screenCorners);
            }*/





            _screenCornersTotal = new BoxVisual3D[meshGeometry.Positions.Count];


            //Console.WriteLine(meshGeometry.Positions.Count + "***" + "meshGeometry.Positions.Count");



            DiffuseMaterial[] _diffuseMatArray = new DiffuseMaterial[]
            {
                new DiffuseMaterial(System.Windows.Media.Brushes.Blue),
                new DiffuseMaterial(System.Windows.Media.Brushes.Green),
                new DiffuseMaterial(System.Windows.Media.Brushes.Red),
                new DiffuseMaterial(System.Windows.Media.Brushes.Yellow),
            };


            point3DCollection = new Point3D[meshGeometry.Positions.Count];
            for (int i = 0; i < meshGeometry.Positions.Count; i++)
            {
                point3DCollection[i] = meshGeometry.Positions[i];
                _point3DCollection.Add(meshGeometry.Positions[i]);

                _screenCorners = new BoxVisual3D()
                {
                    CenterPosition = point3DCollection[i],//new Point3D(0, 0, 0),
                    Size = new Size3D(0.01f, 0.01f, 0.01f), // 10 x 1 x 10 meters
                    Material = _diffuseMatArray[i]//new DiffuseMaterial(System.Windows.Media.Brushes.Green)
                };
                _screenCornersTotal[i] = _screenCorners;

                _rootVisual3D.Children.Add(_screenCorners);
            }








            /*lowestX = point3DCollection.OrderBy(x => x.X).FirstOrDefault();
            highestX = point3DCollection.OrderBy(x => x.X).Last();

            lowestY = point3DCollection.OrderBy(y => y.X).FirstOrDefault();
            highestY = point3DCollection.OrderBy(y => y.X).Last();

            lowestZ = point3DCollection.OrderBy(z => z.X).FirstOrDefault();
            highestZ = point3DCollection.OrderBy(z => z.X).Last();*/



            plane = new Ab3d.Utilities.Plane(_point3DCollection[0], _point3DCollection[1], _point3DCollection[2]);
            pointOnPlane = plane.GetPointOnPlane();
            planeNormal = plane.Normal;




















            _viewport3D.Children.Add(_rootVisual3D);



            _leftControllerQuaternionRotation3D = new QuaternionRotation3D();
            _leftControllerBodyRotateTransform3D = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 0)); // rotate around y (up) axis 
            _leftControllerTranslateTransform3D = new TranslateTransform3D();
            var transform3DGroup0 = new Transform3DGroup();
            transform3DGroup0.Children.Add(new RotateTransform3D(_leftControllerQuaternionRotation3D));
            transform3DGroup0.Children.Add(_leftControllerBodyRotateTransform3D);
            transform3DGroup0.Children.Add(_leftControllerTranslateTransform3D);
            leftControllerVisual3D.Transform = transform3DGroup0;


            _rightControllerQuaternionRotation3D = new QuaternionRotation3D();
            _rightControllerBodyRotateTransform3D = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 0)); // rotate around y (up) axis
            _rightControllerTranslateTransform3D = new TranslateTransform3D();
            var transform3DGroup1 = new Transform3DGroup();
            transform3DGroup1.Children.Add(new RotateTransform3D(_rightControllerQuaternionRotation3D));
            transform3DGroup1.Children.Add(_rightControllerBodyRotateTransform3D);
            transform3DGroup1.Children.Add(_rightControllerTranslateTransform3D);
            rightControllerVisual3D.Transform = transform3DGroup1;


            var transform3DGroup00 = new Transform3DGroup();
            _desktopScreenQuaternionRotation3D = new QuaternionRotation3D();
            _desktopScreenTranslateTransform3D = new TranslateTransform3D(new Vector3D(0, 0, 0));
            _desktopScreenBodyRotateTransform3D = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 0), 0));
            transform3DGroup00.Children.Add(new RotateTransform3D(_desktopScreenQuaternionRotation3D));
            transform3DGroup00.Children.Add(_desktopScreenTranslateTransform3D);
            transform3DGroup00.Children.Add(_desktopScreenBodyRotateTransform3D);
            _desktopScreen.Transform = transform3DGroup00;



      

            _createdSceneObjects = true;




            ///////////////////////////////////////////////////////COOL AB4D API FUNCTIONS
            //_desktopScreen = Ab3d.Models.Model3DFactory.CreateVerticalPlane(new Point3D(0, 0.75f, 3.75f), new System.Windows.Size(_width * 0.00075f, _height * 0.00075f), vertexColorDiffuseMaterial);
            //RayMeshGeometry3DHitTestResult rayMeshResult = (RayMeshGeometry3DHitTestResult)VisualTreeHelper.HitTest(mainViewport, e.GetPosition(mainViewport));
            //Ab3d.Utilities.ModelUtils.PositionAndScaleModel3D(readModel3D, new Point3D(0, -100, 0), PositionTypes.Bottom, new Size3D(200, 200, 200));
            //Ab3d.Utilities.ModelUtils.ChangeMaterial(dragonModel3D, newMaterial: goldMaterial, newBackMaterial: null);
            //Ab3d.Utilities.MeshUtils.CalculateNormals(pyramidMeshGeometry3D);
            //var wpfOptimizedModel3DGroupNode = new Ab3d.DirectX.Models.WpfOptimizedModel3DGroupNode(model3DGroup, name: "Frozen Model3DGroup");
            //wpfOptimizedModel3DGroupNode.AutomaticallyClearWpfObjectsAfterInitialization = true;
            //Ab3d.Utilities.BitmapRendering.RenderToBitmap(_desktopFrame.texture2D);//not working
            //Ab3d.Utilities.TransformationsHelper.
            //Ab3d.Visuals.ModelRotatorVisual3D.InnerRadiusProperty.
            //Ab3d.Utilities.MeshUtils.TransformMeshGeometry3D(System.Windows.Media.Media3D.MeshGeometry3D, System.Windows.Media.Media3D.Transform3D, System.Boolean)">
        }
        #endregion


        #region Update


        private void SetCachingForObject(DependencyObject d)
        {
            RenderOptions.SetCachingHint(d, CachingHint.Cache);
            RenderOptions.SetCacheInvalidationThresholdMinimum(d, 0.5);
            RenderOptions.SetCacheInvalidationThresholdMaximum(d, 2.0);
            //RenderOptions.SetCacheInvalidationThresholdMinimum(d, 0.5);
            //RenderOptions.SetCacheInvalidationThresholdMaximum(d, 2.0);
        }

        private static void GetObjectDataArrays(out Vector3[] positions, out Vector3[] normals, out Vector2[] textureCoordinates, out int[] triangleIndices)
        {
            //// The following commented code is used to generate the arrays definition below:
            //var pyramidMeshGeometry3D = new Ab3d.Meshes.PyramidMesh3D(new Point3D(0, 0, 0), new Size3D(80, 50, 80)).Geometry;

            //if (pyramidMeshGeometry3D.Normals.Count == 0)
            //    pyramidMeshGeometry3D.Normals = Ab3d.Utilities.MeshUtils.CalculateNormals(pyramidMeshGeometry3D);

            //string meshArraysTest = GetMeshArraysText(pyramidMeshGeometry3D);


            // Pyramid mesh data (prepared with the commented code above):
            /*positions = new Vector3[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(1f, 0f, 1f),
                new Vector3(1f, 1f, 0f),
                new Vector3(1f, 1f, 1f),
            };

            normals = new Vector3[]
            {
                new Vector3(-1, 0f, 0),
                new Vector3(-1, 0f, 0),
                new Vector3(-1, 0f, 0),
                new Vector3(-1, 0f, 0),

                /*new Vector3(-1, 0f, -1),
                new Vector3(-1, 0f, -1),
                new Vector3(-1, 0f, -1),
                new Vector3(-1, 0f, -1),*/

            /*new Vector3(-1, 1f, -1),
            new Vector3(-1, 1f, -1),
            new Vector3(-1, 1f, -1),
            new Vector3(-1, 1f, -1),
        };

        textureCoordinates = new Vector2[]
        {
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(0, 1),
        };

        triangleIndices = new int[]
        {
            0,1,2,
            1,3,2,
        };*/
            positions = new Vector3[]
            {
                new Vector3(0f, 2, 0f),
                new Vector3(0f, 2, 0f),
                new Vector3(0f, 2, 0f),
                new Vector3(0f, 2, 0f),
                new Vector3(-1, 0f, -1),
                new Vector3(-1, 0f, -1),
                new Vector3(-1, 0f, -1),
                new Vector3(1, 0f, -1),
                new Vector3(1, 0f, -1),
                new Vector3(1, 0f, -1),
                new Vector3(1, 0f, 1),
                new Vector3(1, 0f, 1),
                new Vector3(1, 0f, 1),
                new Vector3(-1, 0f, 1),
                new Vector3(-1, 0f, 1),
                new Vector3(-1, 0f, 1),
            };

            normals = new Vector3[]
            {
                new Vector3(0f, 0.624695047554424f, -0.78086880944303f),
                new Vector3(0.78086880944303f, 0.624695047554424f, 0f),
                new Vector3(0f, 0.624695047554424f, 0.78086880944303f),
                new Vector3(-0.78086880944303f, 0.624695047554424f, 0f),
                new Vector3(0f, 0.624695047554424f, -0.78086880944303f),
                new Vector3(-0.78086880944303f, 0.624695047554424f, 0f),
                new Vector3(0f, -1f, 0f),
                new Vector3(0f, 0.624695047554424f, -0.78086880944303f),
                new Vector3(0.78086880944303f, 0.624695047554424f, 0f),
                new Vector3(0f, -1f, 0f),
                new Vector3(0.78086880944303f, 0.624695047554424f, 0f),
                new Vector3(0f, 0.624695047554424f, 0.78086880944303f),
                new Vector3(0f, -1f, 0f),
                new Vector3(0f, 0.624695047554424f, 0.78086880944303f),
                new Vector3(-0.78086880944303f, 0.624695047554424f, 0f),
                new Vector3(0f, -1f, 0f),
            };

            textureCoordinates = new Vector2[]
            {
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
            };

            triangleIndices = new int[]
            {
                0, 7, 4,
                1, 10, 8,
                2, 13, 11,
                3, 5, 14,
                6, 9, 15,
                9, 12, 15,
            };
        }

        private void UpdateTitleFpsMeter()
        {
            // We count number of rendered frames for each second.
            _framesCount++;

            // We also store the sum of the time required by DXEngine to render one frame.
            // This will be used to show average render time and the theoretical FPS (by DXEngine) in the window title
            if (_dxViewportView.DXScene != null && _dxViewportView.DXScene.Statistics != null)
            {
                // The Statistics.TotalRenderTimeMs time also contains time where the code waits for ovr.SubmitFrame method.
                // This method takes most of the time because it wait until 90 FPS sync.
                // To get accurate DXEngine rendering time we need to sum the sub-process times.
                // In the future version of DXEngine the VR time will be stored separately so we could substract it from the TotalRenderTimeMs.
                //
                //_renderTime += _dxViewportView.DXScene.Statistics.TotalRenderTimeMs;
                _renderTime += _dxViewportView.DXScene.Statistics.UpdateTimeMs +
                               _dxViewportView.DXScene.Statistics.PrepareRenderTimeMs +
                               _dxViewportView.DXScene.Statistics.RenderShadowsMs +
                               _dxViewportView.DXScene.Statistics.DrawRenderTimeMs +
                               _dxViewportView.DXScene.Statistics.PostProcessingRenderTimeMs +
                               _dxViewportView.DXScene.Statistics.CompleteRenderTimeMs;
            }

            // At the beginning of the next second, we show statistics in the window title
            int currentSecond = DateTime.Now.Second;

            if (_lastFpsMeterSecond == -1)
            {
                _lastFpsMeterSecond = currentSecond;
            }
            else if (currentSecond != _lastFpsMeterSecond)
            {
                // If we are here, then a new second has begun
                if (_isFirstSecond)
                {
                    // We start measuring in the middle of the first second so the result for the first second is not correct - do not show it
                    _isFirstSecond = false;
                }
                else
                {
                    string newTitle = string.Format("{0}  {1} FPS", _originalWindowTitle, _framesCount);
                    if (_renderTime > 0)
                    {
                        double averageRenderTime = _renderTime / _framesCount;
                        SkYaRk_Virtual_Desktop.MainWindow._mainWindow.Title = string.Format("  DXEngine render time: {0:0.00}ms => {1:0} FPS", averageRenderTime, 1000.0 / averageRenderTime);
                    }
                    this.Title = newTitle;
                }
                _framesCount = 0;
                _renderTime = 0;
                _lastFpsMeterSecond = currentSecond;
            }
        }

        // This method is called approximately 60 times per second but only when RenderAt90Fps const is false
        private void CompositionTargetOnRendering(object sender, EventArgs eventArgs)
        {
            // It's possible for Rendering to call back twice in the same frame.
            // So only render when we haven't already rendered in this frame.
            var renderingEventArgs = eventArgs as System.Windows.Media.RenderingEventArgs;
            if (renderingEventArgs != null)
            {
                if (renderingEventArgs.RenderingTime == _lastRenderTime)
                    return;

                _lastRenderTime = renderingEventArgs.RenderingTime;
            }

            if (_dxViewportView == null || _dxViewportView.IsDisposed)
                return; // Window closed

            if (_oculusRiftVirtualRealityProvider != null && _oculusRiftVirtualRealityProvider.LastSessionStatus.ShouldQuit)
                //this.Close(); // Exit the application

                if (_oculusRiftVirtualRealityProvider == null)
                    return;


            UpdateScene();

            // Render the scene again
            _dxViewportView.Refresh();
        }

        // This method is called each time DXEngine frame is rendered
        private void DXViewportViewOnSceneRendered(object sender, EventArgs eventArgs)
        {
            // Measure FPS
            UpdateTitleFpsMeter();
        }
        #endregion

        #region Touch controller handling
        private void ProcessTouchControllers()
        {
            if (_oculusRiftVirtualRealityProvider == null)
                return;
            var sessionPtr = _oculusRiftVirtualRealityProvider.SessionPtr;
            var connectedControllerTypes = _ovr.GetConnectedControllerTypes(sessionPtr);

            /*if (_oncer)
            {
                if (connectedControllerTypes.HasFlag(ControllerType.RTouch)) // Is Left touch controller connected?
                {
                    EnsureRightControllerVisual3D();
                    _viewport3D.Children.Add(_rightControllerVisual3D);
                }
                _oncer = false;
            }*/
            if (leftControllerVisual3D != null || rightControllerVisual3D != null)
            {
                double displayMidpoint = _ovr.GetPredictedDisplayTime(sessionPtr, 0);
                TrackingState trackingState = _ovr.GetTrackingState(sessionPtr, displayMidpoint, true);

                //bool latencyMark = false;
                //TrackingState trackState = _ovr.GetTrackingState(_oculusRiftVirtualRealityProvider.SessionPtr, 0.0f, latencyMark);
                //PoseStatef poseStatefer = trackState.HeadPose;
                //Posef hmdPose = poseStatefer.ThePose;
                //Quaternionf hmdRot = hmdPose.Orientation;

                var cameraPosition = _camera.GetCameraPosition();

                if (rightControllerVisual3D != null)
                {

                    var handPose = trackingState.HandPoses[1].ThePose;
                    var handPosePosition = handPose.Position;

                    // Update transformations that are already assigned to _rightControllerVisual3D

                    // First rotate the controller based on its rotation in our hand
                    _rightControllerQuaternionRotation3D.Quaternion = new System.Windows.Media.Media3D.Quaternion(handPose.Orientation.X, handPose.Orientation.Y, handPose.Orientation.Z, handPose.Orientation.W); // NOTE: Quaternion is struct so no GC "harm" is done here

                    // Now rotate because of our body rotation (the amount of rotation is defined in the _camera.Heading)
                    // We also need to adjust the center of rotation

                    _rightControllerBodyRotateTransform3D.CenterX = -handPosePosition.X;
                    _rightControllerBodyRotateTransform3D.CenterY = -handPosePosition.Y;
                    _rightControllerBodyRotateTransform3D.CenterZ = -handPosePosition.Z;
                    ((AxisAngleRotation3D)_rightControllerBodyRotateTransform3D.Rotation).Angle = -_camera.Heading;

                    //System.Windows.Media.Media3D.Point3D relativePoint = rightControllerVisual3D.TransformToAncestor(_rootVisual3D).Transform(new System.Windows.Media.Media3D.Point3D(0, 0, 0));

                    // Finally move the controller model for the hand pose offset + body offset
                    _rightControllerTranslateTransform3D.OffsetX = handPosePosition.X + cameraPosition.X;
                    _rightControllerTranslateTransform3D.OffsetY = handPosePosition.Y + cameraPosition.Y;
                    _rightControllerTranslateTransform3D.OffsetZ = handPosePosition.Z + cameraPosition.Z;

                    /*// Check the state of the controller Thicksticks and move or rotate accordingly
                    var leftControllerInputState = new InputState();
                    var rightControllerInputState = new InputState();
                    _ovr.GetInputState(sessionPtr, ControllerType.LTouch, ref leftControllerInputState);
                    _ovr.GetInputState(sessionPtr, ControllerType.RTouch, ref rightControllerInputState);

                    // Change camera angle
                    _camera.Heading += rightControllerInputState.Thumbstick[1].X * _xInputCameraController.RotationSpeed;

                    // Now move the camera (use strafe directions)
                    double dx = leftControllerInputState.Thumbstick[0].X * _xInputCameraController.MovementSpeed;
                    double dy = leftControllerInputState.Thumbstick[0].Y * _xInputCameraController.MovementSpeed;

                    // strafeDirection is perpendicular to LookDirection and UpDirection
                    var strafeDirection = Vector3D.CrossProduct(_camera.LookDirection, _camera.UpDirection);
                    strafeDirection.Normalize();

                    Vector3D movementVector = strafeDirection * dx; // move left / right

                    Vector3D usedLookDirection = _camera.LookDirection;
                    if (_xInputCameraController.MoveOnlyHorizontally)
                        usedLookDirection.Y = 0; // Zero y direction when we move only horizontally

                    usedLookDirection.Normalize();

                    movementVector += usedLookDirection * dy; // move forward / backward

                    _camera.Offset += movementVector;*/
                }

                if (leftControllerVisual3D != null)
                {
                    var handPose = trackingState.HandPoses[0].ThePose;
                    var handPosePosition = handPose.Position;

                    // Update transformations that are already assigned to _leftControllerVisual3D

                    // First rotate the controller based on its rotation in our hand
                    _leftControllerQuaternionRotation3D.Quaternion = new System.Windows.Media.Media3D.Quaternion(handPose.Orientation.X, handPose.Orientation.Y, handPose.Orientation.Z, handPose.Orientation.W); // NOTE: Quaternion is struct so no GC "harm" is done here

                    // Now rotate because of our body rotation (the amount of rotation is defined in the _camera.Heading)
                    // We also need to adjust the center of rotation

                    _leftControllerBodyRotateTransform3D.CenterX = -handPosePosition.X;
                    _leftControllerBodyRotateTransform3D.CenterY = -handPosePosition.Y;
                    _leftControllerBodyRotateTransform3D.CenterZ = -handPosePosition.Z;
                    ((AxisAngleRotation3D)_leftControllerBodyRotateTransform3D.Rotation).Angle = -_camera.Heading;

                    // Finally move the controller model for the hand pose offset + body offset

                    _leftControllerTranslateTransform3D.OffsetX = handPosePosition.X + cameraPosition.X;
                    _leftControllerTranslateTransform3D.OffsetY = handPosePosition.Y + cameraPosition.Y;
                    _leftControllerTranslateTransform3D.OffsetZ = handPosePosition.Z + cameraPosition.Z;
                }
            }








            /*if (_oculusRiftVirtualRealityProvider == null)
                return;

            var sessionPtr = _oculusRiftVirtualRealityProvider.SessionPtr;
            var connectedControllerTypes = _ovr.GetConnectedControllerTypes(sessionPtr);

            if (connectedControllerTypes.HasFlag(ControllerType.LTouch)) // Is Left touch controller connected?
            {
                if (_leftControllerVisual3D == null) // If controller is not yet visible, show it
                {
                    EnsureLeftControllerVisual3D();
                    _viewport3D.Children.Add(_leftControllerVisual3D);
                }
            }
            else
            {
                if (_leftControllerVisual3D != null) // If controller is visible, hide it
                {
                    _viewport3D.Children.Remove(_leftControllerVisual3D);
                    _leftControllerVisual3D = null;
                }
            }


            if (connectedControllerTypes.HasFlag(ControllerType.RTouch)) // Is Right touch controller connected?
            {
                if (_rightControllerVisual3D == null) // If controller is not yet visible, show it
                {
                    EnsureRightControllerVisual3D();
                    _viewport3D.Children.Add(_rightControllerVisual3D);
                }
            }
            else
            {
                if (_rightControllerVisual3D != null) // If controller is visible, hide it
                {
                    _viewport3D.Children.Remove(_rightControllerVisual3D);
                    _rightControllerVisual3D = null;
                }
            }








            // If any controller is visible update its transformation
            if (_leftControllerVisual3D != null || _rightControllerVisual3D != null)
            {
                double displayMidpoint = _ovr.GetPredictedDisplayTime(sessionPtr, 0);
                TrackingState trackingState = _ovr.GetTrackingState(sessionPtr, displayMidpoint, true);

                var cameraPosition = _camera.GetCameraPosition();

                if (_leftControllerVisual3D != null)
                {
                    var handPose = trackingState.HandPoses[0].ThePose;
                    var handPosePosition = handPose.Position;

                    // Update transformations that are already assigned to _leftControllerVisual3D

                    // First rotate the controller based on its rotation in our hand
                    _leftControllerQuaternionRotation3D.Quaternion = new System.Windows.Media.Media3D.Quaternion(handPose.Orientation.X, handPose.Orientation.Y, handPose.Orientation.Z, handPose.Orientation.W); // NOTE: Quaternion is struct so no GC "harm" is done here

                    // Now rotate because of our body rotation (the amount of rotation is defined in the _camera.Heading)
                    // We also need to adjust the center of rotation

                    _leftControllerBodyRotateTransform3D.CenterX = -handPosePosition.X;
                    _leftControllerBodyRotateTransform3D.CenterY = -handPosePosition.Y;
                    _leftControllerBodyRotateTransform3D.CenterZ = -handPosePosition.Z;
                    ((AxisAngleRotation3D)_leftControllerBodyRotateTransform3D.Rotation).Angle = -_camera.Heading;

                    // Finally move the controller model for the hand pose offset + body offset
                    _leftControllerTranslateTransform3D.OffsetX = cameraPosition.X + handPosePosition.X;
                    _leftControllerTranslateTransform3D.OffsetY = cameraPosition.Y + handPosePosition.Y;
                    _leftControllerTranslateTransform3D.OffsetZ = cameraPosition.Z + handPosePosition.Z;
                }

                if (_rightControllerVisual3D != null)
                {
                    var handPose = trackingState.HandPoses[1].ThePose;
                    var handPosePosition = handPose.Position;

                    // Update transformations that are already assigned to _rightControllerVisual3D

                    // First rotate the controller based on its rotation in our hand
                    _rightControllerQuaternionRotation3D.Quaternion = new System.Windows.Media.Media3D.Quaternion(handPose.Orientation.X, handPose.Orientation.Y, handPose.Orientation.Z, handPose.Orientation.W); // NOTE: Quaternion is struct so no GC "harm" is done here

                    // Now rotate because of our body rotation (the amount of rotation is defined in the _camera.Heading)
                    // We also need to adjust the center of rotation

                    _rightControllerBodyRotateTransform3D.CenterX = -handPosePosition.X;
                    _rightControllerBodyRotateTransform3D.CenterY = -handPosePosition.Y;
                    _rightControllerBodyRotateTransform3D.CenterZ = -handPosePosition.Z;
                    ((AxisAngleRotation3D)_rightControllerBodyRotateTransform3D.Rotation).Angle = -_camera.Heading;

                    // Finally move the controller model for the hand pose offset + body offset
                    _rightControllerTranslateTransform3D.OffsetX = cameraPosition.X + handPosePosition.X;
                    _rightControllerTranslateTransform3D.OffsetY = cameraPosition.Y + handPosePosition.Y;
                    _rightControllerTranslateTransform3D.OffsetZ = cameraPosition.Z + handPosePosition.Z;
                }

                // Check the state of the controller Thicksticks and move or rotate accordingly
                var leftControllerInputState = new InputState();
                var rightControllerInputState = new InputState();
                _ovr.GetInputState(sessionPtr, ControllerType.LTouch, ref leftControllerInputState);
                _ovr.GetInputState(sessionPtr, ControllerType.RTouch, ref rightControllerInputState);

                // Change camera angle
                _camera.Heading += rightControllerInputState.Thumbstick[1].X * _xInputCameraController.RotationSpeed;

                // Now move the camera (use strafe directions)
                double dx = leftControllerInputState.Thumbstick[0].X * _xInputCameraController.MovementSpeed;
                double dy = leftControllerInputState.Thumbstick[0].Y * _xInputCameraController.MovementSpeed;

                // strafeDirection is perpendicular to LookDirection and UpDirection
                var strafeDirection = Vector3D.CrossProduct(_camera.LookDirection, _camera.UpDirection);
                strafeDirection.Normalize();

                Vector3D movementVector = strafeDirection * dx; // move left / right

                Vector3D usedLookDirection = _camera.LookDirection;
                if (_xInputCameraController.MoveOnlyHorizontally)
                    usedLookDirection.Y = 0; // Zero y direction when we move only horizontally

                usedLookDirection.Normalize();

                movementVector += usedLookDirection * dy; // move forward / backward

                _camera.Offset += movementVector;
            }*/
        }










        private void EnsureLeftControllerMesh()
        {
            // Controller mash is get from: ovr_sdk_win_1.17.0_public\OculusSDK\Samples\OculusWorldDemo\Assets\Tuscany\LeftController.xml
            // This model is also used to render the right controller. It is only flipped on x axis (see OculusWorldDemoApp::RenderControllers)
            /*_leftControllerMesh = (MeshGeometry3D)this.FindResource("LeftTouchController");

            if (_leftControllerMesh == null)
                throw new Exception("Cannot find the LeftTouchController MeshGeometry3D - it is defined in the App.xaml");

            // We also create the material here
            _controllerMaterial = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 20))); // Almost back Diffuse material*/
        }

        private void EnsureLeftControllerVisual3D()
        {
            /*if (_leftControllerVisual3D != null)
                return;

            EnsureLeftControllerMesh();

            var controllerModel3D = new GeometryModel3D(_leftControllerMesh, _controllerMaterial);
            controllerModel3D.BackMaterial = _controllerMaterial;

            // Add transformations
            _leftControllerQuaternionRotation3D = new QuaternionRotation3D();
            _leftControllerBodyRotateTransform3D = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)); // rotate around y (up) axis
            _leftControllerTranslateTransform3D = new TranslateTransform3D();

            var transform3DGroup = new Transform3DGroup();
            transform3DGroup.Children.Add(new RotateTransform3D(_leftControllerQuaternionRotation3D));
            transform3DGroup.Children.Add(_leftControllerBodyRotateTransform3D);
            transform3DGroup.Children.Add(_leftControllerTranslateTransform3D);

            controllerModel3D.Transform = transform3DGroup;



            // Finally create ModelVisualD that can be added to Viewport3D's Children
            _leftControllerVisual3D = controllerModel3D.CreateModelVisual3D();*/
        }




        private void EnsureRightControllerVisual3D()
        {
            /*if (_rightControllerVisual3D != null)
                return;


            // Right controller model is same as left controller but flipped around x axis.
            // 
            // Because flipping x axis also change the winding order of triangles, this would lead to showing the model with rendering the inside of the triangles.
            // To fix that we need to swap the order of triangles with swapping first and second triangle indice.

            EnsureLeftControllerMesh();


            // Flip x position
            var leftPositions = _leftControllerMesh.Positions;
            int count = leftPositions.Count;

            var rightPositions = new Point3DCollection(count);
            for (int i = 0; i < count; i++)
            {
                var leftPosition = leftPositions[i];
                rightPositions.Add(new Point3D(-leftPosition.X, leftPosition.Y, leftPosition.Z));
            }

            // swap 1st and 2nd triangle indice 
            var leftTriangleIndices = _leftControllerMesh.TriangleIndices;
            count = leftTriangleIndices.Count;

            var rightTriangleIndices = new Int32Collection(count);
            for (int i = 0; i < count; i += 3)
            {
                rightTriangleIndices.Add(leftTriangleIndices[i + 1]);
                rightTriangleIndices.Add(leftTriangleIndices[i + 0]);
                rightTriangleIndices.Add(leftTriangleIndices[i + 2]);
            }


            var rightControllerMesh = new MeshGeometry3D()
            {
                Positions = rightPositions,
                TriangleIndices = rightTriangleIndices
            };

            // Now we can crate the GeometryModel3D and ModelVisual3D
            var controllerModel3D = new GeometryModel3D(rightControllerMesh, _controllerMaterial);
            controllerModel3D.BackMaterial = _controllerMaterial;

            // Add transformations
            _rightControllerQuaternionRotation3D = new QuaternionRotation3D();
            _rightControllerBodyRotateTransform3D = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)); // rotate around y (up) axis
            _rightControllerTranslateTransform3D = new TranslateTransform3D();

            var transform3DGroup = new Transform3DGroup();
            transform3DGroup.Children.Add(new RotateTransform3D(_rightControllerQuaternionRotation3D));
            transform3DGroup.Children.Add(_rightControllerBodyRotateTransform3D);
            transform3DGroup.Children.Add(_rightControllerTranslateTransform3D);

            controllerModel3D.Transform = transform3DGroup;

            // Finally create ModelVisualD that can be added to Viewport3D's Children
            _rightControllerVisual3D = controllerModel3D.CreateModelVisual3D();*/
        }
        #endregion

        #region Dispose
        private void DisposeOculusRiftVirtualRealityProvider()
        {
            if (_oculusRiftVirtualRealityProvider != null)
            {
                _oculusRiftVirtualRealityProvider.Dispose();
                _oculusRiftVirtualRealityProvider = null;
            }

            _ovr = null;
        }

        private void Dispose()
        {
            DisposeOculusRiftVirtualRealityProvider();
            _disposables.Dispose();
            if (_dxViewportView != null)
            {
                _dxViewportView.Dispose();
                _dxViewportView = null;
            }

            if (_dxDevice != null)
            {
                _dxDevice.Dispose();
                _dxDevice = null;
            }
        }
        #endregion


       string[] keysStringArray = new string[]
       {
            "Oculus Touch Right Mouse Control",
            "Oculus Touch Left Mouse Control",
       };


        static BitmapSource source;
        static System.Drawing.Rectangle rect;
        static BitmapData bitmapData;
        private static BitmapSource CreateBitmapSource(Bitmap bitmap)
        {
            try
            {
                rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
                source = BitmapSource.Create(bitmap.Width, bitmap.Height, 24, 24, PixelFormats.Pbgra32, null, bitmapData.Scan0, bitmapData.Stride * bitmap.Height, bitmapData.Stride); //bitmap.HorizontalResolution, bitmap.VerticalResolution
                bitmap.UnlockBits(bitmapData);
                return source;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// ALTERNATIVE MOUSE INPUT WITH USER32.DLL
        /// </summary>
        /*public class MouseSimulator
        {
            [DllImport("user32.dll", SetLastError = true)]
            static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

            [StructLayout(LayoutKind.Sequential)]
            struct INPUT
            {
                public SendInputEventType type;
                public MouseKeybdhardwareInputUnion mkhi;
            }
            [StructLayout(LayoutKind.Explicit)]
            struct MouseKeybdhardwareInputUnion
            {
                [FieldOffset(0)]
                public MouseInputData mi;

                [FieldOffset(0)]
                public KEYBDINPUT ki;

                [FieldOffset(0)]
                public HARDWAREINPUT hi;
            }
            [StructLayout(LayoutKind.Sequential)]
            struct KEYBDINPUT
            {
                public ushort wVk;
                public ushort wScan;
                public uint dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }
            [StructLayout(LayoutKind.Sequential)]
            struct HARDWAREINPUT
            {
                public int uMsg;
                public short wParamL;
                public short wParamH;
            }
            struct MouseInputData
            {
                public int dx;
                public int dy;
                public uint mouseData;
                public MouseEventFlags dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }
            [Flags]
            enum MouseEventFlags : uint
            {
                MOUSEEVENTF_MOVE = 0x0001,
                MOUSEEVENTF_LEFTDOWN = 0x0002,
                MOUSEEVENTF_LEFTUP = 0x0004,
                MOUSEEVENTF_RIGHTDOWN = 0x0008,
                MOUSEEVENTF_RIGHTUP = 0x0010,
                MOUSEEVENTF_MIDDLEDOWN = 0x0020,
                MOUSEEVENTF_MIDDLEUP = 0x0040,
                MOUSEEVENTF_XDOWN = 0x0080,
                MOUSEEVENTF_XUP = 0x0100,
                MOUSEEVENTF_WHEEL = 0x0800,
                MOUSEEVENTF_VIRTUALDESK = 0x4000,
                MOUSEEVENTF_ABSOLUTE = 0x8000
            }
            enum SendInputEventType : int
            {
                InputMouse,
                InputKeyboard,
                InputHardware
            }

            public static void ClickLeftMouseButtonAndHold()
            {
                INPUT mouseDownInput = new INPUT();
                mouseDownInput.type = SendInputEventType.InputMouse;
                mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
                SendInput(1, ref mouseDownInput, Marshal.SizeOf(new INPUT()));
            }

            public static void ClickLeftMouseButton()
            {
                INPUT mouseDownInput = new INPUT();
                mouseDownInput.type = SendInputEventType.InputMouse;
                mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
                SendInput(1, ref mouseDownInput, Marshal.SizeOf(new INPUT()));

                INPUT mouseUpInput = new INPUT();
                mouseUpInput.type = SendInputEventType.InputMouse;
                mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
                SendInput(1, ref mouseUpInput, Marshal.SizeOf(new INPUT()));
            }

            public static void unClickLeftMouseButton()
            {
                INPUT mouseUpInput = new INPUT();
                mouseUpInput.type = SendInputEventType.InputMouse;
                mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
                SendInput(1, ref mouseUpInput, Marshal.SizeOf(new INPUT()));
            }

            public static void ClickRightMouseButtonAndHold()
            {
                INPUT mouseDownInput = new INPUT();
                mouseDownInput.type = SendInputEventType.InputMouse;
                mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTDOWN;
                SendInput(1, ref mouseDownInput, Marshal.SizeOf(new INPUT()));
            }

            public static void ClickRightMouseButton()
            {
                INPUT mouseDownInput = new INPUT();
                mouseDownInput.type = SendInputEventType.InputMouse;
                mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTDOWN;
                SendInput(1, ref mouseDownInput, Marshal.SizeOf(new INPUT()));

                INPUT mouseUpInput = new INPUT();
                mouseUpInput.type = SendInputEventType.InputMouse;
                mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTUP;
                SendInput(1, ref mouseUpInput, Marshal.SizeOf(new INPUT()));
            }

            public static void unClickRightMouseButton()
            {
                INPUT mouseUpInput = new INPUT();
                mouseUpInput.type = SendInputEventType.InputMouse;
                mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTUP;
                SendInput(1, ref mouseUpInput, Marshal.SizeOf(new INPUT()));
            }
        }*/



        //FAST CREATION OF BitmapImage - ALTERNATIVE FOR CREATING THE SHADERRESOURCEVIEW
        /*BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    return bitmapimage;
                }
            }
            finally
            {
                bitmap.Dispose();
            }
        }*/

        //KEEPING THAT HERE JUST FOR THE FUTURE - FROM QUAKEII dudes
        /*float InvSqrt(float x)
        {
            float xhalf = 0.5f * x;
            int i = BitConverter.ToInt32(BitConverter.GetBytes(x), 0);
            i = 0x5f3759df - (i >> 1);
            x = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
            x = x * (1.5f - xhalf * x * x);
            return x;
        }*/
    }
}