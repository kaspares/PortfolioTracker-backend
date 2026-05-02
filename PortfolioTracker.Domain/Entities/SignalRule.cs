using PortfolioTracker.Domain.Enums;

namespace PortfolioTracker.Domain.Entities;

public class SignalRule : BaseEntity
{
    public string UserId { get; set; }
    public string Ticker { get; set;  }
    public IndicatorType IndicatorType { get; set; }
    public ComparisonOperator ComparisonOperator { get; set; }
    public decimal Threshold { get; set; }
    public bool IsActive { get; set; }
    public TimeSpan CooldownPeriod { get; set; } = TimeSpan.FromHours(4);
    public DateTime? LastTriggeredAt { get; set; }
}
