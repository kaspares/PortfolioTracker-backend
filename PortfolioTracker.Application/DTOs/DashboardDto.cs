namespace PortfolioTracker.Application.DTOs;

public class DashboardDto
{
    public decimal TotalValue { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal TotalProfitLossPercent { get; set; }
    public int TotalInstruments { get; set; }
}
