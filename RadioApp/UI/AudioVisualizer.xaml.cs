using NAudio.Wave;
using System.Windows;
using System.Windows.Controls;
using Drw = System.Drawing;
using System;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;
using System.Security.Policy;
using System.Windows.Media;
using System.Diagnostics;
using STimer = System.Timers;

namespace RadioApp.UI
{
    /// <summary>
    /// Interaction logic for AudioVisualizer.xaml
    /// </summary>
    public partial class AudioVisualizer : UserControl
    {
        private int _m = 6;
        private STimer.Timer _timer;
        private List<Complex[]> smooth = new List<Complex[]>();
        private int vertical_smoothness = 3;
        private int horizontal_smoothness = 1;
        private int count = 64;

        private int render_count = 0;
        private int data_count = 0;

        private Stopwatch _sw;

        private byte[]? _bytes = null;

        public byte[]? Buffer
        {
            get { return (byte[]?)GetValue(BufferProperty); }
            set { SetValue(BufferProperty, value); }
        }

        public static DependencyProperty BufferProperty = DependencyProperty.Register(
            "Buffer", typeof(byte[]), typeof(AudioVisualizer), new FrameworkPropertyMetadata(BufferChanged));

        public double Time => _sw.ElapsedMilliseconds;
        public double RenderFPS => render_count;
        public double DataFPS => data_count;

        public AudioVisualizer()
        {
            InitializeComponent();

            //_timer = new STimer.Timer(Draw, null, TimeSpan.Zero, TimeSpan.FromMicroseconds(100));
            _timer = new STimer.Timer(TimeSpan.FromMicroseconds(40));
            _timer.AutoReset = true;
            _timer.Elapsed += Draw;
            _timer.Start();
            _sw = Stopwatch.StartNew();

            var t2 = new STimer.Timer(TimeSpan.FromSeconds(1));
            t2.AutoReset = true;
            t2.Elapsed += (_, __) =>
            {
                r1.Dispatcher?.Invoke(() =>
                {
                    r1.Text = Math.Round(RenderFPS, 1).ToString();
                    r2.Text = Math.Round(DataFPS, 1).ToString();
                    r3.Text = Math.Round(Time, 1).ToString();

                    render_count = 0;
                    //data_count = 0;
                });
            };
            t2.Start();
        }

        private void Draw(object? sender, STimer.ElapsedEventArgs e)
        {
            //var big_buffer = new byte[65536];
            //bytes?.CopyTo(big_buffer, 0);

            //IWaveProvider stream32 = new Wave16ToFloatProvider(media);

            var sw = Stopwatch.StartNew();

            var buffer = new WaveBuffer(_bytes);

            //Graphics.SetColor(1, 1, 1);
            if (_bytes == null || buffer == null)
            {
                //Graphics.Print("No buffer available");
                return;
            }

            _bytes = null;

            var size = (float)ActualWidth / count;
            int len = buffer.FloatBuffer.Length / 8;

            // fft
            Complex[] values = new Complex[len];
            for (int i = 0; i < len; i++)
                values[i] = new Complex(buffer.FloatBuffer[i], 0.0);

            var tst = buffer.FloatBuffer[0];

            Fourier.Forward(values, FourierOptions.Default);

            // shift array
            smooth.Add(values);
            if (smooth.Count > vertical_smoothness)
                smooth.RemoveAt(0);

            //var wBitmap = new WriteableBitmap((int)scene.ActualWidth, (int)scene.ActualHeight, 96, 96, PixelFormats.Default, BitmapPalettes.BlackAndWhite);

            var point = sw.Elapsed;

            List<RectangleF> rects = new List<RectangleF>();

            var bmp = new Drw.Bitmap((int)ActualWidth, (int)ActualHeight);
            using (var drawSource = Graphics.FromImage(bmp))
            {
                //drawSource.DrawRectangle(new Drw.Pen(Drw.Color.White), new RectangleF(0, 0, 200, 200));
                drawSource.FillRectangle(new Drw.SolidBrush(Drw.Color.Gray), new RectangleF(0, 0, (float)scene.ActualWidth, (float)scene.ActualHeight));

                for (int i = 0; i < count; i++)
                {
                    double value = BothSmooth(i);
                    //DrawVis(drawSource, i, count, size, value);

                    value *= ActualHeight / 2;

                    value += BothSmooth(i - 1) + BothSmooth(i + 1);
                    value /= 3;

                    var rect = new Drw.RectangleF(i * size, 0, size, (float)value);
                    rects.Add(rect);
                    drawSource.FillRectangle(new Drw.SolidBrush(Drw.Color.Black), rect);
                }
            }

            //bmp.Save("tst.bmp");
            var source = Convert(bmp);
            source.Freeze();
            scene.Dispatcher.Invoke(() => scene.Source = source);
            
            sw.Stop();
            render_count++;
            r1.Dispatcher?.Invoke(() =>
            {
                r1.Text = Math.Round(RenderFPS, 1).ToString();
            });
        }

        public double BothSmooth(int i)
        {
            var s = smooth.ToArray();

            double value = 0;

            for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, 64); h++)
                value += vSmooth(h, s);

            return value / ((horizontal_smoothness + 1) * 2);
        }

        public double vSmooth(int i, Complex[][] s)
        {
            double value = 0;

            for (int v = 0; v < s.Length; v++)
                value += Math.Abs(s[v] != null ? s[v][i].Magnitude : 0.0);

            return value / s.Length;
        }

        private BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, Drw.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private static void BufferChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AudioVisualizer av)
                return;

            av._bytes = (byte[]?)e.NewValue;

            var buffer = new WaveBuffer(av._bytes);

            av.data_count++;

            av.r1.Dispatcher?.Invoke(() =>
            {
                av.r2.Text = Math.Round(av.DataFPS, 1).ToString();
            });

            //av.Draw((byte[]?)e.NewValue);
        }
    }
}
