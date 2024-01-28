namespace VGManager.Library.Services.Interfaces;

public interface IAdapterCommunicator
{
    Task<(bool, string)> CommunicateWithAdapterAsync<T>(
        T request,
        string commandTypes,
        CancellationToken cancellationToken = default
        );
}
