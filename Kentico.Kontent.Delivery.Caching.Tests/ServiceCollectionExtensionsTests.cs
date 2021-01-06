using System;
using FluentAssertions;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Caching.Extensions;
using Kentico.Kontent.Delivery.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using Xunit;

namespace Kentico.Kontent.Delivery.Caching.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private readonly ServiceCollection _serviceCollection;

        public ServiceCollectionExtensionsTests()
        {
            _serviceCollection = new ServiceCollection();
        }

        [Theory]
        [InlineData(CacheTypeEnum.Memory)]
        [InlineData(CacheTypeEnum.Distributed)]
        public void AddDeliveryClientCacheWithDeliveryCacheOptions_ThrowsMissingTypeRegistrationException(CacheTypeEnum cacheType)
        {
            Assert.Throws<MissingTypeRegistrationException>(() => _serviceCollection.AddDeliveryClientCache(new DeliveryCacheOptions() { CacheType = cacheType }));
        }

        [Fact]
        public void AddDeliveryClientCacheWithNullDeliveryCacheOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddDeliveryClientCache(null));
        }

        [Fact]
        public void AddDeliveryClientCacheWitNoPreviousRegistrationDeliveryClient_ThrowsMissingTypeRegistrationException()
        {
            Assert.Throws<MissingTypeRegistrationException>(() => _serviceCollection.AddDeliveryClientCache(new DeliveryCacheOptions()));
        }
    }
}
