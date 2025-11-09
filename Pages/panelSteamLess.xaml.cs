using Microsoft.WindowsAPICodePack.Dialogs;
using Project_Lightning.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Project_Lightning.Pages
{
    public partial class panelSteamLess : Page
    {
        private readonly string dll32 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "dlls", "steam_api.dll");
        private readonly string dll64 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "dlls", "steam_api64.dll");

        private string selectedFolder = string.Empty;
        private MainWindow ventanaPrincipal;
        private NotificationManager notifier;

        public panelSteamLess(MainWindow ventanaPrincipal)
        {
            InitializeComponent();
            this.ventanaPrincipal = ventanaPrincipal;
            notifier = new NotificationManager(ventanaPrincipal.NotificationCanvasPublic);
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select the game’s root folder"
            };

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedFolder = folderDialog.FileName;
                TxtFolderPath.Text = selectedFolder;
                notifier.Show("📂 Folder selected successfully.");
            }
        }

        private void ReplaceDlls_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(selectedFolder) || !Directory.Exists(selectedFolder))
            {
                notifier.Show("❌ Please select a valid folder first.", isError: true);
                return;
            }

            try
            {
                var foundDlls = BuscarDlls(selectedFolder);
                int replacedCount = 0;

                foreach (var path in foundDlls)
                {
                    string fileName = Path.GetFileName(path);
                    string sourceDll = fileName.Equals("steam_api.dll", StringComparison.OrdinalIgnoreCase) ? dll32 : dll64;

                    if (File.Exists(sourceDll))
                    {
                        File.Copy(sourceDll, path, true);
                        replacedCount++;
                    }
                }

                if (replacedCount > 0)
                    notifier.Show($"✅ Replacement completed ({replacedCount} file(s) replaced).");
                else
                    notifier.Show("ℹ️ No files found to replace.", isError: true);
            }
            catch (Exception ex)
            {
                notifier.Show($"❌ Error during replacement: {ex.Message}", isError: true);
            }
        }

        //BUSCA RECURSIVAMENTE LAS DLLs EN TODAS LAS SUBCARPETAS, IGNORANDO ERRORES DE ACCESO
        private List<string> BuscarDlls(string rootFolder)
        {
            List<string> found = new List<string>();

            try
            {
                foreach (string file in Directory.GetFiles(rootFolder))
                {
                    string name = Path.GetFileName(file).ToLower();
                    if (name == "steam_api.dll" || name == "steam_api64.dll")
                        found.Add(file);
                }

                foreach (string dir in Directory.GetDirectories(rootFolder))
                {
                    try
                    {
                        found.AddRange(BuscarDlls(dir));
                    }
                    catch
                    {
                        //IGNORA ERRORES DE ACCESO A CARPETAS
                    }
                }
            }
            catch
            {
                //IGNORA ERRORES DE LECTURA
            }

            return found;
        }
    }
}
