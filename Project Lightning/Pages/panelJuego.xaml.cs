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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Compression;
using Newtonsoft.Json;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
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
        private KeyValuePair<string, panelApp.Juego> juego;
        public panelJuego(string nombreApp, KeyValuePair<string, panelApp.Juego> juego, panelApp panelApp, MainWindow ventanaPrincipal)
        {
            InitializeComponent();

            this.nombreApp = nombreApp;
            this.panelApp = panelApp;
            this.ventanaPrincipal = ventanaPrincipal;
            this.juego = juego;

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

            //SACAR EL APARTADO DE COMENTARIOS
            txtComentarios.Text = juego.Value.comentarios;

        }


        //CLICK PARA EL BOTON DE FIXEAR
        private void fixButtonClick(object sender, RoutedEventArgs e)
        {
            descargarJuego(juego);
        }

        //CLICK DEL BOTON DE VOLVER
        private void volverClick(object sender, RoutedEventArgs e)
        {
            ventanaPrincipal.framePrincipal.Navigate(new panelApp(nombreApp, ventanaPrincipal));
        }




        //METODO PARA DESCARGAR LOS ARCHIVOS FIX DE UN JUEGO INTEPENDIENTE
        private async void descargarJuego(KeyValuePair<string, Juego> keyValuePair)
        {
            string appId = keyValuePair.Key;
            string user = "LightnigFast";
            string repo = "gamesFixes";
            string token = "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";

            string apiUrl = $"https://api.github.com/repos/{user}/{repo}/contents/{appId}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                string json = await client.GetStringAsync(apiUrl);
                var archivos = JsonConvert.DeserializeObject<List<ArchivoGitHub>>(json);

                string escritorio = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string carpetaDestino = System.IO.Path.Combine(escritorio, appId);
                Directory.CreateDirectory(carpetaDestino);

                string rutaZipExtraer = null;

                // DESCARGAR TODO
                foreach (var archivo in archivos)
                {
                    if (archivo.type == "file")
                    {
                        string rutaDestino = System.IO.Path.Combine(carpetaDestino, archivo.name);
                        byte[] datos = await client.GetByteArrayAsync(archivo.download_url);
                        File.WriteAllBytes(rutaDestino, datos);

                        // GUARDAR RUTA AL PRIMER ZIP ENCONTRADO PARA EXTRAERLO DESPUÉS
                        if (archivo.name.EndsWith(".zip") && rutaZipExtraer == null)
                            rutaZipExtraer = rutaDestino;
                    }
                }

                // EXTRAER ZIP SI EXISTE
                if (rutaZipExtraer != null)
                {
                    string carpetaTemporal = System.IO.Path.Combine(carpetaDestino, "temp_extraccion");

                    // Usamos SharpCompress para manejar archivos zip
                    using (var archive = ArchiveFactory.Open(rutaZipExtraer))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.IsDirectory)
                            {
                                string filePath = System.IO.Path.Combine(carpetaTemporal, entry.Key);
                                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath)); // Crear directorios si no existen

                                // Extraer el archivo
                                using (var fileStream = File.Create(filePath))
                                {
                                    entry.WriteTo(fileStream);  // Extrae el contenido del archivo
                                }
                            }
                        }
                    }

                    // MOVER CONTENIDO EXTRAÍDO A CARPETA FINAL
                    foreach (var archivoExtraido in Directory.GetFiles(carpetaTemporal, "*", SearchOption.AllDirectories))
                    {
                        string nombreRelativo = archivoExtraido.Substring(carpetaTemporal.Length + 1);
                        string rutaFinal = System.IO.Path.Combine(carpetaDestino, nombreRelativo);

                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(rutaFinal));
                        File.Move(archivoExtraido, rutaFinal);
                    }

                    // Eliminar archivos temporales
                    Directory.Delete(carpetaTemporal, true);
                    File.Delete(rutaZipExtraer);
                }

                MessageBox.Show("Descarga y extracción completadas: " + carpetaDestino);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }



        //CLASE AUXILIAR PARA DESERIALIZAR LA RESPUESTA DE GITHUB API
        public class ArchivoGitHub
        {
            public string name { get; set; }
            public string path { get; set; }
            public string type { get; set; }
            public string download_url { get; set; }
        }
    }
}
