using Collateral.Api.Models;
using CollateralRisk.BuildingBlocks.Results;
using System.Net.Http.Json;

namespace Collateral.Api.Services;

public sealed class PositionsClient(HttpClient httpClient)
{
    public async Task<ServiceResult<List<PositionDto>>> GetPositionsByCustomerAsync(Guid customerId)
    {
        try
        {
            var response = await httpClient.GetAsync($"/v1/positions?customerId={customerId}");

            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<List<PositionDto>>.Fail(
                    $"Positions API returned status code {(int)response.StatusCode}.");
            }

            var positions = await response.Content.ReadFromJsonAsync<List<PositionDto>>();
            positions ??= new List<PositionDto>();

            return ServiceResult<List<PositionDto>>.Success(positions);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<PositionDto>>.Fail(
                $"Could not retrieve positions from Positions API. Details: {ex.Message}");
        }
    }
}