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
    /// Lógica de interacción para ChangeLog.xaml
    /// </summary>
    public partial class ChangeLog : Window
    {
        public ChangeLog()
        {
            InitializeComponent();
            cargarGif();
        }

        private void cargarGif()
        {
            var gifPath = "/res/media/logos/originals/changelog.gif";  //RUTA RELATIVA

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(gifPath, UriKind.Relative);
            image.EndInit();

            ImageBehavior.SetAnimatedSource(gifImage, image);

        }

        private void CerrarClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
