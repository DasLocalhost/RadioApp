using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RadioApp.Core
{
    /// <summary>
    /// Provides a buffered store of samples with the reading event
    /// Read method will return queued samples or fill buffer with zeroes
    /// Now backed by a circular buffer
    /// </summary>
    public class CustomBufferedWaveProvider : IWaveProvider
    {
        private CircularBuffer circularBuffer;
        private WaveFormat waveFormat;

        private List<byte[]> _samples;

        public EventHandler<byte[]>? OnFrameRead;

        /// <summary>
        /// Creates a new buffered WaveProvider
        /// </summary>
        /// <param name="waveFormat">WaveFormat</param>
        public CustomBufferedWaveProvider(WaveFormat waveFormat)
        {
            this.waveFormat = waveFormat;
            this.BufferLength = waveFormat.AverageBytesPerSecond * 5;
        }

        /// <summary>
        /// Buffer length in bytes
        /// </summary>
        public int BufferLength { get; set; }

        /// <summary>
        /// Buffer duration
        /// </summary>
        public TimeSpan BufferDuration
        {
            get
            {
                return TimeSpan.FromSeconds((double)BufferLength / WaveFormat.AverageBytesPerSecond);
            }
            set
            {
                BufferLength = (int)(value.TotalSeconds * WaveFormat.AverageBytesPerSecond);
            }
        }

        /// <summary>
        /// If true, when the buffer is full, start throwing away data
        /// if false, AddSamples will throw an exception when buffer is full
        /// </summary>
        public bool DiscardOnBufferOverflow { get; set; }

        /// <summary>
        /// The number of buffered bytes
        /// </summary>
        public int BufferedBytes
        {
            get { if (circularBuffer == null) return 0; return circularBuffer.Count; }
        }

        /// <summary>
        /// Buffered Duration
        /// </summary>
        public TimeSpan BufferedDuration
        {
            get { return TimeSpan.FromSeconds((double)BufferedBytes / WaveFormat.AverageBytesPerSecond); }
        }

        /// <summary>
        /// Gets the WaveFormat
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        /// <summary>
        /// Adds samples. Takes a copy of buffer, so that buffer can be reused if necessary
        /// </summary>
        public void AddSamples(byte[] buffer, int offset, int count)
        {
            // create buffer here to allow user to customise buffer length
            if (this.circularBuffer == null)
            {
                this.circularBuffer = new CircularBuffer(this.BufferLength);
            }

            //int sourceBytesRequired = count / 2;
            //byte[] destBuffer = new byte[count * 2];
            //byte[] sourceBuffer = new byte[sourceBytesRequired];
            ////int sourceBytesRead = sourceProvider.Read(sourceBuffer, offset, sourceBytesRequired);
            //WaveBuffer sourceWaveBuffer = new WaveBuffer(sourceBuffer);
            //WaveBuffer destWaveBuffer = new WaveBuffer(destBuffer);

            //int sourceSamples = count / 2;
            //int destOffset = offset / 4;
            //for (int sample = 0; sample < sourceSamples; sample++)
            //{
            //    destWaveBuffer.FloatBuffer[destOffset++] = (sourceWaveBuffer.ShortBuffer[sample] / 32768f) * 1;
            //}

            int written = this.circularBuffer.Write(buffer, offset, count);
            if (written < count && !DiscardOnBufferOverflow)
            {
                throw new InvalidOperationException("Buffer full");
            }

            OnFrameRead?.Invoke(this, buffer);
        }

        /// <summary>
        /// Reads from this WaveProvider
        /// Will always return count bytes, since we will zero-fill the buffer if not enough available
        /// </summary>
        public int Read(byte[] buffer, int offset, int count)
        {
            //count = 65536;
            //buffer = new byte[count];

            int read = 0;
            if (this.circularBuffer != null) // not yet created
            {
                read = this.circularBuffer.Read(buffer, offset, count);
            }
            if (read < count)
            {
                // zero the end of the buffer
                Array.Clear(buffer, offset + read, count - read);
            }

            if (count != 0)
            {
                //var sp = this.ToSampleProvider();

                //var bytesPerFrame = sp.WaveFormat.BitsPerSample / 8 * sp.WaveFormat.Channels;

                //var bufferedFrames = this.BufferedBytes / bytesPerFrame;

                //var frames = new float[bufferedFrames];
                ////sp.Read(frames, 0, bufferedFrames);

                //OnFrameRead?.Invoke(this, buffer);
            }

            return count;
        }
    }
}
