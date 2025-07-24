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
    /// Lógica de interacción para panelOnlineFix.xaml
    /// </summary>
    public partial class panelOnlineFix : Page
    {
        MainWindow ventanaPrincipal;
        public panelOnlineFix(MainWindow ventanaPrincipal)
        {
            InitializeComponent();
            this.ventanaPrincipal = ventanaPrincipal;
        }

        //BOTON DE HOME
        private void home_click(object sender, RoutedEventArgs e)
        {

            ventanaPrincipal.framePrincipal.Navigate(new panelMenuPrincipal());
            ventanaPrincipal.ocultarCabecera();
        }

    }
}
