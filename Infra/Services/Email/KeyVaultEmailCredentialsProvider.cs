using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Infra.Services.Email
{
    /// <summary>
    /// Lee credenciales SMTP desde Azure Key Vault via Managed Identity (ACS MinSalud).
    /// Activado por env var USE_AZURE_KEYVAULT=true.
    ///
    /// Config requerida (env vars o appsettings):
    ///   KV_NAME            nombre Key Vault. Ejemplos validados con MinSalud:
    ///                        PROD:    kv-acs-sispro-e1
    ///                        PREPROD: kv-acs-sispropreprod-e1
    ///   KV_SECRET_USER     nombre secreto username.
    ///                        PROD:    sisproacs-user
    ///                        PREPROD: sispropreprodacs-user
    ///   KV_SECRET_PWD      nombre secreto password (Access Key ACS).
    ///                        PROD:    sisproacs-key
    ///                        PREPROD: sispropreprodacs-key
    ///   SMTP_HOST          smtp.azurecomm.net (default)
    ///   SMTP_PORT          587 (default)
    ///   SMTP_FROM_EMAIL    DoNotReply@sispro.gov.co (PROD) / DoNotReply@sispropreprod.gov.co (PREPROD)
    ///
    /// Requisitos infra:
    /// - App Service / VM con Managed Identity habilitada
    /// - RBAC "Key Vault Secrets User" sobre el Key Vault para el MI
    /// - TLS 1.2 forzado (script ejemplo MinSalud lo exige; .NET 8 lo usa por default)
    /// - Secrets cache 5 min para no golpear KV en cada send
    /// </summary>
    public class KeyVaultEmailCredentialsProvider : IEmailCredentialsProvider
    {
        private readonly IConfiguration _config;
        private readonly ILogger<KeyVaultEmailCredentialsProvider> _logger;
        private static EmailCredentials? _cache;
        private static DateTime _cacheUntil = DateTime.MinValue;
        private static readonly SemaphoreSlim _gate = new(1, 1);

        static KeyVaultEmailCredentialsProvider()
        {
            // Defensivo: scripts ejemplo MinSalud forzan TLS 1.2. .NET 8 ya lo usa
            // pero algunos entornos legacy pueden tener override; aseguramos aqui.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public KeyVaultEmailCredentialsProvider(IConfiguration config, ILogger<KeyVaultEmailCredentialsProvider> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<EmailCredentials?> GetAsync(CancellationToken ct = default)
        {
            if (_cache != null && DateTime.UtcNow < _cacheUntil)
                return _cache;

            await _gate.WaitAsync(ct);
            try
            {
                if (_cache != null && DateTime.UtcNow < _cacheUntil)
                    return _cache;

                var kvName = _config["KV_NAME"] ?? Environment.GetEnvironmentVariable("KV_NAME");
                var secretUser = _config["KV_SECRET_USER"] ?? Environment.GetEnvironmentVariable("KV_SECRET_USER") ?? "sisproacs-user";
                var secretPwd = _config["KV_SECRET_PWD"] ?? Environment.GetEnvironmentVariable("KV_SECRET_PWD") ?? "sisproacs-key";
                var smtpHost = _config["SMTP_HOST"] ?? Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.azurecomm.net";
                var smtpPortStr = _config["SMTP_PORT"] ?? Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587";
                var fromEmail = _config["SMTP_FROM_EMAIL"] ?? Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? string.Empty;

                if (string.IsNullOrWhiteSpace(kvName))
                {
                    _logger.LogError("KV_NAME no configurado. KeyVault provider no puede operar.");
                    return null;
                }

                var kvUri = new Uri($"https://{kvName}.vault.azure.net/");
                var client = new SecretClient(kvUri, new DefaultAzureCredential());

                var userResp = await client.GetSecretAsync(secretUser, cancellationToken: ct);
                var pwdResp = await client.GetSecretAsync(secretPwd, cancellationToken: ct);

                _cache = new EmailCredentials
                {
                    SmtpServer = smtpHost,
                    Port = int.TryParse(smtpPortStr, out var p) ? p : 587,
                    EnableSsl = true,
                    UserName = userResp.Value.Value,
                    Password = pwdResp.Value.Value,
                    FromEmail = string.IsNullOrWhiteSpace(fromEmail) ? userResp.Value.Value : fromEmail
                };
                _cacheUntil = DateTime.UtcNow.AddMinutes(5);
                _logger.LogInformation("Credenciales SMTP cargadas desde Key Vault {kv} (cache 5 min)", kvName);
                return _cache;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo al cargar credenciales desde Key Vault");
                return null;
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
