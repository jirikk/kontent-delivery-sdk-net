using Microsoft.Extensions.DependencyInjection;
using Kentico.Kontent.Delivery.Extensions.DependencyInjection.Extensions;
using Kentico.Kontent.Delivery.ContentItems;
using Kentico.Kontent.Delivery.ContentItems.ContentLinks;
using Kentico.Kontent.Delivery.ContentItems.InlineContentItems;
using Kentico.Kontent.Delivery.Extensions;
using Kentico.Kontent.Delivery.RetryPolicy;
using System;
using Xunit;
using Kentico.Kontent.Delivery.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Kentico.Kontent.Delivery.Caching;
using Kentico.Kontent.Delivery.Caching.Extensions;
using FluentAssertions;
using Scrutor;

namespace Kentico.Kontent.Delivery.Extensions.DependencyInjection.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private readonly ServiceCollection _serviceCollection;
        private const string ProjectId = "d79786fb-042c-47ec-8e5c-beaf93e38b84";

        private readonly ReadOnlyDictionary<Type, Type> _expectedInterfacesWithImplementationTypes = new ReadOnlyDictionary<Type, Type>(
            new Dictionary<Type, Type>
            {
                { typeof(IContentLinkUrlResolver), typeof(DefaultContentLinkUrlResolver) },
                { typeof(ITypeProvider), typeof(TypeProvider) },
                { typeof(IDeliveryHttpClient), typeof(DeliveryHttpClient) },
                { typeof(IInlineContentItemsProcessor), typeof(InlineContentItemsProcessor) },
                { typeof(IInlineContentItemsResolver<object>), typeof(ReplaceWithWarningAboutRegistrationResolver) },
                { typeof(IInlineContentItemsResolver<UnretrievedContentItem>), typeof(ReplaceWithWarningAboutUnretrievedItemResolver) },
                { typeof(IInlineContentItemsResolver<UnknownContentItem>), typeof(ReplaceWithWarningAboutUnknownItemResolver) },
                { typeof(IModelProvider), typeof(ModelProvider) },
                { typeof(IPropertyMapper), typeof(PropertyMapper) },
                { typeof(IRetryPolicyProvider), typeof(DefaultRetryPolicyProvider) },
                { typeof(IDeliveryClient), typeof(DeliveryClient) },
                { typeof(IDeliveryClientFactory), typeof(DeliveryClientFactory) },
            }
        );

        public static IEnumerable<object[]> DeliveryOptionsConfigurationParameters =>
           new[]
           {
                new[] {"as_root"},
                new[] {"under_default_key", "DeliveryOptions"},
                new[] {"under_custom_key", "CustomNameForDeliveryOptions"},
                new[] {"nested_under_default_key", "Options:DeliveryOptions"}
           };


        public ServiceCollectionExtensionsTests()
        {
            _serviceCollection = new ServiceCollection();
        }
        [Fact]
        public void AddDeliveryFactoryClientWithNullDeliveryOptionsBuilder_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddDeliveryClient("named", buildDeliveryOptions: null));
        }

        [Fact]
        public void AddDeliveryFactoryClientWithNullDeliveryOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddDeliveryClient("named", deliveryOptions: null));
        }

        [Fact]
        public void AddDeliveryClientFactoryWithOptions_AllServicesAreRegistered()
        {
            _serviceCollection.AddDeliveryClient("named", new DeliveryOptions { ProjectId = ProjectId });
            var provider = _serviceCollection.BuildServiceProvider();
            AssertDefaultServiceCollection(provider, _expectedInterfacesWithImplementationTypes);
        }

        [Fact]
        public void AddDeliveryClientFactoryWithDeliveryClient_AllServicesAreRegistered()
        {
            _serviceCollection.AddDeliveryClient("named", (builder) =>
                builder.WithProjectId(ProjectId)
                       .UseProductionApi()
                       .Build());

            var provider = _serviceCollection.BuildServiceProvider();
            AssertDefaultServiceCollection(provider, _expectedInterfacesWithImplementationTypes);
        }

        [Fact]
        public void AddDeliveryClientFactoryWithOptions_DeliveryClientIsRegistered()
        {
            _serviceCollection.AddDeliveryClient("named", new DeliveryOptions { ProjectId = ProjectId });
            var provider = _serviceCollection.BuildServiceProvider();
            var deliveryClientFactory = provider.GetRequiredService<IDeliveryClientFactory>();

            var deliveryClient = deliveryClientFactory.Get("named");

            Assert.NotNull(deliveryClient);
        }


        [Theory]
        [InlineData(CacheTypeEnum.Memory)]
        [InlineData(CacheTypeEnum.Distributed)]
        public void AddDeliveryNamedClient_CacheWithDeliveryCacheOptions_GetNamedClient(CacheTypeEnum cacheType)
        {
            _serviceCollection.AddDeliveryClient("named", new DeliveryOptions() { ProjectId = Guid.NewGuid().ToString() });
            _serviceCollection.AddDeliveryClientCache("named", new DeliveryCacheOptions()
            {
                CacheType = cacheType
            });

            var sp = _serviceCollection.BuildServiceProvider();
            var factory = sp.GetRequiredService<IDeliveryClientFactory>();

            var client = factory.Get("named");

            client.Should().NotBeNull();
        }

        [Theory]
        [InlineData(CacheTypeEnum.Memory)]
        [InlineData(CacheTypeEnum.Distributed)]
        public void AddDeliveryNamedClient_CacheWithDeliveryCacheOptions_GetNull(CacheTypeEnum cacheType)
        {
            _serviceCollection.AddDeliveryClient("named", new DeliveryOptions() { ProjectId = Guid.NewGuid().ToString() });
            _serviceCollection.AddDeliveryClientCache("named", new DeliveryCacheOptions()
            {
                CacheType = cacheType
            });

            var sp = _serviceCollection.BuildServiceProvider();
            var factory = sp.GetRequiredService<IDeliveryClientFactory>();

            var client = factory.Get("WrongName");

            client.Should().BeNull();
        }

        [Theory]
        [InlineData(CacheTypeEnum.Memory)]
        [InlineData(CacheTypeEnum.Distributed)]
        public void GetNamedDeliveryCacheManager_WithCorrectName_GetDeliveryCacheManager(CacheTypeEnum cacheType)
        {
            _serviceCollection.AddDeliveryClient("named", new DeliveryOptions() { ProjectId = Guid.NewGuid().ToString() });
            _serviceCollection.AddDeliveryClientCache("named", new DeliveryCacheOptions()
            {
                CacheType = cacheType
            });

            var sp = _serviceCollection.BuildServiceProvider();
            var factory = sp.GetRequiredService<IDeliveryClientFactory>();

            var result = factory.Get("named");

            result.Should().NotBeNull();
        }

        [Fact]
        public void AddDeliveryClientNamedCacheWitNoPreviousRegistrationDeliveryClient_ThrowsMissingTypeRegistrationException()
        {
            Assert.Throws<MissingTypeRegistrationException>(() => _serviceCollection.AddDeliveryClientCache("named", new DeliveryCacheOptions()));
        }

        [Theory]
        [InlineData(CacheTypeEnum.Memory)]
        [InlineData(CacheTypeEnum.Distributed)]
        public void AddDeliveryClientCacheNamedWithDeliveryCacheOptions_ThrowsInvalidOperationException(CacheTypeEnum cacheType)
        {
            Assert.Throws<MissingTypeRegistrationException>(() => _serviceCollection.AddDeliveryClientCache("named", new DeliveryCacheOptions() { CacheType = cacheType }));
        }

        [Fact]
        public void AddDeliveryClientCacheNamedWithNullDeliveryCacheOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddDeliveryClientCache("named", null));
        }


        [Theory]
        [InlineData(CacheTypeEnum.Memory)]
        [InlineData(CacheTypeEnum.Distributed)]
        public void AddDeliveryClient_CacheWithDeliveryCacheOptions_GetNull(CacheTypeEnum cacheType)
        {
            _serviceCollection.AddDeliveryClient("named", new DeliveryOptions() { ProjectId = Guid.NewGuid().ToString() });
            _serviceCollection.AddDeliveryClientCache("named", new DeliveryCacheOptions()
            {
                CacheType = cacheType
            });

            var sp = _serviceCollection.BuildServiceProvider();
            var factory = sp.GetRequiredService<IDeliveryClientFactory>();

            var client = factory.Get("WrongName");

            client.Should().BeNull();
        }


        private void AssertDefaultServiceCollection(ServiceProvider provider, IDictionary<Type, Type> expectedTypes)
        {
            foreach (var type in expectedTypes)
            {
                var imp = provider.GetRequiredService(type.Key);
                Assert.IsType(type.Value, imp);
            }
        }
    }
}
