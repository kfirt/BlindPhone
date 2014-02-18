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
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Media;


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
        private int WaitBetweenPics = 5;

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
                    //wb = new WriteableBitmap((int)cam.Resolution.Width, (int)cam.Resolution.Height);
                    wb = new WriteableBitmap((int)cam.Resolution.Width, (int)cam.Resolution.Height);
                    this.MainImage.Visibility = Visibility.Visible;
                    this.MainImage.Source = wb;
                    cam.CaptureImageAvailable += new EventHandler<ContentReadyEventArgs>(cameraCaptureTask_Completed);
                   
                    cam.FlashMode = FlashMode.Off;

                    PumpARGBFrames(null, null);

                    // creating timer instance
                    DispatcherTimer newTimer = new DispatcherTimer();
                    // timer interval specified as 1 second
                    newTimer.Interval = TimeSpan.FromSeconds(WaitBetweenPics);
                    // Sub-routine OnTimerTick will be called at every 1 second
                    //newTimer.Tick += OnTimerTick;
                    newTimer.Tick += PumpARGBFrames;
                    // starting the timer
                    newTimer.Start();
                });

            }
        }

        private string SavePictures(MemoryStream ms)
        {
            ms.Seek(0, SeekOrigin.Begin);
            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(ms);
            WriteableBitmap wbp = new WriteableBitmap(bitmapImage);

            string time = DateTime.Now.ToString("yyyyMMddHHmmss");
            string filename = "BlindPhone_" + time + ".jpg";

            using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream targetStream = isStore.OpenFile(filename,
                       FileMode.Create, FileAccess.Write))
                {
                    wbp.SaveJpeg(targetStream, bitmapImage.PixelWidth, bitmapImage.PixelHeight, 0, 100);
                }
            }
            return filename;

            // Save photo as JPEG to the local folder.
            //using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
            //{
            //    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(filename + ".ARGB",
            //           FileMode.Create, FileAccess.Write))
            //    {
            //        // Initialize the buffer for 4KB disk pages.
            //        byte[] result = new byte[wbp.Pixels.Length * sizeof(int)];
            //        Buffer.BlockCopy(wbp.Pixels, 0, result, 0, result.Length);

            //        targetStream.Write(result, 0, result.Length);

            //        //// Initialize the buffer for 4KB disk pages.
            //        //byte[] readBuffer = new byte[4096];
            //        //int bytesRead = -1;

            //        //// Copy the image to the local folder. 
            //        //while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
            //        //{
            //        //    targetStream.Write(readBuffer, 0, bytesRead);
            //        //}
            //    }
            //}


            //ms.Seek(0, SeekOrigin.Begin);
            //wb.SetSource(ms);
            ////cam.GetPreviewBufferArgb32(wb.Pixels);
            //// Copy to WriteableBitmap.
            //wb.Invalidate();
            ////});


        }
        private void GenerateListofRectangles(IEnumerable<AnalyzedObject> analyzedObjects)
        {
            foreach (var analyzedObject in analyzedObjects)
            {
                var color = Colors.Blue;
                if (analyzedObject.decision == true)
                {
                    color = Colors.Yellow;
                }
                else if (analyzedObject.color.Equal(AnalyzeTrafficLight.Color.green))
                {
                    color = Colors.Red;
                }
                else if (analyzedObject.color.Equal(AnalyzeTrafficLight.Color.red))
                {
                    color = Colors.Green;
                }

                int rectx = Convert.ToInt32(analyzedObject.bBox.topLeft.y * this.MainImage.Height / cam.Resolution.Height);
                int recty = Convert.ToInt32(analyzedObject.bBox.topLeft.x * this.MainImage.Width / cam.Resolution.Width);
                Rectangle rect = new Rectangle();
                rect.Stroke = new SolidColorBrush(color);
                rect.Width = 15;
                rect.Height = 15;
                this.LayoutRoot.Children.Add(rect);
                Canvas.SetLeft(rect, recty);
                Canvas.SetTop(rect, rectx);
                rects.Add(rect);
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
                        foreach (var oldRect in rects)
                        {
                            this.LayoutRoot.Children.Remove(oldRect);
                        }
                        rects.Clear();

                        string filename = SavePictures(ms); 

                        using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                        //string[] directories = isStore.GetDirectoryNames("*");
                        //string[] files = isStore.GetFileNames("*");
                        //string filename = "\\" + directories[0] + "\\BlindPhone_20140218165648" + ".jpg";
                        using (IsolatedStorageFileStream sourceStream = isStore.OpenFile(filename, FileMode.Open)) { 
                        //WriteableBitmap wbpFromJpg = new WriteableBitmap((int)cam.Resolution.Width, (int)cam.Resolution.Height);
                        //wbpFromJpg.LoadJpeg(sourceStream);
                            sourceStream.Seek(0, SeekOrigin.Begin);
                            wb.LoadJpeg(sourceStream);
                            wb.Invalidate();
                        }

                        Analyzer a = new Analyzer();
                        List<AnalyzedObject> analyzedObjects = a.analyzeImage(wb.Pixels,
                                        (int)cam.Resolution.Width, (int)cam.Resolution.Height);//, ycbcr, cam.YCbCrPixelLayout.RequiredBufferSize);

                        GenerateListofRectangles(analyzedObjects);

                        AnalyzedState state_o = a.decide(analyzedObjects);
                        var uri = string.Format("Assets/{0}.mp3", state_o.ToString());
                        MyMedia.Source = new Uri(uri, UriKind.RelativeOrAbsolute);
                        MyMedia.Play();
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
                //wbmp.SaveJpeg(ms, (int)cam.Resolution.Width, (int)cam.Resolution.Height, 0, 100);

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
