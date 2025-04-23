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
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace Project_Lightning.Windows
{
    /// <summary>
    /// Lógica de interacción para LegalWindow.xaml
    /// </summary>
    public partial class LegalWindow : Window
    {
        public LegalWindow()
        {
            InitializeComponent();
            cargarGif();
        }

        private void cargarGif()
        {
            var gifPath = "/res/media/logos/originals/law.gif";  //RUTA RELATIVA

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(gifPath, UriKind.Relative);
            image.EndInit();

            ImageBehavior.SetAnimatedSource(gifImage, image);

        }

        //EVENTO PARA EL BOTON DE SALIR
        private void botonSalirClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
