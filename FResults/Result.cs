using FResults.Reasoning;

namespace FResults;

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
    IEnumerable<Warning> Warnings { get; }

    /// <summary>
    /// Get all errors
    /// </summary>
    IEnumerable<Error> Errors { get; }

    /// <summary>
    /// Is true if Reasons contains no errors. The opposite of IsFailed
    /// </summary>
    bool IsSuccess { get; }
}

public class Result : IResult
{
    private static readonly Result DefaultOk = new();
    private static readonly Success DefaultSuccess = new();

    public bool IsFailed { get; protected set; }
    public bool IsSuccess => !IsFailed;

    public string? Message { get; set; }
    public Dictionary<string, object> Metadata { get; protected set; }
    public List<IReason> Reasons { get; protected set; }
    public IEnumerable<Success> Successes => Reasons.OfType<Success>();
    public IEnumerable<IAlert> Alerts => Reasons.OfType<IAlert>();
    public IEnumerable<Warning> Warnings => Reasons.OfType<Warning>();
    public IEnumerable<Error> Errors => Reasons.OfType<Error>();

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
    public Result MapErrors(Func<Error, Error> errorMapper) =>
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
    public static Result Fail(IEnumerable<Error> errors)
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

    public Result WithReason(IReason reason)
    {
        Reasons.Add(reason);
        return this;
    }

    /// <summary>
    /// Add multiple reasons (success or error)
    /// </summary>
    public Result WithReasons(IEnumerable<IReason> reasons)
    {
        Reasons.AddRange(reasons);
        return this;
    }

    /// <summary>
    /// Add multiple errors
    /// </summary>
    public Result WithErrors(IEnumerable<Error> errors) => WithReasons(errors);


    /// <summary>
    /// Add multiple warnings
    /// </summary>
    public Result WithWarnings(IEnumerable<Warning> warnings) => WithReasons(warnings);

    /// <summary>
    /// Add an error
    /// </summary>
    public Result WithError<TError>() where TError : Error, new() =>
        WithError(new TError());

    /// <summary>
    /// Add a warning
    /// </summary>
    public Result WithWarning<TWarning>() where TWarning : Warning, new() =>
        WithReason(new TWarning());

    /// <summary>
    /// Add a success
    /// </summary>
    public Result WithSuccess() =>
        WithSuccess(DefaultSuccess);

    /// <summary>
    /// Add a success
    /// </summary>
    public Result WithSuccess(Success success) => WithReason(success);

    /// <summary>
    /// Add an error
    /// </summary>
    public Result WithError(string errorName, Type? errorScope = null, string? errorMessage = null) =>
        WithError(new Error {
            AlertName = errorName,
            AlertScope = errorScope,
            Message = errorMessage
        });


    /// <summary>
    /// Add an error
    /// </summary>
    public Result WithWarning(string warningName, Type? warningScope = null, string? warningMessage = null) =>
        WithWarning(new Warning {
            AlertName = warningName,
            AlertScope = warningScope,
            Message = warningMessage
        });

    /// <summary>
    /// Add a warning
    /// </summary>
    public Result WithWarning(Warning warning) => WithReason(warning);

    /// <summary>
    /// Add an error
    /// </summary>
    public Result WithError(Error error) =>
        WithReason(error);

    public Result WithSuccesses(IEnumerable<Success> successes)
    {
        foreach (var success in successes)
        {
            WithSuccess(success);
        }

        return this;
    }

    /// <summary>
    /// Map all successes of the result via successMapper
    /// </summary>
    /// <param name="successMapper"></param>
    /// <returns></returns>
    public Result MapSuccesses(Func<Success, Success> successMapper) => new Result()
        .WithErrors(Errors)
        .WithSuccesses(Successes.Select(successMapper));

    // public Result<TNewValue> ToResult<TNewValue>(TNewValue newValue = default!)
    // {
    //     return new Result<TNewValue>()
    //         .WithValue(IsFailed ? default : newValue)
    //         .WithReasons(Reasons);
    // }

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
    /// Check if the result object contains an error from a specific type
    /// </summary>
    public bool HasError<TError>() where TError : Error =>
        HasError<TError>(out _);

    /// <summary>
    /// Check if the result object contains an error from a specific type
    /// </summary>
    public bool HasError<TError>(out IEnumerable<TError> result) where TError : Error =>
        HasError(_ => true, out result);

    /// <summary>
    /// Check if the result object contains an error from a specific type and with a specific condition
    /// </summary>
    public bool HasError<TError>(Func<TError, bool> predicate) where TError : Error
        => HasError(predicate, out _);

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

    public static implicit operator Result(Error error) => Fail(error);

    public static implicit operator Result(List<Error> errors) => Fail(errors);

    /// <summary>
    /// Creates a failed result with the given error
    /// </summary>
    public static Result Fail(Error error) => new Result().WithError(error);

