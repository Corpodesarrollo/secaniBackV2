namespace Core.Utilities
{
    public class Funciones
    {
        public static string? CalcularEdad(DateTime? fechaNacimiento)
        {
            if (fechaNacimiento == null)
                return null;

            DateTime fechaActual = DateTime.Now;
            int años = fechaActual.Year - fechaNacimiento.Value.Year;
            int meses = fechaActual.Month - fechaNacimiento.Value.Month;
            int dias = fechaActual.Day - fechaNacimiento.Value.Day;

            años = años < 0 ? 0 : años;
            meses = meses < 0 ? 0 : meses;
            dias = dias < 0 ? 0 : dias;

            if (meses < 0 || (meses == 0 && dias < 0))
                años--;

            if (meses < 0)
                meses += 12;

            if (dias < 0)
            {
                meses--;
                dias += DateTime.DaysInMonth(fechaActual.Year, fechaActual.Month);
            }

            string a = años switch
            {
                1 => $"{años} año",
                > 1 => $"{años} años",
                _ => ""
            };

            string m = meses switch
            {
                1 => $"{meses} mes",
                > 1 => $"{meses} meses",
                _ => ""
            };

            string d = dias switch
            {
                1 => $"{dias} día",
                > 1 => $"{dias} días",
                _ => ""
            };

            var cadena = string.Empty;
            if (años > 0)
                cadena += a;

            if (meses > 0)
                cadena += string.IsNullOrEmpty(cadena) ? m : $", {m}";

            if (dias > 0)
                cadena += string.IsNullOrEmpty(cadena) ? d : $", {d}";

            return cadena;
        }

        public static string? CalcularTiempoTrascurrido(DateTime fechaInicio)
        {
            TimeSpan diferencia = DateTime.UtcNow - fechaInicio.ToUniversalTime();
            diferencia = diferencia < TimeSpan.Zero ? TimeSpan.Zero : diferencia;

            int años = (int)(diferencia.Days / 365.25);
            int meses = (int)(diferencia.Days % 365.25 / 30.44);
            int dias = (int)(diferencia.Days % 365.25 % 30.44);
            int horas = diferencia.Hours;
            int minutos = diferencia.Minutes;

            string a = años switch
            {
                1 => $"{años} año",
                > 1 => $"{años} años",
                _ => ""
            };

            string m = meses switch
            {
                1 => $"{meses} mes",
                > 1 => $"{meses} meses",
                _ => ""
            };

            string d = dias switch
            {
                1 => $"{dias} día",
                > 1 => $"{dias} días",
                _ => ""
            };

            string h = horas switch
            {
                1 => $"{horas} hora",
                > 1 => $"{horas} horas",
                _ => ""
            };

            var cadena = string.Empty;
            if (años > 0)
                cadena += a;

            if (meses > 0)
                cadena += string.IsNullOrEmpty(cadena) ? m : $", {m}";

            if (dias > 0)
                cadena += string.IsNullOrEmpty(cadena) ? d : $", {d}";

            if (horas > 0)
                cadena += string.IsNullOrEmpty(cadena) ? h : $", {h}";

            if (minutos > 0)
                cadena += string.IsNullOrEmpty(cadena) ? "" : $" y {minutos} min";

            return cadena;
        }
    }
}
