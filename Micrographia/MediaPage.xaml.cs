using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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

            DisplayWindow.Image = null;    

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
            myCamera = new Camera(this);
            this.VideoDevices = GetVideoDevices();
            if (VideoDevices.Count > 0)
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No webcam found");
            }

            
           
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
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

                
                using (var bitmap = eventArgs.Frame)
                {
                    Image = ToBitmapImage(bitmap);
                }
                Image.Freeze(); // avoid cross thread operations and prevents leaks

                Dispatcher.BeginInvoke(new ThreadStart(delegate { VideoPlayer.Source = Image; }));
            }
            catch (VideoException exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" +_writer.Codec.ToString()+_writer.BitRate.ToString()+" " +_writer.Height.ToString() +" " + _writer.Width.ToString() + Image.Height.ToString() + " " + _writer.Width.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                myCamera.Stop();
                _recording = false;

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

            string fileName = @"C:\Users\Ale\Desktop\Video.avi";
            _firstFrameTime = null;

            DateTime RecordingStartTime = DateTime.MinValue;
            _writer = new VideoFileWriter();
            _writer.Open(fileName, (int) Math.Round(Image.Width, 0), (int) Math.Round(Image.Height, 0), 1000/24, VideoCodec.MPEG4);
            _recording = true;
        }

        private void Button_Stop_Record(object sender, RoutedEventArgs e)
        {
            _recording = false;
            _writer.Close();
            _writer.Dispose();
        }
    }
}
