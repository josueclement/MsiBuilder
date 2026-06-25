using System;
using System.IO;
using MsiBuilder.Contracts;

namespace MsiBuilder.Worker;

/// <summary>
/// One-shot CLI adapter: reads a serialized <see cref="MsiBuildRequest"/>, runs the build, and writes a
/// <see cref="MsiBuildResult"/>. All WixSharp console output flows through stdout/stderr so the calling UI
/// can stream it as a live build log.
/// </summary>
/// <remarks>
/// Deliberately not bootstrapped with <c>IHost</c>: this is a stateless process spawned per build, with no
/// configuration, logging, lifetime, or DI needs beyond a single call.
/// </remarks>
internal static class Program
{
    // Exit codes: 0 = build succeeded, 1 = build failed, 2 = bad arguments / unexpected error.
    public static int Main(string[] args)
    {
        string? requestPath = null;
        string? resultPath = null;

        for (int i = 0; i < args.Length - 1; i++)
        {
            switch (args[i])
            {
                case "--request":
                    requestPath = args[i + 1];
                    i++;
                    break;
                case "--result":
                    resultPath = args[i + 1];
                    i++;
                    break;
            }
        }

        if (requestPath is null || resultPath is null)
        {
            Console.Error.WriteLine("Usage: MsiBuilder.Worker --request <request.json> --result <result.json>");
            return 2;
        }

        try
        {
            string json = File.ReadAllText(requestPath);
            MsiBuildRequest? request = MsiContractSerializer.Deserialize<MsiBuildRequest>(json);
            if (request is null)
            {
                WriteResult(resultPath, new MsiBuildResult { Success = false, Message = "Request file was empty or invalid." });
                return 2;
            }

            MsiBuildResult result = BuilderConfigurator.Build(request);
            WriteResult(resultPath, result);
            return result.Success ? 0 : 1;
        }
        catch (Exception ex)
        {
            // Surface the failure to stderr (streamed to the UI log) and best-effort to the result file.
            Console.Error.WriteLine(ex);
            TryWriteResult(resultPath, new MsiBuildResult { Success = false, Message = ex.ToString() });
            return 2;
        }
    }

    private static void WriteResult(string path, MsiBuildResult result)
        => File.WriteAllText(path, MsiContractSerializer.Serialize(result));

    private static void TryWriteResult(string path, MsiBuildResult result)
    {
        // Best-effort secondary write; the primary error is already on stderr, so a failure here is moot.
        try
        {
            WriteResult(path, result);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
