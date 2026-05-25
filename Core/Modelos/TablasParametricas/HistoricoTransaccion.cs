using System.Text.Json;

public class HistoricoTransaccion
{
    public string Id { get; set; }
    public string NombreTabla { get; set; }
    public DateTime FechaTransaccion { get; set; } = DateTime.Now;
    public string Transaccion { get; set; } // Puede repetir el valor de TipoId o tener más detalle
    public string UsuarioId { get; set; }
    public string RegistroAnterior { get; set; }
    public string RegistroNuevo { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public string ObtenerCamposModificados()
    {
        if (string.IsNullOrWhiteSpace(RegistroAnterior) || string.IsNullOrWhiteSpace(RegistroNuevo))
            return string.Empty;

        var dicAnterior = JsonSerializer.Deserialize<Dictionary<string, object>>(RegistroAnterior);
        var dicNuevo = JsonSerializer.Deserialize<Dictionary<string, object>>(RegistroNuevo);

        if (dicAnterior == null || dicNuevo == null)
            return string.Empty;

        var camposModificados = new List<string>();

        foreach (var kvp in dicNuevo)
        {
            if (!dicAnterior.TryGetValue(kvp.Key, out var valorAnterior))
                continue;

            var valorAnteriorStr = valorAnterior?.ToString() ?? "";
            var valorNuevoStr = kvp.Value?.ToString() ?? "";

            if (!valorAnteriorStr.Equals(valorNuevoStr, StringComparison.Ordinal))
            {
                camposModificados.Add(kvp.Key);
            }
        }

        return string.Join(", ", camposModificados);
    }
}