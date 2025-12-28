using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions;
using Shared.Application.DTOs;
using Shared.Infrastructure.Configuration;

namespace Shared.Infrastructure.Services;

public class CertomoService : ICertomoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CertomoService> _logger;
    private readonly CertomoOptions _options;

    public CertomoService(HttpClient httpClient, ILogger<CertomoService> logger, IOptions<CertomoOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<CertomoAuthResponse> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CertomoAuthRequest(username, password);
            var response = await _httpClient.PostAsJsonAsync("/authenticate", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CertomoAuthResponse>(cancellationToken: cancellationToken) 
                   ?? throw new InvalidOperationException("Failed to deserialize auth response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate with Certomo API.");
            throw;
        }
    }

    public async Task<List<CertomoBank>> GetAllBanksAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("/bank/getAll", cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<CertomoBank>>(cancellationToken: cancellationToken) 
                   ?? new List<CertomoBank>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch banks from Certomo API.");
            throw;
        }
    }

    public async Task<CertomoBankDataResponse> GetBankDataAsync(string accessToken, string username, string secret, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            // Adding 'secret' to headers as per documentation for GetBankData
            if (!_httpClient.DefaultRequestHeaders.Contains("secret"))
            {
                _httpClient.DefaultRequestHeaders.Add("secret", secret);
            }

            // Using query parameter for username as per spec: /flow/getBankData?username=<<username>>
            var response = await _httpClient.GetAsync($"/flow/getBankData?username={Uri.EscapeDataString(username)}", cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CertomoBankDataResponse>(cancellationToken: cancellationToken)
                   ?? throw new InvalidOperationException($"Failed to deserialize bank data for user {username}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch bank data for user {Username} from Certomo API.", username);
            throw;
        }
    }

    public async Task<CertomoIsraelAuthResponse> InitiateIsraelOpenBankingAuthAsync(string accessToken, CertomoIsraelAuthRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            // Construct query string manually as per GET request spec
            // /cfoAccessInfo/ilObinitiateAuthUrl
            // URL Parameters: psuId, businessUid, bankId, accountType, tsAndCs, partner, channel
            
            var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
            query["psuId"] = request.PsuId;
            if (!string.IsNullOrEmpty(request.BusinessUid)) query["businessUid"] = request.BusinessUid;
            query["bankId"] = request.BankId.ToString();
            query["accountType"] = request.AccountType;
            query["tsAndCs"] = request.TsAndCs.ToString().ToLower(); // API likely expects "true"/"false"
            query["partner"] = request.Partner;
            query["channel"] = request.Channel;

            var response = await _httpClient.GetAsync($"/cfoAccessInfo/ilObinitiateAuthUrl?{query}", cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CertomoIsraelAuthResponse>(cancellationToken: cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize Israel Open Banking auth response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate Israel Open Banking auth.");
            throw;
        }
    }
}
