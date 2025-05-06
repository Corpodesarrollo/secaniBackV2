namespace Core.Interfaces.Services.MSUsuariosyRoles
{
    using Newtonsoft.Json;
    using System = global::System;

    public class MaestroPersonaClientService
    {
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.2.0.0 (NJsonSchema v11.1.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class Client
    {
#pragma warning disable 8618
        private string _baseUrl;
#pragma warning restore 8618

        private System.Net.Http.HttpClient _httpClient;
        private static System.Lazy<Newtonsoft.Json.JsonSerializerSettings> _settings = new System.Lazy<Newtonsoft.Json.JsonSerializerSettings>(CreateSerializerSettings, true);
        private Newtonsoft.Json.JsonSerializerSettings _instanceSettings;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Client(System.Net.Http.HttpClient httpClient)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            BaseUrl = "https://web.sispropreprod.gov.co/interoperabilidad/maestropersona";
            _httpClient = httpClient;
            Initialize();
        }

        private static Newtonsoft.Json.JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            UpdateJsonSerializerSettings(settings);
            return settings;
        }

        public string BaseUrl
        {
            get { return _baseUrl; }
            set
            {
                _baseUrl = value;
                if (!string.IsNullOrEmpty(_baseUrl) && !_baseUrl.EndsWith("/"))
                    _baseUrl += '/';
            }
        }

        protected Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get { return _instanceSettings ?? _settings.Value; } }

        static partial void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings);

        partial void Initialize();

        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url);
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder);
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response);

        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<VIdentificacionPersona?> GetIdVigenteAsync(string apiKey, string tipoIdentificacion, string nroIdentificacion)
        {
            return GetIdVigenteAsync(apiKey, tipoIdentificacion, nroIdentificacion, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async Task<VIdentificacionPersona?> GetIdVigenteAsync(string apiKey, string tipoIdentificacion, string nroIdentificacion, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                if (tipoIdentificacion == null)
                    throw new System.ArgumentNullException("tipoIdentificacion");

                if (nroIdentificacion == null)
                    throw new System.ArgumentNullException("nroIdentificacion");

                using var client_ = _httpClient;
                client_.DefaultRequestHeaders.Add("ApiKey", apiKey);
                var url = $"{_baseUrl}api/Persona/GetIdVigente/{tipoIdentificacion}/{nroIdentificacion}";
                var response = await client_.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonConvert.DeserializeObject<VIdentificacionPersona>(content, JsonSerializerSettings);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new ApiException($"Error: {response.StatusCode}, {errorContent}", (int)response.StatusCode, errorContent, null, null);
                }
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error en GetIdVigenteAsync: {ex.Message}", 404, ex.Message, null, ex);
            }
        }

        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual Task<VIdentificacionPersona?> GetIdVigente2Async(string apiKey, string tipoIdentificacion, string nroIdentificacion, string fechaExpedicion)
        {
            return GetIdVigente2Async(apiKey, tipoIdentificacion, nroIdentificacion, fechaExpedicion, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async Task<VIdentificacionPersona?> GetIdVigente2Async(string apiKey, string tipoIdentificacion, string nroIdentificacion, string fechaExpedicion, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                if (tipoIdentificacion == null)
                    throw new System.ArgumentNullException("tipoIdentificacion");

                if (nroIdentificacion == null)
                    throw new System.ArgumentNullException("nroIdentificacion");

                if (fechaExpedicion == null)
                    throw new System.ArgumentNullException("fechaExpedicion");

                using var client_ = _httpClient;
                client_.DefaultRequestHeaders.Add("ApiKey", apiKey);
                var url = $"{_baseUrl}api/Persona/GetIdVigente/{tipoIdentificacion}/{nroIdentificacion}/{fechaExpedicion}";
                var response = await client_.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonConvert.DeserializeObject<VIdentificacionPersona>(content, JsonSerializerSettings);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new ApiException($"Error: {response.StatusCode}, {errorContent}", (int)response.StatusCode, errorContent, null, null);
                }
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error en GetIdVigenteAsync: {ex.Message}", 404, ex.Message, null, ex);
            }
        }

        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual Task<ICollection<VIdentificacionPersona>?> GetIdAllAsync(string apiKey, string tipoIdentificacion, string nroIdentificacion)
        {
            return GetIdAllAsync(apiKey, tipoIdentificacion, nroIdentificacion, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async Task<ICollection<VIdentificacionPersona>?> GetIdAllAsync(string apiKey, string tipoIdentificacion, string nroIdentificacion, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                if (tipoIdentificacion == null)
                    throw new System.ArgumentNullException("tipoIdentificacion");

                if (nroIdentificacion == null)
                    throw new System.ArgumentNullException("nroIdentificacion");

                using var client_ = _httpClient;
                client_.DefaultRequestHeaders.Add("ApiKey", apiKey);
                var url = $"{_baseUrl}api/Persona/GetIdAll/{tipoIdentificacion}/{nroIdentificacion}";
                var response = await client_.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonConvert.DeserializeObject<ICollection<VIdentificacionPersona>>(content, JsonSerializerSettings);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new ApiException($"Error: {response.StatusCode}, {errorContent}", (int)response.StatusCode, errorContent, null, null);
                }
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error en GetIdVigenteAsync: {ex.Message}", 404, ex.Message, null, ex);
            }
        }

        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual Task<ICollection<VIdentificacionPersona>?> GetIdAll2Async(string apiKey, string tipoIdentificacion, string nroIdentificacion, string fechaExpedicion)
        {
            return GetIdAll2Async(apiKey, tipoIdentificacion, nroIdentificacion, fechaExpedicion, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>OK</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async Task<ICollection<VIdentificacionPersona>?> GetIdAll2Async(string apiKey, string tipoIdentificacion, string nroIdentificacion, string fechaExpedicion, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                if (tipoIdentificacion == null)
                    throw new System.ArgumentNullException("tipoIdentificacion");

                if (nroIdentificacion == null)
                    throw new System.ArgumentNullException("nroIdentificacion");

                if (fechaExpedicion == null)
                    throw new System.ArgumentNullException("fechaExpedicion");

                using var client_ = _httpClient;
                client_.DefaultRequestHeaders.Add("ApiKey", apiKey);
                var url = $"{_baseUrl}api/Persona/GetIdAll/{tipoIdentificacion}/{nroIdentificacion}/{fechaExpedicion}";
                var response = await client_.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonConvert.DeserializeObject<ICollection<VIdentificacionPersona>>(content, JsonSerializerSettings);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new ApiException($"Error: {response.StatusCode}, {errorContent}", (int)response.StatusCode, errorContent, null, null);
                }
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error en GetIdVigenteAsync: {ex.Message}", 404, ex.Message, null, ex);
            }
        }

        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }

            public T Object { get; }

            public string Text { get; }
        }

        public bool ReadResponseAsString { get; set; }

        protected virtual async System.Threading.Tasks.Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(System.Net.Http.HttpResponseMessage response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Threading.CancellationToken cancellationToken)
        {
            if (response == null || response.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }

            if (ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
                try
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var streamReader = new System.IO.StreamReader(responseStream))
                    using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader))
                    {
                        var serializer = Newtonsoft.Json.JsonSerializer.Create(JsonSerializerSettings);
                        var typedBody = serializer.Deserialize<T>(jsonTextReader);
                        return new ObjectResponseResult<T>(typedBody, string.Empty);
                    }
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }

        private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return "";
            }

            if (value is System.Enum)
            {
                var name = System.Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                            as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null)
                        {
                            return attribute.Value != null ? attribute.Value : name;
                        }
                    }

                    var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted == null ? string.Empty : converted;
                }
            }
            else if (value is bool)
            {
                return System.Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[])
            {
                return System.Convert.ToBase64String((byte[])value);
            }
            else if (value is string[])
            {
                return string.Join(",", (string[])value);
            }
            else if (value.GetType().IsArray)
            {
                var valueArray = (System.Array)value;
                var valueTextArray = new string[valueArray.Length];
                for (var i = 0; i < valueArray.Length; i++)
                {
                    valueTextArray[i] = ConvertToString(valueArray.GetValue(i), cultureInfo);
                }
                return string.Join(",", valueTextArray);
            }

            var result = System.Convert.ToString(value, cultureInfo);
            return result == null ? "" : result;
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.2.0.0 (NJsonSchema v11.1.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class VIdentificacionPersona
    {
        [Newtonsoft.Json.JsonProperty("idEvol", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? IdEvol { get; set; }

        [Newtonsoft.Json.JsonProperty("serial_ms_evol", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? Serial_ms_evol { get; set; }

        [Newtonsoft.Json.JsonProperty("tipo_identificacion", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Tipo_identificacion { get; set; }

        [Newtonsoft.Json.JsonProperty("numero_identificacion", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Numero_identificacion { get; set; }

        [Newtonsoft.Json.JsonProperty("primer_apellido", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Primer_apellido { get; set; }

        [Newtonsoft.Json.JsonProperty("segundo_apellido", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Segundo_apellido { get; set; }

        [Newtonsoft.Json.JsonProperty("primer_nombre", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Primer_nombre { get; set; }

        [Newtonsoft.Json.JsonProperty("segundo_nombre", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Segundo_nombre { get; set; }

        [Newtonsoft.Json.JsonProperty("fecha_nacimiento", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset? Fecha_nacimiento { get; set; }

        [Newtonsoft.Json.JsonProperty("sexo", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Sexo { get; set; }

        [Newtonsoft.Json.JsonProperty("fecha_expedicion", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset? Fecha_expedicion { get; set; }

        [Newtonsoft.Json.JsonProperty("vigente", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Vigente { get; set; }

        [Newtonsoft.Json.JsonProperty("esFallecido", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? EsFallecido { get; set; }

        [Newtonsoft.Json.JsonProperty("fechaFallecimiento", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset? FechaFallecimiento { get; set; }

    }



    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.2.0.0 (NJsonSchema v11.1.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ApiException : System.Exception
    {
        public int StatusCode { get; private set; }

        public string Response { get; private set; }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers { get; private set; }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Exception innerException)
            : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null) ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        public override string ToString()
        {
            return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.2.0.0 (NJsonSchema v11.1.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ApiException<TResult> : ApiException
    {
        public TResult Result { get; private set; }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, TResult result, System.Exception innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }

}

