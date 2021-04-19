using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RelayServer.Controllers
{
    [ApiController]
    public class RelayController : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetIndex(string url)
        {
            return await Get(url);
        }

        [HttpGet(@"{url:regex(^(?!\.well-known).*$)}")]
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
            client.DefaultRequestHeaders.Add("OriginalUrl", Request.Host.Host);

            HttpResponseMessage res;
            try
            {
                res = await client.GetAsync($"http://{localIp}/{url}");
            }
            catch (HttpRequestException exc)
            {
                return this.Problem($"Could not fetch data from local server: {exc.Message}");
            }

            string type = res.Content.Headers.GetValues("Content-Type").First();
            byte[] content = await res.Content.ReadAsByteArrayAsync();

            return new FileContentResult(content, type);
        }
    }
}
