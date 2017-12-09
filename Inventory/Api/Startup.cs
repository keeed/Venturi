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
using Swashbuckle.AspNetCore.Swagger;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Dispatchers;
using Xer.Cqrs.CommandStack.Resolvers;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Publishers;
using Xer.Cqrs.EventStack.Resolvers;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            RegisterRepositories(services);
            RegisterCommandHandlers(services);
            RegisterEventHandlers(services);

            // Swagger.
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "PMS", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            // Singleton so that in memory inventory will be persisted as long as app is running.
            services.AddSingleton<IRepository<Inventory, InventoryId>>(s => 
            {
                return new PublisingRepository<Inventory, InventoryId>(new InMemoryInventoryRepository(),
                                                                       s.GetRequiredService<IEventPublisher>());
            });
        }

        private static void RegisterCommandHandlers(IServiceCollection services)
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
            services.AddTransient<ICommandAsyncHandler<UnmarkProductAsForSaleCommand>, UnmarkProductAsForSaleCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<UnregisterProductCommand>, UnregisterProductCommandHandler>();

            // To enable command dispatcher to resolve command handlers from the ASP.NET core IoC container.
            services.AddTransient<ICommandHandlerResolver, ContainerCommandHandlerResolver>();
            services.AddTransient<Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter, AspNetCoreServiceProviderAdapter>();

            // Register command dispatcher.
            services.AddTransient<ICommandAsyncDispatcher, CommandDispatcher>();
        }

        private static void RegisterEventHandlers(IServiceCollection services)
        {
            // Register all event handlers here. No event handlers as of the moment.
            
            // To enable event publisher to resolve event handlers from the ASP.NET core IoC container.
            services.AddTransient<IEventHandlerResolver, ContainerEventHandlerResolver>();
            services.AddTransient<Xer.Cqrs.EventStack.Resolvers.IContainerAdapter, AspNetCoreServiceProviderAdapter>();

            // Register event handler
            services.AddTransient<IEventPublisher, EventPublisher>();
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
