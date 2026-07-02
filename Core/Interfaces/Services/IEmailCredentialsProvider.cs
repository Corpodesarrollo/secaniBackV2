namespace Core.Interfaces.Services
{
    /// <summary>
    /// Provee credenciales SMTP. Dos implementaciones:
    /// - DbEmailCredentialsProvider: lee EmailConfigurations (default, EC2 + Gmail)
    /// - KeyVaultEmailCredentialsProvider: lee Azure Key Vault (Azure Stage/Prod + ACS)
    /// Decidido por env var USE_AZURE_KEYVAULT.
    /// </summary>
    public interface IEmailCredentialsProvider
    {
        Task<EmailCredentials?> GetAsync(CancellationToken ct = default);
    }

    public class EmailCredentials
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
    }
}
