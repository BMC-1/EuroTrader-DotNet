using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.HasData(
            new AppRole
            {
                Name = "admin",
                
                //Always capital letter
                NormalizedName = "ADMIN"
            },
            new AppRole
            {
                Name = "user",
                NormalizedName = "USER"
            });
    }
}