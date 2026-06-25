using System.Collections.Generic;
using System.Threading.Tasks;

namespace MsiBuilderUI.Services;

/// <summary>
/// Framework-neutral folder/file picking. Keeps ViewModels free of Avalonia storage types and mockable in tests.
/// All methods return the chosen local path, or null when the user cancels.
/// </summary>
public interface IStoragePickerService
{
    /// <summary>Pick an existing folder.</summary>
    Task<string?> PickFolderAsync(string title);

    /// <summary>Pick an existing file filtered by the given extensions (without the leading dot).</summary>
    Task<string?> PickFileAsync(string title, string patternName, IReadOnlyList<string> extensions);

    /// <summary>Pick a destination file for saving.</summary>
    Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string patternName, IReadOnlyList<string> extensions);
}
