using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Lógica de interacción para DonateWindow.xaml
    /// </summary>
    public partial class DonateWindow : Window
    {
        public DonateWindow()
        {
            InitializeComponent(); 
            cargarGif();
            //cargarGifDiscord();
        }

        private void cargarGif()
        {
            var gifPath = "/res/media/logos/originals/purple-heart.gif";  //RUTA RELATIVA

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

        //EVENTO PARA EL BOTON DE DISCORD
        private void botonDiscord_Click(object sender, RoutedEventArgs e)
        {
            //URL DE MI PERFIL DE DISCORD
            string url = "https://discord.com/users/507878272072155148";

            //ABRIR URL EN EL NAVEGADOR PREDETERMINADO
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        //EVENTO PARA EL BOTON DE KO-FI
        private void botonKoFi_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://ko-fi.com/lightningfast",
                UseShellExecute = true
            });
        }

        //METODO PARA CARGAR EL GIF DEL BOTON DE FISCORD
        /*
        private void cargarGifDiscord()
        {
            var gifPath = "/res/media/logos/originals/discord_logo.gif";  //RUTA RELATIVA

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(gifPath, UriKind.Relative);
            image.EndInit();

            ImageBehavior.SetAnimatedSource(gifImageButton, image);

        }
        */
    }
}
