using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WaveEdit
{
    public sealed partial class MainPage : Page
    {
        public WaveEditor Editor { get; set; }

        public MainPage()
        {
            this.Editor = new WaveEditor();
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: use MVVM and put this in the ViewModel
            await LoadWave(WaveFilePath.Text);
        }

        public async Task LoadWave(string filePath)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".wav");
            var file = await picker.PickSingleFileAsync();
            var filename = file.Name;

            this.filenameTextBlock.Text = filename;

            this.Editor.WaveData = await WaveUtils.LoadWave(file);
            this.sampleRateTextBlock.Text = $"{this.Editor.WaveData.SampleRate.ToString()} Hz";
            this.formatTextBlock.Text = $"{this.Editor.WaveData.BitsPerSample.ToString()}-bit";
        }
        
        // https://stackoverflow.com/questions/39605118/how-to-draw-an-audio-waveform-to-a-bitmap
        private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (Editor.WaveData.LeftChannel == null) return;

            args.DrawingSession.Clear(sender.ClearColor);

            var size = Editor.WaveData.LeftChannel.Length;
            var height = sender.ActualHeight;
            var width = sender.ActualWidth;
            for (int x = 0; x < width; x++)
            {
                int start = (int)(x * (size / width));
                int end = (int)((x + 1) * (size / width));
                if (end > size)
                    end = size;

                WaveUtils.averages(Editor.WaveData.LeftChannel, start, end, out double posAvg, out double negAvg);

                float yMax = (float)(height - ((posAvg + 1) * .5f * height));
                float yMin = (float)(height - ((negAvg + 1) * .5f * height));

                args.DrawingSession.DrawLine(x, yMax, x, yMin, Colors.Red);
            }

            // Selection
            if (this.Editor.PointerX1 > 0)
            {
                args.DrawingSession.DrawLine((float)this.Editor.PointerX1, 0f, (float)this.Editor.PointerX1, (float)height, Colors.Blue);
            }
            if (this.Editor.PointerX2 > 0)
            {
                args.DrawingSession.DrawLine((float)this.Editor.PointerX2, 0f, (float)this.Editor.PointerX2, (float)height, Colors.Blue);
            }

            if (this.Editor.IsPointerPressed)
            {
                var brush = new CanvasSolidColorBrush(canvas, Colors.LightSkyBlue);
                brush.Opacity = 0.4f;
                args.DrawingSession.FillRectangle((float)this.Editor.PointerX1, 0f, (float)(this.Editor.PointerX2 - this.Editor.PointerX1), (float)height, brush);
            }

        }

        private void canvas_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            if (this.Editor.IsPointerPressed)
            {
                this.Editor.PointerX2 = point.Position.X;
            }
            else
            {
                this.Editor.PointerX1 = point.Position.X;
            }

            canvas.Invalidate();
        }

        private void canvas_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            this.Editor.IsPointerPressed = true;

            canvas.Invalidate();
        }

        private void canvas_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            this.Editor.IsPointerPressed = false;

            canvas.Invalidate();
        }

        private void canvas_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // TODO: zoom
            canvas.Invalidate();
        }
    }
}
