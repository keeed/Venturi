using System;
using System.Threading.Tasks;
using Domain.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ViewModels;
using ViewModels.Queries;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.QueryStack;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/category")]
    public class CategoryController : Controller
    {
        private readonly IQueryAsyncDispatcher _queryDispatcher;

        public CategoryController(IQueryAsyncDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            ProductCategoryListViewModel result = await _queryDispatcher.DispatchAsync<GetAllProductCategoriesQuery, ProductCategoryListViewModel>(
                new GetAllProductCategoriesQuery());

            return Ok(result);
        }

        [HttpGet("{categoryName}")]
        public async Task<IActionResult> GetProductsInCategory(string categoryName)
        {
            ProductCategoryViewModel result = await _queryDispatcher.DispatchAsync<GetProductsInCategoryQuery, ProductCategoryViewModel>(
                new GetProductsInCategoryQuery(categoryName));

            return Ok(result);
        }
    }
}