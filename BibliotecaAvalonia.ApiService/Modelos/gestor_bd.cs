using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System;

namespace BibliotecaAvalonia.Modelos
{
    public class gestor_bd
    {
        private string _con = "Data Source=biblioteca.db";

        // ---------------------------------------------------------------
        // Crea y retorna una conexión a la BD (requerido por el enunciado)
        // ---------------------------------------------------------------
        public SqliteConnection crear_conexion_bd(string ruta)
        {
            var connection = new SqliteConnection($"Data Source={ruta}");
            connection.Open();
            return connection;
        }

        // ---------------------------------------------------------------
        // Crea las tablas si no existen (recibe conexión)
        // ---------------------------------------------------------------
        public void inicializar_bd()
        {
            using var connection = new SqliteConnection(_con);
            connection.Open();
            inicializar_bd(connection);
        }

        public void inicializar_bd(SqliteConnection connection)
        {
            var cmd = connection.CreateCommand();
            // Tabla unificada para Libros y Audiolibros (columnas opcionales según tipo)
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Articulos (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    Tipo            TEXT    NOT NULL,
                    Titulo          TEXT    NOT NULL,
                    Anio            INTEGER NOT NULL,
                    FechaAdquisicion TEXT   NOT NULL,
                    -- Solo Libros
                    Isbn            TEXT,
                    EstaPrestado    INTEGER,
                    -- Solo Audiolibros
                    FechaInicio     TEXT,
                    FechaFin        TEXT
                );
                CREATE TABLE IF NOT EXISTS Valoraciones (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    ArticuloId  INTEGER NOT NULL,
                    Nota        INTEGER NOT NULL,
                    Comentario  TEXT,
                    UsuarioId   TEXT    NOT NULL,
                    FOREIGN KEY (ArticuloId) REFERENCES Articulos(Id)
                );";
            cmd.ExecuteNonQuery();
        }

