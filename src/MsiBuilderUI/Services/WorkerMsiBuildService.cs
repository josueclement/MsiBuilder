using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MsiBuilder.Contracts;
using MsiBuilderUI.Options;

namespace MsiBuilderUI.Services;

/// <summary>
/// Runs the build out-of-process by launching the net472 worker exe with a JSON request file and reading back
/// a JSON result file. The worker's stdout/stderr is streamed to <c>log</c> as the live build log.
/// </summary>
public class WorkerMsiBuildService : IMsiBuildService
{
    private readonly WorkerOptions _options;

    public WorkerMsiBuildService(IOptions<WorkerOptions> options)
    {
        _options = options.Value;
    }

    public async Task<MsiBuildResult> BuildAsync(MsiBuildRequest request, IProgress<string> log, CancellationToken cancellationToken)
    {
        string workerPath = ResolveWorkerPath();
        if (!File.Exists(workerPath))
        {
            return Fail($"Worker executable not found at '{workerPath}'. Build the solution so the worker is copied " +
                        "next to the UI, or set Worker:WorkerPath in appsettings.json.");
        }

        string requestPath = Path.Combine(Path.GetTempPath(), $"msibuild-req-{Guid.NewGuid():N}.json");
        string resultPath = Path.Combine(Path.GetTempPath(), $"msibuild-res-{Guid.NewGuid():N}.json");

        try
        {
            await File.WriteAllTextAsync(requestPath, MsiContractSerializer.Serialize(request), cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = workerPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(workerPath) ?? Environment.CurrentDirectory
            };
            startInfo.ArgumentList.Add("--request");
            startInfo.ArgumentList.Add(requestPath);
            startInfo.ArgumentList.Add("--result");
            startInfo.ArgumentList.Add(resultPath);

            using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            process.OutputDataReceived += (_, e) => { if (e.Data is not null) log.Report(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data is not null) log.Report(e.Data); };

            if (!process.Start())
                return Fail("Failed to start the worker process.");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cancellationToken);

            if (File.Exists(resultPath))
            {
                string json = await File.ReadAllTextAsync(resultPath, cancellationToken);
                MsiBuildResult? result = MsiContractSerializer.Deserialize<MsiBuildResult>(json);
                if (result is not null)
                    return result;
            }

            return Fail($"Worker exited with code {process.ExitCode} but produced no result file.");
        }
        finally
        {
            TryDelete(requestPath);
            TryDelete(resultPath);
        }
    }

    private string ResolveWorkerPath()
    {
        if (!string.IsNullOrWhiteSpace(_options.WorkerPath))
            return _options.WorkerPath;

        return Path.Combine(AppContext.BaseDirectory, _options.Subfolder, _options.ExecutableName);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static MsiBuildResult Fail(string message)
        => new() { Success = false, MsiPath = null, Message = message };
}
