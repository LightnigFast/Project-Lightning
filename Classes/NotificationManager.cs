using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Project_Lightning.Classes
{
    public class NotificationManager
    {
        private readonly Canvas canvas;

        public NotificationManager(Canvas canvasDestino)
        {
            canvas = canvasDestino;
        }

        public void Show(string message, bool isError = false, int durationMs = 3000)
        {
            Border border = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10),
                MinWidth = 50,
                Background = isError ? new SolidColorBrush(Color.FromRgb(255, 50, 50)) : new SolidColorBrush(Color.FromRgb(50, 200, 50)),
                BorderBrush = isError ? Brushes.Red : Brushes.Green,
                BorderThickness = new Thickness(2),
                Opacity = 0,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 8,
                    ShadowDepth = 2,
                    Opacity = 0.3
                }
            };

            TextBlock text = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = text;

            canvas.Children.Add(border);

            // Posición
            Canvas.SetRight(border, 0);
            double topOffset = 0;
            foreach (UIElement child in canvas.Children)
                topOffset += ((FrameworkElement)child).ActualHeight + 10;
            Canvas.SetTop(border, topOffset);

            // Animación de entrada
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            TranslateTransform tt = new TranslateTransform(50, 0);
            border.RenderTransform = tt;
            DoubleAnimation slideIn = new DoubleAnimation(50, 0, TimeSpan.FromMilliseconds(300));

            border.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            tt.BeginAnimation(TranslateTransform.XProperty, slideIn);

            // Auto desaparición
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                fadeOut.Completed += (s2, e2) => canvas.Children.Remove(border);
                border.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            };
            timer.Start();
        }
    }
}
