using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Server.Data;

namespace Server.DB {
    public class AppDbContext : DbContext {
        public DbSet<AccountDB> Accounts { get; set; }
        public DbSet<TokenDB> Tokens { get; set; }
        public DbSet<CharacterDB> Characters { get; set; }
        public DbSet<FriendDB> Friends { get; set; }
        public DbSet<ItemDB> Items { get; set; }
        public DbSet<FarmDB> Farms { get; set; }
        public DbSet<CropDB> Crops { get; set; }
        public DbSet<TitleDB> Titles { get; set; }
        static int i = 0;

        //string _ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SWMDB;Integrated Security=true";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            //TODO: TEMP
            //optionsBuilder.UseMySQL(ConfigManager.Config.connectionString);
            if (i == 0) {
                if (ConfigManager.Config.Deploy) {
                    OracleConfiguration.TnsAdmin = ConfigManager.Config.oracleWalletLocation;
                    OracleConfiguration.WalletLocation = ConfigManager.Config.oracleWalletLocation;
                }
                else {
                    OracleConfiguration.TnsAdmin = ConfigManager.Config.walletLocation;
                    OracleConfiguration.WalletLocation = ConfigManager.Config.walletLocation;
                }
                i = 1;
            }
            optionsBuilder.UseOracle(ConfigManager.Config.connectOracleString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            var entityTypes = modelBuilder.Model.GetEntityTypes().Where(e => typeof(IHasTimeStamps).IsAssignableFrom(e.ClrType));

            //유니크 속성 설정
            modelBuilder.Entity<AccountDB>().HasIndex(a => a.Accountname).IsUnique();
            modelBuilder.Entity<CharacterDB>().HasIndex(u => u.Nickname).IsUnique();

            modelBuilder.Entity<TokenDB>().HasIndex(a => a.Tokenname);
            modelBuilder.Entity<TokenDB>().Property(a => a.Tokenname).IsRequired();
            modelBuilder.Entity<TokenDB>().Property(a => a.AccountDBId).IsRequired();

            //스킬레벨 기본값 1 설정
            modelBuilder.Entity<CharacterDB>(entity => {
                entity.Property(e => e.SpringSkillLevel).HasDefaultValue(1);
                entity.Property(e => e.SummerSkillLevel).HasDefaultValue(1);
                entity.Property(e => e.AutumnSkillLevel).HasDefaultValue(1);
                entity.Property(e => e.WinterSkillLevel).HasDefaultValue(1);
            });

            //디폴트값 설정
            foreach (var entityType in entityTypes) {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IHasTimeStamps.CreatedAt))
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IHasTimeStamps.UpdatedAt))
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                modelBuilder.Entity(entityType.ClrType)
                    .Property("DeletedAt")
                    .HasColumnType("TIMESTAMP")
                    .IsRequired(false);
            }

            #region 관계정의
            //Account, Character 1대1 관계
            modelBuilder.Entity<AccountDB>()
                .HasOne(a => a.Character)
                .WithOne(c => c.Account)
                .HasForeignKey<CharacterDB>(c => c.AccountDBId)
                .OnDelete(DeleteBehavior.Cascade);
            //Character와 Item 간의 일대다 관계 설정
            modelBuilder.Entity<CharacterDB>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Owner)
                .HasForeignKey(i => i.OwnerDBId)
                .OnDelete(DeleteBehavior.Cascade);
            //Character와 Title 간의 일대다 관계 설정
            modelBuilder.Entity<CharacterDB>()
                .HasMany(c => c.Titles)
                .WithOne(i => i.Owner)
                .HasForeignKey(i => i.OwnerDBId)
                .OnDelete(DeleteBehavior.Cascade);
            // Character와 Farm 간의 일대일 관계 설정
            modelBuilder.Entity<CharacterDB>()
                .HasOne(c => c.Farm)
                .WithOne(f => f.Owner)
                .HasForeignKey<FarmDB>(f => f.OwnerDBId)
                .OnDelete(DeleteBehavior.Cascade);
            // Farm과 Crop 간의 일대다 관계 설정
            modelBuilder.Entity<FarmDB>()
                .HasMany(f => f.Crops)
                .WithOne(c => c.BelongingFarm)
                .HasForeignKey(c => c.BelongingFarmDBId)
                .OnDelete(DeleteBehavior.Cascade);
            //Friend N:N 관계
            modelBuilder.Entity<FriendDB>().HasKey(f => new { f.CharacterDBId, f.FriendDBId });
            modelBuilder.Entity<FriendDB>()
                .HasOne(f => f.Character)
                .WithMany(u => u.Friends)
                .HasForeignKey(f => f.CharacterDBId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendDB>()
                .HasOne(f => f.FriendCharacter)
                .WithMany()
                .HasForeignKey(f => f.FriendDBId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }

        public override int SaveChanges() {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IHasTimeStamps &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries) {
                var now = DateTime.UtcNow;
                if (entityEntry.State == EntityState.Added) {
                    ((IHasTimeStamps)entityEntry.Entity).CreatedAt = now;
                }

                ((IHasTimeStamps)entityEntry.Entity).UpdatedAt = now;
            }

            var deletedEntries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IHasTimeStamps && e.State == EntityState.Deleted && !(e.Entity is TokenDB));

            foreach (var entityEntry in deletedEntries) {
                var now = DateTime.Now;
                entityEntry.State = EntityState.Modified;
                ((IHasTimeStamps)entityEntry.Entity).DeletedAt = now;
            }

            return base.SaveChanges();
        }
    }
}
