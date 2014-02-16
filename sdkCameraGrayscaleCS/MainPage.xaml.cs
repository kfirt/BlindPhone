/* 
    Copyright (c) 2012 - 2013 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see all Code Samples for Windows Phone, visit http://code.msdn.microsoft.com/wpapps
  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Threading;

// Directives
using Microsoft.Devices;
using System.Windows.Media.Imaging;
using System.Threading;


namespace sdkCameraGrayscaleCS
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Variables
        PhotoCamera cam = new PhotoCamera();
        private static ManualResetEvent pauseFramesEvent = new ManualResetEvent(true);
        //private WriteableBitmap wb;
        private Thread ARGBFramesThread;
        private bool pumpARGBFrames;
        MediaElement MyMedia = new MediaElement();


        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.LayoutRoot.Children.Add(MyMedia);
        }

        //Code for camera initialization event, and setting the source for the viewfinder
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            // Check to see if the camera is available on the device.
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                 (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
            {
                // Initialize the default camera.
                cam = new Microsoft.Devices.PhotoCamera();

                //Event is fired when the PhotoCamera object has been initialized
                cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);

                //Set the VideoBrush source to the camera
                viewfinderBrush.SetSource(cam);
            }
            else
            {
                // The camera is not supported on the device.
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Write message.
                    txtDebug.Text = "A Camera is not available on this device.";
                });
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                // Dispose of the camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();

                // Release memory, ensure garbage collection.
                cam.Initialized -= cam_Initialized;
            }
        }

        //Update UI if initialization succeeds
        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    txtDebug.Text = "Camera initialized";
                
                    // creating timer instance
                    DispatcherTimer newTimer = new DispatcherTimer();
                    // timer interval specified as 1 second
                    newTimer.Interval = TimeSpan.FromSeconds(5);
                    // Sub-routine OnTimerTick will be called at every 1 second
                    //newTimer.Tick += OnTimerTick;
                    newTimer.Tick += PumpARGBFrames;
                    // starting the timer
                    newTimer.Start();
                });

            }
        }

        // ARGB frame pump
        void PumpARGBFrames(Object sender, EventArgs args)
        {
            // Create capture buffer.
            int[] ARGBPx = new int[(int)cam.PreviewResolution.Width * (int)cam.PreviewResolution.Height];

            try
            {
                PhotoCamera phCam = (PhotoCamera)cam;

                //while (pumpARGBFrames)
                //{
                    pauseFramesEvent.WaitOne();
                    
                    // Copies the current viewfinder frame into a buffer for further manipulation.
                    phCam.GetPreviewBufferArgb32(ARGBPx);
    
                    pauseFramesEvent.Reset();
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        Analyzer a = new Analyzer();
                        var state = a.process(ARGBPx);
                        var uri = string.Format("Assets/{0}.mp3", state);
                        MyMedia.Source = new Uri(uri, UriKind.RelativeOrAbsolute);
                        MyMedia.Play();
            
                        pauseFramesEvent.Set();
                    });
                //}

            }
            catch (Exception e)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Display error message.
                    txtDebug.Text = e.Message;
                });
            }
        }

        void OnTimerTick(Object sender, EventArgs args)
        {
            Analyzer a = new Analyzer();
            var state = a.process(null);
            var uri = string.Format("Assets/{0}.mp3", state);
            MyMedia.Source = new Uri(uri, UriKind.RelativeOrAbsolute);
            MyMedia.Play();
             

        }
    }
}
