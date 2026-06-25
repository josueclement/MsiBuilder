using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace MsiBuilderUI.Services;

/// <summary>
/// <see cref="IStoragePickerService"/> backed by the active window's Avalonia <see cref="IStorageProvider"/>.
/// </summary>
public class AvaloniaStoragePickerService : IStoragePickerService
{
    public async Task<string?> PickFolderAsync(string title)
    {
        IStorageProvider? storage = GetStorageProvider();
        if (storage is null)
            return null;

        IReadOnlyList<IStorageFolder> folders = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
    }

    public async Task<string?> PickFileAsync(string title, string patternName, IReadOnlyList<string> extensions)
    {
        IStorageProvider? storage = GetStorageProvider();
        if (storage is null)
            return null;

        IReadOnlyList<IStorageFile> files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new[] { CreateFileType(patternName, extensions) }
        });

        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string patternName, IReadOnlyList<string> extensions)
    {
        IStorageProvider? storage = GetStorageProvider();
        if (storage is null)
            return null;

        IStorageFile? file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            FileTypeChoices = new[] { CreateFileType(patternName, extensions) }
        });

        return file?.TryGetLocalPath();
    }

    private static FilePickerFileType CreateFileType(string name, IReadOnlyList<string> extensions)
        => new(name) { Patterns = extensions.Select(extension => "*." + extension).ToList() };

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is { } window)
        {
            return window.StorageProvider;
        }

        return null;
    }
}