        // ---------------------------------------------------------------
        // INSERT – inserta cualquier artículo
        // CORREGIDO: ahora usa la tabla Articulos (antes llamaba a Libros)
        // ---------------------------------------------------------------
        public void insertar_articulo(Articulo art)
        {
            using var connection = new SqliteConnection(_con);
            connection.Open();
            var cmd = connection.CreateCommand();

            if (art is Libro l)
            {
                cmd.CommandText = @"INSERT INTO Articulos
                    (Tipo, Titulo, Anio, FechaAdquisicion, Isbn, EstaPrestado)
                    VALUES ('Libro', @t, @a, @f, @isbn, @prest)";
                cmd.Parameters.AddWithValue("@t", l.Titulo);
                cmd.Parameters.AddWithValue("@a", l.Anio);
                cmd.Parameters.AddWithValue("@f", l.FechaAdquisicion.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@isbn", l.Isbn);
                cmd.Parameters.AddWithValue("@prest", l.EstaPrestado ? 1 : 0);
            }
            else if (art is Audiolibro al)
            {
                cmd.CommandText = @"INSERT INTO Articulos
                    (Tipo, Titulo, Anio, FechaAdquisicion, FechaInicio, FechaFin)
                    VALUES ('Audiolibro', @t, @a, @f, @ini, @fin)";
                cmd.Parameters.AddWithValue("@t", al.Titulo);
                cmd.Parameters.AddWithValue("@a", al.Anio);
                cmd.Parameters.AddWithValue("@f", al.FechaAdquisicion.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@ini", al.FechaInicio.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@fin", al.FechaFin.ToString("yyyy-MM-dd"));
            }
            else return;

            cmd.ExecuteNonQuery();

            // Recuperar el Id generado
            var idCmd = connection.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid()";
            art.Id = Convert.ToInt32(idCmd.ExecuteScalar());
        }

        // ---------------------------------------------------------------
        // UPDATE – edita un artículo existente
        // ---------------------------------------------------------------
        public void actualizar_articulo(Articulo art)
        {
            using var connection = new SqliteConnection(_con);
            connection.Open();
            var cmd = connection.CreateCommand();

            if (art is Libro l)
            {
                cmd.CommandText = @"UPDATE Articulos SET
                    Titulo=@t, Anio=@a, Isbn=@isbn, EstaPrestado=@prest
                    WHERE Id=@id";
                cmd.Parameters.AddWithValue("@t", l.Titulo);
                cmd.Parameters.AddWithValue("@a", l.Anio);
                cmd.Parameters.AddWithValue("@isbn", l.Isbn);
                cmd.Parameters.AddWithValue("@prest", l.EstaPrestado ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", l.Id);
            }
            else if (art is Audiolibro al)
            {
                cmd.CommandText = @"UPDATE Articulos SET
                    Titulo=@t, Anio=@a, FechaInicio=@ini, FechaFin=@fin
                    WHERE Id=@id";
                cmd.Parameters.AddWithValue("@t", al.Titulo);
                cmd.Parameters.AddWithValue("@a", al.Anio);
                cmd.Parameters.AddWithValue("@ini", al.FechaInicio.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@fin", al.FechaFin.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@id", al.Id);
            }
            cmd.ExecuteNonQuery();
        }

        // ---------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------
        public void eliminar_articulo(int id)
        {
            using var connection = new SqliteConnection(_con);
            connection.Open();

            // Borrar valoraciones primero (FK)
            var cmd1 = connection.CreateCommand();
            cmd1.CommandText = "DELETE FROM Valoraciones WHERE ArticuloId=@id";
            cmd1.Parameters.AddWithValue("@id", id);
            cmd1.ExecuteNonQuery();

            var cmd2 = connection.CreateCommand();
            cmd2.CommandText = "DELETE FROM Articulos WHERE Id=@id";
            cmd2.Parameters.AddWithValue("@id", id);
            cmd2.ExecuteNonQuery();
        }

        // ---------------------------------------------------------------
        // SELECT – obtiene TODOS los artículos
        // CORREGIDO: antes leía de tabla 'Libros' inexistente
        // ---------------------------------------------------------------
        public List<Articulo> obtener_articulos()
        {
            var lista = new List<Articulo>();
            using var connection = new SqliteConnection(_con);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Tipo, Titulo, Anio, FechaAdquisicion, Isbn, EstaPrestado, FechaInicio, FechaFin FROM Articulos";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string tipo = reader.GetString(1);
                string tit = reader.GetString(2);
                int anio = reader.GetInt32(3);
                var fecha = DateTime.Parse(reader.GetString(4));

                if (tipo == "Libro")
                {
                    var libro = new Libro(tit, anio, fecha, reader.IsDBNull(5) ? "" : reader.GetString(5))
                    {
                        Id = id,
                        EstaPrestado = !reader.IsDBNull(6) && reader.GetInt32(6) == 1
                    };
                    lista.Add(libro);
                }
                else
                {
                    var audio = new Audiolibro(tit, anio, fecha,
                        reader.IsDBNull(7) ? DateTime.Now : DateTime.Parse(reader.GetString(7)),
                        reader.IsDBNull(8) ? DateTime.Now : DateTime.Parse(reader.GetString(8)))
                    {
                        Id = id
                    };
                    lista.Add(audio);
                }
            }

            // Cargar valoraciones de cada artículo
            foreach (var art in lista)
            {
                if (art is IValorable v)
                    v.Valoraciones.AddRange(obtener_valoraciones(art.Id, connection));
            }

            return lista;
        }

        // Obtener solo libros (para compatibilidad con csv_manejador)
        public List<Libro> obtener_libros()
        {
            var todos = obtener_articulos();
            var libros = new List<Libro>();
            foreach (var a in todos)
                if (a is Libro l) libros.Add(l);
            return libros;
        }

        // ---------------------------------------------------------------
        // Buscar artículos – cero, uno o varios títulos + filtros
        // ---------------------------------------------------------------
        public List<Articulo> buscar_items(string[]? titulos = null, string? tipo = null,
                                           int? anio = null, bool? disponible = null)
        {
            var todos = obtener_articulos();
            var resultado = new List<Articulo>();

            foreach (var art in todos)
            {
                // Filtro por tipo
                if (tipo != null)
                {
                    if (tipo == "Libro" && !(art is Libro)) continue;
                    if (tipo == "Audiolibro" && !(art is Audiolibro)) continue;
                }

                // Filtro por año
                if (anio.HasValue && art.Anio != anio.Value) continue;

                // Filtro disponible (solo libros prestables / audiolibros con fecha activa)
                if (disponible.HasValue)
                {
                    if (art is Libro lb && lb.EstaPrestado == disponible.Value) continue;
                    if (art is Audiolibro al && al.EstaDisponible() != disponible.Value) continue;
                }

                // Filtro por títulos (si se pasan)
                if (titulos != null && titulos.Length > 0)
                {
                    bool encontrado = false;
                    foreach (var t in titulos)
                        if (art.Titulo.Contains(t, StringComparison.OrdinalIgnoreCase)) { encontrado = true; break; }
                    if (!encontrado) continue;
                }

                resultado.Add(art);
            }
            return resultado;
        }

        // ---------------------------------------------------------------
        // Valoraciones
        // ---------------------------------------------------------------
        public void insertar_valoracion(int articuloId, Valoracion v)
        {
            using var connection = new SqliteConnection(_con);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO Valoraciones (ArticuloId, Nota, Comentario, UsuarioId)
                                VALUES (@aid, @nota, @com, @uid)";
            cmd.Parameters.AddWithValue("@aid", articuloId);
            cmd.Parameters.AddWithValue("@nota", v.Nota);
            cmd.Parameters.AddWithValue("@com", v.Comentario ?? "");
            cmd.Parameters.AddWithValue("@uid", v.UsuarioId);
            cmd.ExecuteNonQuery();
        }

        private List<Valoracion> obtener_valoraciones(int articuloId, SqliteConnection connection)
        {
            var lista = new List<Valoracion>();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Nota, Comentario, UsuarioId FROM Valoraciones WHERE ArticuloId=@id";
            cmd.Parameters.AddWithValue("@id", articuloId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Valoracion
                {
                    Nota = reader.GetInt32(0),
                    Comentario = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    UsuarioId = reader.GetString(2)
                });
            }
            return lista;
        }
    }
}