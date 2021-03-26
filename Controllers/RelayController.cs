using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RelayServer.Controllers
{
    [ApiController]
    public class RelayController : ControllerBase
    {
        [HttpGet("{*url}")]
        public async Task<IActionResult> Get(string url)
        {
            // string requestUrl = string.Concat(
            //             Request.Scheme,
            //             "://",
            //             Request.Host,
            //             Request.PathBase,
            //             Request.Path,
            //             Request.QueryString);

            using var client = new HttpClient();

            //TODO: Replace fixed IP with configuration file for links with domain names and local IPs.
            HttpResponseMessage res = await client.GetAsync($"http://192.168.178.33/{url}");
            string type = res.Content.Headers.GetValues("Content-Type").First();
            byte[] content = await res.Content.ReadAsByteArrayAsync();

            return new FileContentResult(content, type);
            //Ok($"Received: {url} ({requestUrl})");
        }
    }
}
