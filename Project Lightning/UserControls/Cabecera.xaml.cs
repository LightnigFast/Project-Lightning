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

namespace Project_Lightning.UserControls
{
    /// <summary>
    /// Lógica de interacción para Cabecera.xaml
    /// </summary>
    public partial class Cabecera : UserControl
    {
        public event RoutedEventHandler UbisoftPresionado;
        public event RoutedEventHandler EAPresionado;
        public Cabecera()
        {
            InitializeComponent();
        }

        //UBISOFT CLICK
        public void ubisoft_click(object sender, RoutedEventArgs e)
        {
            UbisoftPresionado?.Invoke(this, e);
        }

        //EA CLICK
        public void ea_click(object sender, RoutedEventArgs e)
        {
            EAPresionado?.Invoke(this, e);
        }
    }
}
