using System.Collections.Generic;

namespace ViewModels
{
    public class ProductCatalogListViewModel
    {
        public ICollection<ProductCatalogViewModel> Catalogs { get; set; }
    }
}