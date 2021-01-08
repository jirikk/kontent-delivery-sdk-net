﻿using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace Kentico.Kontent.Delivery.Extensions.DependencyInjection
{
    internal class DeliveryClientFactory : IDeliveryClientFactory
    {
        private readonly IDeliveryClientFactory _innerDeliveryClientFactory;
        private readonly IOptionsMonitor<DeliveryOptions> _deliveryOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, IDeliveryClient> _cache = new ConcurrentDictionary<string, IDeliveryClient>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryClientFactory"/> class.
        /// </summary>
        /// <param name="deliveryClientFactory">Factory to be decorated.</param>
        /// <param name="deliveryOptions">Used for notifications when <see cref="DeliveryOptions"/> instances change.</param>
        /// <param name="serviceProvider">An <see cref="IServiceProvider"/> instance.</param>
        public DeliveryClientFactory(IDeliveryClientFactory deliveryClientFactory,  IOptionsMonitor<DeliveryOptions> deliveryOptions, IServiceProvider serviceProvider)
        {
            _innerDeliveryClientFactory = deliveryClientFactory;
            _deliveryOptions = deliveryOptions;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IDeliveryClient Get(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!_cache.TryGetValue(name, out var client))
            {
                var deliveryClientOptions = _deliveryOptions.Get(name);

                // Validate that the option object is indeed configured
                if (deliveryClientOptions.ProjectId != null)
                {
                    client = Build(deliveryClientOptions, name);

                    _cache.TryAdd(name, client);
                }
            }

            return client;
        }

        public IDeliveryClient Get() => _innerDeliveryClientFactory.Get();

        private IDeliveryClient Build(DeliveryOptions options, string name)
        {
            return new DeliveryClient(
                new DeliveryOptionsMonitor(options, name),
                GetService<IModelProvider>(),
                GetService<IRetryPolicyProvider>(),
                GetService<ITypeProvider>(),
                GetService<IDeliveryHttpClient>(),
                GetService<JsonSerializer>());
        }


        private T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }
    }
}
