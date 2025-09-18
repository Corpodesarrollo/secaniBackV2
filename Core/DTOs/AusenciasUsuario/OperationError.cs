namespace Core.DTOs.AusenciasUsuario
{
    public sealed class OperationError
    {
        public string Code { get; }
        public string Message { get; }
        public string? Field { get; }

        public OperationError(string code, string message, string? field = null)
        {
            Code = code;
            Message = message;
            Field = field;
        }

        // Atajos útiles
        public static OperationError Validation(string message, string? field = null)
            => new("VALIDATION", message, field);

        public static OperationError NotFound(string message)
            => new("NOT_FOUND", message);

        public static OperationError Conflict(string message, string? field = null)
            => new("CONFLICT", message, field);

        public static OperationError Unexpected(string message)
            => new("UNEXPECTED", message);
    }

    public sealed class OperationResult<T>
    {
        /// <summary>Verdadero si no hay errores.</summary>
        public bool Success => Errors.Count == 0;

        /// <summary>Dato retornado por la operación (puede ser null en fallos).</summary>
        public T? Data { get; private set; }

        /// <summary>Mensaje opcional para contexto humano.</summary>
        public string? Message { get; private set; }

        /// <summary>Errores acumulados (inmutable externamente).</summary>
        public List<OperationError> Errors { get; } = new();

        // --------- Fábricas ---------

        public static OperationResult<T> Ok(T data, string? message = null)
            => new() { Data = data, Message = message };

        public static OperationResult<T> Fail(params OperationError[] errors)
        {
            var result = new OperationResult<T>
            {
                Data = default,
                Message = null
            };

            if (errors is { Length: > 0 })
                result.Errors.AddRange(errors); 

            return result;
        }

        public static OperationResult<T> Fail(string code, string message, string? field = null)
            => Fail(new OperationError(code, message, field));

        // --------- Utilidades fluidas ---------

        public OperationResult<T> WithMessage(string message)
        {
            Message = message;
            return this;
        }

        public OperationResult<T> AddError(OperationError error)
        {
            Errors.Add(error);
            return this;
        }

        public OperationResult<T> AddErrors(IEnumerable<OperationError> errors)
        {
            if (errors is null) return this;
            Errors.AddRange(errors);
            return this;
        }

        public OperationResult<TResult> Map<TResult>(Func<T, TResult> selector)
        {
            if (!Success)
                return OperationResult<TResult>.Fail(Errors.ToArray());

            var result = selector is not null && Data is not null
                ? selector(Data)
                : default;

            return OperationResult<TResult>.Ok(result!, Message);
        }
    }
}