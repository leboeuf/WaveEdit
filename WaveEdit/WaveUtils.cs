using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace WaveEdit
{
    public static class WaveUtils
    {
        // stackoverflow 8754111
        public static double BytesToDouble(byte firstByte, byte secondByte)
        {
            short s = (short)((secondByte << 8) | firstByte);
            return s / 32768.0;
        }

        // https://stackoverflow.com/questions/39605118/how-to-draw-an-audio-waveform-to-a-bitmap
        public static void averages(double[] data, int startIndex, int endIndex, out double posAvg, out double negAvg)
        {
            posAvg = 0.0f;
            negAvg = 0.0f;

            int posCount = 0, negCount = 0;

            for (int i = startIndex; i < endIndex; i++)
            {
                if (data[i] > 0)
                {
                    posCount++;
                    posAvg += data[i];
                }
                else
                {
                    negCount++;
                    negAvg += data[i];
                }
            }

            if (posCount > 0)
                posAvg /= posCount;
            if (negCount > 0)
                negAvg /= negCount;
        }

        // https://stackoverflow.com/questions/36081740/uwp-async-read-file-into-byte
        public static async Task<WaveData> LoadWave(StorageFile file)
        {
            byte[] wav;
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                using (var memoryStream = new MemoryStream())
                {

                    stream.CopyTo(memoryStream);
                    wav = memoryStream.ToArray();
                }
            }

            int channels = wav[22];
            int sampleRate = wav[24] + wav[25] * 256;
            int bitsPerSample = wav[34];
            int pos = 12;

            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
            {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            // Pos is now positioned to start of actual sound data
            int samples = (wav.Length - pos) / 2;   // 2 bytes per sample (16 bit sound mono)
            if (channels == 2) samples /= 2;        // 4 bytes per sample (16 bit stereo)

            // Allocate memory (right will be null if only mono sound)
            var result = new WaveData
            {
                BitsPerSample = bitsPerSample,
                SampleRate = sampleRate,
                LeftChannel = new double[samples],
                RightChannel = channels == 2 ? new double[samples] : null
            };

            int i = 0;
            while (pos < wav.Length)
            {
                result.LeftChannel[i] = WaveUtils.BytesToDouble(wav[pos], wav[pos + 1]);
                pos += 2;
                if (channels == 2)
                {
                    result.RightChannel[i] = WaveUtils.BytesToDouble(wav[pos], wav[pos + 1]);
                    pos += 2;
                }
                i++;
            }

            return result;
        }
    }
}
