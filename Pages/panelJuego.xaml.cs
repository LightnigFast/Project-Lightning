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
using TokenProvider;
using System.Windows.Shapes;
using System.IO.Compression;
using Newtonsoft.Json;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

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
            catch (Exception ex) {
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
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

          
                string json = await client.GetStringAsync(apiUrl);
  
                if (string.IsNullOrEmpty(json))
                {
                    var ventanaError = new Windows.ErrorDialog("Failed to authenticate with GitHub. Please check your token.", Brushes.Red);
                    ventanaError.ShowDialog();
                    return;
                }

                var archivos = JsonConvert.DeserializeObject<List<ArchivoGitHub>>(json);

                //CommonOpenFileDialog PARA SELECCIONAR EL SELECTOR DE FICHEROS DE WINDOWS
                var folderDialog = new CommonOpenFileDialog();
                folderDialog.IsFolderPicker = true;
                folderDialog.Title = "Select the game folder of " + juego.Value.name + ":";

                if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string carpetaDestino = folderDialog.FileName;

                    //VERIFICAR QUE LA CARPETA SELCCIONADA ESTE DENTRO DE "steamapps/common"
                    string carpetaSteamApps = System.IO.Path.Combine(carpetaDestino, "steamapps");
                    string carpetaCommon = System.IO.Path.Combine(carpetaSteamApps, "common");

                    //VERIFICAR QUE LA CARPETA SELCCIONADA ESTE DENTRO DE "steamapps/common"
                    if (!carpetaDestino.Contains("steamapps\\common"))
                    {
                        var ventanaError3 = new Windows.ErrorDialog("You must select the game folder of " + juego.Value.name + ":", Brushes.Red);
                        ventanaError3.ShowDialog();
                        return;
                    }

                    //COMPRUEBO SI LA CARPETA SELECCIONADA ES COMMON
                    if (System.IO.Path.GetFileName(carpetaDestino).Equals("common", StringComparison.OrdinalIgnoreCase))
                    {
                        var ventanaError2 = new Windows.ErrorDialog("You must select the game folder of " + juego.Value.name + ":", Brushes.Red);
                        ventanaError2.ShowDialog();
                        return;
                    }

                    //SI LLEGAMOS HASTA AQUI, LA CARPETA SELCCIONADA ES CORRECTA Y ESTA DENTRO DE "STEAMAPPS/COMMON"

                    //VERIFICAR QUE LA SUBCARPETA DEL JUEGO DENTRO DE 'COMMON' NO EXISTA, SI EXISTE LA ELIMINAMOS
                    string carpetaJuego = System.IO.Path.Combine(carpetaCommon, "game"); 
                    if (Directory.Exists(carpetaJuego))
                    {
                        Directory.Delete(carpetaJuego, true);  //ELIMINO LA CARPETA DEL JUEGO SI YA EXISTE
                    }

                    string rutaZipExtraer = null;
                    string carpetaTemporal = System.IO.Path.Combine(carpetaDestino, "temp_extraccion");

                    //CREAR CARPETA TEMPORAL SI NO EXISTE
                    if (!Directory.Exists(carpetaTemporal))
                    {
                        Directory.CreateDirectory(carpetaTemporal);
                    }

                    //PARA LA BARRA DE PROGRESO
                    progressBar.Value = 0;
                    int totalArchivos = archivos.Count;
                    int archivosProcesados = 0;
                    progressBar.Visibility = Visibility.Visible;
                    textoDescargando.Text = "Downloading...";

                    //DESCARGAR TODO
                    foreach (var archivo in archivos)
                    {
                        if (archivo.type == "file")
                        {
                            string rutaDestino = System.IO.Path.Combine(carpetaDestino, archivo.name);

                            //VERIFICAR SI EL ARCHIVO YA EXISTE Y ELIMINARLO ANTES
                            if (File.Exists(rutaDestino))
                            {
                                try
                                {
                                    //INTENTO ELIMINAR EL ARCHIVO, SI SE ESTÁ USANDO, ESPERO ANTES DE ELIMINAR
                                    File.Delete(rutaDestino);
                                    await Task.Delay(100);  //ESPERO POR SI EL ARCHIVO SE ESTA USANDO
                                }
                                catch (IOException)
                                {
                                    try
                                    {
                                        //FUERZO ELIMINAICION
                                        File.Delete(rutaDestino);
                                    }
                                    catch (IOException ioEx)
                                    {
                                        var ventanaError4 = new Windows.ErrorDialog("Error trying to delete the file: " + ioEx.Message, Brushes.Red);
                                        ventanaError4.ShowDialog();
                                        //System.Windows.MessageBox.Show("Error trying to delete the file: " + ioEx.Message);
                                        return;
                                    }
                                }
                            }

                            byte[] datos = await client.GetByteArrayAsync(archivo.download_url);
                            File.WriteAllBytes(rutaDestino, datos);

                            //GUARDO LA RUTA AL PRIMER ZIP ENCONTRADO PARA EXTRAERLO DESPUÉS
                            if (archivo.name.EndsWith(".zip") && rutaZipExtraer == null)
                                rutaZipExtraer = rutaDestino;

                            archivosProcesados++;
                            progressBar.Value = (double)archivosProcesados / totalArchivos * 100;
                            progressText.Text = $"{(int)((double)archivosProcesados / totalArchivos * 100)}%";
                        }
                    }


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
                                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath)); //CREAR DIRECTORIOS SI NO EXISTEN

                                    //EXTRAIGO EL ARCHIVO
                                    using (var fileStream = File.Create(filePath))
                                    {
                                        entry.WriteTo(fileStream);  //EXTRAIGO EL CONTENIDO DEL ARCHIVO
                                    }
                                }
                            }
                        }

                        //MUEVO EL CONTENIDO EXTRAÍDO A LA CARPETA FINAL (REEMPLAZAR SI YA EXISTEN)
                        foreach (var archivoExtraido in Directory.GetFiles(carpetaTemporal, "*", SearchOption.AllDirectories))
                        {
                            string nombreRelativo = archivoExtraido.Substring(carpetaTemporal.Length + 1);
                            string rutaFinal = System.IO.Path.Combine(carpetaDestino, nombreRelativo);

                            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(rutaFinal));

                            //SI EL ARCHIVO YA EXISTE LO REMPLAZO
                            if (File.Exists(rutaFinal))
                            {
                                File.Delete(rutaFinal);
                            }

                            File.Move(archivoExtraido, rutaFinal);
                        }

                        //ELIMINO ARCHIVOS TEMPORALES
                        Directory.Delete(carpetaTemporal, true);
                        File.Delete(rutaZipExtraer);

                        //ELIMINO TODOS LOS FRAGMENTOS DEL ZIP (EJ: .z01, .z02, etc.)
                        string zipBaseName = System.IO.Path.GetFileNameWithoutExtension(rutaZipExtraer);
                        string zipDir = System.IO.Path.GetDirectoryName(rutaZipExtraer);
                        var fragmentos = Directory.GetFiles(zipDir, zipBaseName + ".*")
                                                  .Where(f => !f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

                        foreach (var frag in fragmentos)
                        {
                            try
                            {
                                File.Delete(frag);
                            }
                            catch (Exception ex)
                            {
                                var ventanaError5 = new Windows.ErrorDialog("Could not delete fragment file: " + ex.Message, Brushes.Red);
                                ventanaError5.ShowDialog();
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
                    //System.Windows.MessageBox.Show("Download and extraction completed: " + carpetaDestino);
                }
                else
                {
                    var ventanaError = new Windows.ErrorDialog("No folder selected.", Brushes.Red);
                    ventanaError.ShowDialog();
                    //System.Windows.MessageBox.Show("No folder selected.");
                }
            }
            catch (Exception ex)
            {
                var ventanaError = new Windows.ErrorDialog("ERROR: " + ex.Message, Brushes.Red);
                ventanaError.ShowDialog();
                //System.Windows.MessageBox.Show("ERROR: " + ex.Message);
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
