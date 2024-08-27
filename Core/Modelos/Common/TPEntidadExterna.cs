using MediatR.NotificationPublishers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Modelos.Common
{
    public class TPEntidadExterna: TPExternalEntityBase
    {
        public string NITConCode { get; set; }
        public string NITSinCode { get; set; }
        public string DigitoVerificacion { get; set; }
        public string RazonSocial { get; set; }
        public string CategoriaVIII { get; set; }
        public string CategoriaIX { get; set; }
        public string Email { get; set; }
    }
}
