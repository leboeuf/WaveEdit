using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveEdit
{
    public class WaveEditor
    {
        /// <summary>
        /// Pointer position when the mouse button was first pressed. Used to place the first anchor.
        /// </summary>
        public double PointerX1 { get; set; }

        /// <summary>
        /// Pointer position when the mouse button was released. Used to place the second anchor.
        /// </summary>
        public double PointerX2 { get; set; }

        /// <summary>
        /// Whether the mouse button is being held down. Used for dragging anchors.
        /// </summary>
        public bool IsPointerPressed { get; set; }

        /// <summary>
        /// The zoom scale. When scale is 1, the full wave file will fit the screen.
        /// </summary>
        public float Scale { get; set; } = 400;

        public WaveData WaveData { get; set; } = new WaveData();
    }
}
