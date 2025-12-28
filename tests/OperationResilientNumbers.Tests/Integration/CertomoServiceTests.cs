using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Shared.Application.DTOs;
using Shared.Infrastructure.Configuration;
using Shared.Infrastructure.Services;
using Xunit;

namespace OperationResilientNumbers.Tests.Integration;

public class CertomoServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly CertomoService _sut;
    private readonly CertomoOptions _options;

    public CertomoServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        var loggerMock = new Mock<ILogger<CertomoService>>();
        _options = new CertomoOptions { BaseUrl = "https://api.certomo.com", ClientId = "test-client", ClientSecret = "test-secret" };
        var optionsMock = new Mock<IOptions<CertomoOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(_options.BaseUrl)
        };

        _sut = new CertomoService(httpClient, loggerMock.Object, optionsMock.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var username = "user";
        var password = "user123";
        // Constructor: (string Jwt)
        var expectedResponse = new CertomoAuthResponse("eyJhbGciOiJIUzI1NiJ9...");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.AbsolutePath.EndsWith("/authenticate")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        // Act
        var result = await _sut.AuthenticateAsync(username, password);

        // Assert
        result.Should().NotBeNull();
        result.Jwt.Should().Be(expectedResponse.Jwt);
    }

    [Fact]
    public async Task GetAllBanksAsync_ShouldReturnBanks_WhenTokenIsValid()
    {
        // Arrange
        var accessToken = "valid-token";
        var expectedBanks = new List<CertomoBank>
        {
            // Constructor: (bool Active, int Id, string Name, string Country, string City, string BankImg, string BaseCurrency)
            new CertomoBank(true, 1, "Nordea", "Denmark", "Copenhagen", "img.png", "EUR")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.AbsolutePath.EndsWith("/bank/getAll") &&
                    req.Headers.Authorization!.Parameter == accessToken),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedBanks)
            });

        // Act
        var result = await _sut.GetAllBanksAsync(accessToken);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Nordea");
    }

    [Fact]
    public async Task GetBankDataAsync_ShouldReturnData_WhenRequestIsValid()
    {
        // Arrange
        var accessToken = "valid-token";
        var username = "testuser";
        var secret = "secret123";
        // Constructor: (bool Active, int? Id, int? ParentId, List<CertomoAccount> AccountsList, List<CertomoTransaction> TransactionList, string DateTimeOfLastUpdate)
        var expectedData = new CertomoBankDataResponse(
            true, 1, null, new List<CertomoAccount>(), new List<CertomoTransaction>(), "2024-01-01");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.AbsolutePath.EndsWith("/flow/getBankData") &&
                    req.RequestUri.Query.Contains($"username={username}") &&
                    req.Headers.Contains("secret") &&
                    req.Headers.Authorization!.Parameter == accessToken),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedData)
            });

        // Act
        // Correct signature: (string accessToken, string username, string secret, CancellationToken cancellationToken = default)
        var result = await _sut.GetBankDataAsync(accessToken, username, secret);

        // Assert
        result.Should().NotBeNull();
        result.DateTimeOfLastUpdate.Should().Be("2024-01-01");
    }
}
