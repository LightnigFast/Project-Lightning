using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project_Lightning.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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
    /// Lógica de interacción para panelBiblioteca.xaml
    /// </summary>
    public partial class panelBiblioteca : Page
    {

        private string steamPath;
        private MainWindow _ventanaPrincipal;
        private NotificationManager notifier;
        public ObservableCollection<JuegoViewModel> Juegos { get; set; }

        private static readonly HttpClient httpClient = new HttpClient();


        public panelBiblioteca(MainWindow ventanaPrincipal)
        {
            InitializeComponent();
            this.DataContext = this;
            Juegos = new ObservableCollection<JuegoViewModel>();

            //INICIAR LAS NOTIFICAIONES
            _ventanaPrincipal = ventanaPrincipal;
            notifier = new Project_Lightning.Classes.NotificationManager(_ventanaPrincipal.NotificationCanvasPublic);


            CargarRutaSteam();

            //INICIALIZAR BASE DE DATOS
            InicializarBD();

            CargarBibliotecaConOverlay();

        }

        private async void CargarBibliotecaConOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Visible; // MOSTRAR OVERLAY
            StartSpinner();

            await Task.Delay(50); // PERMITE QUE LA UI PINTA EL OVERLAY ANTES DE EMPEZAR

            await CargarBiblioteca(steamPath); // LLAMADA AL MÉTODO ASYNC DE CARGA

            LoadingOverlay.Visibility = Visibility.Collapsed; // OCULTAR OVERLAY
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
            LoadingOverlay.Visibility = Visibility.Visible; // MOSTRAR OVERLAY
            StartSpinner();

            try
            {
                string pluginFolder = System.IO.Path.Combine(steamPath, "config", "stplug-in");
                string cacheFolder = System.IO.Path.Combine(steamPath, "appcache", "librarycache");

                if (!Directory.Exists(pluginFolder))
                {
                    notifier.Show("❌ The \"stplug-in\" directory was not found.", isError: true);
                    Juegos.Clear(); // Limpiar la colección
                    return;
                }

                if (!Directory.Exists(cacheFolder))
                {
                    notifier.Show("❌ The \"librarycache\" directory was not found.", isError: true);
                    Juegos.Clear();
                    return;
                }

                Juegos.Clear(); // Limpiar la colección antes de agregar nuevos elementos

                var luaFiles = Directory.GetFiles(pluginFolder, "*.lua");

                foreach (var luaFile in luaFiles)
                {
                    string appId = System.IO.Path.GetFileNameWithoutExtension(luaFile);

                    if (int.TryParse(appId, out int appIdInt))
                    {
                        if (!EstaEnBD(appIdInt))
                            await GuardarJuegosEnBD(luaFiles
                                .Select(f => int.TryParse(System.IO.Path.GetFileNameWithoutExtension(f), out int id) ? id : -1)
                                .Where(id => id > 0));

                    }

                    string appFolder = System.IO.Path.Combine(cacheFolder, appId);
                    if (!Directory.Exists(appFolder))
                    {
                        notifier.Show($"❌ The appid {appId} was not found, please restart Steam.", isError: true);
                        continue;
                    }

                    var subfolders = Directory.GetDirectories(appFolder, "*", SearchOption.AllDirectories)
                                              .Concat(new[] { appFolder });

                    foreach (var folder in subfolders)
                    {
                        string imagePath = System.IO.Path.Combine(folder, "library_600x900.jpg");
                        if (File.Exists(imagePath))
                        {
                            await Task.Run(() =>
                            {
                                BitmapImage bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                bitmap.Freeze();

                                Dispatcher.Invoke(() =>
                                {
                                    using (var conn = new SQLiteConnection(ConnectionString))
                                    {
                                        conn.Open();
                                        string sql = "SELECT MetacriticScore, MetacriticUrl, EdadPais FROM Juegos WHERE AppId=@AppId LIMIT 1;";
                                        using (var cmd = new SQLiteCommand(sql, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@AppId", appIdInt);
                                            using (var reader = cmd.ExecuteReader())
                                            {
                                                int? metacriticScore = null;
                                                string metacriticUrl = null;
                                                string edadPais = null;

                                                if (reader.Read())
                                                {
                                                    if (reader["MetacriticScore"] != DBNull.Value)
                                                        metacriticScore = Convert.ToInt32(reader["MetacriticScore"]);
                                                    metacriticUrl = reader["MetacriticUrl"] as string;
                                                    edadPais = reader["EdadPais"] as string;
                                                }

                                                Juegos.Add(new JuegoViewModel
                                                {
                                                    AppId = appIdInt,
                                                    Imagen = bitmap,
                                                    MetacriticScore = metacriticScore,
                                                    MetacriticUrl = metacriticUrl,
                                                    EdadPais = edadPais
                                                });
                                            }
                                        }
                                    }
                                });


                            });

                            break;
                        }
                    }
                }
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed; // OCULTAR OVERLAY
            }
        }


        public class JuegoViewModel : INotifyPropertyChanged
        {
            public int AppId { get; set; }
            public BitmapImage Imagen { get; set; }

            private int? metacriticScore;
            public int? MetacriticScore
            {
                get => metacriticScore;
                set { metacriticScore = value; OnPropertyChanged(nameof(MetacriticScore)); }
            }

            private string metacriticUrl;
            public string MetacriticUrl
            {
                get => metacriticUrl;
                set { metacriticUrl = value; OnPropertyChanged(nameof(MetacriticUrl)); }
            }

            private string edadPais;
            public string EdadPais
            {
                get => edadPais;
                set { edadPais = value; OnPropertyChanged(nameof(EdadPais)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }





        private void CargarRutaSteam()
        {
            string folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Project_Lightning");
            string configFile = System.IO.Path.Combine(folder, "config.json");

            if (File.Exists(configFile))
            {
                var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(configFile));
                if (config != null && config.ContainsKey("SteamPath"))
                    steamPath = config["SteamPath"];
            }
        }


        //METODO PARA CUANDO HACES CLICK EN UN JUEGO
        private void Juego_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image img && img.DataContext is JuegoViewModel juego)
            {
                string cacheFolder = System.IO.Path.Combine(steamPath, "appcache", "librarycache");
                string appFolder = System.IO.Path.Combine(cacheFolder, juego.AppId.ToString());

                if (!Directory.Exists(appFolder))
                    return;

                //BUSCAR DENTRO DE LAS CARPETAS
                var subfolders = Directory.GetDirectories(appFolder, "*", SearchOption.AllDirectories)
                                          .Concat(new[] { appFolder });

                foreach (var folder in subfolders)
                {
                    //PROBAR PRIMERO CON library_header.jpg
                    string headerPath = System.IO.Path.Combine(folder, "library_header.jpg");
                    if (!File.Exists(headerPath))
                        headerPath = System.IO.Path.Combine(folder, "header.jpg");

                    //PONER IMAGEN
                    if (File.Exists(headerPath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(headerPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        HeaderImage.Source = bitmap;


                        //PONER EL NOMBRE DEL JUEGO
                        using (var conn = new SQLiteConnection(ConnectionString))
                        {
                            conn.Open();
                            string sql = "SELECT Nombre FROM Juegos WHERE AppId=@AppId LIMIT 1;";
                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@AppId", juego.AppId);
                                var result = cmd.ExecuteScalar();
                                if (result != null)
                                {
                                    nombreJuego.Text = result.ToString(); // Asumiendo que HeaderGameName es tu TextBlock
                                }
                            }
                        }


                        break;
                    }

                    

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
            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);

            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Juegos (
                        AppId INTEGER PRIMARY KEY,
                        Nombre TEXT NOT NULL,
                        Windows BOOLEAN NOT NULL,
                        Mac BOOLEAN NOT NULL,
                        Linux BOOLEAN NOT NULL,
                        EdadPais TEXT,                  
                        MetacriticScore INTEGER,  
                        MetacriticUrl TEXT,  
                        FechaGuardado DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
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

                    if (appJson != null && appJson["success"].Value<bool>())
                    {
                        var data = appJson["data"];
                        string nombre = data["name"].Value<string>();
                        bool windows = data["platforms"]["windows"].Value<bool>();
                        bool mac = data["platforms"]["mac"].Value<bool>();
                        bool linux = data["platforms"]["linux"].Value<bool>();

                        //EDAD DEL PAÍS
                        string edadPais = null;
                        if (data["ratings"]?["agcom"] != null)
                        {
                            edadPais = data["ratings"]["agcom"]["rating"]?.Value<string>();
                        }

                        //METACRIT
                        int? metacriticScore = null;
                        string metacriticUrl = null;
                        if (data["metacritic"] != null)
                        {
                            metacriticScore = data["metacritic"]["score"]?.Value<int>();
                            metacriticUrl = data["metacritic"]["url"]?.Value<string>();
                        }

                        using (var conn = new SQLiteConnection(ConnectionString))
                        {
                            conn.Open();
                            string sql = @"
                    INSERT OR REPLACE INTO Juegos (AppId, Nombre, Windows, Mac, Linux, EdadPais, MetacriticScore, MetacriticUrl)
                    VALUES (@AppId, @Nombre, @Windows, @Mac, @Linux, @EdadPais, @MetacriticScore, @MetacriticUrl);";

                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@AppId", appId);
                                cmd.Parameters.AddWithValue("@Nombre", nombre);
                                cmd.Parameters.AddWithValue("@Windows", windows ? 1 : 0);
                                cmd.Parameters.AddWithValue("@Mac", mac ? 1 : 0);
                                cmd.Parameters.AddWithValue("@Linux", linux ? 1 : 0);
                                cmd.Parameters.AddWithValue("@EdadPais", (object)edadPais ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@MetacriticScore", (object)metacriticScore ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@MetacriticUrl", (object)metacriticUrl ?? DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    return; // ✅ éxito
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    Console.WriteLine($"Steam dijo 429 (Too Many Requests) para {appId}. Reintentando...");
                    await Task.Delay(2000); // espera antes de reintentar
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


    }
}
