using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioApp.Core
{
    /// <summary>
    /// Converts 16 bit PCM to IEEE float, optionally adjusting volume along the way
    /// </summary>
    public class CustomWave16ToFloatProvider : IWaveProvider
    {
        private IWaveProvider sourceProvider;
        private readonly WaveFormat waveFormat;
        private volatile float volume;
        private byte[] sourceBuffer;

        private int data_count = 0;
        private Stopwatch? _sw = null;

        public EventHandler<byte[]>? OnFrameRead;

        /// <summary>
        /// Creates a new Wave16toFloatProvider
        /// </summary>
        /// <param name="sourceProvider">the source provider</param>
        public CustomWave16ToFloatProvider(IWaveProvider sourceProvider)
        {
            if (sourceProvider.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
                throw new ApplicationException("Only PCM supported");
            if (sourceProvider.WaveFormat.BitsPerSample != 16)
                throw new ApplicationException("Only 16 bit audio supported");

            waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sourceProvider.WaveFormat.SampleRate, sourceProvider.WaveFormat.Channels);

            this.sourceProvider = sourceProvider;
            this.volume = 1.0f;
        }

        /// <summary>
        /// Helper function to avoid creating a new buffer every read
        /// </summary>
        byte[] GetSourceBuffer(int bytesRequired)
        {
            if (this.sourceBuffer == null || this.sourceBuffer.Length < bytesRequired)
            {
                this.sourceBuffer = new byte[bytesRequired];
            }
            return sourceBuffer;
        }

        /// <summary>
        /// Reads bytes from this wave stream
        /// </summary>
        /// <param name="destBuffer">The destination buffer</param>
        /// <param name="offset">Offset into the destination buffer</param>
        /// <param name="numBytes">Number of bytes read</param>
        /// <returns>Number of bytes read.</returns>
        public int Read(byte[] destBuffer, int offset, int numBytes)
        {
            int sourceBytesRequired = numBytes / 2;
            byte[] sourceBuffer = GetSourceBuffer(sourceBytesRequired);
            int sourceBytesRead = sourceProvider.Read(sourceBuffer, offset, sourceBytesRequired);
            WaveBuffer sourceWaveBuffer = new WaveBuffer(sourceBuffer);
            WaveBuffer destWaveBuffer = new WaveBuffer(destBuffer);

            int sourceSamples = sourceBytesRead / 2;
            int destOffset = offset / 4;
            for (int sample = 0; sample < sourceSamples; sample++)
            {
                destWaveBuffer.FloatBuffer[destOffset++] = (sourceWaveBuffer.ShortBuffer[sample] / 32768f) * volume;
            }


            //var b1 = new WaveBuffer(destBuffer);
            //int len = b1.FloatBuffer.Length / 8;

            //// fft
            //System.Numerics.Complex[] values = new System.Numerics.Complex[len];
            //for (int i = 0; i < len; i++)
            //    values[i] = new System.Numerics.Complex(b1.FloatBuffer[i], 0.0);

            if (_sw == null)
            {
                _sw = Stopwatch.StartNew();
            }

            data_count++;

            if (_sw.ElapsedMilliseconds > 0)
            {
                var fps = (float)data_count / _sw.ElapsedMilliseconds * 1000;
            }

            OnFrameRead?.Invoke(this, destBuffer);

            return sourceSamples * 4;
        }


        /// <summary>
        /// <see cref="IWaveProvider.WaveFormat"/>
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        /// <summary>
        /// Volume of this channel. 1.0 = full scale
        /// </summary>
        public float Volume
        {
            get { return volume; }
            set { volume = value; }
        }
    }
}
