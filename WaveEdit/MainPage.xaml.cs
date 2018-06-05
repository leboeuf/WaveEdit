﻿using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WaveEdit
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public WaveEditor Editor { get; set; }

        public MainPage()
        {
            this.Editor = new WaveEditor();
            this.InitializeComponent();
        }

        public double _pointerX1 { get; set; }
        public double _pointerX2 { get; set; }
        public bool IsPointerPressed { get; set; } // Whether the left mouse button is being held down

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

            this.Editor.WaveData = await WaveUtils.LoadWave(file);
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
            if (_pointerX1 > 0)
            {
                args.DrawingSession.DrawLine((float)_pointerX1, 0f, (float)_pointerX1, (float)height, Colors.Blue);
            }
            if (_pointerX2 > 0)
            {
                args.DrawingSession.DrawLine((float)_pointerX2, 0f, (float)_pointerX2, (float)height, Colors.Blue);
            }

            if (IsPointerPressed)
            {
                var brush = new CanvasSolidColorBrush(canvas, Colors.LightSkyBlue);
                brush.Opacity = 0.4f;
                args.DrawingSession.FillRectangle((float)_pointerX1, 0f, (float)(_pointerX2 - _pointerX1), (float)height, brush);
            }

        }

        private void canvas_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            if (IsPointerPressed)
            {
                _pointerX2 = point.Position.X;
            }
            else
            {
                _pointerX1 = point.Position.X;
            }

            canvas.Invalidate();
        }

        private void canvas_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            IsPointerPressed = true;

            canvas.Invalidate();
        }

        private void canvas_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            IsPointerPressed = false;

            canvas.Invalidate();
        }

        private void canvas_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // TODO: zoom
            canvas.Invalidate();
        }
    }
}
