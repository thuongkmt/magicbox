using KonbiCloud.Configuration;
using Microsoft.Extensions.Options;
using ServiceStack.Redis;
using System;

namespace KonbiCloud.Common
{
    public class RedisService : IRedisService, IDisposable
    {
        private readonly RedisClient _client;
        public RedisService(IOptions<RedisOption> redisOption)
        {
            _client = new RedisClient(redisOption.Value.Host,
                redisOption.Value.Port,
                redisOption.Value.Credential);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public void PublishCommandToMachine(string machineId, string command)
        {
            _client.PublishMessage(machineId, command);
        }
    }
}
