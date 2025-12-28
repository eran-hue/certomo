using Shared.Application.DTOs;

namespace Shared.Application.Abstractions;

public interface ICertomoService
{
    Task<CertomoAuthResponse> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<List<CertomoBank>> GetAllBanksAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<CertomoBankDataResponse> GetBankDataAsync(string accessToken, string username, string secret, CancellationToken cancellationToken = default);
    Task<CertomoIsraelAuthResponse> InitiateIsraelOpenBankingAuthAsync(string accessToken, CertomoIsraelAuthRequest request, CancellationToken cancellationToken = default);
}
