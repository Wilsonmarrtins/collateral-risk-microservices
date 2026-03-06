namespace CollateralRisk.BuildingBlocks.Results;

public sealed class ServiceResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public object? Metadata { get; init; }

    public static ServiceResult<T> Success(T value)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Value = value
        };
    }

    public static ServiceResult<T> Fail(string error, object? metadata = null)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Error = error,
            Metadata = metadata
        };
    }
}