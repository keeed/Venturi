using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ViewModels
{
    public class ProductStockViewModel
    {
        [BsonId]
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}