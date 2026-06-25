namespace MsiBuilderUI.Options;

/// <summary>
/// Locates the net472 worker executable that performs the actual MSI build out-of-process.
/// Bound from the "Worker" configuration section.
/// </summary>
public class WorkerOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Worker";

    /// <summary>Explicit full path to the worker executable. When set, overrides the default lookup.</summary>
    public string? WorkerPath { get; set; }

    /// <summary>Subfolder of the app base directory that contains the worker (default "worker").</summary>
    public string Subfolder { get; set; } = "worker";

    /// <summary>Worker executable file name.</summary>
    public string ExecutableName { get; set; } = "MsiBuilder.Worker.exe";
}
