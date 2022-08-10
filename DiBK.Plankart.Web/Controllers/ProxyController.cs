﻿using DiBK.Plankart.Application.HttpClients.Proxy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DiBK.Plankart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProxyController : BaseController
    {
        private readonly IProxyHttpClient _proxyHttpClient;

        public ProxyController(
            IProxyHttpClient proxyHttpClient,
            ILogger<ProxyController> logger) : base(logger)
        {
            _proxyHttpClient = proxyHttpClient;
        }

        [HttpGet]
        [ResponseCache(VaryByQueryKeys = new[] { "url" }, Duration = 86400)]
        public async Task<IActionResult> Get(string url)
        {
            try
            {
                return await _proxyHttpClient.GetAsync(url);               
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }
    }
}
