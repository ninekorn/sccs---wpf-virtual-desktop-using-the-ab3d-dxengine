// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Drawing.Imaging;
using System.IO;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;


using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

using System.Drawing;
using System.Runtime.InteropServices;

using System.Windows.Input;
using System.Windows.Forms;


using SharpDX.Mathematics.Interop;
//using Device = SharpDX.Direct3D11.Device;
//using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.DXGI.Resource;
using ResultCode = SharpDX.DXGI.ResultCode;

using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

using System.Linq;
using System.Collections.Generic;


namespace SkYaRk_Virtual_Desktop
{
    /// <summary>
    ///   Screen capture of the desktop using DXGI OutputDuplication.
    /// </summary>
    public unsafe class SC_SharpDX_ScreenCapture
    {
        // # of graphics card adapter
        static int _numAdapter = 0;
        // # of output device (i.e. monitor)
        static int _numOutput = 0;

        readonly Adapter1 _adapter;
        //static Factory1 _factory;

        readonly SharpDX.Direct3D11.Device _device;
        //static Output _output;

        

        readonly Output1 _output1;
        static Texture2D _texture2D;
        readonly OutputDuplication _outputDuplication;
        readonly Texture2DDescription _textureDescription;
        System.Drawing.Bitmap _bitmap;

        System.Drawing.Bitmap _bitmapPlayerRect;




        static int _width = 0;
        static int _height = 0;
        int _bytesTotal;
        int _bytesTotalPlayerRect;



        SharpDX.DXGI.Resource _screenResource;
        OutputDuplicateFrameInformation _duplicateFrameInformation;
        OutputDuplicateFrameInformation _previousDuplicateFrameInformation;

        SC_SharpDX_ScreenFrame _frameCaptureData;
        //Bitmap desktopBMP;

