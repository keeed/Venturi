using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Domain.Commands;
using Domain.DomainEvents;
using Domain.Repositories;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
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

            WarehouseConfiguration = Configuration.GetSection("WarehouseConfiguration");
            if (WarehouseConfiguration == null)
            {
                throw new Exception("Warehouse configuration section is not found. Check configuration file.");
            }

            JwtIssuer = WarehouseConfiguration["Jwt:Issuer"];
            JwtKey = WarehouseConfiguration["Jwt:Key"];
        }

        public IConfiguration Configuration { get; }

        public IConfigurationSection WarehouseConfiguration { get; }
        public string JwtIssuer { get; }
        public string JwtKey { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton(Configuration);
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = JwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                    ClockSkew = TimeSpan.Zero // remove delay of token when expire
                };
            });

            services.AddSingleton<InventoryMongoDatabase>(s =>
            {
                var mongoClient = new MongoClient(WarehouseConfiguration["InventoryDatabaseConnectionString"]);
                return new InventoryMongoDatabase(mongoClient.GetDatabase(WarehouseConfiguration["InventoryDatabaseName"]));
            });

            services.AddSingleton<QueryMongoDatabase>(s =>
            {
                var mongoClient = new MongoClient(WarehouseConfiguration["QueryDatabaseConnectionString"]);
                return new QueryMongoDatabase(mongoClient.GetDatabase(WarehouseConfiguration["QueryDatabaseName"]));
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
                
                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // ===== Use Authentication ======
            app.UseAuthentication();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Venturi Inventory V1");
            });

            app.UseMvc();
        }

        private void RegisterDomainRepositories(IServiceCollection services)
        {
            services.AddScoped<IRepository<Product, ProductId>>(s => 
            {
                return new PublisingRepository<Product, ProductId>(new ProductMongoRepository(s.GetRequiredService<InventoryMongoDatabase>()),
                                                                   s.GetRequiredService<IEventPublisher>());
            });
        }

        private void RegisterCommandHandlers(IServiceCollection services)
        {
            // Register all command handlers. This would have been easier with other IoC containers which supports registering
            // open-generic interfaces - i.e SimpleInjector. This could be changed in the future.
            services.AddTransient<ICommandAsyncHandler<AddProductToCategoryCommand>, AddProductToCategoryCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<DecreaseProductStockCommand>, DecreaseProductStockCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<IncreaseProductStockCommand>, IncreaseProductStockCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<MarkProductAsForSaleCommand>, MarkProductAsForSaleCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<RegisterNewProductCommand>, RegisterNewProductCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<RemoveProductFromCategoryCommand>, RemoveProductFromCatalogCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<RepriceProductCommand>, RepriceProductCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<MarkProductAsNotForSaleCommand>, MarkProductAsNotForSaleCommandHandler>();
            services.AddTransient<ICommandAsyncHandler<UnregisterProductCommand>, UnregisterProductCommandHandler>();

            // To enable command dispatcher to resolve command handlers from the ASP.NET core IoC container.
            services.AddSingleton<ICommandHandlerResolver, ContainerCommandAsyncHandlerResolver>();
            services.AddSingleton<Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter, AspNetCoreServiceProviderAdapter>();

            // Register command dispatcher.
            services.AddTransient<ICommandAsyncDispatcher, CommandDispatcher>();
        }

        private void RegisterEventHandlers(IServiceCollection services)
        {            
            // Register all query event handlers here.
            services.AddTransient<IEventAsyncHandler<ProductRegisteredEvent>, ProductViewModelProjector>();
            services.AddTransient<IEventAsyncHandler<ProductUnregisteredEvent>, ProductViewModelProjector>();
            services.AddTransient<IEventAsyncHandler<ProductMarkedForSaleEvent>, ProductViewModelProjector>();
            services.AddTransient<IEventAsyncHandler<ProductMarkedNotForSaleEvent>, ProductViewModelProjector>();
            
            services.AddTransient<IEventAsyncHandler<ProductAddedToCategoryEvent>, ProductCategoryViewModelProjector>();
            services.AddTransient<IEventAsyncHandler<ProductRemovedFromCategoryEvent>, ProductCategoryViewModelProjector>();
            
            // To enable event publisher to resolve event handlers from the ASP.NET core IoC container.
            services.AddSingleton<IEventHandlerResolver, ContainerEventHandlerResolver>();
            services.AddSingleton<Xer.Cqrs.EventStack.Resolvers.IContainerAdapter, AspNetCoreServiceProviderAdapter>();

            // Register event handler
            services.AddTransient<IEventPublisher, LoggingEventPublisher>();
        }

        private void RegisterQueryHandlers(IServiceCollection services)
        {
            // Register all query handlers.
            services.AddTransient<IQueryAsyncHandler<QueryProductListView, ProductListViewModel>, QueryProductListViewHandler>();
            services.AddTransient<IQueryAsyncHandler<QueryProductView, ProductViewModel>, QueryProductViewHandler>();
            services.AddTransient<IQueryAsyncHandler<QueryProductCategoryView, ProductCategoryViewModel>, QueryProductCategoryViewHandler>();
            services.AddTransient<IQueryAsyncHandler<QueryProductCategoryListView, ProductCategoryListViewModel>, QueryProductCategoryListViewHandler>();
                        
            // To enable event publisher to resolve event handlers from the ASP.NET core IoC container.
            services.AddSingleton<IQueryHandlerResolver, ContainerQueryAsyncHandlerResolver>();
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

        public class LoggingEventPublisher : EventPublisher
        {
            public LoggingEventPublisher(IEventHandlerResolver resolver, ILoggerFactory loggerFactory) 
                : base(resolver)
            {
                ILogger logger = loggerFactory.CreateLogger(nameof(LoggingEventPublisher));
                OnError += (e, ex) =>
                {
                    string errorMessage = $"------------------------------------------------------" + 
                                          Environment.NewLine + 

                                          $"{e.GetType().Name} has failed processing." + 

                                          Environment.NewLine +
                                          $"------------------------------------------------------";

                    logger.LogError(ex, errorMessage);
                };
            }
        }
    }
}
