namespace MsiBuilder.Contracts;

/// <summary>
/// Outcome of a build attempt, mirroring the tuple returned by <c>WixMsiBuilder.BuildMsi</c>.
/// Written by the worker to the result file and read back by the UI.
/// </summary>
public class MsiBuildResult
{
    /// <summary>Whether the build succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Path to the generated MSI when <see cref="Success"/> is true; otherwise null.</summary>
    public string? MsiPath { get; set; }

    /// <summary>Error/validation message when <see cref="Success"/> is false; otherwise null.</summary>
    public string? Message { get; set; }
}
