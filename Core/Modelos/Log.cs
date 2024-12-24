using System.ComponentModel.DataAnnotations;

namespace Core.Modelos
{
    public class Log
    {
        [Key]
        public int Id { get; set; }
        public string? Mensaje { get; set; }
    }
}
