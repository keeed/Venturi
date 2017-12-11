using System;
using System.Collections.Generic;

namespace ViewModels
{
    public class ProductListViewModel
    {
        public ICollection<ProductViewModel> Products { get; set; }
    }
}