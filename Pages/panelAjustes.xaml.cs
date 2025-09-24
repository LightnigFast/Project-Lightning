using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Project_Lightning.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Lógica de interacción para panelAjusted.xaml
    /// </summary>
    public partial class panelAjustes : Page
    {
        private MainWindow _ventanaPrincipal;
        private NotificationManager notifier;

        public panelAjustes(MainWindow ventanaPrincipal)
        {
            InitializeComponent();
            _ventanaPrincipal = ventanaPrincipal;
            notifier = new Project_Lightning.Classes.NotificationManager(_ventanaPrincipal.NotificationCanvasPublic);

            //CARGO LA RUTA EN EL TEXTBOX SIEMPRE QUE EXISTA
            CargarRutaSteam();
        }


        // Cargar ruta al iniciar la página
        private void CargarRutaSteam()
        {
            string folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Project_Lightning");
            string configFile = System.IO.Path.Combine(folder, "config.json");

            if (File.Exists(configFile))
            {
                var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(configFile));
                if (config != null && config.ContainsKey("SteamPath"))
                    txtRutaSteam.Text = config["SteamPath"];
            }
        }

        // Guardar ruta cuando el usuario haga click
        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            GuardarRutaSteam();



            //notifier.Show("Saved changes ✅", isError: false);

        }

        private void GuardarRutaSteam()
        {
            string ruta = txtRutaSteam.Text;

            //VALIDAR SI EXISTE EL ARCHIVO steam.exe EN ESA RUTA
            string steamExe = System.IO.Path.Combine(ruta, "steam.exe");
            if (!File.Exists(steamExe))
            {
                //MOSTRAR NOTIFICACIÓN DE ERROR
                var notifier = new Project_Lightning.Classes.NotificationManager(_ventanaPrincipal.NotificationCanvasPublic);
                notifier.Show("❌ The selected path does not contain Steam", isError: true);
                return;
            }

            //SI LA RUTA ES VÁLIDA -> GUARDAR
            string folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Project_Lightning");
            Directory.CreateDirectory(folder);
            string configFile = System.IO.Path.Combine(folder, "config.json");

            var config = new Dictionary<string, string> { { "SteamPath", ruta } };
            File.WriteAllText(configFile, JsonConvert.SerializeObject(config, Formatting.Indented));

            //MOSTRAR NOTIFICACIÓN DE ÉXITO
            var notifierOk = new Project_Lightning.Classes.NotificationManager(_ventanaPrincipal.NotificationCanvasPublic);
            notifierOk.Show("Saved changes ✅.");
        }


        private void btnBuscarSteam_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new CommonOpenFileDialog();
            folderDialog.IsFolderPicker = true;
            folderDialog.Title = "Select the main folder of steam";

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtRutaSteam.Text = folderDialog.FileName;


            }
        }





        //BOTON PARA INSTALAR LIGHTNING TOOLS
        private void btnInstallarLightningTools_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.dropbox.com/scl/fi/3pfqpenurej36rxu5j6d2/hid.dll?rlkey=39k6pjfueqddwrcyh8w2zi6pb&st=hcdyupyw&dl=1";
                          

            string rutaUsuario = txtRutaSteam.Text;

            if (string.IsNullOrWhiteSpace(rutaUsuario) || !Directory.Exists(rutaUsuario))
            {
                notifier.Show("❌ Please select a valid folder first", isError: true, 4000);
                return;
            }

            // Carpeta config dentro de la ruta del usuario
            string configFolder = System.IO.Path.Combine(rutaUsuario, "config");

            if (!Directory.Exists(configFolder))
            {
                notifier.Show("❌ The config folder does not exist in the selected path.", isError: true, 4000);
                return;
            }

            // Verificar y crear stplug-in y depotcache si no existen
            string stPluginFolder = System.IO.Path.Combine(configFolder, "stplug-in");
            string depotcacheFolder = System.IO.Path.Combine(configFolder, "depotcache");

            if (!Directory.Exists(stPluginFolder))
                Directory.CreateDirectory(stPluginFolder);

            if (!Directory.Exists(depotcacheFolder))
                Directory.CreateDirectory(depotcacheFolder);


            // Mantener el nombre original del DLL
            string fileName = System.IO.Path.GetFileName(new Uri(url).LocalPath);
            string destino = System.IO.Path.Combine(rutaUsuario, fileName);

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, destino);
                }

                notifier.Show($"✅ Lightning Tools installed successfully!", isError: false, 4000);
            }
            catch (Exception ex)
            {
                notifier.Show($"❌ Error downloading Lightning Tools: {ex.Message}", isError: true, 4000);
            }
        }




        //BOTON PARA BORRAR TODOS LOS JUEGOS INSTALADOS CON STEAMTOOLS
        private void btnBorrarJuegos_Click(object sender, RoutedEventArgs e)
        {
            string rutaUsuario = txtRutaSteam.Text;

            if (string.IsNullOrWhiteSpace(rutaUsuario) || !Directory.Exists(rutaUsuario))
            {
                notifier.Show("❌ Please select a valid folder first", isError: true, 4000);
                return;
            }

            string stPluginFolder = System.IO.Path.Combine(rutaUsuario, "config", "stplug-in");

            if (!Directory.Exists(stPluginFolder))
            {
                notifier.Show("❌ The stplug-in folder does not exist.", isError: true, 4000);
                return;
            }

            try
            {
                // Borra todos los archivos
                foreach (string file in Directory.GetFiles(stPluginFolder))
                {
                    File.Delete(file);
                }

                // Borra todas las subcarpetas y su contenido
                foreach (string dir in Directory.GetDirectories(stPluginFolder))
                {
                    Directory.Delete(dir, true);
                }

                notifier.Show("✅ All games have been deleted successfully!", isError: false, 4000);
            }
            catch (Exception ex)
            {
                notifier.Show($"❌ Error deleting files: {ex.Message}", isError: true, 4000);
            }
        }



        //BOTON PARA DESINSTALAR LIGHTNINGTOOLS
        private void btnDesinstalar_Click(object sender, RoutedEventArgs e)
        {
            string rutaUsuario = txtRutaSteam.Text;

            if (string.IsNullOrWhiteSpace(rutaUsuario) || !Directory.Exists(rutaUsuario))
            {
                notifier.Show("❌ Please select a valid folder first", isError: true, 4000);
                return;
            }

            string dllPath = System.IO.Path.Combine(rutaUsuario, "hid.dll");

            if (!File.Exists(dllPath))
            {
                notifier.Show("❌ LightningTools not found.", isError: true, 4000);
                return;
            }

            try
            {
                File.Delete(dllPath);

                notifier.Show("✅ LightningTools has been uninstalled successfully!", isError: false, 4000);
            }
            catch (Exception ex)
            {
                notifier.Show($"❌ Error deleting Lightning Tools: {ex.Message}", isError: true, 4000);
            }
        }





    }
}
