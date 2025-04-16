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
    /// Lógica de interacción para panelUbisoft.xaml
    /// </summary>
    public partial class panelApp : Page
    {
        public panelApp(String nomApp)
        {
            InitializeComponent();
            
            //CAMBIO EL NOMBRE DE LA ETIQUETA
            txtApp.Text = nomApp;


        }
    }
}
