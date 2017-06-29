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
using System.Windows.Media;
using System.Threading;
using System.IO;


namespace Micrographia
{

    public class Camera: IDisposable 
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

        public void Dispose()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.Stop();
            }
        }

}

    /// <summary>
    /// Interaction logic for Media.xaml
    /// </summary>
    public partial class MediaPage : Page, INotifyPropertyChanged, IDisposable
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
                myCamera.Start();
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
            catch (Exception exc)
            {
                MessageBox.Show( String.Format("Error on _videoSource_NewFrame:{0}\n",  exc.Message) , "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (!_recording)
            {
                _firstFrameTime = null;
               
                string fileName = String.Format(  @"{0}\Video{1}.avi", AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString());
                RecordButton.Content = "Stop";
                RecordButton.Background = System.Windows.Media.Brushes.Red;

                DateTime RecordingStartTime = DateTime.MinValue;
                _writer = new VideoFileWriter();
                _writer.Open(fileName, (int)Math.Round(Image.Width, 0), (int)Math.Round(Image.Height, 0), 1000 / 20, VideoCodec.MPEG4, 1200 * 1000);
                _recording = true;
            }
            else
            {
                _recording = false;
                RecordButton.Background = PictureButton.Background;
                RecordButton.Content = "Record";
                _writer.Close();
                _writer.Dispose();
            }


        }




        private void Button_TakePicture(object sender, RoutedEventArgs e)
        {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(Image));
                using (var filestream = new FileStream(String.Format(@"{0}\Pic{1}.png", AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), FileMode.Create))
                {
                    encoder.Save(filestream);
                }
            
        }

        public void Dispose()
        {
            myCamera.Dispose();
            _writer?.Dispose();
        }

    }
}
