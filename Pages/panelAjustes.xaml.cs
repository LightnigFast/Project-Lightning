using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Project_Lightning.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
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








    }
}
