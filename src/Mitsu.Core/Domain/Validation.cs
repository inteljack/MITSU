namespace Mitsu.Core.Domain;

public sealed record ValidationError(string Field, string Message)
{
    public override string ToString() => $"{Field}: {Message}";
}

public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = new();

    public IReadOnlyList<ValidationError> Errors => _errors;
    public bool IsValid => _errors.Count == 0;

    public void Add(string field, string message) => _errors.Add(new ValidationError(field, message));

    public void Merge(ValidationResult other) => _errors.AddRange(other._errors);

    public void ThrowIfInvalid()
    {
        if (!IsValid) throw new DomainValidationException(this);
    }

    public override string ToString() => string.Join("\n", _errors);
}

public sealed class DomainValidationException(ValidationResult result) : Exception(result.ToString())
{
    public ValidationResult Result { get; } = result;
}
