using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Redis
{
    public class Model
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IDistributedCache _distributedCache;

        public Worker(ILogger<Worker> logger, IDistributedCache distributedCache)
        {
            _logger = logger;
            _distributedCache = distributedCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var model = new Model { Name = "Thomas", Surname = "La Mendola" };
                _distributedCache.Set("my-cache-item", Utf8Json.JsonSerializer.Serialize(model));

                var fromCache = _distributedCache.Get("my-cache-item");
                var objFromCache = Utf8Json.JsonSerializer.Deserialize<Model>(fromCache);
                _logger.LogInformation($"Name: {objFromCache.Name}; Surname: {objFromCache.Surname}");

                _distributedCache.Remove("my-cache-item");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
