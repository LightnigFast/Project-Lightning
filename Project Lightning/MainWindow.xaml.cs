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
        }

        private void boton_ubisoft_presionado(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("UBISOFT");
        }

        //METODO PARA EA
        private void boton_ea_presionado(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("EA");
        }

    }
}
