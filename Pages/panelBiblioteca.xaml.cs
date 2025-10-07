using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project_Lightning.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Content.Area._3D;
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

namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interacción para panelBiblioteca.xaml
    /// </summary>
    public partial class panelBiblioteca : Page
    {

        private string steamPath;
        private MainWindow _ventanaPrincipal;
        private NotificationManager notifier;
        private bool steamEncontrado = false;
        public ObservableCollection<JuegoViewModel> Juegos { get; set; }

        private static readonly HttpClient httpClient = new HttpClient();

        private DispatcherTimer timer;

        public panelBiblioteca(MainWindow ventanaPrincipal)
        {
            InitializeComponent();

            //CARGAR RELOJ
            cargarReloj();

            this.DataContext = this;
            Juegos = new ObservableCollection<JuegoViewModel>();
            juegosView = CollectionViewSource.GetDefaultView(Juegos);
            juegosView.Filter = FiltroJuegos;

            //INICIAR LAS NOTIFICAIONES
            _ventanaPrincipal = ventanaPrincipal;
            notifier = new Project_Lightning.Classes.NotificationManager(_ventanaPrincipal.NotificationCanvasPublic);

            //PONER QUE EL PLACEHOLDER DESAPAREZCA AL ESCRIBIR ALGO
            AppIdTextBox.TextChanged += (s, e) =>
            {
                PlaceholderText.Visibility = string.IsNullOrEmpty(AppIdTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            };


            CargarRutaSteam();

            if (steamEncontrado)
            {
                //INICIALIZAR BASE DE DATOS
                InicializarBD();

                CargarBibliotecaConOverlay();
            }
            

            

        }

        

        private async void CargarBibliotecaConOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Visible; //MOSTRAR OVERLAY
            juegoIndependiente.Opacity = 0;
            StartSpinner();

            await Task.Delay(50); //PERMITE QUE LA UI PINTA EL OVERLAY ANTES DE EMPEZAR

            await CargarBiblioteca(steamPath); //LLAMADA AL MÉTODO ASYNC DE CARGA

            LoadingOverlay.Visibility = Visibility.Collapsed; //OCULTAR OVERLAY
        }


        private void StartSpinner()
        {
            DoubleAnimation rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            };
            SpinnerRotate.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        }


        //METODO PARA CARGAR LA BIBLIOTECA
        private async Task CargarBiblioteca(string steamPath)
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            StartSpinner();

            try
            {
                string pluginFolder = System.IO.Path.Combine(steamPath, "config", "stplug-in");
                string cacheFolder = System.IO.Path.Combine(steamPath, "appcache", "librarycache");
                string lightningTools = System.IO.Path.Combine(steamPath, "hid.dll");

                if (!File.Exists(lightningTools) || !Directory.Exists(pluginFolder) || !Directory.Exists(cacheFolder))
                {
                    //Si falta alguno de los elementos, llamo a MostrarError y regreso
                    if (!File.Exists(lightningTools))
                    {
                        MostrarError("❌ \"LightningTools\" is not installed.", "This error occurs because LightningTools is not installed. Please go to Settings and install it before using the library.");
                    }
                    else if (!Directory.Exists(pluginFolder))
                    {
                        MostrarError("❌ The \"stplug-in\" directory was not found.", "Before using the library, you must install LightningTools. Go to the settings and install it.");
                    }
                    else
                    {
                        MostrarError("❌ The \"librarycache\" directory was not found.", "This error occurs due to a faulty Steam installation or because you haven’t logged in yet.\nIf you haven’t signed in to your account, please do so, and if it still doesn’t work, reinstall Steam.");
                    }
                    return;
                }

                //Obtengo todos los AppIds de los archivos .lua
                var luaFiles = Directory.GetFiles(pluginFolder, "*.lua");
                var todosAppIds = luaFiles
                                    .Select(f => int.TryParse(System.IO.Path.GetFileNameWithoutExtension(f), out int id) ? id : -1)
                                    .Where(id => id > 0)
                                    .ToList();


                //ACTUALIZAO LA BD
                //Identifico solo los IDs que NO están en la BD para evitar trabajo innecesario.
                var appIdsNuevos = todosAppIds.Where(id => !EstaEnBD(id)).ToList();

                if (appIdsNuevos.Any())
                {
                    await GuardarJuegosEnBD(appIdsNuevos);
                }

                //CARGO LOS JUEGOS EN WRAPPANEL
                Juegos.Clear();
                var juegosTemp = new List<JuegoViewModel>();

                await Task.Run(() =>
                {
                    
                    foreach (var appIdInt in todosAppIds)
                    {
                        string appId = appIdInt.ToString();

                        string appFolder = System.IO.Path.Combine(cacheFolder, appId);

                        if (!Directory.Exists(appFolder))
                        {
                            Dispatcher.Invoke(() =>
                                 notifier.Show($"❌ The appid {appId} was not found, please restart Steam.", isError: true, 4000));
                            continue;
                        }

                        var subfolders = Directory.GetDirectories(appFolder, "*", SearchOption.AllDirectories)
                                                    .Concat(new[] { appFolder });

                        foreach (var folder in subfolders)
                        {
                            string imagePath = System.IO.Path.Combine(folder, "library_600x900.jpg");

                            if (!File.Exists(imagePath))
                            {
                                imagePath = System.IO.Path.Combine(folder, "library_capsule.jpg");

                                if (!File.Exists(imagePath))
                                    continue;
                            }

                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.DecodePixelWidth = 110;
                            bitmap.DecodePixelHeight = 165;
                            bitmap.EndInit();
                            bitmap.Freeze();

                            //Leo Metacritic desde la BD
                            int? metacriticScore = ObtenerMetacriticDesdeBD(appIdInt);

                            juegosTemp.Add(new JuegoViewModel
                            {
                                AppId = appIdInt,
                                Imagen = bitmap,
                                MetacriticScore = metacriticScore
                            });

                            break; //Imagen encontrada, pasa al siguiente juego
                        }
                    }
                });

                //Agrego todos los juegos de la lista temporal a la ObservableCollection
                foreach (var juego in juegosTemp)
                    Juegos.Add(juego);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void MostrarError(string titulo, string mensaje)
        {
            notifier.Show(titulo, isError: true);
            gridAñadirJuego.Visibility = Visibility.Collapsed;
            componenteArrastrar.Visibility = Visibility.Collapsed;
            Juegos.Clear();
            Juegos.Add(new JuegoViewModel
            {
                AppId = -1,
                Imagen = null,
                MetacriticScore = null,
                NombreError = mensaje
            });
        }



        public class JuegoViewModel
        {
            public int AppId { get; set; }
            public BitmapImage Imagen { get; set; }
            public int? MetacriticScore { get; set; }

            // NUEVO: Mensaje de error opcional
            public string NombreError { get; set; }
        }






        private void CargarRutaSteam()
        {
            string folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Project_Lightning");
            string configFile = System.IO.Path.Combine(folder, "config.json");

            if (File.Exists(configFile))
            {
                steamEncontrado = true;
                var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(configFile));
                if (config != null && config.ContainsKey("SteamPath"))
                    steamPath = config["SteamPath"];
            }
            else
            {
                MostrarError("❌ Steam folder not found.", "This error occurs because you haven't specified the correct path to Steam. \n Go to Settings and enter the path where Steam is installed.\r\n");
            }
        }


        //METODO PARA CUANDO HACES CLICK EN UN JUEGO
        private void Juego_Click(object sender, MouseButtonEventArgs e)
        {
            //PONGO TODA LA BARRA DE CADA JUEGO INDEPENDIENTE VISIBLE
            //metacriticGrid.Visibility = Visibility.Visible;
            juegoIndependiente.Opacity = 1;

            if (sender is Image img && img.DataContext is JuegoViewModel juego)
            {
                string cacheFolder = System.IO.Path.Combine(steamPath, "appcache", "librarycache");
                string appFolder = System.IO.Path.Combine(cacheFolder, juego.AppId.ToString());

                if (!Directory.Exists(appFolder))
                    return;

                // BUSCAR DENTRO DE LAS CARPETAS
                var subfolders = Directory.GetDirectories(appFolder, "*", SearchOption.AllDirectories)
                                          .Concat(new[] { appFolder });

                foreach (var folder in subfolders)
                {
                    string headerPath = System.IO.Path.Combine(folder, "library_header.jpg");
                    if (!File.Exists(headerPath))
                        headerPath = System.IO.Path.Combine(folder, "header.jpg");

                    if (!File.Exists(headerPath))
                        headerPath = System.IO.Path.Combine(folder, "library_600x900.jpg");

                    if (File.Exists(headerPath))
                    {
                        // CARGAR IMAGEN
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(headerPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        HeaderImage.Source = bitmap;

                        // LEER DE LA BD
                        using (var conn = new SQLiteConnection(ConnectionString))
                        {
                            conn.Open();
                            string sql = "SELECT Nombre, MetacriticScore, EdadMinima, Windows, Mac, Linux FROM Juegos WHERE AppId=@AppId LIMIT 1;";
                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@AppId", juego.AppId);
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        // NOMBRE
                                        nombreJuego.Text = reader["Nombre"].ToString();

                                        // METACRITIC
                                        if (reader["MetacriticScore"] != DBNull.Value)
                                        {
                                            int score = Convert.ToInt32(reader["MetacriticScore"]);
                                            SetMetacriticScore(score);
                                        }
                                        else
                                        {
                                            metacriticScore.Text = "N/A";
                                            ScoreArc.Data = null;
                                        }

                                        // PEGI
                                        if (reader["EdadMinima"] != DBNull.Value)
                                            SetEdadMinima(Convert.ToInt32(reader["EdadMinima"]));
                                        else
                                            SetEdadMinima(null);

                                        // PLATAFORMAS
                                        bool windows = Convert.ToBoolean(reader["Windows"]);
                                        bool mac = Convert.ToBoolean(reader["Mac"]);
                                        bool linux = Convert.ToBoolean(reader["Linux"]);
                                        SetPlataformas(windows, mac, linux);

                                        //PONER TAG AL BOTON DE ELIMINMAR
                                        btnEliminar.Tag = juego.AppId;
                                    }
                                    else
                                    {
                                        nombreJuego.Text = "Desconocido";
                                        metacriticScore.Text = "N/A";
                                        ScoreArc.Data = null;
                                        edadMinimaText.Text = "N/A";
                                        plataformasPanel.Children.Clear();
                                    }
                                }
                            }
                        }
                        FadeInAsync(HeaderImage, 0.7);
                        FadeInAsync(nombreJuego, 0.7);
                        FadeInAsync(metacriticGrid, 0.7);

                        break;
                    }
                }
            }
        }



        //METODO PARA ELIMINAR UN JUEGO DE LA BIBLIOTECA
        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {

            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int appId))
            {
                try
                {
                    string pluginFolder = System.IO.Path.Combine(steamPath, "config", "stplug-in");
                    string luaPath = System.IO.Path.Combine(pluginFolder, $"{appId}.lua");

                    if (File.Exists(luaPath))
                    {
                        File.Delete(luaPath);
                        notifier.Show($"✅ Game {appId} deleted successfully.", isError: false);

                        // ELIMINAR DE LA LISTA VISIBLE
                        var juego = Juegos.FirstOrDefault(j => j.AppId == appId);
                        if (juego != null)
                            Juegos.Remove(juego);

                        txtBuscar.Text = "";
                        CargarBibliotecaConOverlay();


                    }
                    else
                    {
                        notifier.Show($"⚠️ File for {appId} not found in stplug-in.", isError: true);
                    }
                }
                catch (Exception ex)
                {
                    notifier.Show($"❌ Error deleting {appId}: {ex.Message}", isError: true);
                }
            }
        }








        //PARA LA BASE DE DATOS DE JUEGOS DEL USER

        //RUTA BASE DEL PROYECTO
        private static readonly string BasePath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Project_Lightning"
        );

        //RUTA DE LA BD
        private static readonly string DbPath = System.IO.Path.Combine(BasePath, "steam_games.db");

        //CADENA DE CONEXIÓN
        private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

        //CREAR LA CARPETA SI NO EXISTE
        private void InicializarBD()
        {
            //SI NO EXISTE EL DIRECTORIO BASE, LO CREAMOS
            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);

            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();

                //CREAMOS LA TABLA JUEGOS SI NO EXISTE, ahora con MetacriticScore
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Juegos (
                        AppId INTEGER PRIMARY KEY,
                        Nombre TEXT NOT NULL,
                        Windows BOOLEAN NOT NULL,
                        Mac BOOLEAN NOT NULL,
                        Linux BOOLEAN NOT NULL,
                        FechaGuardado DATETIME DEFAULT CURRENT_TIMESTAMP,
                        MetacriticScore INTEGER,
                        EdadMinima INTEGER
                    );";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery(); //EJECUTAMOS LA CONSULTA DE CREACIÓN
                }
            }
        }





        private async Task GuardarJuegoEnBD(int appId, int intentos = 3)
        {
            for (int i = 0; i < intentos; i++)
            {
                try
                {
                    string url = $"https://store.steampowered.com/api/appdetails?appids={appId}";
                    var response = await httpClient.GetStringAsync(url);
                    var json = JObject.Parse(response);

                    var appJson = json[appId.ToString()];

                    // 1. Verificar éxito y que el nodo 'data' existe.
                    if (appJson != null &&
                        appJson["success"]?.Value<bool>() == true)
                    {
                        var data = appJson["data"] as JObject; // Intentar obtener 'data' como un JObject

                        if (data == null) // Si 'data' no existe o no es un objeto, salimos.
                        {
                            Console.WriteLine($"Advertencia: 'data' no encontrado o inválido para AppId: {appId}");
                            return;
                        }

                        // 2. Extracción de Datos: Usar el operador de navegación segura (?.) y coalescencia (??)

                        string nombre = data["name"]?.Value<string>() ?? $"App Desconocida ({appId})";

                        // Plataformas: Acceso seguro con ?., usando ?? false si el nodo no existe.
                        bool windows = data["platforms"]?["windows"]?.Value<bool>() ?? false;
                        bool mac = data["platforms"]?["mac"]?.Value<bool>() ?? false;
                        bool linux = data["platforms"]?["linux"]?.Value<bool>() ?? false;

                        // PARA LA NOTA DE METACRITIC
                        int? metacriticScore = null;
                        // Acceso seguro: data["metacritic"]?["score"]
                        if (data["metacritic"]?["score"] != null)
                            metacriticScore = data["metacritic"]["score"].Value<int>();


                        // PARA LA EDAD DEL JUEGO
                        int? edadMinima = null;

                        var ratings = data["ratings"];
                        if (ratings != null)
                        {
                            // Primero intenta 'steam_germany'
                            JToken ratingToken = ratings["steam_germany"];

                            // Si no está, intenta obtener el primer rating disponible.
                            if (ratingToken == null && ratings.HasValues)
                            {
                                var firstProp = ratings.First as JProperty;
                                if (firstProp != null)
                                    ratingToken = firstProp.Value;
                            }

                            // Asegurarse de que el token es un objeto (o tiene propiedades accsesibles)
                            if (ratingToken is JToken token)
                            {
                                // Acceso seguro: token["required_age"]?.Value<string>()
                                string ageStr = token["required_age"]?.Value<string>()
                                                    ?? token["rating"]?.Value<string>();

                                if (!string.IsNullOrEmpty(ageStr))
                                {
                                    // QUITAMOS CUALQUIER CARACTER NO NUMÉRICO
                                    ageStr = new string(ageStr.Where(char.IsDigit).ToArray());

                                    if (int.TryParse(ageStr, out int age))
                                        edadMinima = age;
                                }
                            }
                        }

                        // 3. Guardar en Base de Datos (sin cambios, ya que los valores ya son seguros)
                        using (var conn = new SQLiteConnection(ConnectionString))
                        {
                            conn.Open();

                            string sql = @"
                        INSERT OR REPLACE INTO Juegos 
                        (AppId, Nombre, Windows, Mac, Linux, MetacriticScore, EdadMinima) 
                        VALUES (@AppId, @Nombre, @Windows, @Mac, @Linux, @MetacriticScore, @EdadMinima);";

                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@AppId", appId);
                                cmd.Parameters.AddWithValue("@Nombre", nombre);
                                cmd.Parameters.AddWithValue("@Windows", windows ? 1 : 0);
                                cmd.Parameters.AddWithValue("@Mac", mac ? 1 : 0);
                                cmd.Parameters.AddWithValue("@Linux", linux ? 1 : 0);
                                cmd.Parameters.AddWithValue("@MetacriticScore", (object)metacriticScore ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@EdadMinima", (object)edadMinima ?? DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }

                        }
                    }
                    else
                    {
                        // Si 'success' es false o appJson es null, no es un error de código, sino de datos de Steam.
                        Console.WriteLine($"Advertencia: Steam no devolvió datos exitosos para AppId: {appId}");
                    }

                    return; // ✅ éxito o manejo de advertencia.
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    Console.WriteLine($"Steam dijo 429 (Too Many Requests) para {appId}. Reintentando...");
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    // Capturar cualquier otro error inesperado (como un JSON que no se puede parsear bien)
                    Console.WriteLine($"Error general al procesar AppId {appId}: {ex.Message}");
                    return; // Evita el bucle si es un error de parsing o datos no recuperable.
                }
            }
        }


        //METODO PARA VARIOS RETRY
        private async Task GuardarJuegosEnBD(IEnumerable<int> appIds)
        {
            var semaphore = new SemaphoreSlim(5); //máximo 5 peticiones simultáneas
            var tasks = new List<Task>();

            foreach (var appId in appIds)
            {
                if (EstaEnBD(appId))
                    continue;

                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await GuardarJuegoEnBD(appId);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }


        //METODO PARA COMPROBAR SI EL JUEGO ESTA EN LA BD
        private bool EstaEnBD(int appId)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT 1 FROM Juegos WHERE AppId=@AppId LIMIT 1;";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppId", appId);
                    var result = cmd.ExecuteScalar();
                    return result != null;
                }
            }
        }

        







        //METODO PARA SACAR EL METASCORE THE LA BD
        private int? ObtenerMetacriticDesdeBD(int appId)
        {
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();

                string sql = "SELECT MetacriticScore FROM Juegos WHERE AppId = @AppId;";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppId", appId);

                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                }
            }

            return null;
        }


        //CAMBIAR EL COLOR DEL METACRITIC
        private void SetMetacriticScore(int score)
        {
            metacriticScore.Text = score.ToString();

            // DEFINIR COLOR SEGÚN EL SCORE
            SolidColorBrush color;
            if (score >= 75)
                color = new SolidColorBrush(Colors.LightGreen);
            else if (score >= 50)
                color = new SolidColorBrush(Colors.Gold);
            else
                color = new SolidColorBrush(Colors.OrangeRed);

            ScoreArc.Stroke = color;

            // DIBUJAR EL ARCO
            double angle = score * 360.0 / 100.0; // Ángulo proporcional al score
            double radius = 26; // RADIO AJUSTADO (60/2 - StrokeThickness/2)
            double centerX = 30; // Mitad del ancho
            double centerY = 30; // Mitad del alto

            double radians = (Math.PI / 180) * (angle - 90); // -90 para empezar arriba
            double x = centerX + radius * Math.Cos(radians);
            double y = centerY + radius * Math.Sin(radians);

            bool isLargeArc = angle > 180;

            PathFigure figure = new PathFigure();
            figure.StartPoint = new Point(centerX, centerY - radius); // Punto arriba
            ArcSegment arc = new ArcSegment();
            arc.Point = new Point(x, y);
            arc.Size = new Size(radius, radius);
            arc.SweepDirection = SweepDirection.Clockwise;
            arc.IsLargeArc = isLargeArc;

            figure.Segments.Clear();
            figure.Segments.Add(arc);

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            ScoreArc.Data = geometry;
        }


        //CAMBIAR EL COLOR DE LA EDAD MINIMA
        private void SetEdadMinima(int? edad)
        {
            if (edad.HasValue)
            {
                //VALORES OFICIALES PEGI
                int[] pegis = { 3, 7, 12, 16, 18 };

                //SI LLEGA +6 O +17 → BUSCAMOS EL MÁS CERCANO
                int edadFinal = pegis.OrderBy(p => Math.Abs(p - edad.Value)).First();

                edadMinimaText.Text = $"+{edadFinal}";

                string hexColor;
                switch (edadFinal)
                {
                    case 3:
                        hexColor = "#9aca3c";
                        break;
                    case 7:
                        hexColor = "#9aca3c";
                        break;
                    case 12:
                        hexColor = "#f6a31c";
                        break;
                    case 16:
                        hexColor = "#f3a700";
                        break;
                    case 18:
                        hexColor = "#e3021e";
                        break;
                    default:
                        hexColor = "#808080"; 
                        break;
                }

                //FONDO DEL CONTENEDOR
                fondoPegi.Background = (Brush)new BrushConverter().ConvertFrom(hexColor);

                //TEXTO EN BLANCO PARA CONTRASTE
                edadMinimaText.Foreground = Brushes.White;
            }
            else
            {
                edadMinimaText.Text = "N/A";
                fondoPegi.Background = (Brush)new BrushConverter().ConvertFrom("#808080"); //GRIS
                edadMinimaText.Foreground = Brushes.White;
            }
        }


        //PARA PONER LOS BOTONES DE LAS PLATAFORMAS
        private void SetPlataformas(bool windows, bool mac, bool linux)
        {
            plataformasPanel.Children.Clear();

            if (windows)
                plataformasPanel.Children.Add(CreatePlataformaIcon("pack://application:,,,/res/icons/windows.png", "Windows"));
            if (mac)
                plataformasPanel.Children.Add(CreatePlataformaIcon("pack://application:,,,/res/icons/mac.png", "MacOS"));
            if (linux)
                plataformasPanel.Children.Add(CreatePlataformaIcon("pack://application:,,,/res/icons/linux.png", "Linux"));
        }
        //PONER EL ICONO DE CADA PLATAFORMA
        private Border CreatePlataformaIcon(string iconPath, string tooltip)
        {
            var img = new Image
            {
                Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute)),
                Width = 24,
                Height = 24,
                Stretch = Stretch.Uniform
            };

            return new Border
            {
                Background = (Brush)new BrushConverter().ConvertFrom("#FF1E1E1E"),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(5),
                Margin = new Thickness(3, 0, 0, 0),
                Child = img,
                ToolTip = tooltip
            };
        }









        //PARTE PARA EL RELOJ
        private void cargarReloj()
        {
            //ACTUALIZAMOS EL RELOJ INMEDIATAMENTE
            Timer_Tick();

            //ANIMACIÓN DE APARICIÓN (FADE-IN)
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                FillBehavior = System.Windows.Media.Animation.FillBehavior.HoldEnd
            };
            RelojMinimalista.BeginAnimation(OpacityProperty, fadeIn);

            //CREAR Y CONFIGURAR EL TIMER
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => Timer_Tick();
            timer.Start();
        }


        //ACTUALIZA LA HORA CADA SEGUNDO
        private void Timer_Tick()
        {
            RelojMinimalista.Text = DateTime.Now.ToString("HH:mm");
        }











        //PARTE PARA LA DESCARGA DE APPIDS
        private readonly string[] REPOS = Repos.GetRectangle3D();


        private async Task BuscarYAgregarAppIdAsync(string appId)
        {
            string pluginFolder = System.IO.Path.Combine(steamPath, "config", "stplug-in");
            string depotCacheFolder = System.IO.Path.Combine(steamPath, "config", "depotcache");

            Directory.CreateDirectory(pluginFolder);
            Directory.CreateDirectory(depotCacheFolder);

            bool encontrado = false;

            // Revisar cada repositorio
            foreach (var repo in REPOS)
            {
                string zipUrl = $"{repo}/archive/refs/heads/{appId}.zip";

                try
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(zipUrl);
                        if (!response.IsSuccessStatusCode)
                            continue;

                        // Descarga el ZIP en memoria
                        using (var ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync()))
                        {
                            using (var zip = new System.IO.Compression.ZipArchive(ms))
                            {
                                foreach (var entry in zip.Entries)
                                {
                                    string fileName = System.IO.Path.GetFileName(entry.FullName);

                                    if (string.IsNullOrEmpty(fileName))
                                        continue;

                                    // REPO 2 y 3 -> SOLO LUA y filtrar líneas con "addappid"
                                    if ((repo.Contains("sojorepo") || repo.Contains("SteamAutoCracks")) && fileName.EndsWith(".lua"))
                                    {
                                        string destino = System.IO.Path.Combine(pluginFolder, fileName);

                                        using (var entryStream = entry.Open())
                                        using (var reader = new StreamReader(entryStream))
                                        using (var writer = new StreamWriter(destino, false)) // sobrescribe si existe
                                        {
                                            while (!reader.EndOfStream)
                                            {
                                                string line = reader.ReadLine();
                                                if (line.TrimStart().StartsWith("addappid"))
                                                {
                                                    writer.WriteLine(line);
                                                }
                                            }
                                        }
                                    }

                                    // REPO 3 -> ARCHIVOS CON MANIFEST
                                    else if (repo.Contains("ProjectLightningManifests") && (fileName.EndsWith(".lua") || fileName.EndsWith(".manifest")))
                                    {
                                        string destino = fileName.EndsWith(".lua")
                                            ? System.IO.Path.Combine(pluginFolder, fileName)
                                            : System.IO.Path.Combine(depotCacheFolder, fileName);

                                        using (var entryStream = entry.Open())
                                        {
                                            using (var fileStream = File.Create(destino))
                                            {
                                                await entryStream.CopyToAsync(fileStream);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    encontrado = true;
                    break; //NO HACE FALTA SEGUIR BUSCANDO
                }
                catch
                {
                    continue; //INTENTAMOS EL SIGUIENTE REPO
                }
            }

            if (encontrado)
                notifier.Show("✅ Game added successfully, restart Steam and then this library to be able to see the game.", isError: false, 4000);
                //MessageBox.Show($"✅ AppID {appId} agregado correctamente");
            else
                notifier.Show("❌ Appid not found, you'll have to wait a little longer until I add it.", isError: true, 4000);
                //MessageBox.Show($"❌ AppID {appId} no se encontró en los repositorios");
        }


        //EVETO DEL BOTON DE AGREGAR
        private async void AgregarAppId_Click(object sender, RoutedEventArgs e)
        {
            string appId = AppIdTextBox.Text.Trim();
            if (string.IsNullOrEmpty(appId))
            {
                notifier.Show("❌ You need to write an appid.", isError: true);
                //MessageBox.Show("Introduce un AppID válido");
                return;
            }

            LoadingOverlay.Visibility = Visibility.Visible;
            await BuscarYAgregarAppIdAsync(appId);
            LoadingOverlay.Visibility = Visibility.Hidden;

            BibliotecaButtonClick();


        }

        //RECARGAR LA INTERFAZ DE LA BIBLIOTECA
        private void BibliotecaButtonClick()
        {
            var ventanaPrincipal = Application.Current.MainWindow as MainWindow;

            if (ventanaPrincipal != null)
            {

                //CAMBIAR EL CONTENIDO DEL FRAME
                ventanaPrincipal.framePrincipal.Navigate(new panelBiblioteca(ventanaPrincipal));
            }

        }


        //BOTON PARA REINICIAR STEAM
        private void ReiniciarSteam(object sender, RoutedEventArgs e)
        {
            try
            {
                //CERRAR PROCESOS DE STEAM SI EXISTEN
                var procesos = Process.GetProcessesByName("steam");
                foreach (var p in procesos)
                {
                    try { p.Kill(); }
                    catch { }
                }

                //ESPERAR A QUE SE CIERREN
                foreach (var p in procesos)
                {
                    try { p.WaitForExit(5000); } //ESPERA 5 SEGUNDOS
                    catch { }
                }

                Thread.Sleep(1000); //UN PEQUEÑO DELAY EXTRA

                //INICIAR STEAM DE NUEVO EN SEGUNDO PLANO
                Process.Start("steam://open/main");
            }
            catch
            {
                //SI FALLA, ABRIR NORMAL
                Process.Start("steam://open/main");
            }
        }


        //BOTON PARA ABRIR STEAMDB
        private void AbrirSteamDB(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://steamdb.info/",
                    UseShellExecute = true 
                });
            }
            catch (Exception ex)
            {
                notifier.Show($"❌ Error opening SteamDB: {ex.Message}");
            }
        }









        //PARTE PARA ARRASTRAR Y SOLTAR ARCHIVOS
        private void DropArea_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;

                //RESALTAR EL BORDER AL ARRASTRAR
                if (sender is Border border)
                    border.BorderBrush = Brushes.DeepSkyBlue;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DropArea_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border border)
                border.BorderBrush = Brushes.Gray;
        }

        private void DropArea_Drop(object sender, DragEventArgs e)
        {
            if (sender is Border border)
                border.BorderBrush = Brushes.Gray;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                //RUTAS DE DESTINO
                string pluginFolder = System.IO.Path.Combine(steamPath, "config", "stplug-in");
                string depotCacheFolder = System.IO.Path.Combine(steamPath, "config", "depotcache");

                //ASEGURARSE DE QUE EXISTEN
                Directory.CreateDirectory(pluginFolder);
                Directory.CreateDirectory(depotCacheFolder);

                //PARA COMRPOBAR SI NO HA HABIDO ERRORES
                bool allCopied = true;

                foreach (var file in files)
                {
                    string ext = System.IO.Path.GetExtension(file).ToLower();
                    string fileName = System.IO.Path.GetFileName(file);

                    try
                    {
                        if (ext == ".lua")
                        {
                            string destPath = System.IO.Path.Combine(pluginFolder, fileName);
                            File.Copy(file, destPath, overwrite: true);
                            //MessageBox.Show($"Archivo .lua copiado a:\n{destPath}");
                        }
                        else if (ext == ".manifest")
                        {
                            string destPath = System.IO.Path.Combine(depotCacheFolder, fileName);
                            File.Copy(file, destPath, overwrite: true);
                            //MessageBox.Show($"Archivo .manifest copiado a:\n{destPath}");
                        }
                        else if (ext == ".zip")
                        {
                            bool hasLua = false;
                            bool hasManifest = false;

                            using (ZipArchive archive = ZipFile.OpenRead(file))
                            {
                                foreach (var entry in archive.Entries)
                                {
                                    string entryExt = System.IO.Path.GetExtension(entry.FullName).ToLower();
                                    string entryName = System.IO.Path.GetFileName(entry.FullName);

                                    if (string.IsNullOrEmpty(entryExt)) continue; //IGNORAR CARPETAS VACÍAS

                                    if (entryExt == ".lua")
                                    {
                                        string destPath = System.IO.Path.Combine(pluginFolder, entryName);
                                        entry.ExtractToFile(destPath, overwrite: true);
                                        hasLua = true;
                                    }
                                    else if (entryExt == ".manifest")
                                    {
                                        string destPath = System.IO.Path.Combine(depotCacheFolder, entryName);
                                        entry.ExtractToFile(destPath, overwrite: true);
                                        hasManifest = true;
                                    }
                                }
                            }

                            //👉 Solo mostrar error si no había ninguno de los dos
                            if (!hasLua && !hasManifest)
                            {
                                MessageBox.Show("El ZIP no contiene ni .lua ni .manifest");
                                allCopied = false;
                            }
                        }
                        else
                        {
                            //IGNORAR OTROS ARCHIVOS
                            //MessageBox.Show($"Archivo ignorado: {fileName}");
                            allCopied = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        allCopied = false;
                        notifier.Show($"Error copying {fileName}:\n{ex.Message}", isError: true, 3000);
                    }
                }

                //SOLO LO MUESTRO SI NO HAY ERRORES
                if (allCopied)
                {
                    notifier.Show("✅ Game added successfully, restart Steam and then this library to be able to see the game.", isError: false, 4000);

                    BibliotecaButtonClick();
                }
            }
        }









        //PARTE PARA EL BUSCADOR DE JUEGOS DENTRO DE LA LIBRERIA

        private ICollectionView juegosView;
        public ICollectionView JuegosView
        {
            get { return juegosView; }
        }


        private bool FiltroJuegos(object item)
        {
            if (string.IsNullOrWhiteSpace(txtBuscar.Text))
                return true;

            var juego = item as JuegoViewModel;

            // OBTENEMOS EL NOMBRE DESDE LA BD
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT Nombre FROM Juegos WHERE AppId=@AppId LIMIT 1;";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppId", juego.AppId);
                    var nombre = cmd.ExecuteScalar()?.ToString() ?? string.Empty;
                    return nombre.IndexOf(txtBuscar.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
        }


        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            juegosView.Refresh();


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
