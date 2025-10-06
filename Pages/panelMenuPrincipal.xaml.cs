using Project_Lightning.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
            _ = IniciarCarruselPromos();
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






        //PARA LAS PRMOS
        public class Promo
        {
            public BitmapImage Imagen { get; set; }
        }


        private async Task<List<string>> ObtenerUrlsPromosDesdeGitHub()
        {
            var urls = new List<string>();
            string apiUrl = "https://api.github.com/repos/LightnigFast/Project-Lightning/contents/res/media/promos";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp"); // GitHub requiere User-Agent
                var json = await client.GetStringAsync(apiUrl);
                dynamic archivos = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                foreach (var archivo in archivos)
                {
                    string nombre = archivo.name;
                    string tipo = archivo.type;
                    if (tipo == "file" && (nombre.EndsWith(".png") || nombre.EndsWith(".jpg")))
                    {
                        string urlRaw = archivo.download_url;
                        urls.Add(urlRaw);
                    }
                }
            }

            return urls;
        }


        private async Task<List<Promo>> CargarPromosAsync()
        {
            string rutaLocalPromos = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "media", "promos");
            Directory.CreateDirectory(rutaLocalPromos);

            // 🔹 Obtener automáticamente todas las URLs de GitHub
            var urls = await ObtenerUrlsPromosDesdeGitHub();

            // 🔹 Nombres de las imágenes que deberían existir
            var nombresEnGitHub = urls.Select(u => System.IO.Path.GetFileName(u)).ToList();

            // 🔹 Eliminar imágenes locales que ya no están en GitHub
            var archivosLocales = Directory.GetFiles(rutaLocalPromos);
            foreach (var archivoLocal in archivosLocales)
            {
                string nombreArchivoLocal = System.IO.Path.GetFileName(archivoLocal);
                if (!nombresEnGitHub.Contains(nombreArchivoLocal))
                    File.Delete(archivoLocal);
            }

            var promos = new List<Promo>();

            foreach (var url in urls)
            {
                string nombreArchivo = System.IO.Path.GetFileName(url);
                string rutaLocal = System.IO.Path.Combine(rutaLocalPromos, nombreArchivo);

                // Si no está en local, descargar
                if (!File.Exists(rutaLocal))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var data = await client.GetByteArrayAsync(url);
                        await Task.Run(() => File.WriteAllBytes(rutaLocal, data));
                    }
                }

                // Cargar la imagen en memoria
                BitmapImage imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.UriSource = new Uri(rutaLocal, UriKind.Absolute);
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.EndInit();

                promos.Add(new Promo { Imagen = imagen });
            }

            return promos;
        }




        private List<Promo> listaPromos = new List<Promo>();
        private int indicePromoVisible = 0;
        private int indiceActualPromo = 0;
        private DispatcherTimer timerPromos;

        private async Task IniciarCarruselPromos()
        {
            listaPromos = await CargarPromosAsync();

            if (listaPromos.Count == 0)
                return;

            // 🔹 Empezar por la última imagen
            indicePromoVisible = listaPromos.Count - 1;
            imagenPromo.Source = listaPromos[indicePromoVisible].Imagen;

            // 🔹 Aplicar clip para respetar bordes redondos
            imagenPromo.SizeChanged += (s, e) =>
            {
                imagenPromo.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0, 0, imagenPromo.ActualWidth, imagenPromo.ActualHeight),
                    RadiusX = 12,
                    RadiusY = 12
                };
            };

            _ = FadeInAsync(imagenPromo);

            if (listaPromos.Count == 1)
                return;

            // 🔹 Siguiente imagen en el carrusel (circular)
            indiceActualPromo = 0;

            timerPromos = new DispatcherTimer();
            timerPromos.Interval = TimeSpan.FromSeconds(5);
            timerPromos.Tick += (s, e) =>
            {
                CambiarPromoConFade(listaPromos[indiceActualPromo].Imagen);
                indicePromoVisible = indiceActualPromo;
                indiceActualPromo = (indiceActualPromo + 1) % listaPromos.Count;
            };
            timerPromos.Start();

        }




        private void CambiarPromoConFade(BitmapImage nuevaImagen)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
            fadeOut.Completed += (s, e) =>
            {
                imagenPromo.Source = nuevaImagen;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500));
                imagenPromo.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            imagenPromo.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }



        //FADE IN: Aumenta Opacity de 0 a 1
        public static Task FadeInAsync(UIElement element, double durationSeconds = 0.3)
        {
            if (element == null) return Task.CompletedTask;

            element.Visibility = Visibility.Visible; // Aseguramos que se vea
            element.Opacity = 0; // Empezamos desde 0

            var tcs = new TaskCompletionSource<bool>();

            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                FillBehavior = FillBehavior.HoldEnd
            };

            fadeIn.Completed += (s, e) => tcs.SetResult(true);

            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            return tcs.Task;
        }


    }
}
