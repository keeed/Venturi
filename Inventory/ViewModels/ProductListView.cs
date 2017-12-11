using System;
using System.Collections.Generic;

namespace ViewModels
{
    public class ProductListView
    {
        public ICollection<ProductView> Products { get; set; }
    }
}