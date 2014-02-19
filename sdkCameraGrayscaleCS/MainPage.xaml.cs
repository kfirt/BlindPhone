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
        MediaElement MyMedia = new MediaElement();
        private List<Rectangle> Rectangles = new List<Rectangle>();
        private int WaitBetweenPics = 5;
        private DispatcherTimer MyTimer = new DispatcherTimer();
        private const string DemoSeriesFolder = @"/series 2/";
        private string[] DemoSeries;
        private int DemoSeriesIndex = 0;

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

                    // timer interval specified as 1 second
                    MyTimer.Interval = TimeSpan.FromSeconds(WaitBetweenPics);
                    // Sub-routine OnTimerTick will be called at every 1 second

                    StartDemoMode();
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
        }

        private void GenerateListofRectangles(IEnumerable<AnalyzedObject> analyzedObjects)
        {
            foreach (var oldRect in Rectangles)
            {
                this.LayoutRoot.Children.Remove(oldRect);
            }
            Rectangles.Clear();

            foreach (var analyzedObject in analyzedObjects)
            {
                var color = Colors.Blue;
                if (analyzedObject.decision == true)
                {
                    color = System.Windows.Media.Color.FromArgb(255, 255, 255, 146);
                }
                else if (analyzedObject.color.Equal(AnalyzeTrafficLight.Color.green))
                {
                    color = System.Windows.Media.Color.FromArgb(255, 127, 255, 0);
                }
                else if (analyzedObject.color.Equal(AnalyzeTrafficLight.Color.red))
                {
                    color = Colors.Red;
                }

                const int BoxSize = 14;
                int rectx = (int)this.MainImage.Margin.Top + Convert.ToInt32((analyzedObject.bBox.bottomRight.y + analyzedObject.bBox.topLeft.y)/2 * this.MainImage.Height / cam.Resolution.Height);
                int recty = (int)this.MainImage.Margin.Left + Convert.ToInt32((analyzedObject.bBox.topLeft.x + analyzedObject.bBox.bottomRight.x)/2 * this.MainImage.Width / cam.Resolution.Width);
                Rectangle rect = new Rectangle();
                rect.Stroke = new SolidColorBrush(color);
                rect.StrokeThickness = 2.5;
                rect.Width = BoxSize;
                rect.Height = BoxSize;
                this.LayoutRoot.Children.Add(rect);
                Canvas.SetLeft(rect, recty-BoxSize/2);
                Canvas.SetTop(rect, rectx-BoxSize/2);
                Rectangles.Add(rect);
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
                        string filename = SavePictures(ms);

                        analyzeAndDisplayImage(filename);
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

        private void analyzeAndDisplayImage(string filename)
        {
            using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
            using (IsolatedStorageFileStream sourceStream = isStore.OpenFile(filename, FileMode.Open))
            {
                sourceStream.Seek(0, SeekOrigin.Begin);
                wb.LoadJpeg(sourceStream);
                wb.Invalidate();
            }

            Analyzer a = new Analyzer();
            List<AnalyzedObject> analyzedObjects = a.analyzeImage(wb.Pixels,
                            (int)cam.Resolution.Width, (int)cam.Resolution.Height);

            GenerateListofRectangles(analyzedObjects);

            AnalyzedState state_o = a.decide(analyzedObjects);
            var uri = string.Format("Assets/{0}.mp3", state_o.ToString());
            MyMedia.Source = new Uri(uri, UriKind.RelativeOrAbsolute);
            MyMedia.Play();
        }

        void PumpDemoFrames(Object sender, EventArgs args)
        {
            try
            {
                if (DemoSeriesIndex == DemoSeries.Length)
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        txtDebug.Text = "Demo finished";

                    });                    
                }
                else
                {
                    analyzeAndDisplayImage(string.Format("/{0}/{1}", DemoSeriesFolder, DemoSeries[DemoSeriesIndex]));
                    DemoSeriesIndex++;
                }
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

        void PumpLiveFrames(Object sender, EventArgs args)
        {
            try
            {
                PhotoCamera phCam = (PhotoCamera)cam;

                pauseFramesEvent.WaitOne();
                pauseFramesEvent.Reset();
                phCam.CaptureImage();
            }
            catch (Exception e)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    txtDebug.Text = e.Message;
                });
            }
        }

        private void DemoMode_Clicked(object sender, RoutedEventArgs e)
        {
            StartDemoMode();
        }

        private void StartDemoMode()
        {
            this.Video.Visibility = Visibility.Collapsed;

            this.Dispatcher.BeginInvoke(delegate()
            {
                txtDebug.Text = "Demo mode ON";

            });

            DemoSeriesIndex = 0;
            using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                DemoSeries = isStore.GetFileNames(DemoSeriesFolder + "*");
            }
            
            MyTimer.Stop();
            MyTimer.Tick -= PumpLiveFrames;
            MyTimer.Tick += PumpDemoFrames;
            MyTimer.Start();
        }

        private void LiveMode_Clicked(object sender, RoutedEventArgs e)
        {
            StartLiveMode();
        }

        private void StartLiveMode()
        {
            this.Video.Visibility = Visibility.Visible;

            this.Dispatcher.BeginInvoke(delegate()
            {
                txtDebug.Text = "Live mode ON";
            });

            PumpLiveFrames(null, null);

            MyTimer.Stop();
            MyTimer.Tick += PumpLiveFrames;
            MyTimer.Tick -= PumpDemoFrames;
            MyTimer.Start();
        }
    }
}
