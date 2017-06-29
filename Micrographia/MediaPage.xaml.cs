using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Accord.Video;
using Accord.Video.FFMPEG;
using Accord.Video.DirectShow;
using System.Drawing;
using System.Drawing.Imaging;

using System.Threading;
using System.IO;


namespace Micrographia
{

    public class Camera
    {

        public Camera(MediaPage w)
        {
            this.DisplayWindow = w;

        }

        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; }
        }

        public MediaPage DisplayWindow;
        private FilterInfo _currentDevice;
        private IVideoSource _videoSource;

        public void Start()
        {
            if (CurrentDevice != null)
            {
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += this.DisplayWindow.video_NewFrame;
                _videoSource.Start();
            }
        }

        public void Stop()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= new NewFrameEventHandler(this.DisplayWindow.video_NewFrame);
            }
        }

    }

    /// <summary>
    /// Interaction logic for Media.xaml
    /// </summary>
    public partial class MediaPage : Page, INotifyPropertyChanged
    {
        public MediaPage()
        {
            InitializeComponent();
            this.DataContext = this;
            this.VideoDevices = GetVideoDevices();

            myCamera = new Camera(this);
           
        }

        public ObservableCollection<FilterInfo> VideoDevices { get; set; }
        public Camera myCamera;
        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set
            {
                _currentDevice = value;


                this.OnPropertyChanged("CurrentDevice");
                myCamera.CurrentDevice = value;
            }
        }

        private VideoFileWriter _writer;
        private bool _recording;
        private DateTime? _firstFrameTime;


        private FilterInfo _currentDevice;

        private ObservableCollection<FilterInfo> GetVideoDevices()
        {
            VideoDevices = new ObservableCollection<FilterInfo>();
            foreach (FilterInfo filterInfo in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                VideoDevices.Add(filterInfo);
            }

            return VideoDevices;
        }


       

        private void Button_Stop(object sender, RoutedEventArgs e)
        {
            myCamera.Stop();
        }

        private void Button_Start(object sender, RoutedEventArgs e)
        {
            myCamera.Start();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        public void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_recording)
                {
                    if (_firstFrameTime != null)
                    {
                        _writer.WriteVideoFrame(eventArgs.Frame, DateTime.Now - _firstFrameTime.Value);
                    }
                    else
                    {
                        _writer.WriteVideoFrame(eventArgs.Frame);
                        _firstFrameTime = DateTime.Now;
                    }
                }

                BitmapImage bi;
                using (var bitmap = eventArgs.Frame)
                {
                    bi = ToBitmapImage(bitmap);
                }
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate { VideoPlayer.Source = bi; }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //StopCamera();
            }



        }


        private BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private void Button_Record(object sender, RoutedEventArgs e)
        {

            string fileName = @"C:\Users\mariosky\Documents\Video.avi";

            //int height = (int)(area.Height / heightScale);
            //int width = (int)(area.Width / widthScale);

            double height = VideoPlayer.Height;
            double width = VideoPlayer.Width;
            int framerate = 1000 / 20;
            int videoBitRate = 1200 * 1000;



            DateTime RecordingStartTime = DateTime.MinValue;
            _writer = new VideoFileWriter();
            _writer.Open(@"Video.avi", 200, 600);
            _recording = true;
        }
    }
}
