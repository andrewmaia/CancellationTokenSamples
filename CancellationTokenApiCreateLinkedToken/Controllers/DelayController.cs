using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CancellationTokenApiCreateLinkedToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Delay10SecondsController(ILogger<Delay10SecondsController> logger) : ControllerBase
    {
        [HttpGet]
        [Route("/HelloWordWithTimeout")]

        public async Task<Results<Ok<string>,NoContent>> Get([FromQuery] bool timeout, CancellationToken token)
        {
            using var ctstimout = new CancellationTokenSource();
            using var lnkcts = CancellationTokenSource.CreateLinkedTokenSource(token,ctstimout.Token);
            var tm = 1;
            if (timeout)
            {
                tm = 11000;
            }
            ctstimout.CancelAfter(10000);
            var sw = Stopwatch.StartNew();
            var hastimeout = false;
            if (lnkcts.Token.WaitHandle.WaitOne(tm))
            {
                if (token.IsCancellationRequested)
                {
                    logger.LogInformation($"Hello Word has cancled by client");
                }
                else
                {
                    logger.LogInformation($"Hello Word has timeout {sw.Elapsed}");
                }
                hastimeout = true;
            }
            else
            {
                logger.LogInformation($"Hello Word after {sw.Elapsed}");
            }
            sw.Stop();
            if (hastimeout)
            {
                return await Task.FromResult(TypedResults.NoContent());
            }
            return await Task.FromResult(TypedResults.Ok("Hello Word"));
        }
    }
}
