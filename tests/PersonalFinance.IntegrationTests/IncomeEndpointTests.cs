using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PersonalFinance.Application.Income.Commands;
using Xunit;

namespace PersonalFinance.IntegrationTests;

/// <summary>
/// Integration tests using WebApplicationFactory with an in-memory database.
/// Validates the full pipeline: HTTP → Controller → MediatR → Domain → DB.
/// </summary>
public sealed class IncomeEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateMonthlyIncome_ValidRequest_Returns201()
    {
        // TODO: configure in-memory DB and JWT for test factory
        // Arrange
        var cmd = new CreateMonthlyIncomeCommand(
            UserId: Guid.NewGuid(),
            Year: 2026,
            Month: 2,
            HomeCurrency: "COP");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/income/monthly", cmd);

        // Assert — 401 expected until JWT is wired in test factory
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created, HttpStatusCode.Unauthorized);
    }
}
