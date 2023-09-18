using FileService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Configurations.EntityConfigurations;

public sealed class CloudFileConfig : IEntityTypeConfiguration<CloudFile>
{
    public void Configure(EntityTypeBuilder<CloudFile> builder)
    {
        builder.ToTable("File");
    }
}