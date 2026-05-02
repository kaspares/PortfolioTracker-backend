using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Application.DTOs;
using PortfolioTracker.Application.Interfaces;
using PortfolioTracker.Application.Services;

namespace PortfolioTracker.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/{portfolioId}/portfolioItems")]
    public class PortfolioItemController(IPortfolioItemService portfolioItemService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioItemDto>>> GetAllPortfolioItemsByPortfolio(Guid portfolioId)
        {
            var result = await portfolioItemService.GetAllByPortfolio(portfolioId);
            return Ok(result);
        }

        [HttpGet]
        [Route("{portfolioItemId}")]
        public async Task<ActionResult<PortfolioItemDto>> GetPortfolioItemById(Guid portfolioId, Guid portfolioItemId)
        {
            var result = await portfolioItemService.GetByIdAsync(portfolioId, portfolioItemId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePortfolioItemDto portfolioItemDto, Guid portfolioId)
        {
            await portfolioItemService.CreateAsync(portfolioItemDto, portfolioId);
            return NoContent();
        }

        [HttpPut("{portfolioItemId}")]
        public async Task<IActionResult> Update(UpdatePortfolioItemDto portfolioItemDto, Guid portfolioId, Guid portfolioItemId)
        {
            await portfolioItemService.UpdateAsync(portfolioItemDto, portfolioId, portfolioItemId);

            return NoContent();
        }

        [HttpDelete("{portfolioItemId}")]
        public async Task<IActionResult> Delete(Guid portfolioId, Guid portfolioItemId)
        {
            await portfolioItemService.DeleteAsync(portfolioId, portfolioItemId);

            return NoContent();
        }

    }
}
