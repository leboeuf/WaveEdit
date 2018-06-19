using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveEdit
{
    public class WaveData
    {
        public int SampleRate { get; set; }
        public int BitsPerSample { get; set; }
        public double[] LeftChannel { get; set; }
        public double[] RightChannel { get; set; }
        //public [] Markers { get; set; }
    }
}
