using System;
using System.Collections.Generic;
using System.IO;
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
using Newtonsoft.Json;
using static Project_Lightning.Pages.panelApp;

namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interacción para panelJuego.xaml
    /// </summary>
    public partial class panelJuego : Page
    {

        private string nombreApp;
        private panelApp panelApp;
        private MainWindow ventanaPrincipal;
        public panelJuego(string nombreApp, KeyValuePair<string, panelApp.Juego> juego, panelApp panelApp, MainWindow ventanaPrincipal)
        {
            InitializeComponent();

            this.nombreApp = nombreApp;
            this.panelApp = panelApp;
            this.ventanaPrincipal = ventanaPrincipal;

            crearBackground(juego.Key);
            crearLogo(juego.Key);
            sacarInfoJuego(juego);


        }


        private void crearLogo(string key)
        {
            //CREO LA IMAGEN QUE IRA EN CADA BOTON
            Image imagenJuego = new Image
            {
                Stretch = Stretch.Fill
            };

            //INTENTO CARGAR LA IMAGEN ORIGINAL
            imagenJuego.Source = new BitmapImage(new Uri("https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/" + key + "/logo.png"));

            //SI NO SE HA PUESTO NINGUNA IMAGEN (ES DECIR, QUE ESTE JUEGO NO TIENE)
            imagenJuego.ImageFailed += (sender, e) => {};

            gridImagenPeque.Children.Add(imagenJuego);
        }

        private void crearBackground(string key)
        {
            //CREO LA IMAGEN QUE IRA EN CADA BOTON
            Image imagenJuego = new Image
            {
                Stretch = Stretch.Fill
            };

            //INTENTO CARGAR LA IMAGEN ORIGINAL
            imagenJuego.Source = new BitmapImage(new Uri("https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/" + key + "/library_hero.jpg"));

            //SI NO SE HA PUESTO NINGUNA IMAGEN (ES DECIR, QUE ESTE JUEGO NO TIENE)
            imagenJuego.ImageFailed += (sender, e) =>
            {
                imagenJuego.Stretch = Stretch.UniformToFill;
                imagenJuego.HorizontalAlignment = HorizontalAlignment.Stretch;
                imagenJuego.VerticalAlignment = VerticalAlignment.Top;
                imagenJuego.Source = new BitmapImage(new Uri("https://cdn.cloudflare.steamstatic.com/steam/apps/" + key + "/capsule_616x353.jpg"));
            };

            panelBackground.Children.Insert(0,imagenJuego);
        }

        //METODO PARA CAMBIAR EL TAMAÑO DE LA IMAGEN DEPENDIENDO DEL TAMAÑO DE LA VENTANA
        private void panelBackground_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double proporcion = 620.0 / 1920.0; // → 0.3229
            double ancho = panelBackground.ActualWidth;
            double nuevoAlto = ancho * proporcion;

            panelBackground.Height = nuevoAlto;
        }

        //METODO PARA SACAR TODA LA INFORMACION DEL JSON DEL JUEGO SELECCIONADO
        private void sacarInfoJuego(KeyValuePair<string, Juego> juego)
        {
            //PONEMOS EL NOMBRE AL JUEGO
            txtNombreJuego.Text = juego.Value.name;
            //SACAMOS SI ES POSIBLE JUGAR CON STEAM
            if (juego.Value.launch_steam == true)
            {
                txtSteamPosible.Foreground = Brushes.Green;
                txtSteamPosible.Text = "✔";
            }
            else
            {
                txtSteamPosible.Foreground = Brushes.Red;
                txtSteamPosible.Text = "✘";
            }

            //SACAMOS SI ES POSIBLE JUGAR CON EXE
            if (juego.Value.launch_exe == true)
            {
                txtExePosible.Foreground = Brushes.Green;
                txtExePosible.Text = "✔";
            }
            else
            {
                txtExePosible.Foreground = Brushes.Red;
                txtExePosible.Text = "✘";
            }

            //SACAR EL APARTADO DE ERROES
            txtErrors.Text = juego.Value.errores;

        }

         

        //CLICK DEL BOTON DE VOLVER
        private void volverClick(object sender, RoutedEventArgs e)
        {
            ventanaPrincipal.framePrincipal.Navigate(new panelApp(nombreApp, ventanaPrincipal));
        }
    }
}
