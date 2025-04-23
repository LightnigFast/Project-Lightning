using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Project_Lightning.Windows
{
    public partial class ErrorDialog : Window
    {
        private Brush _color;

        public ErrorDialog(string mensaje, Brush color)
        {
            InitializeComponent();
            txtMensaje.Text = mensaje;
            _color = color;
            borde.BorderBrush = _color;  
            botonOk.Background = _color;
            botonOk.BorderBrush = _color;

            //AGREGAR ANIMACIONES AL BOTON
            botonOk.MouseEnter += BotonOk_MouseEnter;
            botonOk.MouseLeave += BotonOk_MouseLeave;
        }

        private void BotonOk_MouseEnter(object sender, RoutedEventArgs e)
        {
            //CREAR ANIMACION PARA EL COLOR DE FONDO DEL BOTON
            var colorAnimado = ((SolidColorBrush)_color).Color;

            var colorAnimation = new ColorAnimation
            {
                From = ((SolidColorBrush)_color).Color,
                To = Colors.Transparent,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false
            };

            Storyboard.SetTarget(colorAnimation, botonOk);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(Button.Background).(SolidColorBrush.Color)"));

            var sb = new Storyboard();
            sb.Children.Add(colorAnimation);
            sb.Begin();
        }

        private void BotonOk_MouseLeave(object sender, RoutedEventArgs e)
        {
            //VOLVER AL COLOR PRINCIPAL
            var colorAnimado = ((SolidColorBrush)_color).Color;

            var colorAnimation = new ColorAnimation
            {

                From = Colors.Transparent,
                To = colorAnimado,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false
            };

            Storyboard.SetTarget(colorAnimation, botonOk);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(Button.Background).(SolidColorBrush.Color)"));

            var sb = new Storyboard();
            sb.Children.Add(colorAnimation);
            sb.Begin();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
