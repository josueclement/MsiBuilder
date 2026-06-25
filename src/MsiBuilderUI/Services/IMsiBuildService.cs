using System;
using System.Threading;
using System.Threading.Tasks;
using MsiBuilder.Contracts;

namespace MsiBuilderUI.Services;

/// <summary>Runs an MSI build for a request, reporting build output line-by-line via <paramref name="log"/>.</summary>
public interface IMsiBuildService
{
    Task<MsiBuildResult> BuildAsync(MsiBuildRequest request, IProgress<string> log, CancellationToken cancellationToken);
}
