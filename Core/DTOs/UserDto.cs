namespace Core.DTOs
{
    public class UserDto
    {
        public string? Id { get; set; }
        public string? IdRol { get; set; }
        public string? Alias { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public bool State { get; set; }
        public string[]? RolCode { get; set; }
        public string? EnterpriseCode { get; set; }
        public string? EnterpriseDeptoCode { get; set; }
        public string? EnterpriseEmail { get; set; }
        public string? EnterpriseName { get; set; }
        public string? EnterpriseIdentification { get; set; }
        public bool IsMinSalud { get; set; }
        public string? rolSecani { get; set; }
        public bool IsCoordinadorAdmin { get; set; }
        public bool IsAgenteSeguimiento { get; set; }
        public bool IsCuidador { get; set; }
        public bool IsET { get; set; }
        public bool IsEAPB { get; set; }
    }
}
