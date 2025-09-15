using Project_Lightning.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Project_Lightning
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override async void OnStartup(StartupEventArgs e)
        {
            //FORZAR RENDERIZADO POR GPU SI ESTÁ DISPONIBLE
            RenderOptions.ProcessRenderMode = RenderMode.Default;

            base.OnStartup(e);

            await Actualizador.ComprobarActualizacion();

        }

    }


}
