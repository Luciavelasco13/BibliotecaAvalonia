using System;
using System.Collections.Generic;
using System.Linq;

namespace BibliotecaAvalonia.Desktop.Modelos
{
    public interface IPrestable
    {
        bool EstaPrestado { get; set; }
        int DiasMaximos { get; }
    }

    public interface IValorable
    {
        List<Valoracion> Valoraciones { get; set; }
        double CalcularMedia();
    }

    public class Valoracion
    {
        public int Nota { get; set; } // 0-10 
        public string Comentario { get; set; } = string.Empty; 
        public string UsuarioId { get; set; } = string.Empty;
    }

    public abstract class Articulo
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public int Anio { get; set; }
        public DateTime FechaAdquisicion { get; set; }

        // Constructor
        protected Articulo(string titulo, int anio, DateTime fecha)
        {
            Titulo = titulo;
            Anio = anio;
            FechaAdquisicion = fecha;
        }
    }

    public class Libro : Articulo, IPrestable, IValorable
    {
        public string Isbn { get; set; }
        public bool EstaPrestado { get; set; } = false;
        public int DiasMaximos => 31;   // atributo compartido por todos los prestables
        public List<Valoracion> Valoraciones { get; set; } = new();

        // Constructor 
        public Libro(string titulo, int anio, DateTime fecha, string isbn)
            : base(titulo, anio, fecha)
        {
            Isbn = isbn;
        }

        // calcula la media real de las notas
        public double CalcularMedia()
            => Valoraciones.Count == 0 ? 0 : Valoraciones.Average(v => v.Nota);

        // Añadir valoración con validación 
        public void AnadirValoracion(int nota, string usuarioId, string comentario = "")
        {
            if (nota < 0 || nota > 10)
                throw new ArgumentOutOfRangeException(nameof(nota), "La nota debe estar entre 0 y 10.");
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentException("El identificador de usuario es obligatorio.");

            Valoraciones.Add(new Valoracion
            {
                Nota = nota,
                Comentario = comentario,
                UsuarioId = usuarioId
            });
        }
    }

    public class Audiolibro : Articulo, IValorable
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<Valoracion> Valoraciones { get; set; } = new();

        // Constructor 
        public Audiolibro(string titulo, int anio, DateTime fecha, DateTime inicio, DateTime fin)
            : base(titulo, anio, fecha)
        {
            FechaInicio = inicio;
            FechaFin = fin;
        }

        // Disponible si la fecha actual está dentro del rango
        public bool EstaDisponible()
            => DateTime.Now >= FechaInicio && DateTime.Now <= FechaFin;

        public double CalcularMedia()
            => Valoraciones.Count == 0 ? 0 : Valoraciones.Average(v => v.Nota);

        public void AnadirValoracion(int nota, string usuarioId, string comentario = "")
        {
            if (nota < 0 || nota > 10)
                throw new ArgumentOutOfRangeException(nameof(nota), "La nota debe estar entre 0 y 10.");
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentException("El identificador de usuario es obligatorio.");

            Valoraciones.Add(new Valoracion
            {
                Nota = nota,
                Comentario = comentario,
                UsuarioId = usuarioId
            });
        }
    }
}
