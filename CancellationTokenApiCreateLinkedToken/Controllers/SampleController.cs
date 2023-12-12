using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CancellationTokenApiCreateLinkedToken.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController(ILogger<SampleController> logger, IHttpClientFactory clientFactory) : ControllerBase
    {

        [HttpGet]
        [Route("/Call/HelloWordWithTimeout")]

        public async Task<Results<Ok<string>, NoContent, ProblemHttpResult>> Get([FromQuery] bool timeout, CancellationToken token)
        {
            var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7084/");
            var sw = Stopwatch.StartNew();
            string aux = string.Empty;
            int status;
            try
            {
                var respose = await client.GetAsync($"/HelloWordWithTimeout?timeout={timeout}", token);
                if (respose.StatusCode == HttpStatusCode.OK)
                {
                    aux = await respose.Content.ReadAsStringAsync(token);
                }
                status = (int)respose.StatusCode;
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning($"Response /Call/HelloWordWithTimeout client aborted");
                return TypedResults.Problem("Client Closed Request", statusCode: 499);
            }
            catch (Exception ex)
            {
                logger.LogError($"Response /Call/HelloWordWithTimeout error: {ex}");
                return TypedResults.Problem("Erro interno", statusCode: 500);
            }
            if (status == 200)
            {
                logger.LogInformation($"Response /Call/HelloWordWithTimeout after {sw.Elapsed}");
                return TypedResults.Ok(aux);
            }
            //timeout
            if (status == 204)
            {
                logger.LogInformation($"Response /Call/HelloWordWithTimeout timeout {sw.Elapsed}");
                return TypedResults.NoContent();
            }
            return TypedResults.Problem("Não mapeado", statusCode: status);
        }
    }
}
