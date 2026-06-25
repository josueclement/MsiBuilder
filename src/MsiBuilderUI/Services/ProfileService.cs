using System.IO;
using System.Threading.Tasks;
using MsiBuilder.Contracts;

namespace MsiBuilderUI.Services;

/// <summary>Reads/writes build profiles as JSON using the shared <see cref="MsiContractSerializer"/>.</summary>
public class ProfileService : IProfileService
{
    public async Task SaveAsync(MsiBuildRequest request, string path)
    {
        string json = MsiContractSerializer.Serialize(request);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task<MsiBuildRequest> LoadAsync(string path)
    {
        string json = await File.ReadAllTextAsync(path);
        MsiBuildRequest? request = MsiContractSerializer.Deserialize<MsiBuildRequest>(json);
        return request ?? throw new InvalidDataException($"Profile '{path}' did not contain a valid build request.");
    }
}
