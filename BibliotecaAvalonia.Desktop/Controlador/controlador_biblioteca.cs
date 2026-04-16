using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using BibliotecaAvalonia.Desktop.Modelos;
using BibliotecaAvalonia.Desktop.Utilidades;

namespace BibliotecaAvalonia.Desktop.Controlador
{
    public partial class controlador_biblioteca : ObservableObject
    {
        private readonly gestor_bd _bd = new gestor_bd();

        // Propiedades para el libro
        [ObservableProperty] private string _txtTitulo = string.Empty;
        [ObservableProperty] private string _txtIsbn = string.Empty;
        [ObservableProperty] private string _txtAnio = string.Empty; 

        // Propiedades para Audiolibro
        [ObservableProperty] private string _txtFechaInicio = string.Empty;
        [ObservableProperty] private string _txtFechaFin = string.Empty;

        // Propiedades para Valoración
        [ObservableProperty] private string _txtNota = string.Empty;
        [ObservableProperty] private string _txtComentario = string.Empty;
        [ObservableProperty] private string _txtUsuarioId = string.Empty;

        // Propiedades para Búsqueda
        [ObservableProperty] private string _txtBusqueda = string.Empty;
        [ObservableProperty] private string _filtroTipo = string.Empty; 

        // Mensajes de estado 
        [ObservableProperty] private string _mensajeEstado = string.Empty;

        public ObservableCollection<Articulo> ListaArticulos { get; } = new();
        public ObservableCollection<Articulo> ListaFiltrada { get; } = new();

        // Artículo actualmente seleccionado
        [ObservableProperty] private Articulo? _articuloSeleccionado;

        public controlador_biblioteca()
        {
            CargarDatos();
        }

        // Carga / recarga desde la BD
        private void CargarDatos()
        {
            var datos = _bd.obtener_articulos();
            ListaArticulos.Clear();
            ListaFiltrada.Clear();
            foreach (var a in datos)
            {
                ListaArticulos.Add(a);
                ListaFiltrada.Add(a);
            }
        }

        // AGREGAR LIBRO
        [RelayCommand]
        public void AgregarLibro()
        {
            MensajeEstado = string.Empty;

            if (!int.TryParse(TxtAnio, out int anio) || !Ayuda.validar_anio(anio))
            {
                MensajeEstado = $"Año inválido. Debe estar entre 1500 y {DateTime.Now.Year}.";
                return;
            }

            if (!Ayuda.validar_isbn(TxtIsbn))
            {
                MensajeEstado = "ISBN-10 inválido.";
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtTitulo))
            {
                MensajeEstado = "El título es obligatorio.";
                return;
            }

            var nuevo = new Libro(Ayuda.formatear_titulo(TxtTitulo), anio, DateTime.Now, TxtIsbn.Trim());
            _bd.insertar_articulo(nuevo);  
            ListaArticulos.Add(nuevo);
            ListaFiltrada.Add(nuevo);

            // Limpiar campos
            TxtTitulo = TxtIsbn = TxtAnio = string.Empty;
            MensajeEstado = "Libro añadido correctamente.";
        }

        // AGREGAR AUDIOLIBRO
        [RelayCommand]
        public void AgregarAudiolibro()
        {
            MensajeEstado = string.Empty;

            if (!int.TryParse(TxtAnio, out int anio) || !Ayuda.validar_anio(anio))
            {
                MensajeEstado = $"Año inválido. Debe estar entre 1500 y {DateTime.Now.Year}.";
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtTitulo))
            {
                MensajeEstado = "El título es obligatorio.";
                return;
            }

            if (!DateTime.TryParse(TxtFechaInicio, out DateTime inicio) ||
                !DateTime.TryParse(TxtFechaFin, out DateTime fin))
            {
                MensajeEstado = "Fechas de disponibilidad inválidas.";
                return;
            }

            var nuevo = new Audiolibro(Ayuda.formatear_titulo(TxtTitulo), anio, DateTime.Now, inicio, fin);
            _bd.insertar_articulo(nuevo);
            ListaArticulos.Add(nuevo);
            ListaFiltrada.Add(nuevo);

