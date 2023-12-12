using Microsoft.AspNetCore.Mvc;

namespace CancellationTokenBackGroudServiceSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController(ILogger<SampleController> logger, IHostApplicationLifetime applicationLifetime) : ControllerBase
    {
        [HttpPost]
        [Route("/StopApplication")]
        public void Post()
        {
            applicationLifetime.StopApplication();
        }
    }
}
