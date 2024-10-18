using Core.Modelos.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MSSeguimiento.Core.Modelos
{
    public class HorarioLaboralAgente : BaseEntity
    {

        [Required]
        public string UserId { get; set; }

        [Required]
        public int Dia { get; set; }

        [Required]
        public TimeSpan HoraEntrada { get; set; }

        [Required]
        public TimeSpan HoraSalida { get; set; }

    
    }
}
