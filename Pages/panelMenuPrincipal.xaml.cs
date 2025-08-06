using Project_Lightning.Windows;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Project_Lightning.Pages.panelBienvenida;

namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interacción para panelMenuPrincipal.xaml
    /// </summary>
    public partial class panelMenuPrincipal : Page
    {

        private List<Sponsor> listaSponsors = new List<Sponsor>();
        private int indiceActualSponsor = 0;
        private DispatcherTimer timerCarrusel;
        private int indiceSponsorVisible = 0;

        public panelMenuPrincipal()
        {
            InitializeComponent();
            cargarSponsors();
        }


        //BOTON DE ONLINE FIX


        //BOTON DE BYPASS
        private void bypassButtonClick(object sender, RoutedEventArgs e)
        {
            var ventanaPrincipal = Application.Current.MainWindow as MainWindow;

            if (ventanaPrincipal != null)
            {

                //CAMBIAR EL CONTENIDO DEL FRAME
                ventanaPrincipal.framePrincipal.Navigate(new panelBypass(ventanaPrincipal));
            }
            
        }

        //BOTON DE ONLINE FIX
        private void onlineFixButtonClick(object sender, RoutedEventArgs e)
        {
            var ventanaPrincipal = Application.Current.MainWindow as MainWindow;

            if (ventanaPrincipal != null)
            {

                //CAMBIAR EL CONTENIDO DEL FRAME
                ventanaPrincipal.framePrincipal.Navigate(new panelOnlineFix(ventanaPrincipal));
            }

        }

        //METODO PARA EL CLICK PARA UNIRSE A MI SERVIDOR
        private void joinServerClick(object sender, RoutedEventArgs e)
        {
            //URL DE MI PERFIL DE DISCORD
            string url = "https://discord.gg/JBB2pTNTqK";

            //ABRIR URL EN EL NAVEGADOR PREDETERMINADO
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }


        private void legalButtonClick(object sender, RoutedEventArgs e)
        {
            var legalWindow = new LegalWindow();
            legalWindow.Show();
        }

        private void changeLogButtonClick(object sender, RoutedEventArgs e)
        {
            var changeLog = new ChangeLog();
            changeLog.Show();
        }

        private void quickGuideButtonClick(object sender, RoutedEventArgs e)
        {
            var quieckGuide = new QuickGuide();
            quieckGuide.Show();
        }

        //DONACION PRESIONADO
        public void donate_click(object sender, RoutedEventArgs e)
        {
            var ventanaDonacion = new DonateWindow();
            ventanaDonacion.Show();
        }


        public class Sponsor
        {
            public BitmapImage Imagen { get; set; }
            public string Url { get; set; }
        }


        //METODO PARA CARGAR LOS SPONSOR
        private void cargarSponsors()
        {
            string rutaSponsors = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "media", "sponsors");
            string rutaMapaEnlaces = System.IO.Path.Combine(rutaSponsors, "sponsors.txt");

            Dictionary<string, string> mapaEnlaces = new Dictionary<string, string>();

            if (File.Exists(rutaMapaEnlaces))
            {
                foreach (string linea in File.ReadAllLines(rutaMapaEnlaces))
                {
                    var partes = linea.Split('|');
                    if (partes.Length == 2)
                    {
                        mapaEnlaces[partes[0]] = partes[1];
                    }
                }
            }

            if (Directory.Exists(rutaSponsors))
            {
                string[] archivosPng = Directory.GetFiles(rutaSponsors, "*.png");

                foreach (string archivo in archivosPng)
                {
                    string nombreArchivo = System.IO.Path.GetFileName(archivo);

                    BitmapImage imagen = new BitmapImage();
                    imagen.BeginInit();
                    imagen.UriSource = new Uri(archivo, UriKind.Absolute);
                    imagen.CacheOption = BitmapCacheOption.OnLoad;
                    imagen.EndInit();

                    string url = mapaEnlaces.ContainsKey(nombreArchivo) ? mapaEnlaces[nombreArchivo] : null;

                    listaSponsors.Add(new Sponsor
                    {
                        Imagen = imagen,
                        Url = url
                    });
                }

                if (listaSponsors.Count > 0)
                {
                    iniciarCarruselSponsors();
                }
            }
        }


        //METODO PARA INICIAR EL CARRUSEL DE LAS IMAGENES
        private void iniciarCarruselSponsors()
        {
            imagenSponsor.MouseLeftButtonUp += ImagenSponsor_MouseLeftButtonUp;
            imagenSponsor.MouseEnter += (s, e) => ZoomIn();
            imagenSponsor.MouseLeave += (s, e) => ZoomOut();

            imagenSponsor.Source = listaSponsors[indiceSponsorVisible].Imagen; //MOSTRAR LA PRIMERA IMAGEN AL INSTANTE

            //SI SOLO HAY UNA IMAGEN, NO HACE FALTA INICIAR EL CARRUSEL
            if (listaSponsors.Count == 1)
            {
                return;
            }

            indiceActualSponsor = 1;

            timerCarrusel = new DispatcherTimer();
            timerCarrusel.Interval = TimeSpan.FromSeconds(4);
            timerCarrusel.Tick += (s, e) =>
            {
                CambiarImagenConFade(listaSponsors[indiceActualSponsor].Imagen);
                indiceSponsorVisible = indiceActualSponsor;
                indiceActualSponsor = (indiceActualSponsor + 1) % listaSponsors.Count;
            };
            timerCarrusel.Start();
        }

        private void CambiarImagenConFade(BitmapImage nuevaImagen)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
            fadeOut.Completed += (s, e) =>
            {
                imagenSponsor.Source = nuevaImagen;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500));
                imagenSponsor.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            imagenSponsor.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }


        private void ImagenSponsor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (listaSponsors.Count > 0)
            {
                string url = listaSponsors[indiceSponsorVisible].Url;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
        }

        private void ZoomIn()
        {
            var scaleTransform = (ScaleTransform)imagenSponsor.RenderTransform;

            var animX = new DoubleAnimation(1.0, 1.1, TimeSpan.FromMilliseconds(200));
            var animY = new DoubleAnimation(1.0, 1.1, TimeSpan.FromMilliseconds(200));

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
        }

        private void ZoomOut()
        {
            var scaleTransform = (ScaleTransform)imagenSponsor.RenderTransform;

            var animX = new DoubleAnimation(1.1, 1.0, TimeSpan.FromMilliseconds(200));
            var animY = new DoubleAnimation(1.1, 1.0, TimeSpan.FromMilliseconds(200));

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
        }
    }
}
