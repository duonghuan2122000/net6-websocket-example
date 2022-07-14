using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using TestSignalR.Hubs;
using TestSignalR.Models;

namespace TestSignalR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;

        public HomeController(ILogger<HomeController> logger, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("IPN")]
        public async Task<IActionResult> IPNWebhook(string who, string message)
        {
            await _hubContext.Clients.Group(who).SendAsync("IPNWebhook", message);
            return Ok();
        }

        [HttpGet("GenQRCode")]
        public IActionResult GenQRCode(string who, string message)
        {
            string host = Request.Scheme + "://" + Request.Host.Value;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode($"{host}/IPN?who={who}&message={message}", QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string qrCodeImageAsBase64 = qrCode.GetGraphic(20);
            ViewBag.QRCodeImg = qrCodeImageAsBase64;
            ViewBag.Who = who;
            return View();
        }

        [HttpGet("Result")]
        public IActionResult Success(string message)
        {
            return Ok("Ok - " + message);
        }
    }
}
