using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Commands;
using Domain.DomainEvents;
using Domain.Repositories;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Swashbuckle.AspNetCore.Swagger;
using ViewModels;
using ViewModels.Queries;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Dispatchers;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Publishers;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Resolvers;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            InventoryConfiguration = Configuration.GetSection("Inventory");
            if (InventoryConfiguration == null)
            {
                throw new Exception("Inventory configuration section is not found. Check configuration file.");
            }
        }

        public IConfiguration Configuration { get; }
        public IConfigurationSection InventoryConfiguration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton(Configuration);

            // Register Mongo Client to be used by write and query side.
            services.AddSingleton<IMongoClient>(s => 
            {
                return new MongoClient(Configuration.GetConnectionString("MongoDbConnectionString"));
            });

            // Domain/Write side.
            RegisterDomainRepositories(services);
            RegisterCommandHandlers(services);
            RegisterEventHandlers(services);

            // Read side.
            RegisterQueryHandlers(services);

            // Swagger.
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Venturi Inventory", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IRepository<Inventory, InventoryId> inventoryRepository, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Venturi Inventory V1");
            });

            app.UseMvc();

            EnsureInventoryIsCreated(inventoryRepository, loggerFactory.CreateLogger(typeof(Startup)));
        }

        private async void EnsureInventoryIsCreated(IRepository<Inventory, InventoryId> inventoryRepository, ILogger logger)
        {
            try
            {
                IConfigurationSection inventoryConfig = Configuration.GetSection("Inventory");

                string configuredInventoryId = inventoryConfig["InventoryId"];
                InventoryId inventoryId = new InventoryId(Guid.Parse(configuredInventoryId));

                Inventory inventory = await inventoryRepository.GetByIdAsync(inventoryId);
                if (inventory == null)
                {
                    string configuredWarehouseId = inventoryConfig["WarehouseId"];
                    WarehouseId warehouseId = new WarehouseId(Guid.Parse(configuredWarehouseId));

                    await inventoryRepository.SaveAsync(new Inventory(inventoryId, warehouseId));
                }
            }
            catch(Exception ex)
            {
                logger.LogCritical(ex, "Failed to initialize inventory.");
            }
        }

        private void RegisterDomainRepositories(IServiceCollection services)
        {
            // Register mongo repository.
            services.AddSingleton<IRepository<Inventory, InventoryId>>(s => 
            {
                return new PublisingRepository<Inventory, InventoryId>(new InventoryMongoRepository(s.GetRequiredService<IMongoClient>()),
                                                                       s.GetRequiredService<IEventPublisher>());
            });
        }

        private void RegisterCommandHandlers(IServiceCollection services)
        {
            // Register all command handlers. This would have been easier with other IoC containers which supports registering
            // open-generic interfaces - i.e SimpleInjector. This could be changed in the future.
            services.AddTransient<ICommandAsyncHandler<AddProductToCatalogCommand>, AddProductToCatalogCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<CreateCatalogCommand>, CreateCatalogCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<DecreaseProductStockCommand>, DecreaseProductStockCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<DeleteCatalogCommand>, DeleteCatalogCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<IncreaseProductStockCommand>, IncreaseProductStockCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<MarkProductAsForSaleCommand>, MarkProductAsForSaleCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<RegisterNewProductCommand>, RegisterNewProductCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<RemoveProductFromCatalogCommand>, RemoveProductFromCatalogCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<RepriceProductCommand>, RepriceProductCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<MarkProductAsNotForSaleCommand>, MarkProductAsNotForSaleCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<UnregisterProductCommand>, UnregisterProductCommandHandler>();

            // To enable command dispatcher to resolve command handlers from the ASP.NET core IoC container.
            services.AddSingleton<ICommandHandlerResolver, ContainerCommandHandlerResolver>();
            services.AddSingleton<Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter, AspNetCoreServiceProviderAdapter>();

            // Register command dispatcher.
            services.AddTransient<ICommandAsyncDispatcher, CommandDispatcher>();
        }

        private void RegisterEventHandlers(IServiceCollection services)
        {
            // Register all event handlers here.
            services.AddTransient<IEventAsyncHandler<ProductRegisteredEvent>, ProductViewProjector>();
            services.AddTransient<IEventAsyncHandler<ProductUnregisteredEvent>, ProductViewProjector>();
            services.AddTransient<IEventAsyncHandler<ProductMarkedForSaleEvent>, ProductViewProjector>();
            services.AddTransient<IEventAsyncHandler<ProductMarkedNotForSaleEvent>, ProductViewProjector>();
            
            services.AddTransient<IEventAsyncHandler<CatalogCreatedEvent>, ProductCatalogViewProjector>();
            services.AddTransient<IEventAsyncHandler<CatalogDeletedEvent>, ProductCatalogViewProjector>();
            services.AddTransient<IEventAsyncHandler<ProductAddedToCatalogEvent>, ProductCatalogViewProjector>();
            services.AddTransient<IEventAsyncHandler<ProductRemovedFromCatalogEvent>, ProductCatalogViewProjector>();
            
            // To enable event publisher to resolve event handlers from the ASP.NET core IoC container.
            services.AddSingleton<IEventHandlerResolver, ContainerEventHandlerResolver>();
            services.AddSingleton<Xer.Cqrs.EventStack.Resolvers.IContainerAdapter, AspNetCoreServiceProviderAdapter>();

            // Register event handler
            services.AddTransient<IEventPublisher, EventPublisher>();
        }

        private void RegisterQueryHandlers(IServiceCollection services)
        {
            // Register all query handlers.
            services.AddTransient<IQueryAsyncHandler<GetAllProductsQuery, ProductListView>, GetAllProductsQueryHandler>();
            services.AddTransient<IQueryAsyncHandler<GetProductByIdQuery, ProductView>, GetProductByIdQueryHandler>();
            services.AddTransient<IQueryAsyncHandler<GetProductsInCatalogQuery, ProductCatalogView>, GetProductsInCatalogQueryHandler>();
            services.AddTransient<IQueryAsyncHandler<GetAllCatalogsQuery, ProductCatalogListView>, GetAllCatalogsQueryHandler>();
                        
            // To enable event publisher to resolve event handlers from the ASP.NET core IoC container.
            services.AddSingleton<IQueryHandlerResolver, ContainerQueryHandlerResolver>();
            services.AddSingleton<Xer.Cqrs.QueryStack.Resolvers.IContainerAdapter, AspNetCoreServiceProviderAdapter>();

            // Register query dispatcher.
            services.AddTransient<IQueryAsyncDispatcher, QueryDispatcher>();
        }

        class AspNetCoreServiceProviderAdapter : Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter,
                                                 Xer.Cqrs.QueryStack.Resolvers.IContainerAdapter,
                                                 Xer.Cqrs.EventStack.Resolvers.IContainerAdapter
        {
            private readonly IServiceProvider _container;

            public AspNetCoreServiceProviderAdapter(IServiceProvider serviceProvider)
            {
                _container = serviceProvider;
            }

            public T Resolve<T>() where T : class
            {
                return _container.GetService<T>();
            }

            public IEnumerable<T> ResolveMultiple<T>() where T : class
            {
                return _container.GetServices<T>();
            }
        }
    }
}
