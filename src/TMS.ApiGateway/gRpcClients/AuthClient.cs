namespace TMS.ApiGateway.gRpcClients;

public interface IAuthClient
{
    Task<bool> ValidateTokenAsync(string token);
}

public class AuthClient : IAuthClient
{
    public Task<bool> ValidateTokenAsync(string token)
    {
        return Task.FromResult(true);
    }
}
