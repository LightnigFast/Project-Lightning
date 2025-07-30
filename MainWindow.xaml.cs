using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using Project_Lightning.Pages;
using Project_Lightning.UserControls;
using Project_Lightning.Windows;

namespace Project_Lightning
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            
            InitializeComponent();
            BackgroundVideo.Play();

            string versionLocal = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            txtVersion.Text = "Version: " + versionLocal + " ";

            ocultarCabecera();
            Cabecera.HomePresionado += boton_home_presionado;
            Cabecera.UbisoftPresionado += boton_ubisoft_presionado;
            Cabecera.EAPresionado += boton_ea_presionado;
            Cabecera.RockstarPresionado += boton_rockstar_presionado;
            Cabecera.DenuvoPresionado += boton_denuvo_presionado;
            Cabecera.PlayStationPresionado += boton_playstation_presionado;
            Cabecera.OthersPresionado += boton_others_presionado;


            framePrincipal.Navigating += (s, e) =>
            {
                //BLOQUEO LA NAVEGACIÓN SI EL MOTIVO ES "MouseButton"
                if (e.NavigationMode == NavigationMode.Back || e.NavigationMode == NavigationMode.Forward)
                {
                    e.Cancel = true; //CANCELO la navegación
                }
            };

            //CUANDO LA VENTANA SE CARGUE POR COMPLETO, ABRO LA VENTANA DMCA
            this.Loaded += (s, e) =>
            {
                string ultimaVersion = Properties.Settings.Default.UltimaVersionConDMCA;

                if (ultimaVersion != versionLocal) //SI ES LA PRIMERA VEZ EN ESTA VERSIÓN
                {
                    var DMCA_Window = new DMCA_Window
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    DMCA_Window.ShowDialog();

                    //GUARDO QUE YA SE MOSTRÓ EN ESTA VERSIÓN
                    Properties.Settings.Default.UltimaVersionConDMCA = versionLocal;
                    Properties.Settings.Default.Save();
                }
            };


        }

        //METODO PARA UBISOFT
        private void boton_home_presionado(object sender, RoutedEventArgs e)
        {

            framePrincipal.Navigate(new panelMenuPrincipal());
            ocultarCabecera();
        }

        //METODO PARA UBISOFT
        private void boton_ubisoft_presionado(object sender, RoutedEventArgs e)
        {

            framePrincipal.Navigate(new panelApp("UBISOFT", this));
        }

        //METODO PARA EA
        private void boton_ea_presionado(object sender, RoutedEventArgs e)
        {
            framePrincipal.Navigate(new panelApp("EA", this));
        }

        //METODO PARA ROCKSTAR
        private void boton_rockstar_presionado(object sender, RoutedEventArgs e)
        {
            framePrincipal.Navigate(new panelApp("ROCKSTAR", this));
        }

        //METODO PARA DENUVO
        private void boton_denuvo_presionado(object sender, RoutedEventArgs e)
        {
            framePrincipal.Navigate(new panelApp("DENUVO", this));
        }

        //METODO PARA PLAY STATION
        private void boton_playstation_presionado(object sender, RoutedEventArgs e)
        {
            framePrincipal.Navigate(new panelApp("PlayStation", this));
        }

        //METODO PARA OTHERS
        private void boton_others_presionado(object sender, RoutedEventArgs e)
        {
            framePrincipal.Navigate(new panelApp("OTHERS", this));
        }

        private void BackgroundVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show("Error al cargar el video: " + e.ErrorException.Message);
        }

        private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Reinicia el video al principio
                BackgroundVideo.Position = TimeSpan.Zero;
                BackgroundVideo.Play();
            }
            catch (Exception ex)
            {
                // Manejo de excepciones para evitar que la app se cierre
                MessageBox.Show("Error al reiniciar el video: " + ex.Message);
            }
        }

        //METODO PARA RESTAURAR LA CABECERA
        public void ocultarCabecera()
        {
            Cabecera.Height = 0;
        }

        //METODO PARA OCULTAR LA CABECERA
        public void mostrarCabecera()
        {
            Cabecera.Height = 70;
        }

    }
}
