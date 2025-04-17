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
using Newtonsoft.Json;

namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interacción para panelUbisoft.xaml
    /// </summary>
    public partial class panelApp : Page
    {
        public panelApp(String nomApp)
        {
            InitializeComponent();
            
            //CAMBIO EL NOMBRE DE LA ETIQUETA
            txtApp.Text = nomApp;

            ponerJuegos(nomApp);

        }

        //CLASE JUEGO QUE CONTENDRA LA INFORMACIÓN DE CADA JUEGO
        public class Juego
        {
            public string name { get; set; }
            public bool launch_steam { get; set; }
            public bool launch_exe { get; set; }
            public string comentarios { get; set; }
            public string errores { get; set; }
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
            //BUCLE PARA SACAR TODOS LOS JUEGOS
            foreach (var juego in juegosApp)
            {
                //CREO LOS BOTONES DE CADA JUEGO Y LE ASIGNO EL TAMAÑO PREDEFINIDO
                Button botonJuego = new Button
                {
                    Width = 198,
                    Height = 298,
                    Margin = new Thickness(17)
                };

                //CREO LA IMAGEN QUE IRA EN CADA BOTON
                Image imagenJuego = new Image
                {
                    Width = 198,
                    Height = 298,
                    Stretch = Stretch.Fill,
                    Source = new BitmapImage(new Uri("https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/" + juego.Key + "/library_600x900.jpg"))
                };

                //AGREGO LA IMAGEN AL BOTON
                botonJuego.Content = imagenJuego;

                //POR ÚLTIMO, LOS AGREGO AL PANEL DE JUEGOS
                panelJuegos.Children.Add(botonJuego);
            }
        }

        //METODO PARA DESCARGAR LOS ARCHIVOS FIX DE UN JUEGO INTEPENDIENTE
        private async void descargarJuego(KeyValuePair<string, Juego> keyValuePair)
        {
            string appId = keyValuePair.Key;
            string user = "LightnigFast"; 
            string repo = "gamesFixes";
            string token = "token";

            string apiUrl = $"https://api.github.com/repos/{user}/{repo}/contents/{appId}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                string json = await client.GetStringAsync(apiUrl);
                var archivos = JsonConvert.DeserializeObject<List<ArchivoGitHub>>(json);

                string escritorio = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string carpetaDestino = System.IO.Path.Combine(escritorio, appId);
                Directory.CreateDirectory(carpetaDestino);

                foreach (var archivo in archivos)
                {
                    if (archivo.type == "file")
                    {
                        string rutaDestino = System.IO.Path.Combine(carpetaDestino, archivo.name);
                        byte[] datos = await client.GetByteArrayAsync(archivo.download_url);
                        File.WriteAllBytes(rutaDestino, datos);
                    }
                }

                MessageBox.Show("Descarga completada: " + carpetaDestino);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
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
