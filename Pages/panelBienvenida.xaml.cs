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
using Project_Lightning.Windows;

namespace Project_Lightning.Pages
{
    /// <summary>
    /// Lógica de interacción para panelBienvenida.xaml
    /// </summary>
    public partial class panelBienvenida : Page
    {
        public panelBienvenida()
        {
            InitializeComponent();
        }

        private void legalButtonClick(object sender, RoutedEventArgs e)
        {
            var legalWindow = new LegalWindow();
            legalWindow.Show();
        }

        private void changeLogButtonClick(object sender, RoutedEventArgs e)
        {
            var changeLog = new ChangeLog();
            changeLog.Show();
        }

        private void quickGuideButtonClick(object sender, RoutedEventArgs e)
        {
            var quieckGuide = new QuickGuide();
            quieckGuide.Show();
        }


    }
}
