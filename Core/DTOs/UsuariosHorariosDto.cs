namespace Core.DTOs
{
    public class UsuariosHorariosDto
    {
        public string? UserId { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }

        public DateTime? Fecha { get; set; }
        public TimeSpan? HoraEntrada { get; set; }

        private TimeSpan? _horaSalida;
        public TimeSpan? HoraSalida
        {
            get => _horaSalida; set
            {
                _horaSalida = value;
                _ = HorasTrabajadas;
                _ = CantidadSeguimientos;
            }
        }

        public float HorasTrabajadas
        {
            get
            {
                if (HoraEntrada == null || HoraSalida == null)
                    return 0;

                return (HoraSalida.Value - HoraEntrada.Value).Hours > 8 ? 8 : (HoraSalida.Value - HoraEntrada.Value).Hours;
            }
        }

        public float CantidadSeguimientos
        {
            get
            {
                if (CantidadSeguimientosDisponibles == 0)
                    CantidadSeguimientosDisponibles = HorasTrabajadas * 45 / 8;

                return HorasTrabajadas * 45 / 8;
            }
        }

        public float CantidadSeguimientosDisponibles { get; set; }
    }
}
