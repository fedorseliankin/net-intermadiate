using net_intermediate.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace net_intermediate
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMemoryCache();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            });

            services.AddDbContext<TicketingContext>(options =>
                options.UseMySQL("server=localhost;database=netintermediate;user=root;password=Fyodor123!"));

            services.AddScoped<IVenueRepository, VenueRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ITicketingContext, TicketingContext>();
            services.AddScoped<IDatabaseTransactionManager, DatabaseTransactionManager>();


            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
