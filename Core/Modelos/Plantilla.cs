using Core.Modelos.Common;

namespace Core.Modelos
{
    public class Plantilla : BaseEntity
    {
        public string Headerjpg { get; set; }
        public string Footerjpg { get; set; }
        public long ConfigurationEmailId { get; set; }

    }
}
