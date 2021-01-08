using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Kentico.Kontent.Delivery.Extensions.DependencyInjection
{
    internal class DeliveryClientCacheFactory : IDeliveryClientFactory
    {
        private readonly IOptionsMonitor<DeliveryCacheOptions> _deliveryCacheOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDeliveryClientFactory _innerDeliveryClientFactory;
        private readonly ConcurrentDictionary<string, IDeliveryClient> _cache = new ConcurrentDictionary<string, IDeliveryClient>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryClientCacheFactory"/> class.
        /// </summary>
        /// <param name="deliveryClientFactory">Factory to be decorated.</param>
        /// <param name="deliveryCacheOptions">Cache configuration options.</param>
        /// <param name="serviceProvider">An <see cref="IServiceProvider"/> instance.</param>
        public DeliveryClientCacheFactory(IDeliveryClientFactory deliveryClientFactory, IOptionsMonitor<DeliveryCacheOptions> deliveryCacheOptions, IServiceProvider serviceProvider)
        {
            _deliveryCacheOptions = deliveryCacheOptions;
            _serviceProvider = serviceProvider;
            _innerDeliveryClientFactory = deliveryClientFactory;
        }

        public IDeliveryClient Get(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!_cache.TryGetValue(name, out var client))
            {
                client = _innerDeliveryClientFactory.Get(name);
                var cacheOptions = _deliveryCacheOptions.Get(name);
                if (cacheOptions.Name == name)
                {
                    // Build caching services according to the options
                    IDeliveryCacheManager manager;
                    if (cacheOptions.CacheType == CacheTypeEnum.Memory)
                    {
                        var memoryCache = _serviceProvider.GetService<IMemoryCache>();
                        manager = new MemoryCacheManager(memoryCache, Options.Create(cacheOptions));
                    }
                    else
                    {
                        var distributedCache = _serviceProvider.GetService<IDistributedCache>();
                        manager = new DistributedCacheManager(distributedCache, Options.Create(cacheOptions));
                    }

                    // Decorate the client with a caching layer
                    client = new DeliveryClientCache(manager, client);

                    _cache.TryAdd(name, client);
                }
            }

            return client;
        }

        public IDeliveryClient Get() => _innerDeliveryClientFactory.Get();
    }
}
