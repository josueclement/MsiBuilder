using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Carbon.Avalonia.Desktop.Services;

namespace MsiBuilderUI.Services;

/// <summary>
/// <see cref="IStoragePickerService"/> backed by Carbon's file/folder dialog services. The active
/// window's Avalonia <see cref="IStorageProvider"/> is wired once at startup via
/// <c>SetStorageProvider</c> (see <c>App.OnFrameworkInitializationCompleted</c>).
/// </summary>
public class CarbonStoragePickerService : IStoragePickerService
{
    private readonly IFileDialogService _files;
    private readonly IFolderDialogService _folders;

    public CarbonStoragePickerService(IFileDialogService files, IFolderDialogService folders)
    {
        _files = files;
        _folders = folders;
    }

    public async Task<string?> PickFolderAsync(string title)
    {
        IEnumerable<string> folders = await _folders.ShowOpenFolderDialogAsync(title: title, allowMultiple: false);
        return folders.FirstOrDefault();
    }

    public async Task<string?> PickFileAsync(string title, string patternName, IReadOnlyList<string> extensions)
    {
        IEnumerable<string> files = await _files.ShowOpenFileDialogAsync(
            title: title,
            allowMultiple: false,
            fileTypeFilter: new[] { CreateFileType(patternName, extensions) });

        return files.FirstOrDefault();
    }

    public async Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string patternName, IReadOnlyList<string> extensions)
        => await _files.ShowSaveFileDialogAsync(
            title: title,
            suggestedFileName: suggestedFileName,
            fileTypeChoices: new[] { CreateFileType(patternName, extensions) });

    private static FilePickerFileType CreateFileType(string name, IReadOnlyList<string> extensions)
        => new(name) { Patterns = extensions.Select(extension => "*." + extension).ToList() };
}
