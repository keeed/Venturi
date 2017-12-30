using System;
using System.Collections.Generic;
using Xer.Cqrs.EventStack;

namespace Domain
{
    public class ProductCategory // Value object
    {
        public string Name { get; private set; }

        public ProductCategory(string name)
        {
            Name = name;
        }
    }
}