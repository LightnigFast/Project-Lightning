using Newtonsoft.Json;
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

namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interacción para panelBiblioteca.xaml
    /// </summary>
    public partial class panelBiblioteca : Page
    {

        private string steamPath;

        public panelBiblioteca(MainWindow ventanaPrincipal)
        {
            InitializeComponent();

            CargarRutaSteam();

            CargarBiblioteca(steamPath);

        }




        // Método para cargar la biblioteca
        private void CargarBiblioteca(string steamPath)
        {
            string pluginFolder = System.IO.Path.Combine(steamPath, "config", "stplug-in");
            string cacheFolder = System.IO.Path.Combine(steamPath, "appcache", "librarycache");

            if (!Directory.Exists(pluginFolder) || !Directory.Exists(cacheFolder))
            {
                MessageBox.Show($"❌ No se encontro directorio ");
                return;
            }
                

            panelJuegos.Children.Clear();

            //ITERAR EN TODOS LOS ARCHIVOS LUA PARA SACAR TODOS LOS JUEGOS
            var luaFiles = Directory.GetFiles(pluginFolder, "*.lua");

            foreach (var luaFile in luaFiles)
            {
                string appId = System.IO.Path.GetFileNameWithoutExtension(luaFile);

                //JUNTO LA RUTA DONDE ESTAN TODAS LAS IMAGNES EN CACHE CON EL APPID PARA BUSCAR SOLO IMAGENES DE ESE APPID
                string appFolder = System.IO.Path.Combine(cacheFolder, appId);
                if (!Directory.Exists(appFolder))
                {
                    MessageBox.Show($"❌ No existe carpeta para el juego con appId: {appId}");
                    continue;
                }

                //BUSCAR DENTRO DE TODAS LAS SUBCARPETAS 
                var subfolders = Directory.GetDirectories(appFolder, "*", SearchOption.AllDirectories)
                                          .Concat(new[] { appFolder });

                //bool foundImage = false;

                foreach (var folder in subfolders)
                {
                    string imagePath = System.IO.Path.Combine(folder, "library_600x900.jpg");

                    if (File.Exists(imagePath))
                    {
                        //foundImage = true;

                        Image img = new Image
                        {
                            Width = 180,
                            Height = 270,
                            Margin = new Thickness(5),
                            Stretch = System.Windows.Media.Stretch.UniformToFill
                        };

                        try
                        {
                            img.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                            panelJuegos.Children.Add(img);
                        }
                        catch
                        {
                            //IGNORAR IMAGENES QUE NO SE PUEDEN CARGAR
                        }

                        break; //SI ENCUENTRO LA IMAMGEN, DEJO DE BUSCAR
                    }
                }

                /*
                //VERIFICACION
                if (!foundImage)
                {
                    MessageBox.Show($"❌ No se encontró la imagen para el juego con appId: {appId}");
                }
                else
                {
                    MessageBox.Show($"✅ Imagen cargada para el juego con appId: {appId}");
                }
                */
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


    }
}
