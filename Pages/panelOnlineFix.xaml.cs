using Newtonsoft.Json;
using Project_Lightning.Windows;
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

namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interacción para panelOnlineFix.xaml
    /// </summary>
    public partial class panelOnlineFix : Page
    {
        MainWindow ventanaPrincipal;
        public panelOnlineFix(MainWindow ventanaPrincipal)
        {
            InitializeComponent();
            this.ventanaPrincipal = ventanaPrincipal;


            ponerJuegos();
        }

        //BOTON DE HOME
        private void home_click(object sender, RoutedEventArgs e)
        {

            ventanaPrincipal.framePrincipal.Navigate(new panelMenuPrincipal());
            ventanaPrincipal.ocultarCabecera();
        }



        //CLASE JUEGO QUE CONTENDRA LA INFORMACIÓN DE CADA JUEGO
        public class Juego
        {
            public string name { get; set; }
            public string nombre_fix { get; set; }
            public string custom_images { get; set; }
        }


        private async void ponerJuegos()
        {
            var juegosApp = await sacarJuegosDeApp();

            colocarBotones(juegosApp);


        }


        //ESTE METODO BUSCA SACAR TODOS LOS JUEGOS DE UNA SOLA COMPAÑIA DADA POR EL nomApp
        private async Task<Dictionary<string, Juego>> sacarJuegosDeApp()
        {
            string rutaJson = System.IO.Path.GetFullPath(@"..\\..\\data-fix.json");
            string rutaJsonApp = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data-fix.json");
            string urlJson = "https://raw.githubusercontent.com/LightnigFast/Project-Lightning/main/data-fix.json";

            if (await EsArchivoGitHubDiferente(urlJson, rutaJson) || await EsArchivoGitHubDiferente(urlJson, rutaJsonApp))
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string contenidoGitHub = await client.GetStringAsync(urlJson);
                        File.WriteAllText(rutaJson, contenidoGitHub);
                        File.WriteAllText(rutaJsonApp, contenidoGitHub);
                    }
                }
                catch (Exception ex)
                {
                    var ventanaError = new Windows.ErrorDialog("Error updating the game list, please try again later: " + ex.Message, Brushes.Red);
                    ventanaError.ShowDialog();
                }
            }

            // CARGAR JSON LOCAL
            string json = File.ReadAllText(rutaJsonApp);

            // YA NO HAY NIVELES INTERNOS, SOLO APPID -> Juego
            var data = JsonConvert.DeserializeObject<Dictionary<string, Juego>>(json);

            return data ?? new Dictionary<string, Juego>();
        }


        private async Task<bool> EsArchivoGitHubDiferente(string url, string rutaLocal)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string contenidoGitHub = await client.GetStringAsync(url);

                    if (!File.Exists(rutaLocal))
                    {
                        return true; //NO EXISTE EL LOCAL, ES DIFERENTE
                    }

                    string contenidoLocal = File.ReadAllText(rutaLocal);
                    return !contenidoLocal.Equals(contenidoGitHub);

                }
            }
            catch (Exception ex)
            {
                var ventanaError = new ErrorDialog("Error comparing files: " + ex.Message, Brushes.Red);
                ventanaError.Show();
                //MessageBox.Show("Error al comparar archivos: " + ex.Message);
                return false;
            }
        }

        private void colocarBotones(Dictionary<string, Juego> juegosApp)
        {
            panelJuegos.Children.Clear(); //LIMPIA EL PANEL ANTES DE AGREGAR NUEVOS

            foreach (var kvp in juegosApp)
            {
                var juego = kvp.Value;

                //GRID CONTENEDOR DEL JUEGO
                Grid contenedor = new Grid
                {
                    Width = 300,
                    Height = 270,
                    Margin = new Thickness(10),
                    Background = Brushes.Yellow
                };

                //DEFINIR 3 FILAS
                contenedor.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // IMAGEN
                contenedor.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); // NOMBRE
                contenedor.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); // BOTÓN

                //IMAGEN (SE ADAPTA A LA FILA)
                Image imagen = new Image
                {
                    Source = new BitmapImage(new Uri(juego.custom_images)),
                    Stretch = Stretch.Uniform, // MANTIENE LA PROPORCIÓN
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top, // EVITA ESTIRAR VERTICALMENTE
                };

                Grid.SetRow(imagen, 0);
                contenedor.Children.Add(imagen);

                //NOMBRE DEL JUEGO
                TextBlock nombre = new TextBlock
                {
                    Text = juego.name,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(5),
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetRow(nombre, 1);
                contenedor.Children.Add(nombre);

                //BOTÓN
                Button botonFix = new Button
                {
                    Content = "Apply Fix",
                    Width = 120,
                    Height = 35,
                    Background = Brushes.DodgerBlue,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(5),
                    Tag = kvp.Key
                };
                botonFix.Click += BotonFix_Click;
                Grid.SetRow(botonFix, 2);
                contenedor.Children.Add(botonFix);

                //AÑADIR A WRAPPANEL
                panelJuegos.Children.Add(contenedor);
            }
        }



        private void BotonFix_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.Tag is string appId)
            {
                //LLAMADA A TU LÓGICA DE FIX
                MessageBox.Show($"Apply fix para el juego con ID: {appId}");
                //Ejemplo: aplicarFix(appId);
            }
        }



    }
}
