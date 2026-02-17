using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsAggregator.Domain.Entities;

namespace SportsAggregator.Infrastructure.Data.Configurations;

public sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("games");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.ScheduledAtUtc)
            .HasColumnName("scheduled_at_utc")
            .IsRequired();

        builder.Property(x => x.SportType)
            .HasColumnName("sport_type")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.CompetitionName)
            .HasColumnName("competition_name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.HomeTeam)
            .HasColumnName("home_team")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.AwayTeam)
            .HasColumnName("away_team")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Fingerprint)
            .HasColumnName("fingerprint")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(x => x.Fingerprint)
            .IsUnique();

        builder.HasIndex(x => x.SportType);
        builder.HasIndex(x => x.CompetitionName);
        builder.HasIndex(x => x.ScheduledAtUtc);
        builder.HasIndex(x => new { x.SportType, x.ScheduledAtUtc });
    }
}
