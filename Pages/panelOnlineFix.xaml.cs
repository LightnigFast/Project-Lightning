using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Project_Lightning.Windows;
using SharpCompress.Archives;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TokenProvider;
using static Project_Lightning.Pages.panelJuego;

namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interacción para panelOnlineFix.xaml
    /// </summary>
    public partial class panelOnlineFix : Page
    {
        MainWindow ventanaPrincipal;

        //VARIABLES PARA LA PAGINACION
        private const int JuegosPorPagina = 18;
        private int paginaActual = 1;
        private Dictionary<string, Juego> todosLosJuegos = new Dictionary<string, Juego>();

        //DICCIONARIO PARA EL ESTADO DEL BOTON
        private Dictionary<string, string> estadoBotonesFix = new Dictionary<string, string>();

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
            todosLosJuegos = await sacarJuegosDeApp();
            MostrarPagina(paginaActual);

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

        private void MostrarPagina(int pagina)
        {
            panelJuegos.Children.Clear(); //LIMPIA PANEL

            var juegosPaginados = todosLosJuegos
                .Skip((pagina - 1) * JuegosPorPagina)
                .Take(JuegosPorPagina)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            colocarBotones(juegosPaginados);

            //VOLVER ARRIBA
            scrollViewerJuegos.ScrollToVerticalOffset(0);

            //ACTUALIZAR BOTONES DE PÁGINA
            ActualizarBotonesDePagina();
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
                    Width = 260,
                    Height = 270,
                    Margin = new Thickness(20),
                    Background = (Brush)new BrushConverter().ConvertFrom("#1E1E1E"),
                };

                //CREAR EL TRANSFORM PARA ZOOM
                var scaleTransform = new ScaleTransform(1.0, 1.0);
                contenedor.RenderTransform = scaleTransform;
                contenedor.RenderTransformOrigin = new Point(0.5, 0.5); //CENTRAR ESCALADO

                //EVENTO: MOUSE ENTER -> ZOOM IN
                contenedor.MouseEnter += (s, e) =>
                {
                    var zoomIn = new DoubleAnimation
                    {
                        To = 1.05,
                        Duration = TimeSpan.FromMilliseconds(150),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, zoomIn);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, zoomIn);
                };

                //EVENTO: MOUSE LEAVE -> ZOOM OUT
                contenedor.MouseLeave += (s, e) =>
                {
                    var zoomOut = new DoubleAnimation
                    {
                        To = 1.0,
                        Duration = TimeSpan.FromMilliseconds(150),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, zoomOut);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, zoomOut);
                };


                //DEFINIR 3 FILAS
                contenedor.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) }); //IMAGEN
                contenedor.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); //NOMBRE
                contenedor.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); //ESPACIADOR
                contenedor.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }); //BOTÓN

                //IMAGEN (SE ADAPTA A LA FILA)
                Image imagen = new Image
                {
                    Source = new BitmapImage(new Uri(juego.custom_images)),
                    Stretch = Stretch.Uniform, //MANTENGO LA PROPORCION
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top, //EVITO QUE SE ESTIRE VERTICALMENTE
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
                    VerticalAlignment = VerticalAlignment.Top,
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
                    Margin = new Thickness(5),
                    Tag = kvp.Key
                };
                botonFix.Click += BotonFix_Click;
                botonFix.Style = (Style)this.FindResource("MinimalButtonStyle");
                AplicarEsquinasRedondeadas(botonFix, 10);
                string estado = estadoBotonesFix.ContainsKey(kvp.Key) ? estadoBotonesFix[kvp.Key] : "Apply Fix";

                switch (estado)
                {
                    case "Downloading":
                        ponerBotonDescargando(botonFix);
                        break;
                    case "Ready":
                        ponerBotonListo(botonFix);
                        break;
                    default:
                        ponerBotonApplyFix(botonFix);
                        break;
                }


                Grid.SetRow(botonFix, 3);
                contenedor.Children.Add(botonFix);

                AplicarEsquinasRedondeadas(contenedor, 15);

                //AÑADIR A WRAPPANEL
                panelJuegos.Children.Add(contenedor);
            }
        }

        private void AplicarEsquinasRedondeadas(FrameworkElement contenedor, double radio)
        {
            contenedor.Loaded += (s, e) =>
            {
                var rect = new RectangleGeometry
                {
                    RadiusX = radio,
                    RadiusY = radio,
                    Rect = new Rect(0, 0, contenedor.ActualWidth, contenedor.ActualHeight)
                };

                contenedor.Clip = rect;

                contenedor.SizeChanged += (s2, e2) =>
                {
                    rect.Rect = new Rect(0, 0, contenedor.ActualWidth, contenedor.ActualHeight);
                };
            };
        }


        private void BotonFix_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.Tag is string appId)
            {
                if (todosLosJuegos.TryGetValue(appId, out Juego juego))
                {
                    //MessageBox.Show($"Apply fix para el juego: {juego.name}");

                    boton.IsEnabled = false;
                    try
                    {
                        if (todosLosJuegos.TryGetValue(appId, out Juego juegoEncontrado))
                        {
                            descargarJuego(new KeyValuePair<string, Juego>(appId, juegoEncontrado), boton);
                        }

                    }
                    catch (Exception)
                    {
                        var ventanaError = new Windows.ErrorDialog("An unexpected error occurred while trying to download the game " + juego.name +
                            ". Please wait a few minutes and, if the error persists, contact the developer.", Brushes.Red);
                        ventanaError.ShowDialog();
                        boton.IsEnabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("No se encontró el juego con el appId: " + appId);
                }
            }
        }



        private void ActualizarBotonesDePagina()
        {
            paginacionPanel.Children.Clear();
            int totalPaginas = (int)Math.Ceiling((double)todosLosJuegos.Count / JuegosPorPagina);

            for (int i = 1; i <= totalPaginas; i++)
            {
                Button boton = new Button
                {
                    Content = i.ToString(),
                    Margin = new Thickness(5),
                    Height = 35,
                    Width = 35,
                    Padding = new Thickness(10, 5, 10, 5),
                    Tag = i,
                    Style = (Style)this.FindResource("MinimalButtonStyle")
                };

                //ESTILO PERSONALIZADO SI ES LA PÁGINA ACTUAL
                if (i == paginaActual)
                {
                    boton.Background = Brushes.Transparent;
                    boton.Foreground = Brushes.White;
                    boton.BorderBrush = Brushes.White;
                }

                boton.Click += (s, e) =>
                {
                    paginaActual = (int)((Button)s).Tag;
                    MostrarPagina(paginaActual);
                };

                paginacionPanel.Children.Add(boton);
            }
        }

        //METODO PARA LA BARRA DE BUSQUEDA
        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = txtBuscar.Text.Trim().ToLower();

            //SI ESTÁ VACÍO, MOSTRAR LA PÁGINA ACTUAL
            if (string.IsNullOrWhiteSpace(texto))
            {
                MostrarPagina(paginaActual);
                return;
            }

            //FILTRAR JUEGOS POR NOMBRE O NOMBRE_FIX
            var juegosFiltrados = todosLosJuegos
                .Where(kvp =>
                    kvp.Value.name.ToLower().Contains(texto) ||
                    kvp.Value.nombre_fix.ToLower().Contains(texto))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            //LIMPIAR Y MOSTRAR LOS JUEGOS FILTRADOS
            panelJuegos.Children.Clear();
            colocarBotones(juegosFiltrados);
        }

        //METODO PARA DECARGAR EL JUEGO
        private async void descargarJuego(KeyValuePair<string, Juego> keyValuePair, Button fixButton)
        {
            string appId = keyValuePair.Key;
            string user = "LightnigFast";
            string repo = "onlineFixes";
            string token = TokenManager.GetGithubFixToken();

            string apiUrl = $"https://api.github.com/repos/{user}/{repo}/contents/{appId}";

            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);


                string json = await client.GetStringAsync(apiUrl);

                if (string.IsNullOrEmpty(json))
                {
                    var ventanaError = new Windows.ErrorDialog("Failed to authenticate with GitHub. Please check your token.", Brushes.Red);
                    ventanaError.ShowDialog();
                    fixButton.IsEnabled = true;
                    return;
                }

                var archivos = JsonConvert.DeserializeObject<List<ArchivoGitHub>>(json);

                //CommonOpenFileDialog PARA SELECCIONAR EL SELECTOR DE FICHEROS DE WINDOWS
                var folderDialog = new CommonOpenFileDialog();
                folderDialog.IsFolderPicker = true;
                folderDialog.Title = "Select the game folder of " + keyValuePair.Value.name + ":";

                if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string carpetaDestino = folderDialog.FileName;

                    //VERIFICAR QUE LA CARPETA SELCCIONADA ESTE DENTRO DE "steamapps/common"
                    string carpetaSteamApps = System.IO.Path.Combine(carpetaDestino, "steamapps");
                    string carpetaCommon = System.IO.Path.Combine(carpetaSteamApps, "common");

                    //VERIFICAR QUE LA CARPETA SELCCIONADA ESTE DENTRO DE "steamapps/common"
                    if (!carpetaDestino.Contains("steamapps\\common"))
                    {
                        var ventanaError3 = new Windows.ErrorDialog("You must select the game folder of " + keyValuePair.Value.name + ":", Brushes.Red);
                        ventanaError3.ShowDialog();
                        fixButton.IsEnabled = true;

                        return;
                    }

                    //COMPRUEBO SI LA CARPETA SELECCIONADA ES COMMON
                    if (System.IO.Path.GetFileName(carpetaDestino).Equals("common", StringComparison.OrdinalIgnoreCase))
                    {
                        var ventanaError2 = new Windows.ErrorDialog("You must select the game folder of " + keyValuePair.Value.name + ":", Brushes.Red);
                        ventanaError2.ShowDialog();
                        fixButton.IsEnabled = true;
                        return;
                    }

                    //SI LLEGAMOS HASTA AQUI, LA CARPETA SELCCIONADA ES CORRECTA Y ESTA DENTRO DE "STEAMAPPS/COMMON"

                    //VERIFICAR QUE LA SUBCARPETA DEL JUEGO DENTRO DE 'COMMON' NO EXISTA, SI EXISTE LA ELIMINAMOS
                    string carpetaJuego = System.IO.Path.Combine(carpetaCommon, "game");
                    if (Directory.Exists(carpetaJuego))
                    {
                        Directory.Delete(carpetaJuego, true);  //ELIMINO LA CARPETA DEL JUEGO SI YA EXISTE
                    }

                    string rutaZipExtraer = null;
                    string carpetaTemporal = System.IO.Path.Combine(carpetaDestino, "temp_extraccion");

                    //CREAR CARPETA TEMPORAL SI NO EXISTE
                    if (!Directory.Exists(carpetaTemporal))
                    {
                        Directory.CreateDirectory(carpetaTemporal);
                    }


                    //DESCARGAR TODO
                    gridPrincipal.IsEnabled = false;
                    ponerBotonDescargando(fixButton);

                    foreach (var archivo in archivos)
                    {
                        if (archivo.type == "file")
                        {
                            string rutaDestino = System.IO.Path.Combine(carpetaDestino, archivo.name);

                            //VERIFICAR SI EL ARCHIVO YA EXISTE Y ELIMINARLO ANTES
                            if (File.Exists(rutaDestino))
                            {
                                try
                                {
                                    //INTENTO ELIMINAR EL ARCHIVO, SI SE ESTÁ USANDO, ESPERO ANTES DE ELIMINAR
                                    File.Delete(rutaDestino);
                                    await Task.Delay(100);  //ESPERO POR SI EL ARCHIVO SE ESTA USANDO
                                }
                                catch (IOException)
                                {
                                    try
                                    {
                                        //FUERZO ELIMINAICION
                                        File.Delete(rutaDestino);
                                    }
                                    catch (IOException ioEx)
                                    {
                                        var ventanaError4 = new Windows.ErrorDialog("Error trying to delete the file: " + ioEx.Message, Brushes.Red);
                                        ventanaError4.ShowDialog();
                                        fixButton.IsEnabled = true;
                                        gridPrincipal.IsEnabled = true;
                                        ponerBotonApplyFix(fixButton);
                                        //System.Windows.MessageBox.Show("Error trying to delete the file: " + ioEx.Message);
                                        return;
                                    }
                                }
                            }

                            byte[] datos = await client.GetByteArrayAsync(archivo.download_url);
                            File.WriteAllBytes(rutaDestino, datos);

                            //GUARDO LA RUTA AL PRIMER ZIP ENCONTRADO PARA EXTRAERLO DESPUÉS
                            if (archivo.name.EndsWith(".zip") && rutaZipExtraer == null)
                                rutaZipExtraer = rutaDestino;

                        }
                    }


                    //EXTRAIGO EL ZIP SI EXISTE
                    if (rutaZipExtraer != null)
                    {
                        //PARA MANEJAR LOS ARCHIVOS ZIP
                        using (var archive = ArchiveFactory.Open(rutaZipExtraer))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                if (!entry.IsDirectory)
                                {
                                    string filePath = System.IO.Path.Combine(carpetaTemporal, entry.Key);
                                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath)); //CREAR DIRECTORIOS SI NO EXISTEN

                                    //EXTRAIGO EL ARCHIVO
                                    using (var fileStream = File.Create(filePath))
                                    {
                                        entry.WriteTo(fileStream);  //EXTRAIGO EL CONTENIDO DEL ARCHIVO
                                    }
                                }
                            }
                        }

                        //MUEVO EL CONTENIDO EXTRAÍDO A LA CARPETA FINAL (REEMPLAZAR SI YA EXISTEN)
                        foreach (var archivoExtraido in Directory.GetFiles(carpetaTemporal, "*", SearchOption.AllDirectories))
                        {
                            string nombreRelativo = archivoExtraido.Substring(carpetaTemporal.Length + 1);
                            string rutaFinal = System.IO.Path.Combine(carpetaDestino, nombreRelativo);

                            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(rutaFinal));

                            //SI EL ARCHIVO YA EXISTE LO REMPLAZO
                            if (File.Exists(rutaFinal))
                            {
                                File.Delete(rutaFinal);
                            }

                            File.Move(archivoExtraido, rutaFinal);
                        }

                        //ELIMINO ARCHIVOS TEMPORALES
                        Directory.Delete(carpetaTemporal, true);
                        File.Delete(rutaZipExtraer);

                        //ELIMINO TODOS LOS FRAGMENTOS DEL ZIP (EJ: .z01, .z02, etc.)
                        string zipBaseName = System.IO.Path.GetFileNameWithoutExtension(rutaZipExtraer);
                        string zipDir = System.IO.Path.GetDirectoryName(rutaZipExtraer);
                        var fragmentos = Directory.GetFiles(zipDir, zipBaseName + ".*")
                                                  .Where(f => !f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

                        foreach (var frag in fragmentos)
                        {
                            try
                            {
                                File.Delete(frag);
                            }
                            catch (Exception ex)
                            {
                                var ventanaError5 = new Windows.ErrorDialog("Could not delete fragment file: " + ex.Message, Brushes.Red);
                                ventanaError5.ShowDialog();
                                fixButton.IsEnabled = true;
                                gridPrincipal.IsEnabled = true;
                                ponerBotonApplyFix(fixButton);
                            }
                        }
                    }

                    var ventanaError = new Windows.ErrorDialog("Game ready to play", Brushes.Green);
                    ponerBotonListo(fixButton);
                    ventanaError.ShowDialog();
                    fixButton.IsEnabled = true;
                    gridPrincipal.IsEnabled = true;

                    mostrarMensajeDeDonacion();
                    //System.Windows.MessageBox.Show("Download and extraction completed: " + carpetaDestino);
                }
                else
                {
                    var ventanaError = new Windows.ErrorDialog("No folder selected.", Brushes.Red);
                    ventanaError.ShowDialog();
                    fixButton.IsEnabled = true;
                    gridPrincipal.IsEnabled = true;
                    ponerBotonApplyFix(fixButton);
                    //System.Windows.MessageBox.Show("No folder selected.");
                }
            }
            catch (Exception ex)
            {
                var ventanaError = new Windows.ErrorDialog("ERROR: " + ex.Message, Brushes.Red);
                ventanaError.ShowDialog();
                fixButton.IsEnabled = true;
                gridPrincipal.IsEnabled = true;
                ponerBotonApplyFix(fixButton);
                //System.Windows.MessageBox.Show("ERROR: " + ex.Message);
            }

        }

        private void mostrarMensajeDeDonacion()
        {
            Random random = new Random();
            //PROBABILIDAD DEL 50%
            if (random.NextDouble() < 0.5)
            {
                //MOSTRAR LA VENTANA DE DONACIÓN
                var ventana = new Windows.DonateWindow();
                ventana.ShowDialog();
            }
        }


        //PARA LOS COLORES DEL BOTON
        private void ponerBotonDescargando(Button fixButton)
        {
            fixButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007BCF"));
            fixButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007BCF"));
            fixButton.Content = "Downloading";

            if (fixButton.Tag is string appId)
                estadoBotonesFix[appId] = "Downloading";
        }

        private void ponerBotonListo(Button fixButton)
        {
            fixButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00B515"));
            fixButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00B515"));
            fixButton.Content = "Ready";

            if (fixButton.Tag is string appId)
                estadoBotonesFix[appId] = "Ready";
        }

        private void ponerBotonApplyFix(Button fixButton)
        {
            fixButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A0DAD"));
            fixButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A0DAD"));
            fixButton.Content = "Apply Fix";

            if (fixButton.Tag is string appId)
                estadoBotonesFix[appId] = "Apply Fix";
        }

    }


}
