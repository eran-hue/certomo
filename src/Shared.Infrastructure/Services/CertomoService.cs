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
            // API expected payload format is key: "username", "password" (lowercase) - trying this as "Password" failed.
            var payload = new 
            {
                username = username,
                password = password
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/authenticate");
            request.Content = JsonContent.Create(payload);

             if (!string.IsNullOrEmpty(_options.Secret))
            {
                request.Headers.Add("secret", _options.Secret);
            }
            else if (!string.IsNullOrEmpty(_options.ClientSecret))
            {
                 request.Headers.Add("secret", _options.ClientSecret);
            }
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            // Log response if failure for debugging (only in dev/debug)
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Certomo Auth Failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
            }

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
            var request = new HttpRequestMessage(HttpMethod.Get, "/bank/getAll");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
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
            var request = new HttpRequestMessage(HttpMethod.Get, $"/flow/getBankData?username={Uri.EscapeDataString(username)}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            // Adding 'secret' to headers as per documentation for GetBankData
            request.Headers.Add("secret", secret);

            var response = await _httpClient.SendAsync(request, cancellationToken);
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

    public async Task<CertomoIsraelAuthResponse> InitiateIsraelOpenBankingAuthAsync(string accessToken, CertomoIsraelAuthRequest authRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            // Construct query string manually as per GET request spec
            // /cfoAccessInfo/ilObinitiateAuthUrl
            // URL Parameters: psuId, businessUid, bankId, accountType, tsAndCs, partner, channel
            
            var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
            query["psuId"] = authRequest.PsuId;
            if (!string.IsNullOrEmpty(authRequest.BusinessUid)) query["businessUid"] = authRequest.BusinessUid;
            query["bankId"] = authRequest.BankId.ToString();
            query["accountType"] = authRequest.AccountType;
            query["tsAndCs"] = authRequest.TsAndCs.ToString().ToLower(); // API likely expects "true"/"false"
            query["partner"] = authRequest.Partner;
            query["channel"] = authRequest.Channel;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/cfoAccessInfo/ilObinitiateAuthUrl?{query}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
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
