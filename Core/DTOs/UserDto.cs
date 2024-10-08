namespace Core.DTOs
{
    public class UserDto
    {
        public string? Id { get; set; }
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
        public bool IsAuth { get; set; }
    }
}
