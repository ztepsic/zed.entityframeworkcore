using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zed.EntityFrameworkCore.Tests.Model {
    public class TagMapping : IEntityTypeConfiguration<Tag> {
        public void Configure(EntityTypeBuilder<Tag> builder) {
            builder.ToTable("Tags")
                  .HasKey(t => t.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Name)
                .HasField("name")
                .IsRequired();

            builder.Property(x => x.Slug)
                .HasField("slug")
                .IsRequired();

            builder.HasOne(x => x.BaseTag)
                .WithMany()
                .HasForeignKey("BaseTagId")
                .OnDelete(DeleteBehavior.Restrict);

            // Entity Framework Core requires that a navigation property be defined using HasOne, HasMany, or OwnsOne/OwnsMany
            // methods before you can configure it with the Navigation() method.
            builder.Navigation(x => x.BaseTag)
                .HasField("baseTag");
        }
    }
}
