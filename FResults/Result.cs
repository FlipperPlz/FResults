using FResults.Reasoning;

namespace FResults;

using Extensions;

public interface IResult : IReason
{
    /// <summary>
    /// Is true if Reasons contains at least one error, or one warning that's configured to be treated as an error
    /// </summary>
    bool IsFailed { get; }

    /// <summary>
    /// Get all reasons (successes and alerts)
    /// </summary>
    List<IReason> Reasons { get; }

    /// <summary>
    /// Get all successes
    /// </summary>
    IEnumerable<Success> Successes { get; }

    /// <summary>
    /// Get all alerts (errors and warnings)
    /// </summary>
    IEnumerable<IAlert> Alerts { get; }

    /// <summary>
    /// Get all warnings
    /// </summary>
    IEnumerable<WarningBase> Warnings { get; }

    /// <summary>
    /// Get all errors
    /// </summary>
    IEnumerable<ErrorBase> Errors { get; }

    /// <summary>
    /// Is true if Reasons contains no errors. The opposite of IsFailed
    /// </summary>
    bool IsSuccess { get; }
}

public class Result : IResult
{
    internal static readonly Result DefaultOk = new();
    internal static readonly Success DefaultSuccess = new();

    public bool IsFailed { get; protected set; }
    public bool IsSuccess => !IsFailed;

    public string? Message { get; set; }
    public Dictionary<string, object> Metadata { get; protected set; }
    public List<IReason> Reasons { get; protected set; }
    public IEnumerable<Success> Successes => Reasons.OfType<Success>();
    public IEnumerable<IAlert> Alerts => Reasons.OfType<IAlert>();
    public IEnumerable<WarningBase> Warnings => Reasons.OfType<WarningBase>();
    public IEnumerable<ErrorBase> Errors => Reasons.OfType<ErrorBase>();

