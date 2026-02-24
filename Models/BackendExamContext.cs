using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BackendExam.Models;

public partial class BackendExamContext : DbContext
{
    public BackendExamContext()
    {
    }

    public BackendExamContext(DbContextOptions<BackendExamContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketComment> TicketComments { get; set; }

    public virtual DbSet<TicketStatusLog> TicketStatusLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("server=DESKTOP-TO5U8DE\\SQLEXPRESS01; database=BackendExam; trusted_connection=true; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.RoleName, "IX_Roles").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.Property(e => e.TicketId)
                .HasColumnName("TicketID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Priority)
                .HasMaxLength(255)
                .HasDefaultValue("Medium")
                .HasColumnName("priority");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasDefaultValue("Open");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.TicketAssignedToNavigations)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK_Tickets_Users1");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TicketCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_Users");
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(e => e.TicketComId).HasName("PK_TicketComment");

            entity.Property(e => e.TicketComId)
                .HasColumnName("TicketComID");
            entity.Property(e => e.Comment).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketComments)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_TicketComments_Tickets");

            entity.HasOne(d => d.User).WithMany(p => p.TicketComments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_TicketComments_Users");
        });

        modelBuilder.Entity<TicketStatusLog>(entity =>
        {
            entity.Property(e => e.TicketStatusLogId)
                .HasColumnName("TicketStatusLogID");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NewStatus).HasMaxLength(255);
            entity.Property(e => e.OldStatus).HasMaxLength(255);
            entity.Property(e => e.TicketId).HasColumnName("TicketID");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.TicketStatusLogs)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("FK_TicketStatusLogs_Users");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketStatusLogs)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_TicketStatusLogs_Tickets");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserName).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
