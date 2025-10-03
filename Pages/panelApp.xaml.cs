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
    /// Lógica de interacción para panelUbisoft.xaml
    /// </summary>
    public partial class panelApp : Page
    {

        MainWindow ventanaPrincipal;
        string nombreApp;
        private Dictionary<string, Juego> juegosCargados = new Dictionary<string, Juego>();
        //CARPETA BASE PARA LAS IMÁGENES
        private readonly string gamesImagesFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Project_Lightning", "gamesImages");


        public panelApp(String nomApp, MainWindow mainWindow)
        {
            InitializeComponent();
            
            gridPrincipal.Visibility = Visibility.Hidden;

            //CAMBIO EL NOMBRE DE LA ETIQUETA
            txtApp.Text = nomApp;
            //CAMBIO DE COLOR DE LA ETIQUETA
            switch (nomApp)
            {
                case "UBISOFT": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A0DAD ")); break;
                case "EA": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f74545")); break;
                case "ROCKSTAR": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f7a600")); break;
                case "DENUVO": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#607D8B")); break;
                case "PlayStation": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0300b3")); break;
                case "OTHERS": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8BBD0")); break;

            }

            //INICIALIZO LA VENTANA PARA TENER UNA REFERENCIA DE LA VENTANA PRINCIPAL Y LA VARIABLE DE TEXTO
            ventanaPrincipal = mainWindow;
            nombreApp = nomApp;

            ponerJuegos(nomApp);

        }

        //CLASE JUEGO QUE CONTENDRA LA INFORMACIÓN DE CADA JUEGO
        public class Juego
        {
            public string name { get; set; }
            public bool launch_steam { get; set; }
            public bool launch_exe { get; set; }
            public string comentarios { get; set; }
            public List<string> programas_necesarios { get; set; }
            public List<string> errores { get; set; }
            public string nombre_fix { get; set; }
            public Dictionary<string, string> custom_images { get; set; }
        }


        //METODO PARA PRECARGAR LAS IMAGENES
        private async Task PreloadAllImages(Dictionary<string, Juego> todosLosJuegos)
        {
            var tasks = new List<Task>();

            foreach (var kvp in todosLosJuegos)
            {
                string appid = kvp.Key;
                Juego juego = kvp.Value;

                if (juego.custom_images != null)
                {
                    foreach (var img in juego.custom_images)
                    {
                        //img.Key = "hero_image", "background", etc
                        //img.Value = URL de la imagen
                        tasks.Add(GetGameImageAsync(appid, img.Value, img.Key));
                    }
                }
                else
                {
                    // OPCIONAL: descarga la imagen estándar de Steam
                    string urlDefault = $"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{appid}/library_600x900.jpg";
                    tasks.Add(GetGameImageAsync(appid, urlDefault, "library_600x900"));
                }
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


        private async void ponerJuegos(string nomApp)
        {
            //var juegosApp = await sacarJuegosDeApp(nomApp);
            //colocarBotones(juegosApp);
            juegosCargados = await sacarJuegosDeApp(nomApp); //GUARDAMOS LA LISTA ORIGINAL
            ventanaPrincipal.ocultarCabecera();
            //PRECARGO IMÁGENES (primera vez)
            overlayCarga.Visibility = Visibility.Visible;
            await PreloadAllImages(juegosCargados);
            await colocarBotones(juegosCargados);
            overlayCarga.Visibility = Visibility.Collapsed;
            ventanaPrincipal.mostrarCabecera();
            await FadeInAsync(gridPrincipal, 0.5);
            
            //descargarJuego(juegosApp.First());

        }


        //ESTE METODO BUSCA SACAR TODOS LOS JUEGOS DE UNA SOLA COMPAÑIA DADA POR EL nomApp
        private async Task<Dictionary<string, Juego>> sacarJuegosDeApp(string nomApp)
        {
            string rutaJson = System.IO.Path.GetFullPath(@"..\\..\\data.json");
            string rutaJsonApp = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json");
            string urlJson = "https://raw.githubusercontent.com/LightnigFast/Project-Lightning/main/data.json";

            if (await EsArchivoGitHubDiferente(urlJson, rutaJson) || await EsArchivoGitHubDiferente(urlJson, rutaJsonApp))
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string contenidoGitHub = await client.GetStringAsync(urlJson);
                        File.WriteAllText(rutaJson, contenidoGitHub); //ACTUALIZA ARCHIVO LOCAL
                        File.WriteAllText(rutaJsonApp, contenidoGitHub); //ACTUALIZA ARCHIVO LOCAL
                        //MessageBox.Show("Se ha actualizado el archivo data.json desde GitHub");
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Error al actualizar data.json: " + ex.Message);
                    var ventanaError = new Windows.ErrorDialog("Error updating the game list, please try again later: " + ex.Message, Brushes.Red);
                    ventanaError.ShowDialog();
                }
            }

            //CARGAR LOCAL
            string json = File.ReadAllText(rutaJsonApp);
            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Juego>>>(json);

            if (data.ContainsKey(nomApp))
            {
                return data[nomApp];
            }

            return new Dictionary<string, Juego>();
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


        //ESTE METODO BUSCA CREAR TODOS LOS BOTONES, COLCOAR SU IMAGEN Y SU RESPECTIVO METODO DE CLICK
        private async Task colocarBotones(Dictionary<string, Juego> juegosApp)
        {
            panelJuegos.Children.Clear();

            if (juegosApp.Count != 0)
            {
                foreach (var juego in juegosApp)
                {
                    Button botonJuego = new Button
                    {
                        Width = 198,
                        Height = 298,
                        Margin = new Thickness(17),
                        Style = (Style)FindResource("Boton_juego")
                    };

                    Image imagenJuego = new Image
                    {
                        Width = 198,
                        Height = 298,
                        Stretch = Stretch.Fill
                    };

                    BitmapImage bitmap = null;

                    if (juego.Value.custom_images != null &&
                        juego.Value.custom_images.TryGetValue("hero_image", out string urlPersonalizada) &&
                        !string.IsNullOrWhiteSpace(urlPersonalizada))
                    {
                        //📂 GUARDAR Y CARGAR DESDE CACHE LOCAL
                        bitmap = await GetGameImageAsync(juego.Key, urlPersonalizada, "hero_image");
                    }
                    else
                    {
                        //📂 IMAGEN POR DEFECTO DE STEAM
                        string urlDefault = $"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{juego.Key}/library_600x900.jpg";
                        bitmap = await GetGameImageAsync(juego.Key, urlDefault, "library_600x900");
                    }

                    //SI EXISTE, LA ASIGNO
                    if (bitmap != null)
                    {
                        imagenJuego.Source = bitmap;
                    }
                    else
                    {
                        //⚠️ IMAGEN DE RESPALDO (capsule pequeña de Steam)
                        imagenJuego.Stretch = Stretch.Uniform;
                        imagenJuego.Source = new BitmapImage(new Uri(
                            $"https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/{juego.Key}/capsule_184x69.jpg"));
                    }

                    botonJuego.Content = imagenJuego;

                    botonJuego.Click += (sender, e) =>
                    {
                        ventanaPrincipal.framePrincipal.Navigate(new panelJuego(nombreApp, juego, this, ventanaPrincipal));
                    };

                    panelJuegos.Children.Add(botonJuego);
                }
            }
            else
            {
                //SI NO HAY JUEGOS, MENSAJE DE VACÍO
                TextBlock textBlock = new TextBlock
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = 40,
                    FontFamily = (FontFamily)this.Resources["FuenteJohnInclinada"],
                    Padding = new Thickness(20)
                };

                textBlock.Inlines.Add(new Run { Text = "THERE ARE NO GAMES ", Foreground = Brushes.White });
                textBlock.Inlines.Add(new Run { Text = "FOR NOW", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A0DAD")) });
                textBlock.Inlines.Add(new Run { Text = "🚧", Foreground = Brushes.Yellow });

                Binding binding = new Binding("ActualWidth") { Source = panelJuegos };
                textBlock.SetBinding(FrameworkElement.WidthProperty, binding);

                Border border = new Border
                {
                    Background = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Child = textBlock
                };

                panelJuegos.Children.Add(border);
            }
        }


        //EVENTO DEL BUSCADOR
        private async void txtBuscador_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = txtBuscador.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(filtro))
            {
                await colocarBotones(juegosCargados); //MOSTRAR TODOS
            }
            else
            {
                var filtrados = juegosCargados
                    .Where(j => j.Value.name != null && j.Value.name.ToLower().Contains(filtro))
                    .ToDictionary(j => j.Key, j => j.Value);

                await colocarBotones(filtrados);
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
