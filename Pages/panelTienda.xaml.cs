using Newtonsoft.Json;
using Project_Lightning.Windows;
using System;
using System.Collections.Generic;
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
using System.IO;


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
            



        }


    }
}
