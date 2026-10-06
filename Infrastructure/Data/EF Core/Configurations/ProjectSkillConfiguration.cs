using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class ProjectSkillConfiguration : IEntityTypeConfiguration<ProjectSkill>
{
    public void Configure(EntityTypeBuilder<ProjectSkill> builder)
    {
        builder.HasKey(ps => new { ps.ProjectId, ps.SkillId });

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(ps => ps.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Skill>()
            .WithMany()
            .HasForeignKey(ps => ps.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("ProjectSkills");
    }
}
