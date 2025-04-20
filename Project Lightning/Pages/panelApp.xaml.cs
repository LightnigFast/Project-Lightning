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
using Newtonsoft.Json;

namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interacción para panelUbisoft.xaml
    /// </summary>
    public partial class panelApp : Page
    {

        MainWindow ventanaPrincipal;
        string nombreApp;
        public panelApp(String nomApp, MainWindow mainWindow)
        {
            InitializeComponent();
            
            //CAMBIO EL NOMBRE DE LA ETIQUETA
            txtApp.Text = nomApp;
            //CAMBIO DE COLOR DE LA ETIQUETA
            switch (nomApp)
            {
                case "UBISOFT": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A0DAD ")); break;
                case "EA": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f74545")); break;
                case "ROCKSTAR": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f7a600")); break;
                case "DENUVO": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#607D8B")); break;
                case "PlayStation": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0300b3")); break;
                case "OTHERS": txtApp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8BBD0")); break;

            }

            //INICIALIZO LA VENTANA PARA TENER UNA REFERENCIA DE LA VENTANA PRINCIPAL Y LA VARIABLE DE TEXTO
            ventanaPrincipal = mainWindow;
            nombreApp = nomApp;

            ponerJuegos(nomApp);

        }

        //CLASE JUEGO QUE CONTENDRA LA INFORMACIÓN DE CADA JUEGO
        public class Juego
        {
            public string name { get; set; }
            public bool launch_steam { get; set; }
            public bool launch_exe { get; set; }
            public string comentarios { get; set; }
            public List<string> programas_necesarios { get; set; }
            public List<string> errores { get; set; }
            public string nombre_fix { get; set; }
        }
        

        private void ponerJuegos(string nomApp)
        {
            var juegosApp = sacarJuegosDeApp(nomApp);

            colocarBotones(juegosApp);

            //descargarJuego(juegosApp.First());

        }


        //ESTE METODO BUSCA SACAR TODOS LOS JUEGOS DE UNA SOLA COMPAÑIA DADA POR EL nomApp
        private Dictionary<string, Juego> sacarJuegosDeApp(string nomApp)
        {
            string rutaJson = System.IO.Path.GetFullPath(@"..\..\..\data.json");
            string json = File.ReadAllText(rutaJson);

            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Juego>>>(json);

            if (data.ContainsKey(nomApp))
            {
                return data[nomApp]; //DEVUELVO LOS JUEGOS DE ESA APP
            }

            return new Dictionary<string, Juego>(); //SI NO HAY JUEGOS, NO DEVUELVO NADA
        }

        //ESTE METODO BUSCA CREAR TODOS LOS BOTONES, COLCOAR SU IMAGEN Y SU RESPECTIVO METODO DE CLICK
        private void colocarBotones(Dictionary<string, Juego> juegosApp)
        {
            if (juegosApp.Count != 0)
            {
                //BUCLE PARA SACAR TODOS LOS JUEGOS
                foreach (var juego in juegosApp)
                {
                    //CREO LOS BOTONES DE CADA JUEGO Y LE ASIGNO EL TAMAÑO PREDEFINIDO
                    Button botonJuego = new Button
                    {
                        Width = 198,
                        Height = 298,
                        Margin = new Thickness(17),
                    };

                    //APLICO EL ESTILO QUE HE HECHO EN EL XAML
                    botonJuego.Style = (Style)FindResource("Boton_juego");

                    //CREO LA IMAGEN QUE IRA EN CADA BOTON
                    Image imagenJuego = new Image
                    {
                        Width = 198,
                        Height = 298,
                        Stretch = Stretch.Fill
                    };

                    //INTENTO CARGAR LA IMAGEN ORIGINAL
                    imagenJuego.Source = new BitmapImage(new Uri("https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/" + juego.Key + "/library_600x900.jpg"));

                    //SI NO SE HA PUESTO NINGUNA IMAGEN (ES DECIR, QUE ESTE JUEGO NO TIENE)
                    imagenJuego.ImageFailed += (sender, e) =>
                    {
                        imagenJuego.Stretch = Stretch.Uniform;
                        imagenJuego.Source = new BitmapImage(new Uri("https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/" + juego.Key + "/capsule_184x69.jpg?t=1739176298"));
                    };

                    //AGREGO LA IMAGEN AL BOTON
                    botonJuego.Content = imagenJuego;

                    //EVENTO CUANDO SE HACE CLICK EN UN BOTÓN
                    botonJuego.Click += (sender, e) =>
                    {

                        ventanaPrincipal.framePrincipal.Navigate(new panelJuego(nombreApp, juego, this, ventanaPrincipal));
                    };

                    //POR ÚLTIMO, LOS AGREGO AL PANEL DE JUEGOS
                    panelJuegos.Children.Add(botonJuego);
                }
            }
            else
            {

                TextBlock textBlock = new TextBlock
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = 40,
                    FontFamily = (FontFamily)this.Resources["FuenteJohnInclinada"],
                    Padding = new Thickness(20)
                };

                //TEXTO NORMAL
                textBlock.Inlines.Add(new Run
                {
                    Text = "THERE ARE NO GAMES ",
                    Foreground = Brushes.White
                });

                //TEXTO PARA EL FOR NOW
                textBlock.Inlines.Add(new Run
                {
                    Text = "FOR NOW",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A0DAD"))

                });

                //TEXTO PARA EL EMOJI
                textBlock.Inlines.Add(new Run
                {
                    Text = "🚧",
                    Foreground = Brushes.Yellow
                });

                //CREO EL BINDING DEL ANCHO
                Binding binding = new Binding("ActualWidth")
                {
                    Source = panelJuegos
                };
                textBlock.SetBinding(FrameworkElement.WidthProperty, binding);

                //CREO EL BORDER
                Border border = new Border
                {
                    Background = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Child = textBlock
                };

                panelJuegos.Children.Add(border);

            }
            
        }



    }
}