        byte[] _emptyArrayPaste;
        byte[] _arrayPlayerPos;
        byte[] _arrayPreviousPlayerPos;
        byte[] _currentArrayPlayerPos;


        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        static Thread _thread;
        Action<string> fuckOff;
        public SC_SharpDX_ScreenCapture(int adapter, int numOutput)
        {
            _buffer = new byte[widthOfRectanglePlayerShip * 4];
            _bufferPlayerShipRect = new byte[widthOfRectanglePlayerShip * 4];

            //_frameCaptureData = new SC_SharpDX_ScreenFrame();
            _frameCaptureData = new SC_SharpDX_ScreenFrame();




            fuckOff = new Action<string>(writeTo);



            _numAdapter = adapter;
            _numOutput = numOutput;

            _currentByteArray = new byte[1920 * 1080 * 4];

            for (int i = 0; i < _currentByteArray.Length; i++)
            {
                _currentByteArray[i] = 0;
            }

            _previousTextureByteArray = new byte[1920 * 1080 * 4];

            for (int i = 0; i < _previousTextureByteArray.Length; i++)
            {
                _previousTextureByteArray[i] = 0;
            }


            try
            {
                using (var _factory = new Factory1())
                {
                    this._adapter = _factory.GetAdapter1(_numAdapter);
                }
            }
            catch (SharpDXException ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            try
            {
                //initializeDevice();
                this._device = new Device(_adapter);
            }
            catch (SharpDXException ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            try
            {
                //initializeOutput();
                using (var _output = _adapter.GetOutput(_numOutput))
                {
                    // Width/Height of desktop to capture
                    //getDesktopBoundaries();
                    _width = ((SharpDX.Rectangle)_output.Description.DesktopBounds).Width;
                    _height = ((SharpDX.Rectangle)_output.Description.DesktopBounds).Height;
                    _frameCaptureData.width = _width;
                    _frameCaptureData.height = _height;
                    this._output1 = _output.QueryInterface<Output1>();
                }
            }
            catch (SharpDXException ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            try
            {
                //duplicateOutput();
                this._outputDuplication = _output1.DuplicateOutput(_device);
            }
            catch (SharpDXException ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            try
            {
                //getTextureDescription();
                this._textureDescription = new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.Read,
                    BindFlags = BindFlags.None,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = _width,
                    Height = _height,
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Staging
                };
            }
            catch (SharpDXException ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }



            _texture2D = new Texture2D(_device, _textureDescription);


            try
            {
                _bitmap = new System.Drawing.Bitmap(_width, _height, PixelFormat.Format32bppArgb);
                var boundsRect = new System.Drawing.Rectangle(0, 0, _width, _height);
                var bmpData = _bitmap.LockBits(boundsRect, ImageLockMode.ReadOnly, _bitmap.PixelFormat);
                _bytesTotal = Math.Abs(bmpData.Stride) * _bitmap.Height;
                _bitmap.UnlockBits(bmpData);
                _textureByteArray = new byte[_bytesTotal];

                _bitmapPlayerRect = new System.Drawing.Bitmap(widthOfRectanglePlayerShip, heightOfRectanglePlayerShip, PixelFormat.Format32bppArgb);
                var boundsRect0 = new System.Drawing.Rectangle(0, 0, widthOfRectanglePlayerShip, heightOfRectanglePlayerShip);
                var bmpData0 = _bitmapPlayerRect.LockBits(boundsRect0, ImageLockMode.ReadOnly, _bitmapPlayerRect.PixelFormat);
                _bytesTotalPlayerRect = Math.Abs(bmpData0.Stride) * _bitmapPlayerRect.Height;
                _bitmapPlayerRect.UnlockBits(bmpData0);




            }
            catch (SharpDXException ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }


        void writeTo(string test)
        {
            Console.WriteLine(test);
        }



        IntPtr memIntPtr;
        // Get a byte pointer from the IntPtr object.
        byte* memBytePtr;
        bool _hasAcquiredFrame = false;

        [STAThread]
        public SC_SharpDX_ScreenFrame ScreenCapture(int timeOut)
        {
            _hasAcquiredFrame = false;
            try
            {
                //acquireFrame(timeOut);
                if (!acquireFrame(timeOut))
                {
                    _hasAcquiredFrame = false;
                    ///return _frameCaptureData;
                    return _frameCaptureData;
                }
                else
                {
                    //releaseFrame();
                    _hasAcquiredFrame = true;
                }




                if (!copyResource())
                {
                    //_hasAcquiredFrame = false;
                    //return _frameCaptureData;
                }

                if (!mapSubResource())
                {
                    //return _frameCaptureData;
                    //_hasAcquiredFrame = false;
                }
            }
            catch //(SharpDXException ex)
            {
                //Console.WriteLine(ex.ToString());
            }

            if (_hasAcquiredFrame)
            {
                releaseFrame();
            }
            return _frameCaptureData;
            /*finally
            {
                if (_hasAcquiredFrame)
                {

                }
            }*/

            /*if (!releaseFrame())
            {
                return _frameCaptureData;
            }

            return _frameCaptureData;*/
            // Try to get duplicated frame within given time        
        }

        int _testCounter = 0;

        Stopwatch _testWatch = new Stopwatch();

        int _totalFrames = 0;
        bool _startStopWatch = true;
        bool acquireFrame(int timeOut)
        {
            _screenResource = null;
            try
            {
                /*if (_startStopWatch)
                {
                    _testWatch.Stop();
                    _testWatch.Reset();
                    _testWatch.Start();
                    _startStopWatch = false;
                }*/
                
                //_duplicateFrameInformation = new OutputDuplicateFrameInformation();
                //_screenResource = new Resource(IntPtr.Zero);

                //var result = this.TryAcquireNextFrame(timeoutInMilliseconds, out frameInfoRef, out desktopResourceOut);result.CheckError();

                //_outputDuplication.AcquireNextFrame(timeOut, out _duplicateFrameInformation, out _screenResource);
                //Console.WriteLine(_totalFrames);
                if (_totalFrames >= 1)
                {
                    //_testWatch.Stop();
                    //int _milli = _testWatch.Elapsed.Milliseconds;
                    //Console.WriteLine("_milli" + "***" + _milli);
                    Result _result = _outputDuplication.TryAcquireNextFrame(0, out _duplicateFrameInformation, out _screenResource);
                    _totalFrames = 0;
                }



                

                //if (_milli <= 0)
                //{
                //    Thread.Sleep(100);
                //}

                //if (_testCounter >= 100)
                //{
                //    _testCounter = 0;
                //}
                //Thread.Sleep(10);
                //Console.WriteLine("_testCounter" + "***" + _testCounter );
                //_testCounter++;
            }
            catch (SharpDXException ex)
            {
                /*if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {

                    return true;
                }*/

                //Console.WriteLine(ex.ToString());

                /*if (ex.ResultCode.Failure)
                {
                    //var yo = SharpDX.DXGI.DXGIDebug.;
                    //Configuration.EnableObjectTracking = false;
                    //SharpDX.DXGI.DXGIDebug.LogMemoryLeakWarning = fuckOff;
                    //SharpDX.DXGI.DXGIDebug1
                    //Thread.Sleep(10);
                    //Console.WriteLine("_testCounter" + "***" + _testCounter );                
                    _testCounter++;
                }*/
                //GC.Collect();
            }
            _totalFrames++;

            if (_screenResource != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }




        public void Disposer()
        {

            _device.Dispose();
            _output1.Dispose();
            _texture2D.Dispose();
            _outputDuplication.Dispose();
            //_textureDescription.Dispose();
            _bitmap.Dispose();
            _bitmapPlayerRect.Dispose();
            _screenResource.Dispose();
            //_frameCaptureData.Dispose();
            GC.Collect();
        }
















        bool copyResource()
        {
            try
            {
                //_texture2D = new Texture2D(_device, _textureDescription);
                // copy resource into memory that can be accessed by the CPU
                using (var screenTexture2D = _screenResource.QueryInterface<Texture2D>())
                {
                    _device.ImmediateContext.CopyResource(screenTexture2D, _texture2D);
                }
            }
            catch
            {
                return false;
            }
            _screenResource.Dispose();
            return true;
        }

        public byte[] _textureByteArray;
        public byte[] _previousTextureByteArray;
        public byte[] _dummyTextureByteArray;


        public byte[] _currentByteArray;

        //System.Drawing.Rectangle _moveScreenRect = new System.Drawing.Rectangle();
        //System.Drawing.Rectangle _playerRect = new System.Drawing.Rectangle();


        SharpDX.Rectangle _moveScreenRect = new SharpDX.Rectangle();
        SharpDX.Rectangle _playerRect = new SharpDX.Rectangle();

        //SharpDX.Rectangle test = new SharpDX.Rectangle();
        int counter0 = 0;

        int frameCounter = 0;


        //PLAYER SHIP
        ///////////////////////////////////////////////////////////// 736000 pixels just for the player ship
        int widthOfRectanglePlayerShip = 460; // middle of Width = 1920/2 = 960
        int heightOfRectanglePlayerShip = 400;// middle of height = 1080/2 = 540

        int xxPlayerShip = 720; //(y * width) + x = byte position 
        int yyPlayerShip = 360;
        //UI TOP LEFT
        /////////////////////////////////////////////////////////////
        int widthOfRectangleUITOPLEFT = 400; // 280
        int heightOfRectangleUITOPLEFT = 90;// 40

        int xxUITOPLEFT = 0; //(y * width) + x = byte position 
        int yyUITOPLEFT = 0;
        //UI BOTTOM LEFT
        /////////////////////////////////////////////////////////////
        int widthOfRectangleUIBOTTOMLEFT = 395; // 280
        int heightOfRectangleBOTTOMLEFT = 100;// 40

        int xxBOTTOMLEFT = 0; //(y * width) + x = byte position 
        int yyBOTTOMLEFT = 980;
        //UI CONSUMABLES BAR
        /////////////////////////////////////////////////////////////
        int widthOfRectangleCONSUMABLESBAR = 185; // 280
        int heightOfRectangleCONSUMABLESBAR = 65;// 40

        int xxCONSUMABLESBAR = 685; //(y * width) + x = byte position 
        int yyCONSUMABLESBAR = 1015;
        //UI SKILLS BAR
        /////////////////////////////////////////////////////////////
        int widthOfRectangleSKILLSBAR = 357; // 280
        int heightOfRectangleSKILLSBAR = 65;// 40

        int xxSKILLSBAR = 880; //(y * width) + x = byte position 
        int yySKILLSBAR = 1015;
        //UI WEAPONS BAR
        /////////////////////////////////////////////////////////////
        int widthOfRectangleWEAPONSBAR = 320; // 280
        int heightOfRectangleWEAPONSBAR = 200;// 40

        int xxWEAPONSBAR = 1600; //(y * width) + x = byte position 
        int yyWEAPONSBAR = 880;
        //UI TOP RIGHT MAP
        /////////////////////////////////////////////////////////////
        int widthOfRectangleTOPRIGHTMAP = 315; // 280
        int heightOfRectangleTOPRIGHTMAP = 320;// 40

        int xxTOPRIGHTMAP = 1605; //(y * width) + x = byte position 
        int yyTOPRIGHTMAP = 0;

        IntPtr isActive;
        int bytePos;

        int i = 0;

        bool _startOnce = true;


        Stopwatch _countingTime = new Stopwatch();
        static bool _startThread = false;


        int _previousWidthOfRectangle;
        int _previousHeightOfRectangle;

        int _previousxxRectStartingPos;
        int _previousyyRectStartingPos;

        //byte a = 0;
        //byte r = 0;
        //byte g = 0;
        //byte b = 0;



        void _mainThreadStarter()
        {
            _mainThreadLoop:

            if (_startThread)
            {
                try
                {
                    //_countingTime.Stop();
                    //_countingTime.Reset();
                    //_countingTime.Start();

                    //Console.WriteLine("test");
                    //pointerTest();


                    //_countingTime.Stop();
                    //var milliseconds = _countingTime.Elapsed.Milliseconds;
                    //Console.WriteLine(milliseconds);


                    //_previousTextureByteArray = _currentByteArray;
                    _startThread = false;
                }
                catch
                {

                }
            }
            else
            {
                Thread.Sleep(1);
            }
            goto _mainThreadLoop;
        }


        /*public string ConvertToString(byte[] arr)
        {
            unsafe
            {
                string returnStr;
                fixed (byte* fixedPtr = arr)
                {
                    returnStr = new string((sbyte*)fixedPtr);
                }
            }

            return (returnStr);
        }*/

        unsafe static string Test(char* characters)
        {
            return new string(characters);
        }

        byte[] _totalArray = new byte[460 * 400 * 4];
        byte[] _widthScreen = new byte[1920 * 4];



        /*fixed (byte* textureByteArray = _textureByteArray)//, previousTextureByteArray = _previousTextureByteArray, currentByteArray = _currentByteArray)
        {
            for (int yyy = 0; yyy < 1080; yyy++)
            {
                for (int xxx = 0; xxx < 1920; xxx++)
                {
                    var bytePoser = (((yyy) * 1920) + (xxx)) * 4;

                    if (textureByteArray[bytePoser + 0] + textureByteArray[bytePoser + 1] + textureByteArray[bytePoser + 2] >= 765)
                    {
                        textureByteArray[bytePoser + 0] = 0;
                        textureByteArray[bytePoser + 1] = 0;
                        textureByteArray[bytePoser + 2] = 0;
                        textureByteArray[bytePoser + 3] = 0;
                    }
                }
            }
        }*/



        int _frameCount = 0;
        void pointerTest()
        {       
            //int xx = 0;
            //int* _playerXX = &xx;

            int x = xxPlayerShip;
            int y = yyPlayerShip;

            /*int* _playerX = &x;
            int* _playerY = &y;

            int _widther = 1920;
            int* _widtherPointer = &_widther;

            int length = _bufferPlayerShipRect.Length;
            int _widthShip = x + widthOfRectanglePlayerShip;

            int _widthShiper = widthOfRectanglePlayerShip;
            int* _widthShipRect = &_widthShiper;

            int _heightShip = y + heightOfRectanglePlayerShip;
            int* _pointerWidth = &length;
            int* _pointerHeight = &_heightShip;
            int _byte = 4;
            int* _pointerByte = &_byte;*/

            // int _maxColor = 750;
            //int* maxColor = &_maxColor;
            //int zero = 0;
            //int* _zero = &zero;
            //byte zeroByte = 0;
            //byte* _zeroByte = &zeroByte;
            //int one = 0;
            //int* _one = &one;
            //int two = 0;
            //int* _two = &two;
            //int three = 0;
            //int* _three = &three;

            //int* looper = _zero;


            _countingTime.Stop();
            _countingTime.Reset();
            _countingTime.Start();


            /*char[] charArray = "My name is Rex. Welcome to my unsafe world.".ToCharArray();

            char* uCharArray = stackalloc char[charArray.Length];
            char* uChar = uCharArray;
            for (int i = 0; i < charArray.Length; i++, uChar++)
            {
                *uChar = charArray[i];
            }*/

            //byte* uChar = arraySetup;

            /*for (int i = 0; i < _textureByteArray.Length; i++, uChar++)
            {

            }*/
            /*byte* arraySetup = stackalloc byte[1920 * 4];
           
            fixed (byte* textureByteArray = _textureByteArray)//widthScreen = _widthScreen)
            {
                for (int yyy = 0; yyy < 1080; yyy++)
                {
                    byte* someByte = arraySetup;
                    for (int xxx = 0; xxx < 1920; xxx++, someByte++)
                    {
                        var bytePoser = (((yyy) * 1920) + (xxx)) * 4;
                        *someByte = textureByteArray[bytePoser];                                        
                    }

                    for (int i = 0; i < 1920 * 4; i++)
                    {
                        if (someByte[i + 0] + someByte[i + 1] + someByte[i + 2] >= 765)
                        {
                            someByte[i + 0] = 0;
                            someByte[i + 1] = 0;
                            someByte[i + 2] = 0;
                            someByte[i + 3] = 0;
                        }
                    }
                }
            }*/



            /*fixed (byte* textureByteArray = _textureByteArray, currentByteArray= _currentByteArray)
            {
                for (int yyy = yyPlayerShip; yyy < yyPlayerShip + heightOfRectanglePlayerShip; yyy++)
                {
                    for (int xxx = xxPlayerShip; xxx < xxPlayerShip + widthOfRectanglePlayerShip; xxx++)
                    {
                        var bytePoser = (((yyy) * 1920) + (xxx)) * 4;

                        if (textureByteArray[bytePoser + 0] + textureByteArray[bytePoser + 1] + textureByteArray[bytePoser + 2] >= 750)
                        {
                            currentByteArray[bytePoser + 0] = 0;
                            currentByteArray[bytePoser + 1] = 0;
                            currentByteArray[bytePoser + 2] = 0;
                            currentByteArray[bytePoser + 3] = 0;
                        }
                        else
                        {
                            currentByteArray[bytePoser + 0] = textureByteArray[bytePoser + 0];
                            currentByteArray[bytePoser + 1] = textureByteArray[bytePoser + 1];
                            currentByteArray[bytePoser + 2] = textureByteArray[bytePoser + 2];
                            currentByteArray[bytePoser + 3] = textureByteArray[bytePoser + 3];
                        }
                    }
                }
            }*/














            /*fixed (byte* textureByteArray = _textureByteArray)//, previousTextureByteArray = _previousTextureByteArray, currentByteArray = _currentByteArray)
            {
                for (int yyy = yyPlayerShip; yyy < yyPlayerShip + heightOfRectanglePlayerShip; yyy++)
                {
                    for (int xxx = xxPlayerShip; xxx < xxPlayerShip + widthOfRectanglePlayerShip; xxx++)
                    {
                        var bytePoser = (((yyy) * 1920) + (xxx)) * 4;

                        if (textureByteArray[bytePoser + 0] + textureByteArray[bytePoser + 1] + textureByteArray[bytePoser + 2] >= 765)
                        {
                            textureByteArray[bytePoser + 0] = 0;
                            textureByteArray[bytePoser + 1] = 0;
                            textureByteArray[bytePoser + 2] = 0;
                            textureByteArray[bytePoser + 3] = 0;
                        }
                        /*else
                        {
                            currentByteArray[bytePoser + 0] = textureByteArray[bytePoser + 0];
                            currentByteArray[bytePoser + 1] = textureByteArray[bytePoser + 1];
                            currentByteArray[bytePoser + 2] = textureByteArray[bytePoser + 2];
                            currentByteArray[bytePoser + 3] = textureByteArray[bytePoser + 3];
                        }*/
            /*if (textureByteArray[bytePoser + 0] + textureByteArray[bytePoser + 1] + textureByteArray[bytePoser + 2] >= 765)
            {
                currentByteArray[bytePoser + 0] = 0;
                currentByteArray[bytePoser + 1] = 0;
                currentByteArray[bytePoser + 2] = 0;
                currentByteArray[bytePoser + 3] = 0;
            }
            else
            {
                if (textureByteArray[bytePoser + 0] != previousTextureByteArray[bytePoser + 0] ||
                    textureByteArray[bytePoser + 1] != previousTextureByteArray[bytePoser + 1] ||
                    textureByteArray[bytePoser + 2] != previousTextureByteArray[bytePoser + 2] ||
                    textureByteArray[bytePoser + 3] != previousTextureByteArray[bytePoser + 3])
                {
                    currentByteArray[bytePoser + 0] = textureByteArray[bytePoser + 0];
                    currentByteArray[bytePoser + 1] = textureByteArray[bytePoser + 1];
                    currentByteArray[bytePoser + 2] = textureByteArray[bytePoser + 2];
                    currentByteArray[bytePoser + 3] = textureByteArray[bytePoser + 3];
                }
            }
        }
    }
}*/
            _countingTime.Stop();
            int _milliseconds = _countingTime.Elapsed.Milliseconds;
            Console.WriteLine(_milliseconds);




            /*if (_previousTextureByteArray[bytePos + 0] != 0 ||
                _previousTextureByteArray[bytePos + 1] != 0 ||
                _previousTextureByteArray[bytePos + 2] != 0 ||
                _previousTextureByteArray[bytePos + 3] != 0)
            {
                _currentByteArray[bytePos + 0] = 0; // A
                _currentByteArray[bytePos + 1] = 0; // R
                _currentByteArray[bytePos + 2] = 0; // G
                _currentByteArray[bytePos + 3] = 0; // B
                counter0++;
            }*/














            /*//fixed (byte* textureByteArray = _textureByteArray)
            {
                for (int yyy = 0; yyy <  1080; yyy++)
                {
                    var bytePoser = (((yyy) * 1920) + (0)) * 4;

                    System.Buffer.BlockCopy(_textureByteArray, bytePoser, _widthScreen, 0, _widthScreen.Length);

                    for (int i = 0; i < 1920 * 4; i += 4)
                    {
                        if (_widthScreen[i + 0] + _widthScreen[i + 1] + _widthScreen[i + 2] >= 765)
                        {
                            _widthScreen[i + 0] = 0;
                            _widthScreen[i + 1] = 0;
                            _widthScreen[i + 2] = 0;
                            _widthScreen[i + 3] = 0;
                        }
                    }

                    System.Buffer.BlockCopy(_widthScreen, 0, _textureByteArray, bytePoser, _widthScreen.Length);         



                    /*for (int xxx = 0; xxx < 1920; xxx++)
                    {
                        if (textureByteArray[bytePoser + 0] + textureByteArray[bytePoser + 1] + textureByteArray[bytePoser + 2] >= 765)
                        {
                            textureByteArray[bytePoser + 0] = 0;
                            textureByteArray[bytePoser + 1] = 0;
                            textureByteArray[bytePoser + 2] = 0;
                            textureByteArray[bytePoser + 3] = 0;
                        }
                    }
                }
                //System.Buffer.BlockCopy(_bufferPlayerShipRect, 0, _totalArray, bytePoser2, _bufferPlayerShipRect.Length);         
            }*/







            /*for (int yyy = yyPlayerShip; yyy < yyPlayerShip + heightOfRectanglePlayerShip; yyy++)
           {
               for (int xxx = xxPlayerShip; xxx < xxPlayerShip + widthOfRectanglePlayerShip; xxx++)
               {
                   var bytePoser = (((yyy) * 1920) + (xxx)) * 4;
                   if (textureByteArray[bytePoser + 0] + textureByteArray[bytePoser + 1] + textureByteArray[bytePoser + 2] >= 765)
                   {
                       textureByteArray[bytePoser + 0] = 0;
                       textureByteArray[bytePoser + 1] = 0;
                       textureByteArray[bytePoser + 2] = 0;
                       textureByteArray[bytePoser + 3] = 0;
                   }
               }
           }*/













            //byte* ptr = (byte*)int_ptr;

            //int _size = Marshal.SizeOf(_textureByteArray[0] * _textureByteArray.Length);
            //memIntPtr = Marshal.AllocHGlobal(_size);
            //memBytePtr = (byte*)memIntPtr.ToPointer();
            //_unmanagedMemoryStreamPlayerRect = new UnmanagedMemoryStream(memBytePtr, _textureByteArray.Length, _textureByteArray.Length, FileAccess.ReadWrite);
            //_unmanagedMemoryStreamPlayerRect.Seek(bytePos, SeekOrigin.Begin);
            //_unmanagedMemoryStreamPlayerRect.Read(_dummyTextureByteArray, bytePos, _bufferPlayerShipRect.Length);

            //_unmanagedMemoryStreamPlayerRect.SetLength(0);

            /*Byte[] bArr = { 1, 2, 3 };
            fixed (Byte* pbArr = &bArr[0])
            {
                array<Byte> ^ bArr2 = gcnew array < Byte >{ 1,2,3};
                interior_ptr<Byte> pbArr2 = &bArr2[0];
            }*/

            /*char[] charArray = "My name is Rex. Welcome to my unsafe world.".ToCharArray();

            char* uCharArray = stackalloc char[charArray.Length];
            char* uChar = uCharArray;
            for (int i = 0; i < charArray.Length; i++, uChar++)
            {
                *uChar = charArray[i];
            }
                
            Console.WriteLine(Test(uCharArray));
            */

            //byte* arraySetup = stackalloc byte[_textureByteArray.Length];
            //byte* uChar = arraySetup;
            //for (int i = 0; i < _textureByteArray.Length; i++, uChar++)
            //{
            //    *uChar = _textureByteArray[i];
            //}

            //int _TOTALSIZE = 1920 * 1080 * 4;
            //int* _Pointer_TOTALSIZE = &_TOTALSIZE;







            /*int posY = 0;
            byte[] _totalArray = new byte[widthOfRectanglePlayerShip * heightOfRectanglePlayerShip * 4];
            for (int* _yPointer = _playerY; *_yPointer < *_pointerHeight; (*_yPointer)++)
            {
                var bytePoser = (((*_yPointer) * (*_widtherPointer)) + (*_playerX)) * (*_pointerByte);
                _unmanagedMemoryStreamPlayerRect.Write(_textureByteArray, bytePoser, widthOfRectanglePlayerShip * 4);
                _unmanagedMemoryStreamPlayerRect.Seek(0, SeekOrigin.Begin);
                _unmanagedMemoryStreamPlayerRect.Read(_bufferPlayerShipRect, 0, _bufferPlayerShipRect.Length);

                var bytePoser2 = ((posY) * widthOfRectanglePlayerShip +0) * (*_pointerByte);
                System.Buffer.BlockCopy(_bufferPlayerShipRect, 0, _totalArray, bytePoser2, _bufferPlayerShipRect.Length);

                posY++;
                _unmanagedMemoryStreamPlayerRect.SetLength(0);
            }*/




            /*fixed (byte* textureByteArray = _textureByteArray, totalArray = _totalArray)
            {
                int posY = 0;
                for (int* _yPointer = _playerY; *_yPointer < *_pointerHeight; (*_yPointer)++)
                {
                    var bytePoser = ((*_yPointer * 1920) + *_xPointer) * (*_pointerByte);

                    for (int i = 0; i < widthOfRectanglePlayerShip; i++)
                    {
                        totalArray[i] = textureByteArray[bytePoser];
                        posY++;
                    }
                }
            }

            int _length = _totalArray.Length;
            int* lengthOfArray = &_length;

            fixed (byte* totalArray = _totalArray)
            {
                for (int* i = _zero; i < lengthOfArray; i += (*_pointerByte))
                {
                    if (totalArray[*i + 0] + totalArray[*i + 1] + totalArray[*i + 2] >= *maxColor)
                    {
                        totalArray[*i + 0] = 0;
                        totalArray[*i + 1] = 0;
                        totalArray[*i + 2] = 0;
                        totalArray[*i + 3] = 0;
                    }
                }
            }



            for (int* _yPointer = _playerY; *_yPointer < *_pointerHeight; (*_yPointer)++)
            {
                var bytePoser = (((*_yPointer) * (*_widtherPointer)) + (*_playerX)) * (*_pointerByte);

                System.Buffer.BlockCopy(_totalArray, 0, _textureByteArray, bytePoser, _bufferPlayerShipRect.Length);
            }*/











            /*fixed (byte* textureByteArray = _textureByteArray, totalArray = _totalArray)
            {
                int posY = 0;
                for (int yyyy = yyPlayerShip; yyyy < yyPlayerShip + heightOfRectanglePlayerShip; yyyy++)
                {
                    for (int xxxx = xxPlayerShip; xxxx < xxPlayerShip + widthOfRectanglePlayerShip; xxxx++)
                    {
                        var bytePoser = ((yyyy * 1920) + xxxx) * 4;

                        totalArray[posY] = textureByteArray[bytePoser];
                        posY++;
                    }
                }
            }

            int _length = _totalArray.Length;
            int* lengthOfArray = &_length;

            fixed (byte* totalArray = _totalArray)
            {
                for (int i = 0; i < _length; i ++)
                {
                    if (totalArray[i + 0] + totalArray[i + 1] + totalArray[i + 2] >= 765)
                    {
                        totalArray[i + 0] = 0;
                        totalArray[i + 1] = 0;
                        totalArray[i + 2] = 0;
                        totalArray[i + 3] = 0;
                    }
                }
            }

            fixed (byte* textureByteArray = _textureByteArray, totalArray = _totalArray)
            {
                int posY = 0;
                for (int yyyy = yyPlayerShip; yyyy < yyPlayerShip + heightOfRectanglePlayerShip; yyyy++)
                {
                    for (int xxxx = xxPlayerShip; xxxx < xxPlayerShip + widthOfRectanglePlayerShip; xxxx++)
                    {
                        var bytePoser = ((yyyy * 1920) + xxxx) * 4;

                        textureByteArray[bytePoser] = totalArray[posY];
                        posY++;
                    }
                }
            }*/



            /*fixed (byte* totalArray = _totalArray) // currentByteArray = _currentByteArray, //bufferPlayerShipRect = _bufferPlayerShipRect,
            {
                for (int* _yPointer = _playerY; *_yPointer < *_pointerHeight; (*_yPointer)++)
                {
                    //for (int yyy = yyPlayerShip; yyy < yyPlayerShip + heightOfRectanglePlayerShip; yyy++)
                    //{
                    //bytePos = ((yyy * 1920) + x) * 4;

                    //var bytePoser = (((*_yPointer) * (*_widtherPointer)) + (*_playerX)) * (*_pointerByte);

                    //_unmanagedMemoryStreamPlayerRect.Write(_textureByteArray, bytePoser, widthOfRectanglePlayerShip * 4);

                    //_unmanagedMemoryStreamPlayerRect = new UnmanagedMemoryStream(textureByteArray, *_Pointer_TOTALSIZE, *_Pointer_TOTALSIZE, FileAccess.ReadWrite);
                    //_unmanagedMemoryStreamPlayerRect.Seek(bytePos, SeekOrigin.Begin);
                    //_unmanagedMemoryStreamPlayerRect.Read(_bufferPlayerShipRect, 0, _bufferPlayerShipRect.Length);

                    for (int i = 0; i < *_widthShipRect; i += 4)
                    //for (int xxx = xxPlayerShip; xxx < xxPlayerShip + widthOfRectanglePlayerShip; xxx++)
                    {
                        //var bytePoser = (((*_yPointer) * (*_widtherPointer)) + (xxx)) * (*_pointerByte);
                        //bytePos = ((yyy * 1920) + xxx) * 4;

                        if (totalArray[i + 0] + totalArray[i + 1] + totalArray[i + 2] >= 765)
                        {
                            totalArray[i + 0] = 0;
                            totalArray[i + 1] = 0;
                            totalArray[i + 2] = 0;
                            totalArray[i + 3] = 0;
                        }
                    }
                }

                System.Buffer.BlockCopy(_totalArray, 0, _currentByteArray, bytePoser, _bufferPlayerShipRect.Length);
                //Marshal.Copy(new IntPtr(bufferPlayerShipRect), _currentByteArray, bytePos, _bufferPlayerShipRect.Length);
                //_unmanagedMemoryStreamPlayerRect.SetLength(0);
            }*/
            //_unmanagedMemoryStreamPlayerRect.SetLength(0);

























            //memIntPtr = Marshal.AllocHGlobal(_bufferPlayerShipRect.Length);
            //memBytePtr = (byte*)memIntPtr.ToPointer();
            //Marshal.Copy(new IntPtr(pointerToConvert), byteArrayName, 0, arraySize);

            /*//fixed (byte* textureByteArray = _textureByteArray) // currentByteArray = _currentByteArray, //bufferPlayerShipRect = _bufferPlayerShipRect,
            {
                //_unmanagedMemoryStreamPlayerRect = new UnmanagedMemoryStream(textureByteArray, *_Pointer_TOTALSIZE, *_Pointer_TOTALSIZE, FileAccess.ReadWrite);

                //_unmanagedMemoryStreamPlayerRect.Write(_textureByteArray, 0, _TOTALSIZE);
                //_unmanagedMemoryStreamPlayerRect.Seek(bytePos, SeekOrigin.Begin);
                //_unmanagedMemoryStreamPlayerRect.Read(_bufferPlayerShipRect, 0, _bufferPlayerShipRect.Length);

                fixed (byte* textureByteArray = _textureByteArray) // currentByteArray = _currentByteArray, //bufferPlayerShipRect = _bufferPlayerShipRect,
                {
                    for (int* _yPointer = _playerY; *_yPointer < *_pointerHeight; (*_yPointer)++)
                    {
                        //for (int yyy = yyPlayerShip; yyy < yyPlayerShip + heightOfRectanglePlayerShip; yyy++)
                        //{
                        //bytePos = ((yyy * 1920) + x) * 4;

                        //var bytePoser = (((*_yPointer) * (*_widtherPointer)) + (*_playerX)) * (*_pointerByte);

                        //_unmanagedMemoryStreamPlayerRect.Write(_textureByteArray, bytePoser, widthOfRectanglePlayerShip * 4);

                        //_unmanagedMemoryStreamPlayerRect = new UnmanagedMemoryStream(textureByteArray, *_Pointer_TOTALSIZE, *_Pointer_TOTALSIZE, FileAccess.ReadWrite);
                        //_unmanagedMemoryStreamPlayerRect.Seek(bytePos, SeekOrigin.Begin);
                        //_unmanagedMemoryStreamPlayerRect.Read(_bufferPlayerShipRect, 0, _bufferPlayerShipRect.Length);

                        //for (int i = 0; i < *_widthShipRect; i += 4)
                        for (int xxx = xxPlayerShip; xxx < xxPlayerShip + widthOfRectanglePlayerShip; xxx++)
                        {
                            var bytePoser = (((*_yPointer) * (*_widtherPointer)) + (xxx)) * (*_pointerByte);
                            //bytePos = ((yyy * 1920) + xxx) * 4;

                            if (textureByteArray[bytePoser + 0] + textureByteArray[bytePoser + 1] + textureByteArray[bytePoser + 2] >= 765)
                            {
                                textureByteArray[bytePoser + 0] = 0;
                                textureByteArray[bytePoser + 1] = 0;
                                textureByteArray[bytePoser + 2] = 0;
                                textureByteArray[bytePoser + 3] = 0;
                            }
                        }

                    }

                    //System.Buffer.BlockCopy(_bufferPlayerShipRect, 0, _currentByteArray, bytePoser, _bufferPlayerShipRect.Length);
                    //Marshal.Copy(new IntPtr(bufferPlayerShipRect), _currentByteArray, bytePos, _bufferPlayerShipRect.Length);
                    //_unmanagedMemoryStreamPlayerRect.SetLength(0);
                }
                //_unmanagedMemoryStreamPlayerRect.SetLength(0);
            }*/










            /*fixed (byte* bufferPlayerShipRect = _bufferPlayerShipRect, currentByteArray = _currentByteArray, textureByteArray = _textureByteArray)
            {
                //for (int yyy = yyPlayerShip; yyy < yyPlayerShip + heightOfRectanglePlayerShip; yyy++)
                //for (int* _yPointer = _playerY; *_yPointer < *_pointerHeight; (*_yPointer)++)
                //{
                for (int yyy = yyPlayerShip; yyy < _heightShip; yyy++)
                {
                    //var bytePoser = (((*_yPointer) * (*_widtherPointer)) + (*_playerX)) * (*_pointerByte);
                    bytePos = ((yyy * 1920) + x) * 4;

                    _unmanagedMemoryStreamPlayerRect.Write(_textureByteArray, bytePos, widthOfRectanglePlayerShip * 4);
                    _unmanagedMemoryStreamPlayerRect.Seek(0, SeekOrigin.Begin);
                    _unmanagedMemoryStreamPlayerRect.Read(_bufferPlayerShipRect, 0, _bufferPlayerShipRect.Length);

                    for (int i = 0; i < *_widthShipRect * (*_pointerByte); i += (*_pointerByte))
                    {
                        if (bufferPlayerShipRect[i + 0] + bufferPlayerShipRect[i + 1] + bufferPlayerShipRect[i + 2] >= *maxColor)
                        {
                            bufferPlayerShipRect[i + 0] = 0;
                            bufferPlayerShipRect[i + 1] = 0;
                            bufferPlayerShipRect[i + 2] = 0;
                            bufferPlayerShipRect[i + 3] = 0;
                        }
                    }

                    //Marshal.Copy(new IntPtr(bufferPlayerShipRect), _currentByteArray, bytePoser, _bufferPlayerShipRect.Length);
                    System.Buffer.BlockCopy(_bufferPlayerShipRect, 0, _currentByteArray, bytePos, _bufferPlayerShipRect.Length);
                    _unmanagedMemoryStreamPlayerRect.SetLength(0);
                }
            }*/










            /*fixed (byte* textureByteArray = _textureByteArray, currentByteArray = _currentByteArray)//, previousTextureByteArray = _previousTextureByteArray)
            {
                byte* buffer0;
                byte* buffer1;
                byte* buffer2;

                for (int x = xxPlayerShip; x < xxPlayerShip + widthOfRectanglePlayerShip; x++)
                {
                    for (int y = yyPlayerShip; y < yyPlayerShip + (heightOfRectanglePlayerShip); y++)
                    {
                        bytePos = ((y * 1920) + x) * 4;

                        buffer0 = &textureByteArray[bytePos + 0];
                        buffer1 = &textureByteArray[bytePos + 1];
                        buffer2 = &textureByteArray[bytePos + 2];

                        //int _sum0 = textureByteArray[bytePos + 0] + textureByteArray[bytePos + 1] + textureByteArray[bytePos + 2];
                        //int _sum0 = *buffer0 + *buffer1 + *buffer1;

                        if (*buffer0 + *buffer1 + *buffer1 >= 750)
                        {
                            byte* buffer00 = &currentByteArray[bytePos + 0];
                            byte* buffer11 = &currentByteArray[bytePos + 1];
                            byte* buffer22 = &currentByteArray[bytePos + 2];
                            byte* buffer33 = &currentByteArray[bytePos + 2];

                            *buffer00 = 0;
                            *buffer11 = 0;
                            *buffer22 = 0;
                            *buffer33 = 0;


                            /*byte* buffer00 = &currentByteArray[bytePos + 0];
                            byte* buffer11 = &currentByteArray[bytePos + 1];
                            byte* buffer22 = &currentByteArray[bytePos + 2];
                            byte* buffer33 = &currentByteArray[bytePos + 2];

                            *buffer00 = 0;
                            *buffer11 = 0;
                            *buffer22 = 0;
                            *buffer33 = 0;*/

            /*if (*buffer00!=0)
            {
                *buffer00 = 0;
            }
            else if (*buffer11 != 0)
            {
                *buffer11 = 0;
            }
            else if (*buffer22 != 0)
            {
                *buffer22 = 0;
            }
            else if (*buffer33 != 0)
            {
                *buffer33 = 0;
            }
            //currentByteArray[bytePos + 0] = 0;
            //currentByteArray[bytePos + 1] = 0;
            //currentByteArray[bytePos + 2] = 0;
            //currentByteArray[bytePos + 3] = 0;




    }
    else
    {
        if (textureByteArray[bytePos + 0] != previousTextureByteArray[bytePos + 0] ||
            textureByteArray[bytePos + 1] != previousTextureByteArray[bytePos + 1] ||
            textureByteArray[bytePos + 2] != previousTextureByteArray[bytePos + 2] ||
            textureByteArray[bytePos + 3] != previousTextureByteArray[bytePos + 3])
        {
            currentByteArray[bytePos + 0] = textureByteArray[bytePos + 0]; // A
            currentByteArray[bytePos + 1] = textureByteArray[bytePos + 1]; // R
            currentByteArray[bytePos + 2] = textureByteArray[bytePos + 2]; // G
            currentByteArray[bytePos + 3] = textureByteArray[bytePos + 3]; // B
        }
    }
 }
 }
 }*/
            //_countingTime.Stop();
            //int _milliseconds = _countingTime.Elapsed.Milliseconds;
            //Console.WriteLine(_milliseconds);
        }



        MemoryStream _memoryStream = new MemoryStream();


        UnmanagedMemoryStream _unmanagedMemoryStreamPlayerRect;

        MemoryStream _lastMemoryStream = new MemoryStream();

        bool _startOncer = true;
        byte[] _buffer;
        byte[] _bufferPlayerShipRect;



        int counter = 0;
        bool mapSubResource()
        {
            try
            {
                var dataBox = _device.ImmediateContext.MapSubresource(_texture2D, 0, MapMode.Read, MapFlags.None);
                //var boundsRect = new System.Drawing.Rectangle(0, 0, _width, _height);
                var sourcePtr = dataBox.DataPointer;
                Marshal.Copy(sourcePtr, _textureByteArray, 0, _bytesTotal);
                _device.ImmediateContext.UnmapSubresource(_texture2D, 0);
                DeleteObject(sourcePtr);

                _startThread = true;
                //pointerTest();

                _frameCaptureData.bitmapByteArray = _textureByteArray;              
                //_frameCaptureData.bitmapByteArray = _currentByteArray;
                //_previousTextureByteArray = _currentByteArray;

                //var pos = Control.MousePosition;
                //_frameCaptureData.cursorPointPos = pos;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                releaseFrame();
                return false;
            }
            return true;
        }





        private void ProcessModifiedRegions()
        {
            var numberOfBytes = 0;
            var rectangles = new RawRectangle[_duplicateFrameInformation.TotalMetadataBufferSize];

            if (_duplicateFrameInformation.TotalMetadataBufferSize > 0)
            {
                _outputDuplication.GetFrameDirtyRects(rectangles.Length, rectangles, out numberOfBytes);
            }

            ScreenFrameRectangle[] test = new ScreenFrameRectangle[numberOfBytes / Marshal.SizeOf(typeof(RawRectangle))];

            /*var numberOfBytes = 0;
            var rectangles = new OutputDuplicateMoveRectangle[_duplicateFrameInformation.TotalMetadataBufferSize];

            Console.WriteLine(rectangles.Length);

            var numberOfBytes = 0;
            var rectangles = new OutputDuplicateMoveRectangle[_duplicateFrameInformation.TotalMetadataBufferSize];

            if (_duplicateFrameInformation.TotalMetadataBufferSize > 0)
            {
                _outputDuplication.GetFrameMoveRects(rectangles.Length, rectangles, out numberOfBytes);
            }
            ScreenFrameRegion[] test = new ScreenFrameRegion[numberOfBytes / Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle))];


            Console.WriteLine(test.Length);*/


            //Console.WriteLine(numberOfBytes);
            //Console.WriteLine(rectangles[0].Bottom + "_" + rectangles[1].Left + "_" + rectangles[2].Right + "_" + rectangles[3].Top);
            //_frame.ModifiedRegions = new ScreenFrameRectangle[numberOfBytes / Marshal.SizeOf(typeof(RawRectangle))];
            /*for (var i = 0; i < rectangles.Length; i++)
            {
                rectangles[i].Bottom
                rectangles[i].Left;
                rectangles[i].Right;
                rectangles[i].Top;
            }*/
        }


        public struct ScreenFrameRegion
        {
            public ScreenFrameRectangle Destination;
            public int X;
            public int Y;
        }


        public struct ScreenFrameRectangle
        {
            public int Bottom;
            public int Left;
            public int Right;
            public int Top;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        //https://stackoverflow.com/questions/9668872/how-to-get-windows-position
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        Process _voidExpanse = null;
        Process _SCSkyArk = null;

        string targetProcessName0 = "voidexpanse";
        string targetProcessName2 = "server";
        string targetProcessName1 = "SC_SkyArk";


        IntPtr handleVoidExpanse = IntPtr.Zero;
        IntPtr handleSCSkyArk = IntPtr.Zero;










        /*
        //https://stackoverflow.com/questions/4226740/how-do-i-get-the-current-mouse-screen-coordinates-in-wpf
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static System.Drawing.Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new System.Drawing.Point(w32Mouse.X, w32Mouse.Y);
        }*/

        /*Bitmap CaptureCursor(ref int x, ref int y)
        {
            Bitmap bmp;
            IntPtr hicon;
            Win32Stuff.CURSORINFO ci = new Win32Stuff.CURSORINFO();
            Win32Stuff.ICONINFO icInfo;
            ci.cbSize = Marshal.SizeOf(ci);
            if (Win32Stuff.GetCursorInfo(out ci))
            {
                if (ci.flags == Win32Stuff.CURSOR_SHOWING)
                {
                    hicon = Win32Stuff.CopyIcon(ci.hCursor);
                    if (Win32Stuff.GetIconInfo(hicon, out icInfo))
                    {
                        x = ci.ptScreenPos.x - ((int)icInfo.xHotspot);
                        y = ci.ptScreenPos.y - ((int)icInfo.yHotspot);

                        Icon ic = Icon.FromHandle(hicon);
                        bmp = ic.ToBitmap();
                        return bmp;
                    }
                }
            }
            return null;
        }*/






        bool releasedFrame = true;
        bool releaseFrame()
        {
            //_texture2D.Dispose(); // lags like fucking hell
            for (int i = 0; i < 2; i++)
            {
                releasedFrame = true;
                try
                {
                    _outputDuplication.ReleaseFrame();
                }
                catch (SharpDXException ex)
                {
                    releasedFrame = false;
                    Console.WriteLine(ex.ToString());
                }

                if (releasedFrame)
                {
                    break;
                }
            }
            if (releasedFrame)
            {
                return true;
            }
            else
            {
                SkYaRk_Virtual_Desktop.Customizations.SC_AI_VR._desktopDupe = new SC_SharpDX_ScreenCapture(0,0);
                return false;
            }
        }
    }
}