using System;
using Kentico.Kontent.Delivery.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Kontent.Delivery
{
    /// <summary>
    /// A factory class for <see cref="IDeliveryClient"/>
    /// </summary>
    internal class DeliveryClientFactory : IDeliveryClientFactory
    {
        protected readonly IServiceProvider _serviceProvider;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryClientFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">An <see cref="IServiceProvider"/> instance.</param>
        public DeliveryClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public virtual IDeliveryClient Get(string name) => null;

        /// <inheritdoc />	
        public IDeliveryClient Get()
        {
            return _serviceProvider.GetRequiredService<IDeliveryClient>();
        }

        private T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }
    }
}
