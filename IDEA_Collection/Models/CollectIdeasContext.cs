using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace IDEA_Collection.Models
{
    public partial class CollectIdeasContext : DbContext
    {
        public CollectIdeasContext()
        {
        }

        public CollectIdeasContext(DbContextOptions<CollectIdeasContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Idea> Ideas { get; set; }
        public virtual DbSet<Like> Likes { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Unlike> Unlikes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CollectIdeas;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Avatar).HasMaxLength(255);

                entity.Property(e => e.Birthday).HasColumnType("datetime");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.FullName).HasMaxLength(150);

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(50);

                entity.Property(e => e.Phone)
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.DepartmentId)
                    .HasConstraintName("FK_Accounts_Departments");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_Accounts_Roles");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CatId);

                entity.Property(e => e.CatId).HasColumnName("CatID");

                entity.Property(e => e.CatName).HasMaxLength(250);

                entity.Property(e => e.Thumb).HasMaxLength(250);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.CommentId).HasColumnName("CommentID");

                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.Alias).HasMaxLength(255);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.PostId).HasColumnName("PostID");

                entity.Property(e => e.Thumb).HasMaxLength(255);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_Comments_Accounts");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK_Comments_Ideas");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.ClosureDates).HasColumnType("datetime");

                entity.Property(e => e.DepartmentName).HasMaxLength(50);

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.StartDates).HasColumnType("datetime");
            });

            modelBuilder.Entity<Idea>(entity =>
            {
                entity.HasKey(e => e.PostId);

                entity.Property(e => e.PostId).HasColumnName("PostID");

                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.Alias).HasMaxLength(255);

                entity.Property(e => e.Author).HasMaxLength(255);

                entity.Property(e => e.CatId).HasColumnName("CatID");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Scontents)
                    .HasMaxLength(255)
                    .HasColumnName("SContents");

                entity.Property(e => e.Thumb).HasMaxLength(255);

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Ideas)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_Ideas_Accounts");

                entity.HasOne(d => d.Cat)
                    .WithMany(p => p.Ideas)
                    .HasForeignKey(d => d.CatId)
                    .HasConstraintName("FK_Ideas_Categories");
            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.Property(e => e.LikeId).HasColumnName("LikeID");

                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.PostId).HasColumnName("PostID");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_Likes_Accounts");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.LikesNavigation)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK_Likes_Ideas");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.RoleName).HasMaxLength(50);
            });

            modelBuilder.Entity<Unlike>(entity =>
            {
                entity.Property(e => e.UnlikeId).HasColumnName("UnlikeID");

                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.PostId).HasColumnName("PostID");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Unlikes)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_Unlikes_Accounts");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.UnlikesNavigation)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK_Unlikes_Ideas");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
