using System;
using System.IO;
using System.Collections.Generic;
using BibliotecaAvalonia.Desktop.Modelos;

namespace BibliotecaAvalonia.Desktop.Utilidades
{
    public static class csv_manejador
    {
        // EXPORTAR – todos los artículos 
        public static void exportar_csv(string ruta, List<Articulo> articulos)
        {
            using var writer = new StreamWriter(ruta);
            // Cabecera con todos los campos posibles
            writer.WriteLine("Tipo;Titulo;Anio;FechaAdquisicion;ISBN;EstaPrestado;FechaInicio;FechaFin;MediaValoraciones");

            foreach (var art in articulos)
            {
                double media = art is IValorable v ? v.CalcularMedia() : 0;

                if (art is Libro l)
                {
                    writer.WriteLine(
                        $"Libro;{Escapar(l.Titulo)};{l.Anio};{l.FechaAdquisicion:yyyy-MM-dd};" +
                        $"{l.Isbn};{l.EstaPrestado};;;{media:F2}");
                }
                else if (art is Audiolibro al)
                {
                    writer.WriteLine(
                        $"Audiolibro;{Escapar(al.Titulo)};{al.Anio};{al.FechaAdquisicion:yyyy-MM-dd};" +
                        $";;{al.FechaInicio:yyyy-MM-dd};{al.FechaFin:yyyy-MM-dd};{media:F2}");
                }
            }
        }

        // Sobrecarga para compatibilidad si alguien pasa List<Libro>
        public static void exportar_csv(string ruta, List<Libro> libros)
        {
            var todos = new List<Articulo>();
            foreach (var l in libros) todos.Add(l);
            exportar_csv(ruta, todos);
        }

        // IMPORTAR – lee un CSV previamente exportado por esta clase
        public static List<Articulo> importar_csv(string ruta)
        {
            var lista = new List<Articulo>();
            if (!File.Exists(ruta)) return lista;

            using var reader = new StreamReader(ruta);
            string? linea;
            bool primera = true;

            while ((linea = reader.ReadLine()) != null)
            {
                if (primera) { primera = false; continue; } 
                if (string.IsNullOrWhiteSpace(linea)) continue;

                var cols = linea.Split(';');
                if (cols.Length < 9) continue;

                try
                {
                    string tipo = cols[0].Trim();
                    string tit = cols[1].Trim();
                    int anio = int.Parse(cols[2].Trim());
                    var fechaAdq = DateTime.Parse(cols[3].Trim());

                    if (tipo == "Libro")
                    {
                        string isbn = cols[4].Trim();
                        bool prestado = cols[5].Trim().Equals("True", StringComparison.OrdinalIgnoreCase);
                        lista.Add(new Libro(tit, anio, fechaAdq, isbn) { EstaPrestado = prestado });
                    }
                    else if (tipo == "Audiolibro")
                    {
                        var inicio = DateTime.Parse(cols[6].Trim());
                        var fin = DateTime.Parse(cols[7].Trim());
                        lista.Add(new Audiolibro(tit, anio, fechaAdq, inicio, fin));
                    }
                }
                catch
                {
                    
                }
            }
            return lista;
        }

        private static string Escapar(string valor)
            => valor?.Replace(";", ",") ?? string.Empty;
    }
}
