using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Lógica de interacción para paginaBypass.xaml
    /// </summary>
    public partial class panelBypass : Page
    {
        MainWindow ventanaPrincipal;
        public panelBypass(MainWindow ventanaPrincipal)
        {
            InitializeComponent();

            this.ventanaPrincipal = ventanaPrincipal;
        }

        //CLICK DEL BOTON DE VOLVER
        private void volverClick(object sender, RoutedEventArgs e)
        {
            ventanaPrincipal.framePrincipal.Navigate(new panelMenuPrincipal());
        }


        //METODO PARA UBISOFT
        private void boton_ubisoft_presionado(object sender, RoutedEventArgs e)
        {
            ventanaPrincipal.framePrincipal.Navigate(new panelApp("UBISOFT", ventanaPrincipal));
            ventanaPrincipal.mostrarCabecera();
        }

        //METODO PARA EA
        private void boton_ea_presionado(object sender, RoutedEventArgs e)
        {
            ventanaPrincipal.framePrincipal.Navigate(new panelApp("EA", ventanaPrincipal));
            ventanaPrincipal.mostrarCabecera();
        }

        //METODO PARA ROCKSTAR
        private void boton_rockstar_presionado(object sender, RoutedEventArgs e)
        {
            ventanaPrincipal.framePrincipal.Navigate(new panelApp("ROCKSTAR", ventanaPrincipal));
            ventanaPrincipal.mostrarCabecera();
        }

        //METODO PARA DENUVO
        private void boton_denuvo_presionado(object sender, RoutedEventArgs e)
        {
            ventanaPrincipal.framePrincipal.Navigate(new panelApp("DENUVO", ventanaPrincipal));
            ventanaPrincipal.mostrarCabecera();
        }

        //METODO PARA PLAY STATION
        private void boton_playstation_presionado(object sender, RoutedEventArgs e)
        {
            ventanaPrincipal.framePrincipal.Navigate(new panelApp("PlayStation", ventanaPrincipal));
            ventanaPrincipal.mostrarCabecera();
        }

        //METODO PARA OTHERS
        private void boton_others_presionado(object sender, RoutedEventArgs e)
        {
            ventanaPrincipal.framePrincipal.Navigate(new panelApp("OTHERS", ventanaPrincipal));
            ventanaPrincipal.mostrarCabecera();
        }
    }
}
