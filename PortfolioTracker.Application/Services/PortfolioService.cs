using AutoMapper;
using Microsoft.Extensions.Logging;
using PortfolioTracker.Application.DTOs;
using PortfolioTracker.Application.Exceptions;
using PortfolioTracker.Application.Interfaces;
using PortfolioTracker.Domain.Entities;
using PortfolioTracker.Domain.Interfaces;
using PortfolioTracker.Domain.Repositories;

namespace PortfolioTracker.Application.Services;

public class PortfolioService(ILogger<PortfolioService> logger,
    IMapper mapper,
    IPortfolioRepository portfolioRepository,
    ICurrentUserService currentUser,
    IMarketDataProvider marketDataProvider) : IPortfolioService
{
    private async Task EnrichWithMarketDataAsync(PortfolioDetailDto dto)
    {
        var tickers = dto.Items.Select(i => i.Ticker).Distinct();
        var prices = new Dictionary<string, decimal>();

        foreach (var ticker in tickers)
        {
            try
            {
                prices[ticker] = await marketDataProvider.GetCurrentPriceAsync(ticker);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch price for {Ticker}", ticker);
            }
        }

        foreach (var item in dto.Items)
        {
            if (!prices.TryGetValue(item.Ticker, out var price)) continue;

            item.CurrentPrice = price;
            item.MarketValue = item.Quantity * price;
            var cost = item.Quantity * item.PurchasePrice;
            item.ProfitLoss = item.MarketValue - cost;
            item.ProfitLossPercent = cost != 0 ? Math.Round(item.ProfitLoss.Value / cost * 100, 2) : 0;
        }

        dto.TotalValue = Math.Round(dto.Items.Sum(i => i.MarketValue ?? 0));
        dto.TotalCost = Math.Round(dto.Items.Sum(i => i.Quantity * i.PurchasePrice));
        dto.TotalProfitLoss = dto.TotalValue - dto.TotalCost;
        dto.TotalProfitLossPercent = dto.TotalCost != 0
            ? Math.Round(dto.TotalProfitLoss.Value / dto.TotalCost.Value * 100, 2) : 0;
    }

    public async Task CreateAsync(CreatePortfolioDto dto)
    {
        logger.LogInformation("Creating new portfolio: {@Portfolio}", dto);

        var portfolio = mapper.Map<Portfolio>(dto);
        portfolio.UserId = currentUser.userId;
        
        await portfolioRepository.AddAsync(portfolio);
    }

    public async Task DeleteAsync(Guid id)
    {
        logger.LogInformation("Deleting portfolio with id: {@Id}", id);
        var portfolio = await portfolioRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Portfolio), id.ToString());

        await portfolioRepository.DeleteAsync(portfolio);
    }

    public async Task<PortfolioDetailDto> GetByIdWithItemsAsync(Guid id)
    {
        logger.LogInformation("Getting portfolio with id: {@Id}", id);
        var portfolio = await portfolioRepository.GetByIdWithItemsAsync(id)
            ?? throw new NotFoundException(nameof(Portfolio), id.ToString());
        if (portfolio.UserId != currentUser.userId)
            throw new NotImplementedException("Forbidden");

        var dto = mapper.Map<PortfolioDetailDto>(portfolio);
        await EnrichWithMarketDataAsync(dto);
        return dto;
    }

    public async Task<IEnumerable<PortfolioSummaryDto>> GetUserPortfoliosAsync()
    {
        logger.LogInformation("Getting portfolios for {@CurrentUser}", currentUser.userId);
        var userPortfolios = await portfolioRepository.GetAllByUserIdAsync(currentUser.userId)
            ?? throw new NotFoundException(nameof(Portfolio), currentUser.userId.ToString());

        return mapper.Map<IEnumerable<PortfolioSummaryDto>>(userPortfolios);
    }

    public async Task UpdateAsync(UpdatePortfolioDto dto, Guid id)
    {
        logger.LogInformation("Updating portfolio with id: {@Id}", id);
        var portfolio = await portfolioRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Portfolio), id.ToString());

        mapper.Map(dto, portfolio);
        await portfolioRepository.UpdateAsync(portfolio);
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid portfolioId)
    {
        logger.LogInformation("Getting dashboard for portfolio with id: {@PortfolioId}", portfolioId);
        var detail = await GetByIdWithItemsAsync(portfolioId);

        return new DashboardDto
        {
            TotalValue = detail.TotalValue ?? 0,
            TotalProfitLoss = detail.TotalProfitLoss ?? 0,
            TotalProfitLossPercent = detail.TotalProfitLossPercent ?? 0,
            TotalInstruments = detail.Items.Count
        };
    }

    public async Task<List<PortfolioValuePointDto>> GetValueHistoryAsync(Guid portfolioId)
    {
        var portfolio = await portfolioRepository.GetByIdWithItemsAsync(portfolioId)
            ?? throw new NotFoundException(nameof(Portfolio), portfolioId.ToString());
        
        if (portfolio.UserId != currentUser.userId)
            throw new NotImplementedException("Forbidden");

        var calendarDays = (DateTime.UtcNow - portfolio.CreatedAt).Days;
        var tradingDays = Math.Max((int)(calendarDays * 5.0 / 7.0) + 10, 30);

        var pricesByTicker = new Dictionary<string, Dictionary<string, decimal>>();

        foreach (var item in portfolio.Items)
        {
            if (pricesByTicker.ContainsKey(item.Ticker)) continue;

            var candles = await marketDataProvider.GetHistoricalPricesAsync(item.Ticker, tradingDays);
            pricesByTicker[item.Ticker] = candles.ToDictionary(
                    c => c.Data.ToString("yyyy-MM-dd"),
                    c => c.Close
            );
        }

        var allDates = pricesByTicker.Values
            .SelectMany(d => d.Keys)
            .Distinct()
            .Order()
            .ToList();

        var result = new List<PortfolioValuePointDto>();

        foreach (var date in allDates)
        {
            decimal totalValue = 0;
            foreach (var item in portfolio.Items)
            {
                if (pricesByTicker.TryGetValue(item.Ticker, out var prices)
                    && prices.TryGetValue(date, out var price))
                {
                    totalValue += item.Quantity * price;
                }
            }

            result.Add(new PortfolioValuePointDto
            {
                Date = date,
                Value = Math.Round(totalValue, 2)
            });
        }

        return result;

    }
}
