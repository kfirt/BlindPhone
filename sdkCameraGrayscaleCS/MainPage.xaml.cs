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
using System.IO;

// Directives
using Microsoft.Devices;
using System.Windows.Media.Imaging;
using System.Threading;
using AnalyzeTrafficLight;

namespace sdkCameraGrayscaleCS
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Variables
        PhotoCamera cam = new PhotoCamera();
        private static ManualResetEvent pauseFramesEvent = new ManualResetEvent(true);
        private WriteableBitmap wb;
        //private Thread ARGBFramesThread;
        //private bool pumpARGBFrames;
        MediaElement MyMedia = new MediaElement();
        private List<Rectangle> rects = new List<Rectangle>();

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
                    wb = new WriteableBitmap((int)cam.Resolution.Width, (int)cam.Resolution.Height);
                    this.MainImage.Visibility = Visibility.Visible;
                    this.MainImage.Source = wb;
                    cam.CaptureImageAvailable += new EventHandler<ContentReadyEventArgs>(cameraCaptureTask_Completed);
                    //cam.GetPreviewBufferArgb32(wb.Pixels);
                    //wb.Invalidate();

                    //PumpARGBFrames(null, null);
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

        void cameraCaptureTask_Completed(object sender, ContentReadyEventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                e.ImageStream.Seek(0, SeekOrigin.Begin);
                e.ImageStream.CopyTo(ms);
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    try
                    {
                        wb.SetSource(ms);
                        // Copy to WriteableBitmap.
                        wb.Invalidate();

                        foreach (var oldRect in rects)
                        {
                            this.LayoutRoot.Children.Remove(oldRect);
                        }
                        rects.Clear();

                        Analyzer a = new Analyzer();
                        IEnumerable<AnalyzedObject> analyzedObjects = a.analyzeImage(wb.Pixels, (int)cam.Resolution.Width, (int)cam.Resolution.Height);//, ycbcr, cam.YCbCrPixelLayout.RequiredBufferSize);
                        
                        string state = "Unknown";
                        foreach (var analyzedObject in analyzedObjects)
                        {
                            var color = Colors.Yellow;
                            if (analyzedObject.decision == true)
                            {
                                color = Colors.Blue;

                                if (analyzedObject.color.Equal(AnalyzeTrafficLight.Color.green))
                                {
                                    state = "Green";
                                }
                                else if (analyzedObject.color.Equal(AnalyzeTrafficLight.Color.red))
                                {
                                    state = "Red";
                                }
                            }
                            else if (analyzedObject.color.Equal(AnalyzeTrafficLight.Color.green))
                            {
                                color = Colors.Green;
                            }
                            else if (analyzedObject.color.Equal(AnalyzeTrafficLight.Color.red))
                            {
                                color = Colors.Red;
                            }

                            int rectx = Convert.ToInt32(analyzedObject.leftTop.x * 640 / cam.Resolution.Width);
                            int recty = Convert.ToInt32(analyzedObject.leftTop.y * 480 / cam.Resolution.Height);
                            Rectangle rect = new Rectangle();
                            rect.Stroke = new SolidColorBrush(color);
                            rect.Width = 30;
                            rect.Height = 30;
                            this.LayoutRoot.Children.Add(rect);
                            Canvas.SetLeft(rect, rectx);
                            Canvas.SetTop(rect, rectx);
                            rects.Add(rect);
                        }


                        //var uri = string.Format("Assets/{0}.mp3", state);
                        //MyMedia.Source = new Uri(uri, UriKind.RelativeOrAbsolute);
                        //MyMedia.Play();

                        //pauseFramesEvent.Set();
                    }
                    finally
                    {
                        ms.Close();
                    }
                });
            }
            finally
            {
                e.ImageStream.Close();
                pauseFramesEvent.Set();
            }
        }

        // ARGB frame pump
        void PumpARGBFrames(Object sender, EventArgs args)
        {
            try
            {
                PhotoCamera phCam = (PhotoCamera)cam;


                pauseFramesEvent.WaitOne();
                pauseFramesEvent.Reset();
                phCam.CaptureImage();


                //while (pumpARGBFrames)
                //{

                // Copies the current viewfinder frame into a buffer for further manipulation.

                //byte[] ycbcr = new byte[cam.YCbCrPixelLayout.RequiredBufferSize];
                //cam.Capture
                //phCam.CaptureCompleted += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
                //MemoryStream captureStream1 = new MemoryStream();
                //PhotoCaptureDevice c = (CaptureDevice)cam;
                //phCam.GetPreviewBufferArgb32(wb.Pixels);
                //phCam.GetPreviewBufferYCbCr(ycbcr);
                //MemoryStream ms = new MemoryStream();
                //wbmp.SaveJpeg(ms, (int)cam.PreviewResolution.Width, (int)cam.PreviewResolution.Height, 0, 100);

                //BitmapImage bmp = new BitmapImage();
                //bmp.SetSource(ms);
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
    }
}
