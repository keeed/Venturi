using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Api.HostedServices;
using Domain;
using Domain.Commands;
using Domain.DomainEvents;
using Domain.Repositories;
using Domain.Services;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

            JwtIssuer = Configuration["Jwt:Issuer"];
            JwtKey = Configuration["Jwt:Key"];
            ExpireDays = int.Parse(Configuration["Jwt:ExpireDays"]);
        }

        public IConfiguration Configuration { get; }
        public string JwtIssuer { get; }
        public string JwtKey { get; }
        public int ExpireDays { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ===== Add our DbContext ========
            // services.AddDbContext<ApplicationDbContext>();

            // ===== Add Identity ========
            // services.AddIdentity<IdentityUser, IdentityRole>()
            //         .AddEntityFrameworkStores<ApplicationDbContext>()
            //         .AddDefaultTokenProviders();

            // ===== Add Jwt Authentication ========
            // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims

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
                    ValidIssuer = JwtIssuer,
                    ValidAudience = JwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                    ClockSkew = TimeSpan.Zero // remove delay of token when expire
                };
                System.Console.WriteLine(cfg.Audience);
            });

            // ===== Add MVC ========
            services.AddMvc();

            RegisterDomainServices(services);

            // Swagger.
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Venturi Auth", Version = "v1" });

                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                        Name = "Authorization",
                        In = "header",
                        Type = "apiKey"
                });
            });
        }

        private void RegisterDomainServices(IServiceCollection services)
        {
            services.AddTransient<ITokenGenerator>(s => new JwtTokenGenerator(JwtIssuer, JwtKey, TimeSpan.FromDays(ExpireDays)));
            services.AddTransient<IPasswordHasher, BcryptPasswordHasher>();
            services.AddTransient<AuthenticationService>();

            services.AddSingleton<IUserRepository>(s => 
            {
                return new PublishingUserRepository(new UserInMemoryRepository(),
                                                    s.GetRequiredService<IEventPublisher>());
            });

            // Hosted services.
            services.AddSingleton<IHostedService, UserLockoutReleaseHostedService>(s =>
            {
                return new UserLockoutReleaseHostedService(
                        new UserLockoutTimeElapsedEventHandler(s.GetRequiredService<IUserRepository>(), 1000));
            });

            RegisterCommandHandlers(services);
            RegisterEventHandlers(services);
        }

        private void RegisterCommandHandlers(IServiceCollection services)
        {
            // Register all command handlers. This would have been easier with other IoC containers which supports registering
            // open-generic interfaces - i.e SimpleInjector. This could be changed in the future.
            services.AddTransient<ICommandAsyncHandler<RegisterUserCommand>, RegisterUserCommandHandler>();

            // To enable command dispatcher to resolve command handlers from the ASP.NET core IoC container.
            services.AddSingleton<ICommandHandlerResolver, ContainerCommandAsyncHandlerResolver>();
            services.AddSingleton<Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter, AspNetCoreServiceProviderAdapter>();

            // Register command dispatcher.
            services.AddTransient<ICommandAsyncDispatcher, CommandDispatcher>();
        }

        private void RegisterEventHandlers(IServiceCollection services)
        {
            services.AddTransient<IEventAsyncHandler<UserFailedSignInEvent>, UserFailedSignInEventHandler>();

            // To enable command dispatcher to resolve command handlers from the ASP.NET core IoC container.
            services.AddSingleton<IEventHandlerResolver, ContainerEventHandlerResolver>();
            services.AddSingleton<Xer.Cqrs.EventStack.Resolvers.IContainerAdapter, AspNetCoreServiceProviderAdapter>();

            // Register command dispatcher.
            services.AddTransient<IEventPublisher, LoggingEventPublisher>();
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Venturi Auth V1");
            });

            app.UseMvc();
        }

        class AspNetCoreServiceProviderAdapter : Xer.Cqrs.CommandStack.Resolvers.IContainerAdapter,
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
            public LoggingEventPublisher(IEventHandlerResolver resolver, ILoggerFactory loggerFactory) : base(resolver)
            {
                ILogger logger = loggerFactory.CreateLogger(nameof(LoggingEventPublisher));
                OnError += (e, ex) =>
                {
                    string errorMessage = $"------------------------------------------------------" +
                        Environment.NewLine +

                        $"{e.GetType().Name} have failed processing." +

                        Environment.NewLine +
                        $"------------------------------------------------------";

                    logger.LogError(ex, errorMessage);
                };
            }
        }
    }
}