using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Project_Lightning.Windows;

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

                    //MessageBox.Show($"Version local: { versionLocal} vs Version github: {versionRemota}");

                    if (versionRemota > versionLocal)
                    {
                        var ventanaActualizacion = new Windows.ErrorDialog($"New version available: {versionRemota}. The update will be downloaded once the window is closed..", Brushes.Green);
                        ventanaActualizacion.ShowDialog();

                        //MessageBox.Show($"New version available: {versionRemota}. The update will be downloaded once the window is closed..");

                        string rutaDescarga = Path.Combine(Path.GetTempPath(), nombreInstalador);

                        using (var s = await client.GetStreamAsync(urlInstalador))
                        using (var fs = new FileStream(rutaDescarga, FileMode.Create, FileAccess.Write))
                            await s.CopyToAsync(fs);

                        Process.Start(rutaDescarga);
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex)
            {
                var ventanaActualizacion = new Windows.ErrorDialog($"Could not check for updates: ", Brushes.Green);
                ventanaActualizacion.Show();
                //MessageBox.Show("No se pudo comprobar la actualización: " + ex.Message);
            }
        }
    }

}