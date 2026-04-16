using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BibliotecaAvalonia.Controlador;
using BibliotecaAvalonia.Modelos;
using BibliotecaAvalonia.Vista;

namespace BibliotecaAvalonia
{
    public partial class App : Avalonia.Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Inicializar la BD al arrancar
                new gestor_bd().inicializar_bd();

                desktop.MainWindow = new ventanaMain()
                {
                    DataContext = new controlador_biblioteca()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}