using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Security.Claims;
using UrlShortener.DTOs;
using UrlShortener.Services;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("api/urls")]
    [Authorize]
    public class UrlController:ControllerBase
    {
        private readonly UrlService _urlService;
        public UrlController(UrlService urlService)
        {
            urlService = _urlService;
        }
        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUrlDto dto)
        {
            try
            {
                var result = await _urlService.CreateAsync(dto, GetUserId());
                return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
            }
            catch(Exception ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _urlService.GetUserUrlsAsync(GetUserId());
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _urlService.DeleteAsync(id, GetUserId());
            if (!result) return NotFound("Url not found");
            return Ok("Successfully deleted");
        }

        [HttpGet("{id}/analytics")]
        public async Task<IActionResult> Analytics(int id)
        {
            var result = await _urlService.GetAnalyticsAsync(id, GetUserId());
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("{id}/qr")]
        public async Task<IActionResult> QrCode(int id)
        {
            var urls = await _urlService.GetUserUrlsAsync(GetUserId());
            var url = urls.FirstOrDefault(u=>u.Id == id);
            if (url == null) return NotFound();

            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(url.ShortUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var pngBytes = qrCode.GetGraphic(20);

            return File(pngBytes, "image/png");
        }
    }
}
