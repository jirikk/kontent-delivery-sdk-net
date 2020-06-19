// This code was generated by a kontent-generators-net tool 
// (see https://github.com/Kentico/kontent-generators-net).
// 
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated. 
// For further modifications of the class, create a separate file with the partial class.

using System.Collections.Generic;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.ContentItems;
using Kentico.Kontent.Delivery.TaxonomyGroups;

namespace Kentico.Kontent.Delivery.Rx.Tests.Models.ContentTypes
{
    public class Accessory
    {
        public const string Codename = "accessory";
        public const string ProductNameCodename = "product_name";
        public const string PriceCodename = "price";
        public const string ImageCodename = "image";
        public const string ManufacturerCodename = "manufacturer";
        public const string ProductStatusCodename = "product_status";
        public const string ShortDescriptionCodename = "short_description";
        public const string LongDescriptionCodename = "long_description";
        public const string UrlPatternCodename = "url_pattern";

        public string ProductName { get; set; }
        public decimal? Price { get; set; }
        public IEnumerable<IAsset> Image { get; set; }
        public string Manufacturer { get; set; }
        public IEnumerable<ITaxonomyTerm> ProductStatus { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string UrlPattern { get; set; }
        public ContentItemSystemAttributes System { get; set; }
    }
}