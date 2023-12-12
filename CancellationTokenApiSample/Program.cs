using Microsoft.EntityFrameworkCore;
using CancellationTokenApiSample.Data;

namespace CancellationTokenApiSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<CancellationTokenApiSampleContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("CancellationTokenApiSampleContext") ?? throw new InvalidOperationException("Connection string 'CancellationTokenApiSampleContext' not found."),
                    options => options.EnableRetryOnFailure()));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
