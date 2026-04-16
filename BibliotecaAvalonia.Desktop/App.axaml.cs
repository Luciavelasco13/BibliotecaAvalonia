using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BibliotecaAvalonia.Desktop.Controlador;
using BibliotecaAvalonia.Desktop.Modelos;
using BibliotecaAvalonia.Desktop.Vista;

namespace BibliotecaAvalonia.Desktop
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