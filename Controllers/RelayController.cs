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

            string localIp = Program.Settings.FirstOrDefault(a => a.DomainName.Equals(Request.Host.Host))?.LocalIp;

            if (string.IsNullOrWhiteSpace(localIp))
                return this.BadRequest($"\"{Request.Host.Host}\" is not a valid domain name for this server.");

            using var client = new HttpClient();

            HttpResponseMessage res = await client.GetAsync($"http://{localIp}/{url}");
            string type = res.Content.Headers.GetValues("Content-Type").First();
            byte[] content = await res.Content.ReadAsByteArrayAsync();

            return new FileContentResult(content, type);
        }
    }
}
