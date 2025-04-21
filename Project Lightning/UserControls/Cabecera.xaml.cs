using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace Project_Lightning.UserControls
{
    /// <summary>
    /// Lógica de interacción para Cabecera.xaml
    /// </summary>
    public partial class Cabecera : UserControl
    {
        public event RoutedEventHandler UbisoftPresionado;
        public event RoutedEventHandler EAPresionado;
        public event RoutedEventHandler RockstarPresionado;
        public event RoutedEventHandler DenuvoPresionado;
        public event RoutedEventHandler PlayStationPresionado;
        public event RoutedEventHandler OthersPresionado;
        public Cabecera()
        {
            InitializeComponent();
            cargarGif();
        }

        private void cargarGif()
        {
            var gifPath = "/res/media/logos/originals/donate.gif";  // Ruta relativa en tu proyecto

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(gifPath, UriKind.Relative);
            image.EndInit();

            ImageBehavior.SetAnimatedSource(gifImage, image);
        }

        //UBISOFT CLICK
        public void ubisoft_click(object sender, RoutedEventArgs e)
        {
            UbisoftPresionado?.Invoke(this, e);
        }

        //EA CLICK
        public void ea_click(object sender, RoutedEventArgs e)
        {
            EAPresionado?.Invoke(this, e);
        }

        //ROCKSTAR CLICK
        public void rockstar_click(object sender, RoutedEventArgs e)
        {
            RockstarPresionado?.Invoke(this, e);
        }

        //DENUVO CLICK
        public void denuvo_click(object sender, RoutedEventArgs e)
        {
            DenuvoPresionado?.Invoke(this, e);
        }

        //PLAYSTATION CLICK
        public void playstation_click(object sender, RoutedEventArgs e)
        {
            PlayStationPresionado?.Invoke(this, e);
        }

        //OTHERS PRESIONADO
        public void others_click(object sender, RoutedEventArgs e)
        {
            OthersPresionado?.Invoke(this, e);
        }


    }
}
