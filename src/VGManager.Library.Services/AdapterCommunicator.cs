using System.Text.Json;
using VGManager.Adapter.Client.Interfaces;
using VGManager.Library.Services.Interfaces;

namespace VGManager.Library.Services;

public class AdapterCommunicator(IVGManagerAdapterClientService clientService) : IAdapterCommunicator
{
    private readonly IVGManagerAdapterClientService _clientService = clientService;

    public async Task<(bool, string)> CommunicateWithAdapterAsync<T>(
        T request,
        string commandTypes,
        CancellationToken cancellationToken = default
        )
    {
        (bool isSuccess, string response) = await _clientService.SendAndReceiveMessageAsync(
            commandTypes,
            JsonSerializer.Serialize(request),
            cancellationToken);
        return (isSuccess, response);
    }
}
