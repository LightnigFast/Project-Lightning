using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Project_Lightning.Classes
{
    public class Actualizador
    {
        private static readonly string urlVersion = "https://raw.githubusercontent.com/LightnigFast/Project-Lightning/main/latest-version.txt";
        private static readonly string urlInstalador = "https://github.com/LightnigFast/Project-Lightning/releases/latest/download/ProjectLightningInstaller.exe";
        private static readonly string nombreInstalador = "ProjectLightningInstaller.exe";

        public static async Task ComprobarActualizacion()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string ultimaVersionTexto = await client.GetStringAsync(urlVersion);
                    Version versionRemota = new Version(ultimaVersionTexto.Trim());
                    Version versionLocal = Assembly.GetExecutingAssembly().GetName().Version;

                    //versionLocal = new Version("0.0.0.0");
                    //MessageBox.Show("VERSION GITHUB: " + versionRemota);
                    //MessageBox.Show("VERSION APP: " + versionLocal);

                    if (versionRemota > versionLocal)
                    {
                        //OBTENGO LA VENTANA PRINCIPAL
                        var ventanaPrincipal = Application.Current.MainWindow;

                        //MUESTRO LA VENTANA DE ACTUALIZACION DISPONIBLE
                        var dialogoAdvertencia = new Windows.ErrorDialog($"New version available: {versionRemota}.", Brushes.Green);
                        dialogoAdvertencia.ShowDialog(); 
                        
                        //CREO LA VENTANA DE PROGRESO DE DESCARGA
                        var ventanaProgreso = new Windows.ProgresoDescarga(versionRemota.ToString());

                        //PONGO ESTA NUEVA VENTANA COMO PROPIETARIO
                        ventanaProgreso.Owner = ventanaPrincipal;

                        //DESHABILITO LA VENTANA PRINCIPAL
                        if (ventanaPrincipal != null)
                        {
                            ventanaPrincipal.IsEnabled = false;
                        }

                        ventanaProgreso.Show();

                        //INICIO LA DESCARGA
                        try
                        {
                            string rutaDescarga = await DescargarConProgreso(client, urlInstalador, nombreInstalador, ventanaProgreso.ActualizarProgreso);

                            //CIERRO TODO E INICIAR EL INSTALADOR
                            ventanaProgreso.Close();
                            if (ventanaPrincipal != null)
                            {
                                ventanaPrincipal.IsEnabled = true; //Habilito la ventana principal brevemente antes de salir
                            }

                            System.Diagnostics.Process.Start(rutaDescarga);
                            Environment.Exit(0);
                        }
                        catch (Exception exDescarga)
                        {
                            //SI HAY ALGUN ERROR, VUELVO A HABILITAR LA VENTANA POR SI ACASO
                            if (ventanaPrincipal != null)
                            {
                                ventanaPrincipal.IsEnabled = true;
                            }
                            ventanaProgreso.Close();
                            var ventanaError = new Windows.ErrorDialog("Download failed: " + exDescarga.Message, Brushes.Red);
                            ventanaError.Show();
                        }
                    }
                }
            }
            catch (Exception)
            {
                var ventanaError = new Windows.ErrorDialog("Could not check for updates.", Brushes.Red);
                ventanaError.Show();
            }
        }

        //METODO PARA LA DESCARGA CON PROGRESO
        private static async Task<string> DescargarConProgreso(HttpClient client, string url, string nombreArchivo, Action<double> onProgressUpdate)
        {
            string rutaDescarga = Path.Combine(Path.GetTempPath(), nombreArchivo);

            using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;
                long bytesDescargados = 0;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(rutaDescarga, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    int bytesLeidos;

                    while ((bytesLeidos = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesLeidos);
                        bytesDescargados += bytesLeidos;

                        //ACTUALIZO EL PROGRESO SOLO SI CONOZCO EL TAMAÑO TOTAL DEL ARCHIVO
                        if (totalBytes.HasValue)
                        {
                            double progreso = (double)bytesDescargados / totalBytes.Value;
                            onProgressUpdate(progreso * 100);
                        }
                    }
                }
            }

            return rutaDescarga;
        }
    }
}