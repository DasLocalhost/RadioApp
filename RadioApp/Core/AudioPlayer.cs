using NAudio.Gui;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace RadioApp.Core
{
    public class AudioPlayer : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion

        private Thread? _streamingThread;
        private Thread? _audioThread;
        private CancellationTokenSource? _audioThreadTokenSource;

        private CustomBufferedWaveProvider? bufferedWaveProvider;
        private CustomWave16ToFloatProvider? wave16ToFloatProvider;
        private IWavePlayer? waveOut;
        private volatile PlaybackState playbackState = PlaybackState.Stopped;
        private volatile bool fullyDownloaded;
        private static HttpClient? httpClient;
        private VolumeWaveProvider16? volumeProvider;

        public bool IsPlaying => playbackState is PlaybackState.Playing or PlaybackState.Buffering;

        private string? _url = null;
        public string? Url { get => _url; }

        public EventHandler<byte[]?>? OnDataReceived;

        public AudioPlayer()
        {
            playbackState = PlaybackState.Stopped;
        }

        public void SetUrl(string url)
        {
            Stop();
            _url = url;
            Play();
        }

        public async void Play()
        {
            // reset buffer
            bufferedWaveProvider = null;

            playbackState = PlaybackState.Buffering;
            OnPropertyChanged(nameof(PlaybackState));

            while (!await CheckStatus())
            {
                // TODO : add callbacks / logs / exit condition here
            }

            _audioThreadTokenSource = new CancellationTokenSource();
            _streamingThread = new Thread(ReadStream);
            _audioThread = new Thread(PlayStream);

            _streamingThread.Start();
            _audioThread.SetApartmentState(ApartmentState.STA);
            _audioThread.Start();
        }

        private async Task<bool> CheckStatus()
        {
            if (_streamingThread == null || _audioThread == null)
                return true;

            if (_streamingThread.ThreadState != System.Threading.ThreadState.Stopped || _audioThread.ThreadState != System.Threading.ThreadState.Stopped)
            {
                await Task.Delay(1000);
                return false;
            }

            return true;
        }

        public void Stop()
        {
            StopPlayback();
            playbackState = PlaybackState.Stopped;
            OnPropertyChanged(nameof(PlaybackState));

            if (_audioThread == null || _audioThreadTokenSource == null)
                return;

            _audioThreadTokenSource.Cancel();
        }

        int data_count = 0;
        Stopwatch _sw = Stopwatch.StartNew();
        private void ReadStream()
        {
            // TODO : add log handler here.
            if (_url == null)
                return;
            //throw new Exception();
            IMp3FrameDecompressor? decompressor = null;

            _sw = Stopwatch.StartNew();
            data_count = 0;
            try
            {
                using (var httpClient = new HttpClient())
                using (var stream = httpClient.GetStreamAsync(_url).Result)
                {
                    var buffer = new byte[38400];

                    try
                    {
                        using (var rfs = new ReadFullyStream(stream))
                        {
                            do
                            {

                                if (_audioThreadTokenSource != null && _audioThreadTokenSource.IsCancellationRequested)
                                {
                                    _audioThreadTokenSource?.Token.ThrowIfCancellationRequested();
                                }

                                if (IsBufferNearlyFull)
                                {
                                    //Debug.WriteLine("Buffer getting full, taking a break");
                                    Thread.Sleep(500);
                                }
                                else
                                {
                                    Mp3Frame frame;
                                    try
                                    {
                                        frame = Mp3Frame.LoadFromStream(rfs);
                                    }
                                    catch (EndOfStreamException)
                                    {
                                        fullyDownloaded = true;
                                        // reached the end of the MP3 file / stream
                                        break;
                                    }
                                    catch (WebException)
                                    {
                                        // probably we have aborted download from the GUI thread
                                        break;
                                    }
                                    if (frame == null) break;
                                    if (decompressor == null)
                                    {
                                        // don't think these details matter too much - just help ACM select the right codec
                                        // however, the buffered provider doesn't know what sample rate it is working at
                                        // until we have a frame
                                        decompressor = CreateFrameDecompressor(frame);
                                        bufferedWaveProvider = new CustomBufferedWaveProvider(decompressor.OutputFormat);
                                        bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(20); // allow us to get well ahead of ourselves

                                        wave16ToFloatProvider = new CustomWave16ToFloatProvider(bufferedWaveProvider);

                                        bufferedWaveProvider.OnFrameRead += (_, __) => OnDataReceived?.Invoke(this, __);
                                    }

                                    int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                                    //Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
                                    bufferedWaveProvider?.AddSamples(buffer, 0, decompressed);

                                    var b1 = new WaveBuffer(buffer);
                                    int len = b1.FloatBuffer.Length / 8;

                                    data_count++;
                                    var fps = data_count / _sw.Elapsed.TotalSeconds;

                                    // fft
                                    System.Numerics.Complex[] values = new System.Numerics.Complex[len];
                                    for (int i = 0; i < len; i++)
                                        values[i] = new System.Numerics.Complex(b1.FloatBuffer[i], 0.0);

                                }
                            } while (playbackState != PlaybackState.Stopped);
                        }
                    }
                    finally
                    {
                        decompressor?.Dispose();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                decompressor?.Dispose();
            }
        }

        private void PlayStream()
        {
            try
            {
                while (true)
                {
                    if (_audioThreadTokenSource != null && _audioThreadTokenSource.IsCancellationRequested)
                    {
                        _audioThreadTokenSource?.Token.ThrowIfCancellationRequested();
                    }

                    if (playbackState != PlaybackState.Stopped)
                    {
                        if (waveOut == null && bufferedWaveProvider != null)
                        {
                            // TODO : check UI responses as colume and buffer
                            //Debug.WriteLine("Creating WaveOut Device");
                            waveOut = CreateWaveOut();
                            //waveOut.PlaybackStopped += OnPlaybackStopped;
                            //volumeProvider = new VolumeWaveProvider16(wave16ToFloatProvider);
                            ////volumeProvider.Volume = volumeSlider1.Volume;
                            //volumeProvider.Volume = 1;
                            waveOut.Init(wave16ToFloatProvider);
                            //progressBarBuffer.Maximum = (int)bufferedWaveProvider.BufferDuration.TotalMilliseconds;
                        }
                        else if (bufferedWaveProvider != null)
                        {
                            var bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
                            //ShowBufferState(bufferedSeconds);
                            // make it stutter less if we buffer up a decent amount before playing
                            if (bufferedSeconds < 0.5 && playbackState == PlaybackState.Playing && !fullyDownloaded)
                            {
                                waveOut?.Pause();
                            }
                            else if (bufferedSeconds > 4 && playbackState == PlaybackState.Buffering)
                            {
                                waveOut?.Play();
                                playbackState = PlaybackState.Playing;
                            }
                            else if (fullyDownloaded && bufferedSeconds == 0)
                            {
                                Debug.WriteLine("Reached end of stream");
                                StopPlayback();
                            }
                        }
                    }

                    Thread.Sleep(250);
                }
            }
            catch (OperationCanceledException)
            {

            }
        }

        private bool IsBufferNearlyFull
        {
            get
            {
                return bufferedWaveProvider != null &&
                       bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                       < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
            }
        }

        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }
        private IWavePlayer CreateWaveOut()
        {
            var wo = new WaveOut();

            return new WaveOut();
        }

        private void Wi_DataAvailable(object? sender, WaveInEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void StopPlayback()
        {
            if (playbackState != PlaybackState.Stopped)
            {
                if (!fullyDownloaded)
                {
                    //webRequest.Abort();
                }

                playbackState = PlaybackState.Stopped;
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;
                }
                // n.b. streaming thread may not yet have exited
                //Thread.Sleep(500);
                //ShowBufferState(0);
            }
        }
    }

    public enum PlaybackState
    {
        Stopped,
        Playing,
        Buffering,
        Paused
    }
}