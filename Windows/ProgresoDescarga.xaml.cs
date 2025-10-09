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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Project_Lightning.Windows
{
    public partial class ProgresoDescarga : Window
    {
        public ProgresoDescarga(string version)
        {
            InitializeComponent();
            TxtMensaje.Text = $"Downloading update version {version}...";
        }

        //METODO PARA ACTUALIZAR EL PROGRESO DE LA BARRA
        public void ActualizarProgreso(double porcentaje)
        {
            //Es necesario usar Dispatcher.Invoke para actualizar la UI desde un hilo diferente
            Dispatcher.Invoke(() =>
            {
                ProgressBarDescarga.Value = porcentaje;
                TxtProgreso.Text = $"{porcentaje:F0}%";

                if (porcentaje >= 99.9)
                {
                    TxtMensaje.Text = "Download complete. Starting installer...";
                }
            });
        }
    }
}