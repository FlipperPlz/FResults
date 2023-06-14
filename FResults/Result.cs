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
    /// Get all alerts (errors and warnings)
    /// </summary>
    IEnumerable<IAlert> Alerts => Reasons.OfType<IAlert>();
    
    /// <summary>
    /// Get all warnings
    /// </summary>
    IEnumerable<Warning> Warnings => Reasons.OfType<Warning>();
    
    
    /// <summary>
    /// Get all errors
    /// </summary>
    IEnumerable<Error> Errors => Reasons.OfType<Error>();
    

    /// <summary>
    /// Is true if Reasons contains no errors. The opposite of IsFailed
    /// </summary>
    bool IsSuccess => !IsFailed;

    string IReason.Message => IsFailed ? "Failed." : "Succeeded.";
}