            TxtTitulo = TxtAnio = TxtFechaInicio = TxtFechaFin = string.Empty;
            MensajeEstado = "Audiolibro añadido correctamente.";
        }

        // EDITAR – actualiza el artículo seleccionado
        [RelayCommand]
        public void Editar()
        {
            MensajeEstado = string.Empty;
            if (ArticuloSeleccionado == null) { MensajeEstado = "Selecciona un artículo primero."; return; }

            if (!int.TryParse(TxtAnio, out int anio) || !Ayuda.validar_anio(anio))
            {
                MensajeEstado = $"Año inválido.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(TxtTitulo))
                ArticuloSeleccionado.Titulo = Ayuda.formatear_titulo(TxtTitulo);

            ArticuloSeleccionado.Anio = anio;

            if (ArticuloSeleccionado is Libro l && Ayuda.validar_isbn(TxtIsbn))
                l.Isbn = TxtIsbn.Trim();

            if (ArticuloSeleccionado is Audiolibro al)
            {
                if (DateTime.TryParse(TxtFechaInicio, out var ini)) al.FechaInicio = ini;
                if (DateTime.TryParse(TxtFechaFin, out var fin)) al.FechaFin = fin;
            }

            _bd.actualizar_articulo(ArticuloSeleccionado);
            CargarDatos();
            MensajeEstado = "Artículo actualizado.";
        }

        // ELIMINAR
        [RelayCommand]
        public void Eliminar()
        {
            MensajeEstado = string.Empty;
            if (ArticuloSeleccionado == null) { MensajeEstado = "Selecciona un artículo primero."; return; }

            _bd.eliminar_articulo(ArticuloSeleccionado.Id);
            ListaArticulos.Remove(ArticuloSeleccionado);
            ListaFiltrada.Remove(ArticuloSeleccionado);
            ArticuloSeleccionado = null;
            MensajeEstado = "Artículo eliminado.";
        }

        // PRÉSTAMO – marcar/desmarcar prestado (solo libros)
        [RelayCommand]
        public void CambiarEstadoPrestamo()
        {
            MensajeEstado = string.Empty;
            if (ArticuloSeleccionado is not Libro libro)
            {
                MensajeEstado = "Solo los libros son prestables.";
                return;
            }
            libro.EstaPrestado = !libro.EstaPrestado;
            _bd.actualizar_articulo(libro);
            MensajeEstado = libro.EstaPrestado ? "Libro marcado como prestado." : "Libro disponible.";
        }

        // AÑADIR VALORACIÓN al artículo seleccionado
        [RelayCommand]
        public void AnadirValoracion()
        {
            MensajeEstado = string.Empty;

            if (ArticuloSeleccionado == null)
            {
                MensajeEstado = "Selecciona un artículo primero.";
                return;
            }

            if (!int.TryParse(TxtNota, out int nota) || !Ayuda.validar_nota(nota))
            {
                MensajeEstado = "La nota debe ser un número entre 0 y 10.";
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtUsuarioId))
            {
                MensajeEstado = "El identificador de usuario es obligatorio.";
                return;
            }

            var valoracion = new Valoracion
            {
                Nota = nota,
                Comentario = TxtComentario.Trim(),
                UsuarioId = TxtUsuarioId.Trim()
            };

            if (ArticuloSeleccionado is Libro lb)
                lb.AnadirValoracion(nota, TxtUsuarioId.Trim(), TxtComentario.Trim());
            else if (ArticuloSeleccionado is Audiolibro al)
                al.AnadirValoracion(nota, TxtUsuarioId.Trim(), TxtComentario.Trim());

            _bd.insertar_valoracion(ArticuloSeleccionado.Id, valoracion);

            TxtNota = TxtComentario = TxtUsuarioId = string.Empty;
            MensajeEstado = "Valoración añadida correctamente.";
        }

        // BUSCAR
        [RelayCommand]
        public void Buscar()
        {
            string[] titulos = string.IsNullOrWhiteSpace(TxtBusqueda)
                ? Array.Empty<string>()
                : new[] { TxtBusqueda.Trim() };

            string? tipo = string.IsNullOrWhiteSpace(FiltroTipo) ? null : FiltroTipo;

            var resultado = _bd.buscar_items(titulos, tipo);

            ListaFiltrada.Clear();
            foreach (var a in resultado) ListaFiltrada.Add(a);
            MensajeEstado = $"{resultado.Count} resultado(s) encontrado(s).";
        }

        [RelayCommand]
        public void LimpiarBusqueda()
        {
            TxtBusqueda = FiltroTipo = string.Empty;
            ListaFiltrada.Clear();
            foreach (var a in ListaArticulos) ListaFiltrada.Add(a);
            MensajeEstado = string.Empty;
        }

        // EXPORTAR CSV 
        [RelayCommand]
        public void Exportar()
        {
            csv_manejador.exportar_csv("datos.csv", new List<Articulo>(ListaArticulos));
            MensajeEstado = "Catálogo exportado a datos.csv";
        }

        // IMPORTAR CSV
        [RelayCommand]
        public void Importar()
        {
            var importados = csv_manejador.importar_csv("datos.csv");
            int nuevos = 0;
            foreach (var art in importados)
            {
                _bd.insertar_articulo(art);
                ListaArticulos.Add(art);
                ListaFiltrada.Add(art);
                nuevos++;
            }
            MensajeEstado = $"{nuevos} artículo(s) importados desde datos.csv";
        }

        // Cargar datos del artículo seleccionado en el formulario
        partial void OnArticuloSeleccionadoChanged(Articulo? value)
        {
            if (value == null) return;
            TxtTitulo = value.Titulo;
            TxtAnio = value.Anio.ToString();
            if (value is Libro l)
            {
                TxtIsbn = l.Isbn;
                TxtFechaInicio = string.Empty;
                TxtFechaFin = string.Empty;
            }
            else if (value is Audiolibro al)
            {
                TxtIsbn = string.Empty;
                TxtFechaInicio = al.FechaInicio.ToString("yyyy-MM-dd");
                TxtFechaFin = al.FechaFin.ToString("yyyy-MM-dd");
            }
        }
    }
}
