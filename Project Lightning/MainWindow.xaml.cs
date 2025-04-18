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
using Project_Lightning.Pages;
using Project_Lightning.UserControls;

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

            Cabecera.UbisoftPresionado += boton_ubisoft_presionado;
            Cabecera.EAPresionado += boton_ea_presionado;
            Cabecera.RockstarPresionado += boton_rockstar_presionado;
            Cabecera.DenuvoPresionado += boton_denuvo_presionado;
            Cabecera.PlayStationPresionado += boton_playstation_presionado;
            Cabecera.OthersPresionado += boton_others_presionado;
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




    }
}
