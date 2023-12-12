using Microsoft.EntityFrameworkCore;

namespace CancellationTokenApiSample.Data
{
    public class CancellationTokenApiSampleContext : DbContext
    {
        public CancellationTokenApiSampleContext (DbContextOptions<CancellationTokenApiSampleContext> options)
            : base(options)
        {
        }

        public DbSet<ModelSample> ModelSample { get; set; } = default!;
    }
}
