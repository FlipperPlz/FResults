namespace FResults.Extensions;

using Reasoning;

public static class ResultBuilderExtensions
{
    public static Result WithReason(this Result result, IReason reason)
    {
        result.Reasons.Add(reason);
        return result;
    }

    public static Result WithoutReason(this Result result, IReason reason)
    {
        result.Reasons.Remove(reason);
        return result;
    }

    /// <summary>
    /// Add multiple reasons (success or error)
    /// </summary>
    public static Result WithReasons(this Result result, IEnumerable<IReason> reasons)
    {
        result.Reasons.AddRange(reasons);
        return result;
    }

    /// <summary>
    /// Remove multiple reasons (success or error)
    /// </summary>
    public static Result WithoutReasons(this Result result, IEnumerable<IReason> reasons)
    {
        foreach (var reason in reasons)
        {
            WithoutReason(result, reason);
        }
        return result;
    }

    /// <summary>
    /// Add multiple errors
    /// </summary>
    public static Result WithErrors(this Result result, IEnumerable<ErrorBase> errors) =>
        WithReasons(result, errors);

    /// <summary>
    /// Removes multiple errors
    /// </summary>
    public static Result WithoutErrors(this Result result, IEnumerable<ErrorBase> errors) =>
        WithoutReasons(result, errors);


    /// <summary>
    /// Add multiple warnings
    /// </summary>
    public static Result WithWarnings(this Result result, IEnumerable<WarningBase> warnings) =>
        WithReasons(result, warnings);

    /// <summary>
    /// Remove multiple warnings
    /// </summary>
    public static Result WithoutWarnings(this Result result, IEnumerable<WarningBase> warnings) =>
        WithoutReasons(result, warnings);

    /// <summary>
    /// Add an error
    /// </summary>
    public static Result WithError<TError>(this Result result) where TError : ErrorBase, new() =>
        WithError(result, new TError());

    /// <summary>
    /// Removes a type of error
    /// </summary>
    public static Result WithoutError<TError>(this Result result) where TError : ErrorBase, new() =>
        WithoutErrors(result, result.Errors.OfType<TError>());

    /// <summary>
    /// Add a warning
    /// </summary>
    public static Result WithWarning<TWarning>(this Result result) where TWarning : WarningBase, new() =>
        WithReason(result, new TWarning());

    /// <summary>
    /// Add an error
    /// </summary>
    public static Result WithoutWarnings<TWarning>(this Result result) where TWarning : WarningBase, new() =>
        WithoutWarnings(result, result.Warnings.OfType<TWarning>());

    /// <summary>
    /// Add a success
    /// </summary>
    public static Result WithSuccess(this Result result) =>
        WithSuccess(result, Result.DefaultSuccess);

    /// <summary>
    /// Add a success
    /// </summary>
    public static Result WithSuccess(this Result result, Success success) => WithReason(result, success);

    /// <summary>
    /// Add an error
    /// </summary>
    public static Result WithError(this Result result, string errorName, Type? errorScope = null, string? errorMessage = null) =>
        WithError(result, new Error {
            AlertName = errorName,
            AlertScope = errorScope,
            Message = errorMessage
        });

    /// <summary>
    /// Add an error
    /// </summary>
    public static Result WithWarning(this Result result, string warningName, Type? warningScope = null, string? warningMessage = null) =>
        WithWarning(result, new Warning {
            AlertName = warningName,
            AlertScope = warningScope,
            Message = warningMessage
        });

    /// <summary>
    /// Add a warning
    /// </summary>
    public static Result WithWarning(this Result result, WarningBase warning) =>
        WithReason(result, warning);

    /// <summary>
    /// Add an error
    /// </summary>
    public static Result WithError(this Result result, ErrorBase error) =>
        WithReason(result, error);

    public static Result WithSuccesses(this Result result, IEnumerable<Success> successes)
    {
        foreach (var success in successes)
        {
            WithSuccess(result, success);
        }

        return result;
    }
}