    /// <summary>
    /// Check if the result object contains an error from a specific type and with a specific condition
    /// </summary>
    public bool HasError<TError>(Func<TError, bool> predicate, out IEnumerable<TError> result) where TError : Error
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return HasError(Errors, predicate, out result);
    }

    public static Result Merge(IEnumerable<Result> results) =>
        Ok().WithReasons(results.SelectMany(result => result.Reasons));

    /// <summary>
    /// Create a success/failed result depending on the parameter isSuccess
    /// </summary>
    public static Result OkIf(bool isSuccess, Error error) =>
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
    /// Error is lazily evaluated.
    /// </remarks>
    public static Result OkIf(bool isSuccess, Func<Error> errorFactory) =>
        isSuccess ? Ok() : Fail(errorFactory.Invoke());


    /// <summary>
    /// Create a success/failed result depending on the parameter isSuccess
    /// </summary>
    /// <remarks>
    /// Error is lazily evaluated.
    /// </remarks>
    public static Result OkIf(bool isSuccess, Func<string> errorMessageFactory) => isSuccess ? Ok() : Fail(errorMessageFactory.Invoke());

    /// <summary>
    /// Create a success/failed result depending on the parameter isFailure
    /// </summary>
    public static Result FailIf(bool isFailure, Error error) => isFailure ? Fail(error) : Ok();

    /// <summary>
    /// Create a success/failed result depending on the parameter isFailure
    /// </summary>
    public static Result FailIf(bool isFailure, string error) => isFailure ? Fail(error) : Ok();

    /// <summary>
    /// Create a success/failed result depending on the parameter isFailure
    /// </summary>
    /// <remarks>
    /// Error is lazily evaluated.
    /// </remarks>
    public static Result FailIf(bool isFailure, Func<Error> errorFactory) =>
        isFailure ? Fail(errorFactory.Invoke()) : Ok();

    /// <summary>
    /// Create a success/failed result depending on the parameter isFailure
    /// </summary>
    /// <remarks>
    /// Error is lazily evaluated.
    /// </remarks>
    public static Result FailIf(bool isFailure, Func<string> errorMessageFactory) =>
        isFailure ? Fail(errorMessageFactory.Invoke()) : Ok();


    public static bool HasError<TError>(
        IEnumerable<Error> errors,
        Func<TError, bool> predicate,
        out IEnumerable<TError> result
    ) where TError : Error
    {
        // var foundErrors = errors.OfType<ExceptionalError>()
        //     .Where(e => e.Exception is TException rootExceptionOfTException
        //                 && predicate(rootExceptionOfTException))
        //     .ToList();
        //
        // if (foundErrors.Any())
        // {
        //     result = foundErrors;
        //     return true;
        // } as a reminder of what this method originally did

        foreach (var error in errors)
        {
            if (!HasError(error.ErrorReasons, predicate, out var fErrors))
            {
                continue;
            }

            result = fErrors;
            return true;
        }

        result = Array.Empty<TError>();
        return false;
    }

    /// <summary>
    /// Check if the result object contains an error with a specific condition
    /// </summary>
    public bool HasError(Func<Error, bool> predicate) => HasError(predicate, out _);

    /// <summary>
    /// Check if the result object contains an error with a specific condition
    /// </summary>
    public bool HasError(Func<Error, bool> predicate, out IEnumerable<Error> result)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return HasError(Errors, predicate, out result);
    }

    /// <summary>
    /// Check if the result object contains a success from a specific type
    /// </summary>
    public bool HasSuccess<TSuccess>() where TSuccess : Success =>
        HasSuccess<TSuccess>(_ => true, out _);

    /// <summary>
    /// Check if the result object contains a success from a specific type
    /// </summary>
    public bool HasSuccess<TSuccess>(out IEnumerable<TSuccess> result) where TSuccess : Success =>
        HasSuccess(_ => true, out result);

    /// <summary>
    /// Check if the result object contains a success from a specific type and with a specific condition
    /// </summary>
    public bool HasSuccess<TSuccess>(Func<TSuccess, bool> predicate) where TSuccess : Success =>
        HasSuccess(predicate, out _);

    public bool HasSuccess<TSuccess>(Func<TSuccess, bool> predicate, out IEnumerable<TSuccess> result)
        where TSuccess : Success =>
        HasSuccess(Successes, predicate, out result);

    /// <summary>
    /// Check if the result object contains a success with a specific condition
    /// </summary>
    public bool HasSuccess(Func<Success, bool> predicate, out IEnumerable<Success> result) =>
        HasSuccess(Successes, predicate, out result);

    /// <summary>
    /// Check if the result object contains a success with a specific condition
    /// </summary>
    public bool HasSuccess(Func<Success, bool> predicate) =>
        HasSuccess(Successes, predicate, out _);

    public static bool HasSuccess<TSuccess>(
        IEnumerable<Success> successes,
        Func<TSuccess, bool> predicate,
        out IEnumerable<TSuccess> result) where TSuccess : Success
    {
        var foundSuccesses = successes.OfType<TSuccess>()
            .Where(predicate)
            .ToList();
        if (foundSuccesses.Any())
        {
            result = foundSuccesses;
            return true;
        }

        result = Array.Empty<TSuccess>();
        return false;
    }

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


    /// <summary>
    /// Deconstruct Result
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="isFailed"></param>
    /// <param name="errors"></param>
    public void Deconstruct(out bool isSuccess, out bool isFailed, out IEnumerable<Error> errors)
    {
        isSuccess = IsSuccess;
        isFailed = IsFailed;
        errors = IsFailed ? Errors : Enumerable.Empty<Error>();
    }
}
