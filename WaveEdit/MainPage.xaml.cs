using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
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

            const int FONT_SIZE = 14;

            args.DrawingSession.Clear(sender.ClearColor);
            args.DrawingSession.Antialiasing = Microsoft.Graphics.Canvas.CanvasAntialiasing.Aliased;
            var textFormat = new CanvasTextFormat { FontSize = FONT_SIZE, Direction = CanvasTextDirection.TopToBottomThenLeftToRight };
            var negTextFormat = new CanvasTextFormat { FontSize = FONT_SIZE, Direction = CanvasTextDirection.BottomToTopThenLeftToRight };

            var scale = Editor.Scale;
            var size = Editor.WaveData.LeftChannel.Length;
            var height = sender.ActualHeight;
            var width = sender.ActualWidth * scale;

            float previousX = 0;
            float previousY = (float)(height / 2);
            for (int x = 0; x < width / scale; x++)
            {
                int start = (int)(x * (size / width));
                int end = (int)((x + 1) * (size / width));
                if (end > size)
                    end = size;

                if (start == end)
                {
                    // Blank pixel
                    continue;
                }

                WaveUtils.averages(Editor.WaveData.LeftChannel, start, end, out double posAvg, out double negAvg);

                float yMax = (float)(height - ((posAvg + 1) * .5f * height));
                float yMin = (float)(height - ((negAvg + 1) * .5f * height));

                args.DrawingSession.DrawLine(x, yMax, x, yMin, Colors.Red);

                if (posAvg > 0)
                {
                    args.DrawingSession.DrawText(posAvg.ToString(), x - FONT_SIZE / 2, yMin, Colors.Blue, textFormat);
                    args.DrawingSession.DrawLine(previousX, previousY, x, yMax, Colors.Silver);
                    previousY = yMax;
                }
                else if (negAvg < 0)
                {
                    args.DrawingSession.DrawText(negAvg.ToString(), x - FONT_SIZE / 2, yMax, Colors.Blue, negTextFormat);
                    args.DrawingSession.DrawLine(previousX, previousY, x, yMin, Colors.Silver);
                    previousY = yMin;
                }
                else
                {
                    args.DrawingSession.DrawText("0", x - FONT_SIZE / 2, yMin, Colors.Blue, textFormat);
                    previousY = (float)(height / 2);
                }

                previousX = x;
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
            const float MIN_SCALE = 1;
            const float MAX_SCALE = 1000;

            var newScale = this.Editor.Scale + e.GetCurrentPoint(null).Properties.MouseWheelDelta * .01f;

            // Clamp between MIN_SCALE and MAX_SCALE
            newScale = (newScale < MIN_SCALE) ? MIN_SCALE : (newScale > MAX_SCALE) ? MAX_SCALE : newScale;
            this.Editor.Scale = newScale;

            canvas.Invalidate();
        }
    }
}
