using Newtonsoft.Json;
using Project_Lightning.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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


namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interacción para panelTienda.xaml
    /// </summary>
    public partial class panelTienda : Page
    {
        MainWindow ventanaPrincipal;
        private Dictionary<string, Juego> todosLosJuegos = new Dictionary<string, Juego>();
        private readonly string gamesImagesFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Project_Lightning", "gamesImages");


        public panelTienda(MainWindow ventanaPrincipal)
        {
            InitializeComponent();
            this.ventanaPrincipal = ventanaPrincipal;
            gridCabecera.Visibility = Visibility.Hidden;
            bordeJuegos.Visibility = Visibility.Hidden;

            ponerJuegos();


        }

        //CLASE JUEGO QUE CONTENDRA LA INFORMACIÓN DE CADA JUEGO
        public class Juego
        {
            public string name { get; set; }
            public bool activo { get; set; }
            public string imgCabecera { get; set; }
            public string imgLogo { get; set; }
            public string imgVertical { get; set; }
            public int precioNormal { get; set; }
            public int precioDonadores { get; set; }
            public int descuento { get; set; }
        }

        //METODO PARA PRECARGAR LAS IMAGENES
        private async Task PreloadAllImages(Dictionary<string, Juego> todosLosJuegos)
        {
            var tasks = new List<Task>();

            foreach (var kvp in todosLosJuegos)
            {
                string appid = kvp.Key;
                Juego juego = kvp.Value;

                // Descarga las tres imágenes en paralelo
                tasks.Add(GetGameImageAsync(appid, juego.imgVertical, "library_600x900"));
                tasks.Add(GetGameImageAsync(appid, juego.imgCabecera, "library_hero"));
                tasks.Add(GetGameImageAsync(appid, juego.imgLogo, "logo"));
            }

            await Task.WhenAll(tasks);
        }


        //METODO PARA OBTENER UNA IMAGEN: DESCARGA SI NO EXISTE
        private async Task<BitmapImage> GetGameImageAsync(string appid, string url, string imageName)   
        {
            try
            {
                //CARPETA DEL JUEGO
                string appFolder = System.IO.Path.Combine(gamesImagesFolder, appid);
                if (!Directory.Exists(appFolder))
                    Directory.CreateDirectory(appFolder);

                string localPath = System.IO.Path.Combine(appFolder, imageName + ".jpg");

                //DESCARGAR SI NO EXISTE
                if (!File.Exists(localPath) && !string.IsNullOrEmpty(url))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(url);
                        File.WriteAllBytes(localPath, bytes);
                    }
                }

                //CARGAR IMAGEN
                if (File.Exists(localPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(localPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch
            {
                //SI FALLA, DEVUELVE NULL
            }

            return null;
        }

        private async void ponerJuegos()
        {
            todosLosJuegos = await sacarJuegosDeApp();
            overlayCarga.Visibility = Visibility.Visible;
            // Pre-cargar todas las imágenes
            await PreloadAllImages(todosLosJuegos);
            overlayCarga.Visibility = Visibility.Collapsed;
            await colocarJuegos(todosLosJuegos);
            await FadeInAsync(bordeJuegos, 0.5);

        }


        //ESTE METODO BUSCA SACAR TODOS LOS JUEGOS DE UNA SOLA COMPAÑIA DADA POR EL nomApp
        private async Task<Dictionary<string, Juego>> sacarJuegosDeApp()
        {
            string rutaJson = System.IO.Path.GetFullPath(@"..\\..\\shop.json");
            string rutaJsonApp = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shop.json");
            string urlJson = "https://raw.githubusercontent.com/LightnigFast/Project-Lightning/main/shop.json";

            if (await EsArchivoGitHubDiferente(urlJson, rutaJson) || await EsArchivoGitHubDiferente(urlJson, rutaJsonApp))
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string contenidoGitHub = await client.GetStringAsync(urlJson);
                        File.WriteAllText(rutaJson, contenidoGitHub);
                        File.WriteAllText(rutaJsonApp, contenidoGitHub);
                    }
                }
                catch (Exception ex)
                {
                    var ventanaError = new Windows.ErrorDialog("Error updating the game list, please try again later: " + ex.Message, Brushes.Red);
                    ventanaError.ShowDialog();
                }
            }

            //CARGAR JSON LOCAL
            string json = File.ReadAllText(rutaJsonApp);

            //YA NO HAY NIVELES INTERNOS, SOLO APPID -> Juego
            var data = JsonConvert.DeserializeObject<Dictionary<string, Juego>>(json);
            //MessageBox.Show(data.Count.ToString());

            return data ?? new Dictionary<string, Juego>();
        }


        private async Task<bool> EsArchivoGitHubDiferente(string url, string rutaLocal)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string contenidoGitHub = await client.GetStringAsync(url);

                    if (!File.Exists(rutaLocal))
                    {
                        return true; //NO EXISTE EL LOCAL, ES DIFERENTE
                    }

                    string contenidoLocal = File.ReadAllText(rutaLocal);

                    return !contenidoLocal.Equals(contenidoGitHub);

                }
            }
            catch (Exception ex)
            {
                var ventanaError = new ErrorDialog("Error comparing files: " + ex.Message, Brushes.Red);
                ventanaError.Show();
                //MessageBox.Show("Error al comparar archivos: " + ex.Message);
                return false;
            }
        }


        //METODO PARA COLOCAR LOS JUEGOS EN EL PANEL
        private async Task colocarJuegos(Dictionary<string, Juego> todosLosJuegos)
        {

            //LIMPIAR EL PANEL ANTES
            panelJuegos.Children.Clear();

            foreach (var kvp in todosLosJuegos)
            {
                Juego juego = kvp.Value;

                //CREAR BORDER CON BORDES REDONDEADOS
                Border border = new Border
                {
                    Width = 150,
                    Height = 225,
                    Margin = new Thickness(5),
                    Cursor = Cursors.Hand,
                    Tag = kvp.Key
                };

                //CREAR IMAGEN
                Image img = new Image
                {
                    Stretch = Stretch.UniformToFill,
                    Style = (Style)FindResource("GameImageStyle") //APLICAR ESTILO
                };

                //CARGAR IMAGEN LOCAL O DESCARGAR
                BitmapImage bitmap = await GetGameImageAsync(kvp.Key, juego.imgVertical, "library_600x900");
                img.Source = bitmap;

                //CLIP PARA REDONDEAR BORDES
                img.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0, 0, border.Width, border.Height),
                    RadiusX = 12,
                    RadiusY = 12
                };

                border.Child = img;

                //EVENTO CLICK
                border.MouseLeftButtonUp += async (s, e) =>
                {
                    await PonerEnCabeceraAsync(juego, kvp.Key);
                };

                panelJuegos.Children.Add(border);
            }




        }

        //METODO PARA CAMBIAR LA CABECERA DE LA PAGINA
        private async Task PonerEnCabeceraAsync(Juego juego, string key)
        {
            try
            {

                await FadeOutAsync(gridCabecera, 0.3);
                //FONDO CABECERA
                BitmapImage fondo = await GetGameImageAsync(key, juego.imgCabecera, "library_hero");
                if (fondo != null)
                {
                    gridCabecera.Background = new ImageBrush(fondo)
                    {
                        Stretch = Stretch.UniformToFill,
                        AlignmentX = AlignmentX.Center,
                        AlignmentY = AlignmentY.Center
                    };
                }
                else
                    gridCabecera.Background = null;

                //LOGO
                BitmapImage logo = await GetGameImageAsync(key, juego.imgLogo, "logo");
                imgLogo.Source = logo;

                //PRECIOS
                percioNormal.Text = $" {juego.precioNormal} LC";
                percioDonador.Text = $" {juego.precioDonadores} LC";
                percioNormal.TextDecorations = juego.descuento > 0 ? TextDecorations.Strikethrough : null;

                await FadeInAsync(gridCabecera, 0.3);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al mostrar el juego: " + ex.Message);
            }
        }





        //EVENTO PARA CUANDO HAGA CLICK EN EL BOTON DE IR A DISCORD
        private void irADiscord(object sender, MouseButtonEventArgs e)
        {
            string url = "https://discord.com/channels/1399694698783703193/1405153983995187352";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true //NECESARIO EN .NET CORE / .NET 5+
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el enlace: " + ex.Message);
            }
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

        //FADE OUT: Disminuye Opacity de 1 a 0
        public static Task FadeOutAsync(UIElement element, double durationSeconds = 0.3)
        {
            if (element == null) return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();

            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = element.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                FillBehavior = FillBehavior.HoldEnd
            };

            fadeOut.Completed += (s, e) =>
            {
                element.Visibility = Visibility.Collapsed; // Ocultamos al terminar
                tcs.SetResult(true);
            };

            element.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            return tcs.Task;
        }


    }
}
