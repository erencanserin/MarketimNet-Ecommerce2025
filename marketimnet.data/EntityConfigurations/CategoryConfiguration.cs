using marketimnet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace marketimnet.Data.EntityConfigurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasColumnType("nvarchar(50)").HasMaxLength(50);
            builder.Property(x => x.Description).HasColumnType("nvarchar(500)").HasMaxLength(500);
            builder.Property(x => x.Image).HasColumnType("nvarchar(100)").HasMaxLength(100);
        }
    }
} 