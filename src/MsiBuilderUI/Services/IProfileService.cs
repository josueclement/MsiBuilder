using System.Threading.Tasks;
using MsiBuilder.Contracts;

namespace MsiBuilderUI.Services;

/// <summary>
/// Persists/loads a build profile (a serialized <see cref="MsiBuildRequest"/>) to/from a file path.
/// </summary>
public interface IProfileService
{
    Task SaveAsync(MsiBuildRequest request, string path);

    Task<MsiBuildRequest> LoadAsync(string path);
}
