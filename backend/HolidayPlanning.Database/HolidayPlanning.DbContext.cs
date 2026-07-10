using System.Text.Json;
using HolidayPlanning.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HolidayPlanning.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TripEntity> Trips => Set<TripEntity>();
    public DbSet<TripMemberEntity> TripMembers => Set<TripMemberEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TripEntity>(trip =>
        {
            trip.Property(t => t.Name).HasMaxLength(200);
            trip.HasMany(t => t.Members)
                .WithOne(m => m.Trip)
                .HasForeignKey(m => m.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripMemberEntity>(member =>
        {
            member.Property(m => m.DisplayName).HasMaxLength(100);
            member.HasIndex(m => m.TripId);

            member.Property(m => m.VibeWeights)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, double>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, double>(),
                    JsonValueComparer<Dictionary<string, double>>());

            member.Property(m => m.Dealbreakers)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<List<DealbreakerRecord>>(v, JsonSerializerOptions.Default) ?? new List<DealbreakerRecord>(),
                    JsonValueComparer<List<DealbreakerRecord>>());
        });
    }

    private static ValueComparer<T> JsonValueComparer<T>() where T : class => new(
        (left, right) => JsonSerializer.Serialize(left, JsonSerializerOptions.Default) == JsonSerializer.Serialize(right, JsonSerializerOptions.Default),
        value => JsonSerializer.Serialize(value, JsonSerializerOptions.Default).GetHashCode(),
        value => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonSerializerOptions.Default), JsonSerializerOptions.Default)!);
}