    public Result(string? message = null, List<IReason>? reasons = null, Dictionary<string, object>? metadata = null)
    {
        Reasons = reasons ?? new List<IReason>();
        Message = Reasons.Count == 1 && message is not null
            ? Reasons[0].Message ?? (IsFailed ? "Complete Failure." : "Complete Success.")
            : message ?? (IsFailed ? "Complete Failure." : "Complete Success.");
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a failed result with the given error messages. Internally a list of error objects from the error factory is created
    /// </summary>
    public static Result Fail(IEnumerable<string> errorMessages)
    {
        ArgumentNullException.ThrowIfNull(errorMessages);

        var result = new Result();
        result.WithErrors(errorMessages.Select(m => new Error { Message = m}));
        return result;
    }

    /// <summary>
    /// Map all errors of the result via errorMapper
    /// </summary>
    /// <param name="errorMapper"></param>
    /// <returns></returns>
    public Result MapErrors(Func<ErrorBase, ErrorBase> errorMapper) =>
        IsSuccess ? this : new Result()
        .WithErrors(Errors.Select(errorMapper))
        .WithSuccesses(Successes);

    /// <summary>
    /// Creates a failed result with the given error message. Internally an error object from the error factory is created.
    /// </summary>
    public static Result Fail(string message)
    {
        var result = new Result();
        result.WithError(new Error {
            Message = message
        });
        return result;
    }

    /// <summary>
    /// Creates a failed result with the given errors.
    /// </summary>
    public static Result Fail(IEnumerable<ErrorBase> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var result = new Result();
        result.WithErrors(errors);
        return result;
    }

    /// <summary>
    /// Creates a success result
    /// </summary>
    public static Result Ok() => new();

    /// <summary>
    /// Creates a faster singleton success result
    /// </summary>
    public static Result ImmutableOk() => DefaultOk;



    /// <summary>
    /// Map all successes of the result via successMapper
    /// </summary>
    /// <param name="successMapper"></param>
    /// <returns></returns>
    public Result MapSuccesses(Func<Success, Success> successMapper) => new Result()
        .WithErrors(Errors)
        .WithSuccesses(Successes.Select(successMapper));

    /// <summary>
    /// Execute an action which returns a <see cref="Result"/>.
    /// </summary>
    /// <example>
    /// <code>
    ///  var done = result.Bind(ActionWhichMayFail);
    /// </code>
    /// </example>
    /// <param name="action">Action that may fail.</param>
    public Result Bind(Func<Result> action)
    {
        var result = new Result();
        result.WithReasons(Reasons);

        return IsSuccess ? result.WithReasons(action().Reasons) : result;
    }

    /// <summary>
    /// Execute an action which returns a <see cref="Result"/> asynchronously.
    /// </summary>
    /// <example>
    /// <code>
    ///  var done = result.Bind(ActionWhichMayFail);
    /// </code>
    /// </example>
    /// <param name="action">Action that may fail.</param>
    public async Task<Result> Bind(Func<Task<Result>> action)
    {
        var result = new Result().WithReasons(Reasons);

        return IsSuccess ? result.WithReasons((await action()).Reasons) : result;
    }

    /// <summary>
    /// Execute an action which returns a <see cref="Result"/> asynchronously.
    /// </summary>
    /// <example>
    /// <code>
    ///  var done = result.Bind(ActionWhichMayFail);
    /// </code>
    /// </example>
    /// <param name="action">Action that may fail.</param>
    public async ValueTask<Result> Bind(Func<ValueTask<Result>> action)
    {
        var result = new Result();
        result.WithReasons(Reasons);

        return IsSuccess ? result.WithReasons((await action()).Reasons) : result;
    }

    public static implicit operator Result(ErrorBase error) => Fail(error);

    public static implicit operator Result(List<ErrorBase> errors) => Fail(errors);

    /// <summary>
    /// Creates a failed result with the given error
    /// </summary>
    public static Result Fail(ErrorBase error) => new Result().WithError(error);


    public static Result Merge(IEnumerable<Result> results) =>
        Ok().WithReasons(results.SelectMany(result => result.Reasons));


    public static Result Merge(params Result[] results) =>
        Ok().WithReasons(results.SelectMany(result => result.Reasons));

    /// <summary>
    /// Create a success/failed result depending on the parameter isSuccess
    /// </summary>
    public static Result OkIf(bool isSuccess, ErrorBase error) =>
        isSuccess ? Ok() : Fail(error);

    /// <summary>
    /// Create a success/failed result depending on the parameter isSuccess
    /// </summary>
    public static Result OkIf(bool isSuccess, string error) =>
        isSuccess ? Ok() : Fail(error);


    /// <summary>
    /// Create a success/failed result depending on the parameter isSuccess
    /// </summary>
    /// <remarks>
    /// ErrorBase is lazily evaluated.
    /// </remarks>
    public static Result OkIf(bool isSuccess, Func<ErrorBase> errorFactory) =>
        isSuccess ? Ok() : Fail(errorFactory.Invoke());


    /// <summary>
    /// Create a success/failed result depending on the parameter isSuccess
    /// </summary>
    /// <remarks>
    /// ErrorBase is lazily evaluated.
    /// </remarks>
    public static Result OkIf(bool isSuccess, Func<string> errorMessageFactory) => isSuccess ? Ok() : Fail(errorMessageFactory.Invoke());

    /// <summary>
    /// Create a success/failed result depending on the parameter isFailure
    /// </summary>
    public static Result FailIf(bool isFailure, ErrorBase error) => isFailure ? Fail(error) : Ok();

    /// <summary>
    /// Create a success/failed result depending on the parameter isFailure
    /// </summary>
    public static Result FailIf(bool isFailure, string error) => isFailure ? Fail(error) : Ok();

    /// <summary>
    /// Create a success/failed result depending on the parameter isFailure
    /// </summary>
    /// <remarks>
    /// ErrorBase is lazily evaluated.
    /// </remarks>
    public static Result FailIf(bool isFailure, Func<ErrorBase> errorFactory) =>
        isFailure ? Fail(errorFactory.Invoke()) : Ok();

    /// <summary>
    /// Create a success/failed result depending on the parameter isFailure
    /// </summary>
    /// <remarks>
    /// ErrorBase is lazily evaluated.
    /// </remarks>
    public static Result FailIf(bool isFailure, Func<string> errorMessageFactory) =>
        isFailure ? Fail(errorMessageFactory.Invoke()) : Ok();


    /// <summary>
    /// Check if the result object contains an error from a specific type
    /// </summary>
    public bool HasWarning<TWarning>() where TWarning : ErrorBase =>
        HasError<TWarning>(out _);

    /// <summary>
    /// Check if the result object contains an error from a specific type
    /// </summary>
    public bool HasWarning<TWarning>(out IEnumerable<TWarning> result) where TWarning : WarningBase =>
        (result = Warnings.OfType<TWarning>()).Any();


    /// <summary>
    /// Check if the result object contains an error from a specific type
    /// </summary>
    public bool HasActualError<TError>() where TError : Error =>
        HasError<TError>(out _);

    /// <summary>
    /// Check if the result object contains an error from a specific type
    /// </summary>
    public bool HasActualError<TError>(out IEnumerable<TError> result) where TError : Error =>
        (result = Errors.OfType<TError>()).Any();

    /// <summary>
    /// Check if the result object contains an error from a specific type
    /// </summary>
    public bool HasAlert<TAlert>() where TAlert : IAlert =>
        HasAlert<TAlert>(out _);

    /// <summary>
    /// Check if the result object contains an error from a specific type
    /// </summary>
    public bool HasAlert<TAlert>(out IEnumerable<TAlert> result) where TAlert : IAlert =>
        (result = Errors.OfType<TAlert>()).Any();


    /// <summary>
    /// Check if the result object contains an alert flagged as an error from a specific type
    /// </summary>
    public bool HasError<TError>() where TError : IAlert =>
        HasError<TError>(out _);

    /// <summary>
    /// Check if the result object contains an alert flagged as an error from a specific type
    /// </summary>
    public bool HasError<TError>(out IEnumerable<TError> result) where TError : IAlert =>
        (result = Alerts.OfType<TError>().Where(e => e.IsError)).Any();

    /// <summary>
    /// Check if the result object contains a success from a specific type
    /// </summary>
    public bool HasSuccess<TSuccess>() where TSuccess : Success =>
        HasSuccess<TSuccess>(out _);

    /// <summary>
    /// Check if the result object contains a success from a specific type
    /// </summary>
    public bool HasSuccess<TSuccess>(out IEnumerable<TSuccess> result) where TSuccess : Success =>
        (result = Successes.OfType<TSuccess>()).Any();

    /// <summary>
    /// Deconstruct Result
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="isFailed"></param>
    public void Deconstruct(out bool isSuccess, out bool isFailed)
    {
        isSuccess = IsSuccess;
        isFailed = IsFailed;
    }

    public static implicit operator bool(Result result) => result.IsSuccess;


    public override string ToString() => string.Join("\n", Alerts.Select(a => a.ToString()));

    /// <summary>
    /// Deconstruct Result
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="isFailed"></param>
    /// <param name="alerts"></param>
    public void Deconstruct(out bool isSuccess, out bool isFailed, out IEnumerable<IAlert> alerts)
    {
        isSuccess = IsSuccess;
        isFailed = IsFailed;
        alerts = Alerts;
    }
}
