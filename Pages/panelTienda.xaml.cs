using Newtonsoft.Json;
using Project_Lightning.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Lógica de interacción para panelTienda.xaml
    /// </summary>
    public partial class panelTienda : Page
    {
        MainWindow ventanaPrincipal;
        private Dictionary<string, Juego> todosLosJuegos = new Dictionary<string, Juego>();
        public panelTienda(MainWindow ventanaPrincipal)
        {
            InitializeComponent();
            this.ventanaPrincipal = ventanaPrincipal;
            gridCabecera.Visibility = Visibility.Hidden;

            ponerJuegos();


        }

        //CLASE JUEGO QUE CONTENDRA LA INFORMACIÓN DE CADA JUEGO
        public class Juego
        {
            public string name { get; set; }
            public bool activo { get; set; }
            public string imgCabecera { get; set; }
            public string imgLogo { get; set; }
            public string imgVertical { get; set; }
            public int precioNormal { get; set; }
            public int precioDonadores { get; set; }
            public int descuento { get; set; }
        }




        private async void ponerJuegos()
        {
            todosLosJuegos = await sacarJuegosDeApp();
            colocarJuegos(todosLosJuegos);

        }


        //ESTE METODO BUSCA SACAR TODOS LOS JUEGOS DE UNA SOLA COMPAÑIA DADA POR EL nomApp
        private async Task<Dictionary<string, Juego>> sacarJuegosDeApp()
        {
            string rutaJson = System.IO.Path.GetFullPath(@"..\\..\\shop.json");
            string rutaJsonApp = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shop.json");
            string urlJson = "https://raw.githubusercontent.com/LightnigFast/Project-Lightning/main/shop.json";

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

            //CARGAR JSON LOCAL
            string json = File.ReadAllText(rutaJsonApp);

            //YA NO HAY NIVELES INTERNOS, SOLO APPID -> Juego
            var data = JsonConvert.DeserializeObject<Dictionary<string, Juego>>(json);
            //MessageBox.Show(data.Count.ToString());

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


        //METODO PARA COLOCAR LOS JUEGOS EN EL PANEL
        private void colocarJuegos(Dictionary<string, Juego> todosLosJuegos)
        {

            //LIMPIAR EL PANEL ANTES
            panelJuegos.Children.Clear();

            foreach (var kvp in todosLosJuegos)
            {
                Juego juego = kvp.Value;

                //CREAR IMAGEN
                Image img = new Image
                {
                    Width = 150,
                    Height = 225,
                    Margin = new Thickness(5),
                    Stretch = Stretch.UniformToFill,
                    Cursor = Cursors.Hand, //PARA QUE SE VEA INTERACTIVO
                    Tag = kvp.Key //GUARDAMOS EL APPID
                };

                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(juego.imgVertical, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    img.Source = bitmap;
                }
                catch
                {
                    //SI FALLA LA CARGA, PONGO UN RECTÁNGULO O TEXTO EN LUGAR DE IMAGEN
                    img.Source = null;
                }

                //EVENTO CLICK
                img.MouseLeftButtonUp += (s, e) =>
                {
                    ponerEnCabecera(juego);
                };

                //AÑADIR AL WRAPPANEL
                panelJuegos.Children.Add(img);
            }


        }

        //METODO PARA CAMBIAR LA CABECERA DE LA PAGINA
        private void ponerEnCabecera(Juego juego)
        {
            try
            {
                //FONDO DE LA CABECERA
                if (!string.IsNullOrEmpty(juego.imgCabecera))
                {
                    BitmapImage fondo = new BitmapImage();
                    fondo.BeginInit();
                    fondo.UriSource = new Uri(juego.imgCabecera, UriKind.Absolute);
                    fondo.CacheOption = BitmapCacheOption.OnLoad;
                    fondo.EndInit();

                    gridCabecera.Background = new ImageBrush(fondo)
                    {
                        Stretch = Stretch.UniformToFill, //CUBRE EL GRID
                        AlignmentX = AlignmentX.Center,
                        AlignmentY = AlignmentY.Center
                    };
                }
                else
                {
                    MessageBox.Show("dfsf");
                    gridCabecera.Background = null;
                }

                //LOGO
                if (!string.IsNullOrEmpty(juego.imgLogo))
                {
                    BitmapImage logo = new BitmapImage();
                    logo.BeginInit();
                    logo.UriSource = new Uri(juego.imgLogo, UriKind.Absolute);
                    logo.CacheOption = BitmapCacheOption.OnLoad;
                    logo.EndInit();
                    imgLogo.Source = logo;
                }
                else
                {
                    imgLogo.Source = null; //POR SI NO TIENE LOGO
                }

                gridCabecera.Visibility = Visibility.Visible;

                //PRECIO NORMAL
                percioNormal.Text = $" {juego.precioNormal} LC";

                //PRECIO DONADOR
                percioDonador.Text = $" {juego.precioDonadores} LC";

                //SI HAY DESCUENTO
                if (juego.descuento > 0)
                {
                    percioNormal.TextDecorations = TextDecorations.Strikethrough;
                }
                else
                {
                    percioNormal.TextDecorations = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al mostrar el juego: " + ex.Message);
            }
        }




        //EVENTO PARA CUANDO HAGA CLICK EN EL BOTON DE IR A DISCORD
        private void irADiscord(object sender, MouseButtonEventArgs e)
        {
            string url = "https://discord.com/channels/1399694698783703193/1405153983995187352";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true //NECESARIO EN .NET CORE / .NET 5+
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el enlace: " + ex.Message);
            }
        }


    }
}
