using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Application.DTOs;
using PortfolioTracker.Application.Interfaces;
using PortfolioTracker.Domain.Entities;

namespace PortfolioTracker.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/portfolios")]
    public class PortfolioController(IPortfolioService portfolioService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioSummaryDto>>> GetAllByUser()
        {
            var result = await portfolioService.GetUserPortfoliosAsync();
            return Ok(result);
        }

        [HttpGet("{portfolioId}")]
        public async Task<ActionResult<PortfolioDetailDto>> GetByIdWithItems(Guid portfolioId)
        {
            var result = await portfolioService.GetByIdWithItemsAsync(portfolioId);
            return Ok(result);
        }

        [HttpGet("{portfolioId}/dashboard")]
        public async Task<ActionResult<IEnumerable<PortfolioSummaryDto>>> GetDashboardById(Guid portfolioId)
        {
            var result = await portfolioService.GetDashboardAsync(portfolioId);
            return Ok(result);
        }

        [HttpGet("{portfolioId}/value-history")]
        public async Task<ActionResult<List<PortfolioValuePointDto>>> GetValueHistory(Guid portfolioId)
        {
            var result = await portfolioService.GetValueHistoryAsync(portfolioId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePortfolioDto portfolio)
        {
            await portfolioService.CreateAsync(portfolio);

            return NoContent();
        }

        [HttpPut("{portfolioId}")]
        public async Task<IActionResult> Update(UpdatePortfolioDto portfolio, Guid portfolioId)
        {
            await portfolioService.UpdateAsync(portfolio, portfolioId);

            return NoContent();
        }

        [HttpDelete("{portfolioId}")]
        public async Task<IActionResult> Delete(Guid portfolioId)
        {
            await portfolioService.DeleteAsync(portfolioId);

            return NoContent();
        }
    }
}
