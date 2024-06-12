using Microsoft.AspNetCore.Mvc;

namespace Custom.Client.Controllers
{
    [ApiController]
    public class ClientController : ControllerBase
    {
        [HttpGet]
        [Route("/client/test")]
        public Task<string> TestAsync()
        {
            return Task.FromResult("Message from client.");
        }
    }
}
