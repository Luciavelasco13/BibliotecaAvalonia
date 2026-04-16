using System;

namespace BibliotecaAvalonia.Utilidades
{
    public static class Ayuda
    {
        // ---------------------------------------------------------------
        // Validación matemática ISBN-10
        // CORREGIDO: se acepta 'X'/'x' solo en última posición,
        //            y se comprueba que los 9 primeros sean dígitos.
        // ---------------------------------------------------------------
        public static bool validar_isbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return false;

            // Eliminar guiones si los trae (formato habitual: 0-7475-3269-9)
            isbn = isbn.Replace("-", "").Trim();

            if (isbn.Length != 10) return false;

            try
            {
                int suma = 0;
                for (int i = 0; i < 9; i++)
                {
                    // Los 9 primeros deben ser dígitos numéricos
                    if (!char.IsDigit(isbn[i])) return false;
                    suma += (isbn[i] - '0') * (10 - i);
                }

                // El décimo puede ser dígito o 'X'/'x' (vale 10)
                char ultimo = isbn[9];
                int valorUltimo;
                if (ultimo == 'X' || ultimo == 'x')
                    valorUltimo = 10;
                else if (char.IsDigit(ultimo))
                    valorUltimo = ultimo - '0';
                else
                    return false;

                suma += valorUltimo;
                return (suma % 11 == 0);
            }
            catch
            {
                return false;
            }
        }

        // ---------------------------------------------------------------
        // Formateo de título
        // CORREGIDO: antes hacía Substring(1).ToLower() lo que ponía
        //            todo en minúsculas. Ahora respeta las mayúsculas
        //            internas y solo asegura la primera letra en mayúscula.
        // ---------------------------------------------------------------
        public static string formatear_titulo(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            texto = texto.Trim(); // sin espacios al inicio/fin
            return char.ToUpper(texto[0]) + texto.Substring(1);
        }

        // ---------------------------------------------------------------
        // Validación de año (1500 – año actual)
        // ---------------------------------------------------------------
        public static bool validar_anio(int anio)
            => anio >= 1500 && anio <= DateTime.Now.Year;

        // ---------------------------------------------------------------
        // Validación de nota de valoración (0-10)
        // ---------------------------------------------------------------
        public static bool validar_nota(int nota)
            => nota >= 0 && nota <= 10;
    }
}