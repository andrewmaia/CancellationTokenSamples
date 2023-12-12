using System.Diagnostics;
using CancellationTokenApiSample.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CancellationTokenApiSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController(ILogger<SampleController> logger,IHttpClientFactory clientFactory, CancellationTokenApiSampleContext db) : ControllerBase
    {

        //Sem CancellationToken
        [HttpGet]
        [Route("/Call/HelloWord")]
        public async Task<string> GetSemToken()
        {
            var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7004/");
            var sw = Stopwatch.StartNew();
            var respose = await client.GetAsync("/HelloWord");
            var aux = await respose.Content.ReadAsStringAsync();
            logger.LogInformation($"Response /Call/HelloWord after {sw.Elapsed}");
            return aux!;
        }

        //Com CancellationToken com uma tafefa cancelavel
        [HttpGet]
        [Route("/CallCT/HelloWordCancelation")]
        public async Task<string> GetHelloWordCancelation(CancellationToken token)
        {
            var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7004/");
            var aux = string.Empty;
            var sw = Stopwatch.StartNew();
            try
            {
                //propagando o token
                var respose = await client.GetAsync("/HelloWordCancelation", token);
                aux = await respose.Content.ReadAsStringAsync(token);
                logger.LogInformation($"Response /CallCT/HelloWordCancelation after {sw.Elapsed}");
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation($"Response /CallCT/HelloWordCancelation after {sw.Elapsed}");
            }
            return aux;
        }

        //Com CancellationToken sem uma tafefa cancelavel
        [HttpGet]
        [Route("/CallCT/HelloWord")]
        public async Task<string> GetHelloWord(CancellationToken token)
        {
            var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7004/");
            var aux = string.Empty;
            var sw = Stopwatch.StartNew();
            try
            {
                //propagando o token
                var respose = await client.GetAsync("/HelloWord", token);
                aux = await respose.Content.ReadAsStringAsync(token);
                logger.LogInformation($"Response /CallCT/HelloWord after {sw.Elapsed}");
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation($"Response /CallCT/HelloWord after {sw.Elapsed}");
            }
            return aux;
        }


        [HttpPut]
        [Route("/Call/OpenDb")]
        public async Task<Results<Ok, ProblemHttpResult>> Put()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await OpenCloseDb();
                return TypedResults.Ok();
            }
            catch (SqlException)
            {
                logger.LogInformation($"Response /CallCT/DbModelSampleafter after {sw.Elapsed}");
                return TypedResults.Problem("Error");
            }
        }

        private async Task OpenCloseDb()
        {
            await db.Database.OpenConnectionAsync();
            db.Database.CloseConnection();
        }


        [HttpPut]
        [Route("/CallCT/OpenDb")]
        public async Task<Results<Ok, NotFound,ProblemHttpResult >> Put(CancellationToken token)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await OpenCloseDbCancelavel(token);
                return TypedResults.Ok();
            }
            catch (SqlException)
            {
                logger.LogInformation($"Response /CallCT/DbModelSample after {sw.Elapsed}");
                return TypedResults.Problem("Error");
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation($"Response /CallCT/DbModelSample after {sw.Elapsed}");
                return TypedResults.NotFound();
            }
        }

        private async Task OpenCloseDbCancelavel(CancellationToken token)
        {
            await db.Database.OpenConnectionAsync(token);
            db.Database.CloseConnection();
        }


    }
}
