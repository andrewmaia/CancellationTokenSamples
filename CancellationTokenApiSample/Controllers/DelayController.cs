using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CancellationTokenApiSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Delay10SecondsController(ILogger<Delay10SecondsController> logger) : ControllerBase
    {
        [HttpGet]
        [Route("/HelloWord")]
        public string Get()
        {
            var sw = Stopwatch.StartNew();
            Thread.Sleep(10000);
            sw.Stop();
            logger.LogInformation($"Hello Word after {sw.Elapsed}");
            return $"Hello Word";
        }

        [HttpGet]
        [Route("/HelloWordCancelation")]
        public string Get(CancellationToken token)
        {
            var sw = Stopwatch.StartNew();
            token.WaitHandle.WaitOne(10000);
            sw.Stop();
            logger.LogInformation($"Hello Word after {sw.Elapsed}");
            return $"Hello Word";
        }

    }
}
