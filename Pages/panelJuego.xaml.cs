using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TokenProvider;
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

            progressBar.Visibility = Visibility.Collapsed;
            crearBackground(juego.Key);
            crearLogo(juego.Key);
            sacarInfoJuego(juego);


        }


        private void crearLogo(string key)
        {
            //CREO LA IMAGEN QUE IRA EN CADA BOTON
            Image imagenJuego = new Image
            {
                Stretch = Stretch.Uniform
                
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

            string imagenPersonalizada = null;

            if (juego.Value != null &&
                juego.Value.custom_images.TryGetValue("background", out imagenPersonalizada) &&
                !string.IsNullOrWhiteSpace(imagenPersonalizada))
            {
                //SI HAY IMAGEN PERSONALIZADA, LA USO
                imagenJuego.Source = new BitmapImage(new Uri(imagenPersonalizada));
                imagenJuego.Stretch = Stretch.UniformToFill;
                imagenJuego.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                //INTENTO CARGAR LA IMAGEN ORIGINAL DE STEAM
                imagenJuego.Source = new BitmapImage(new Uri("https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/" + key + "/library_hero.jpg"));

                //SI FALLA LA CARGA, PONGO UNA IMAGEN DE RESPALDO
                imagenJuego.ImageFailed += (sender, e) =>
                {
                    imagenJuego.Stretch = Stretch.UniformToFill;
                    imagenJuego.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    imagenJuego.VerticalAlignment = VerticalAlignment.Top;
                    imagenJuego.Source = new BitmapImage(new Uri("https://cdn.cloudflare.steamstatic.com/steam/apps/" + key + "/capsule_616x353.jpg"));
                };
            }

            /*
            //INTENTO CARGAR LA IMAGEN ORIGINAL
            imagenJuego.Source = new BitmapImage(new Uri("https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/" + key + "/library_hero.jpg"));

            //SI NO SE HA PUESTO NINGUNA IMAGEN (ES DECIR, QUE ESTE JUEGO NO TIENE)
            imagenJuego.ImageFailed += (sender, e) =>
            {
                imagenJuego.Stretch = Stretch.UniformToFill;
                imagenJuego.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                imagenJuego.VerticalAlignment = VerticalAlignment.Top;
                imagenJuego.Source = new BitmapImage(new Uri("https://cdn.cloudflare.steamstatic.com/steam/apps/" + key + "/capsule_616x353.jpg"));
            };
            */
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

            //SACAR APARTADO DE PROGRAMAS NECESARIOS
            if (juego.Value.programas_necesarios != null && juego.Value.programas_necesarios.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var programa in juego.Value.programas_necesarios)
                {
                    sb.AppendLine($"    • {programa}");
                }
                txtProgramasNecesarios.Text = sb.ToString();
            }
            else
            {
                txtProgramasNecesarios.Text = "No additional programs are required.";
            }

            //SACAR EL APARTADO DE ERROES
            if (juego.Value.errores != null && juego.Value.errores.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var error in juego.Value.errores)
                {
                    sb.AppendLine($"{error}");
                }
                txtErrors.Text = sb.ToString();
            }
            else
            {
                txtProgramasNecesarios.Text = "No errors found.";
            }

            //SACAR EL APARTADO DE COMENTARIOS
            txtComentarios.Text = juego.Value.comentarios;

        }


        //CLICK PARA EL BOTON DE FIXEAR
        private void fixButtonClick(object sender, RoutedEventArgs e)
        {

            fixButton.IsEnabled = false;
            try
            {
                descargarJuego(juego);
            }
            catch (Exception ) {
                var ventanaError = new Windows.ErrorDialog("An unexpected error occurred while trying to download the game " + juego.Value.name + 
                    ". Please wait a few minutes and, if the error persists, contact the developer.", Brushes.Red);
                ventanaError.ShowDialog();
                //System.Windows.MessageBox.Show(ex.Message);
                fixButton.IsEnabled = true;
            }
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
            string token = TokenManager.GetGithubToken();

            string apiUrl = $"https://api.github.com/repos/{user}/{repo}/contents/{appId}";

            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromHours(4);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string json = await client.GetStringAsync(apiUrl);

                if (string.IsNullOrEmpty(json))
                {
                    var ventanaError = new Windows.ErrorDialog("Failed to authenticate with GitHub. Please check your token.", Brushes.Red);
                    ventanaError.ShowDialog();
                    fixButton.IsEnabled = true;
                    return;
                }

                var archivos = JsonConvert.DeserializeObject<List<ArchivoGitHub>>(json);

                //CommonOpenFileDialog PARA SELECCIONAR EL SELECTOR DE FICHEROS DE WINDOWS
                var folderDialog = new CommonOpenFileDialog();
                folderDialog.IsFolderPicker = true;
                folderDialog.Title = "Select the game folder of " + keyValuePair.Value.name + ":";

                if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string carpetaDestino = folderDialog.FileName;

                    string carpetaCommon = System.IO.Path.Combine(carpetaDestino, "steamapps", "common");

                    if (!carpetaDestino.Contains("steamapps\\common") || System.IO.Path.GetFileName(carpetaDestino).Equals("common", StringComparison.OrdinalIgnoreCase))
                    {
                        var ventanaError3 = new Windows.ErrorDialog("You must select the *game folder* inside 'steamapps\\common' for " + keyValuePair.Value.name + ".", Brushes.Red);
                        ventanaError3.ShowDialog();
                        fixButton.IsEnabled = true;
                        return;
                    }

                    //VERIFICO Y ELIMINO LA CARPETA DEL JUEGO SI EXISTE
                    string carpetaJuego = System.IO.Path.Combine(carpetaCommon, "game");
                    if (Directory.Exists(carpetaJuego))
                    {
                        Directory.Delete(carpetaJuego, true);
                    }

                    string rutaZipExtraer = null;
                    string carpetaTemporal = System.IO.Path.Combine(carpetaDestino, "temp_extraccion");

                    //CREAR CARPETA TEMPORAL SI NO EXISTE
                    if (!Directory.Exists(carpetaTemporal))
                    {
                        Directory.CreateDirectory(carpetaTemporal);
                    }


                    //Filtrar solo los archivos y calcular el tamaño total
                    var archivosADescargar = archivos.Where(a => a.type == "file").ToList();
                    long totalBytesADescargar = archivosADescargar.Sum(a => a.size);
                    long totalBytesDescargadosHastaAhora = 0;

                    progressBar.Value = 0;
                    progressBar.Visibility = Visibility.Visible;
                    textoDescargando.Text = "Downloading...";

                    //DESCARGAR CADA ARCHIVO Y ACTUALIZAR EL PROGRESO GLOBAL
                    foreach (var archivo in archivosADescargar)
                    {
                        string rutaDestino = System.IO.Path.Combine(carpetaDestino, archivo.name);

                        //Lógica de eliminación de archivos existentes
                        if (File.Exists(rutaDestino))
                        {
                            try { File.Delete(rutaDestino); await Task.Delay(100); }
                            catch (IOException ioEx)
                            {
                                try { File.Delete(rutaDestino); }
                                catch (IOException)
                                {
                                    var ventanaError4 = new Windows.ErrorDialog("Error trying to delete the file: " + ioEx.Message, Brushes.Red);
                                    ventanaError4.ShowDialog();
                                    fixButton.IsEnabled = true;
                                    return;
                                }
                            }
                        }

                        var fileProgress = new Progress<long>(bytesLeidosDelArchivo =>
                        {
                            //Progreso total = (Bytes_YA_Descargados + Bytes_Leídos_AHORA) / Total_Absoluto
                            long totalActual = totalBytesDescargadosHastaAhora + bytesLeidosDelArchivo;
                            double porcentaje = (double)totalActual / totalBytesADescargar * 100;

                            //Actualizar UI
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                progressBar.Value = porcentaje;
                                progressText.Text = $"{porcentaje:0.0}%";
                            });
                        });

                        //Llamar al método auxiliar para la descarga con progreso
                        await DownloadFileWithProgress(client, archivo.download_url, rutaDestino, fileProgress);

                        //Una vez terminado el archivo, actualizo el contador global de bytes
                        totalBytesDescargadosHastaAhora += archivo.size;

                        //GUARDAR RUTA DEL ZIP
                        if (archivo.name.EndsWith(".zip") && rutaZipExtraer == null)
                            rutaZipExtraer = rutaDestino;
                    }

                    //Ajuste final del progreso al 100% (antes de la extracción)
                    progressBar.Value = 100;
                    progressText.Text = "100%";
                    textoDescargando.Text = "Extracting...";

                    //EXTRAIGO EL ZIP SI EXISTE
                    if (rutaZipExtraer != null)
                    {
                        //PARA MANEJAR LOS ARCHIVOS ZIP
                        using (var archive = ArchiveFactory.Open(rutaZipExtraer))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                if (!entry.IsDirectory)
                                {
                                    string filePath = System.IO.Path.Combine(carpetaTemporal, entry.Key);
                                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));

                                    using (var fileStream = File.Create(filePath))
                                    {
                                        entry.WriteTo(fileStream);
                                    }
                                }
                            }
                        }

                        //MUEVO EL CONTENIDO EXTRAÍDO A LA CARPETA FINAL
                        foreach (var archivoExtraido in Directory.GetFiles(carpetaTemporal, "*", SearchOption.AllDirectories))
                        {
                            string nombreRelativo = archivoExtraido.Substring(carpetaTemporal.Length + 1);
                            string rutaFinal = System.IO.Path.Combine(carpetaDestino, nombreRelativo);

                            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(rutaFinal));

                            if (File.Exists(rutaFinal))
                            {
                                File.Delete(rutaFinal);
                            }

                            File.Move(archivoExtraido, rutaFinal);
                        }

                        //ELIMINO ARCHIVOS TEMPORALES Y ZIP
                        Directory.Delete(carpetaTemporal, true);
                        File.Delete(rutaZipExtraer);

                        //ELIMINO TODOS LOS FRAGMENTOS DEL ZIP (EJ: .z01, .z02, etc.)
                        string zipBaseName = System.IO.Path.GetFileNameWithoutExtension(rutaZipExtraer);
                        string zipDir = System.IO.Path.GetDirectoryName(rutaZipExtraer);
                        var fragmentos = Directory.GetFiles(zipDir, zipBaseName + ".*")
                                                 .Where(f => !f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

                        foreach (var frag in fragmentos)
                        {
                            try { File.Delete(frag); }
                            catch (Exception ex)
                            {
                                var ventanaError5 = new Windows.ErrorDialog("Could not delete fragment file: " + ex.Message, Brushes.Red);
                                ventanaError5.ShowDialog();
                                fixButton.IsEnabled = true;
                            }
                        }
                    }

                    textoDescargando.Text = "Done";
                    var ventanaError = new Windows.ErrorDialog("Game ready to play", Brushes.Green);
                    ventanaError.ShowDialog();
                    progressBar.Value = 0;
                    progressText.Text = "";
                    progressBar.Visibility = Visibility.Collapsed;
                    textoDescargando.Text = "";
                    fixButton.IsEnabled = true;
                }
                else
                {
                    var ventanaError = new Windows.ErrorDialog("No folder selected.", Brushes.Red);
                    ventanaError.ShowDialog();
                    fixButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                var ventanaError = new Windows.ErrorDialog("ERROR: " + ex.Message, Brushes.Red);
                ventanaError.ShowDialog();
                fixButton.IsEnabled = true;
            }
        }


        //METODO AUXILIAR PARA QUE LA DESCARGA DE JUEGOS Y VER SUS BYTES
        private async Task DownloadFileWithProgress(HttpClient client, string url, string destinationPath, IProgress<long> progress)
        {
            using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    long totalRead = 0;
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;

                        //Reporto el progreso por bytes para este archivo
                        progress?.Report(totalRead);
                    }
                }
            }
        }











        //CLASE AUXILIAR PARA DESERIALIZAR LA RESPUESTA DE GITHUB API
        public class ArchivoGitHub
        {
            public string name { get; set; }
            public string path { get; set; }
            public string type { get; set; }
            public long size { get; set; }
            public string download_url { get; set; }
        }
    }
}
