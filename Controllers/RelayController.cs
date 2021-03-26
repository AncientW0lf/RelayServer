using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RelayServer.Controllers
{
    [ApiController]
    public class RelayController : ControllerBase
    {
        [HttpGet("{*url}")]
        public IActionResult Get(string url)
        {
            string requestUrl = string.Concat(
                        Request.Scheme,
                        "://",
                        Request.Host,
                        Request.PathBase,
                        Request.Path,
                        Request.QueryString);

            return Ok($"Received: {url} ({requestUrl})");
        }
    }
}
