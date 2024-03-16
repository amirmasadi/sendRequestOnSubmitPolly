using Microsoft.AspNetCore.Mvc;
using NationalCodeChecker.Models;
using System.Diagnostics;
using System.Net;
using Polly;
using Polly.Retry;

namespace NationalCodeChecker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult CheckInfo(string Ncode, string mobile)
        {
            string url = "https://scrapeme.live/shop/123";
            RetryStrategy(url);
            Console.WriteLine($"NCode: {Ncode}, Phone Number: {mobile}");
            return RedirectToAction("Index");
        }

        //Polly Retry Strategy
        async public static void RetryStrategy(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var optionsComplex = new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(r => r.StatusCode == HttpStatusCode.NotFound),
                    MaxRetryAttempts = 4,
                    Delay = TimeSpan.Zero,
                    OnRetry = static args =>
                    {
                        Console.WriteLine("OnRetry, Attempt: {0}", args.AttemptNumber);
                        // Event handlers can be asynchronous; here, we return an empty ValueTask.
                        return default;
                    }
                };

                var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(optionsComplex)
                .Build();

                var response = await pipeline.ExecuteAsync(async token => await httpClient.GetAsync(url));
                Console.WriteLine($"Response: {response.StatusCode}");
            }
        }
    }
}
