using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CVSante.Models;

public partial class CvsanteContext : DbContext
{
    public CvsanteContext()
    {
    }

    public CvsanteContext(DbContextOptions<CvsanteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Commentaire> Commentaires { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<CompanyRole> CompanyRoles { get; set; }

    public virtual DbSet<FamilyList> FamilyLists { get; set; }

    public virtual DbSet<HistoriqueParam> HistoriqueParams { get; set; }

    public virtual DbSet<HostedAccount> HostedAccounts { get; set; }

    public virtual DbSet<UserAdresse> UserAdresses { get; set; }

    public virtual DbSet<UserAllergy> UserAllergies { get; set; }

    public virtual DbSet<UserAntecedent> UserAntecedents { get; set; }

    public virtual DbSet<UserCitoyen> UserCitoyens { get; set; }

    public virtual DbSet<UserFamily> UserFamilies { get; set; }

    public virtual DbSet<UserHandicap> UserHandicaps { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    public virtual DbSet<UserMedication> UserMedications { get; set; }

    public virtual DbSet<UserParamedic> UserParamedics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetRo__3214EC07679ACB59");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetRo__3214EC07EFA270EA");

            entity.Property(e => e.RoleId).HasMaxLength(450);

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__AspNetRol__RoleI__1332DBDC");
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetUs__3214EC07C0010FD5");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__AspNetUse__RoleI__07C12930"),
                    l => l.HasOne<AspNetUser>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__AspNetUse__UserI__06CD04F7"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__AspNetUs__AF2760ADC25C17C5");
                        j.ToTable("AspNetUserRoles");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetUs__3214EC070A5BA62B");

            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AspNetUse__UserI__0A9D95DB");
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey }).HasName("PK__AspNetUs__2B2C5B5278600BD3");

            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AspNetUse__UserI__0D7A0286");
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name }).HasName("PK__AspNetUs__8CC49841C5BE8CB4");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AspNetUse__UserI__10566F31");
        });

        modelBuilder.Entity<Commentaire>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Comment).HasMaxLength(4000);
            entity.Property(e => e.Date).HasColumnType("smalldatetime");
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.FkUserparamedic).HasColumnName("FK_USERPARAMEDIC");

            entity.HasOne(d => d.FkUser).WithMany(p => p.Commentaires)
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Commentaires_UserCitoyen");

            entity.HasOne(d => d.FkUserparamedicNavigation).WithMany(p => p.Commentaires)
                .HasForeignKey(d => d.FkUserparamedic)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Commentaires_UserParamedic");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.IdComp);

            entity.ToTable("Company");

            entity.Property(e => e.IdComp).HasColumnName("ID_Comp");
            entity.Property(e => e.AdId)
                .HasMaxLength(20)
                .HasColumnName("AD_ID");
            entity.Property(e => e.AdLink)
                .HasMaxLength(500)
                .HasColumnName("AD_Link");
            entity.Property(e => e.CompName)
                .HasMaxLength(450)
                .HasColumnName("Comp_name");
        });

        modelBuilder.Entity<CompanyRole>(entity =>
        {
            entity.HasKey(e => e.IdRole);

            entity.ToTable("Company_Roles");

            entity.Property(e => e.IdRole).HasColumnName("ID_Role");
            entity.Property(e => e.RoleDroits).HasColumnName("Role_Droits");
            entity.Property(e => e.RoleName)
                .HasMaxLength(450)
                .HasColumnName("Role_Name");
        });

        modelBuilder.Entity<FamilyList>(entity =>
        {
            entity.HasKey(e => e.FamilyId);

            entity.ToTable("FamilyList");

            entity.Property(e => e.FamilyId).HasColumnName("Family_ID");
            entity.Property(e => e.FamilyName)
                .HasMaxLength(450)
                .HasColumnName("Family_Name");
        });

        modelBuilder.Entity<HistoriqueParam>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("HistoriqueParam");

            entity.Property(e => e.Action).HasMaxLength(450);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.FkParamId).HasColumnName("FK_PARAM_ID");
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");

            entity.HasOne(d => d.FkParam).WithMany()
                .HasForeignKey(d => d.FkParamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistoriqueParam_UserParamedic");

            entity.HasOne(d => d.FkUser).WithMany()
                .HasForeignKey(d => d.FkUserId)
                .HasConstraintName("FK_HistoriqueParam_UserCitoyen");
        });

        modelBuilder.Entity<HostedAccount>(entity =>
        {
            entity.HasKey(e => e.IdRepondants);

            entity.ToTable("Hosted_Accounts");

            entity.Property(e => e.IdRepondants)
                .HasColumnType("numeric(18, 0)")
                .HasColumnName("ID_Repondants");
            entity.Property(e => e.FkAspRepondants)
                .HasMaxLength(450)
                .HasColumnName("FK_ASP_REPONDANTS");
        });

        modelBuilder.Entity<UserAdresse>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UserAdresse");

            entity.Property(e => e.AdressePrimaire).HasColumnName("Adresse_Primaire");
            entity.Property(e => e.Appartement).HasMaxLength(450);
            entity.Property(e => e.CodePostal)
                .HasMaxLength(450)
                .HasColumnName("Code_Postal");
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.NumCivic)
                .HasMaxLength(450)
                .HasColumnName("Num_Civic");
            entity.Property(e => e.Rue).HasMaxLength(450);
            entity.Property(e => e.TelphoneAdresse)
                .HasMaxLength(450)
                .HasColumnName("Telphone_adresse");
            entity.Property(e => e.Ville).HasMaxLength(450);

            entity.HasOne(d => d.FkUser).WithMany()
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAdresse_UserCitoyen");
        });

        modelBuilder.Entity<UserAllergy>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.AllergieIntolerance)
                .HasMaxLength(450)
                .HasColumnName("Allergie_Intolerance");
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.Gravite).HasMaxLength(450);
            entity.Property(e => e.Produit).HasMaxLength(450);

            entity.HasOne(d => d.FkUser).WithMany()
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAllergies_UserCitoyen");
        });

        modelBuilder.Entity<UserAntecedent>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Antecedent).HasMaxLength(2500);
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");

            entity.HasOne(d => d.FkUser).WithMany()
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAntecedents_UserCitoyen");
        });

        modelBuilder.Entity<UserCitoyen>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("UserCitoyen");

            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.FkIdentityUser)
                .HasMaxLength(450)
                .HasColumnName("FK_IDENTITY_USER");

            entity.HasOne(d => d.FkIdentityUserNavigation).WithMany(p => p.UserCitoyens)
                .HasForeignKey(d => d.FkIdentityUser)
                .HasConstraintName("FK_UserCitoyen_AspNetUsers");
        });

        modelBuilder.Entity<UserFamily>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UserFamily");

            entity.Property(e => e.FamilyRole)
                .HasMaxLength(450)
                .HasColumnName("Family_Role");
            entity.Property(e => e.FkFamilyId).HasColumnName("FK_FAMILY_ID");
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.IsFamilyAccount).HasColumnName("Is_family_account");

            entity.HasOne(d => d.FkFamily).WithMany()
                .HasForeignKey(d => d.FkFamilyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFamily_FamilyList");

            entity.HasOne(d => d.FkUser).WithMany()
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFamily_UserCitoyen");
        });

        modelBuilder.Entity<UserHandicap>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UserHandicap");

            entity.Property(e => e.Definition).HasMaxLength(2500);
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.FkUser).WithMany()
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserHandicap_UserCitoyen");
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.DateNaissance)
                .HasMaxLength(450)
                .HasColumnName("Date_Naissance");
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.Nom).HasMaxLength(450);
            entity.Property(e => e.Poids).HasMaxLength(450);
            entity.Property(e => e.Prenom).HasMaxLength(450);
            entity.Property(e => e.Pronoms).HasMaxLength(450);
            entity.Property(e => e.Sexe).HasMaxLength(450);
            entity.Property(e => e.Taille).HasMaxLength(450);
            entity.Property(e => e.TelephoneCell)
                .HasMaxLength(450)
                .HasColumnName("Telephone_Cell");
            entity.Property(e => e.TypeSanguin)
                .HasMaxLength(450)
                .HasColumnName("Type_Sanguin");

            entity.HasOne(d => d.FkUser).WithMany()
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserInfos_UserCitoyen");
        });

        modelBuilder.Entity<UserMedication>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UserMedication");

            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.MedicamentProduitNat)
                .HasMaxLength(50)
                .HasColumnName("Medicament_ProduitNat");
            entity.Property(e => e.Nom).HasMaxLength(450);
            entity.Property(e => e.Posologie).HasMaxLength(450);
            entity.Property(e => e.Raison).HasMaxLength(450);

            entity.HasOne(d => d.FkUser).WithMany()
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserMedicaiton_UserCitoyen");
        });

        modelBuilder.Entity<UserParamedic>(entity =>
        {
            entity.HasKey(e => e.ParamId);

            entity.ToTable("UserParamedic");

            entity.Property(e => e.ParamId).HasColumnName("Param_ID");
            entity.Property(e => e.FkCompany).HasColumnName("FK_COMPANY");
            entity.Property(e => e.FkIdentityUser)
                .HasMaxLength(450)
                .HasColumnName("FK_IDENTITY_USER");
            entity.Property(e => e.Matricule).HasMaxLength(450);
            entity.Property(e => e.Nom).HasMaxLength(450);
            entity.Property(e => e.ParamIsActive)
                .HasDefaultValue(true)
                .HasColumnName("Param_Is_Active");
            entity.Property(e => e.Prenom).HasMaxLength(450);
            entity.Property(e => e.Telephone).HasMaxLength(450);
            entity.Property(e => e.Ville).HasMaxLength(450);

            entity.HasOne(d => d.FkCompanyNavigation).WithMany(p => p.UserParamedics)
                .HasForeignKey(d => d.FkCompany)
                .HasConstraintName("FK_UserParamedic_Company1");

            entity.HasOne(d => d.FkIdentityUserNavigation).WithMany(p => p.UserParamedics)
                .HasForeignKey(d => d.FkIdentityUser)
                .HasConstraintName("FK_UserParamedic_AspNetUsers");

            entity.HasMany(d => d.FkRoles).WithMany(p => p.FkParams)
                .UsingEntity<Dictionary<string, object>>(
                    "UserCompanyRoleId",
                    r => r.HasOne<CompanyRole>().WithMany()
                        .HasForeignKey("FkRoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserCompanyRoleID_Company_Roles"),
                    l => l.HasOne<UserParamedic>().WithMany()
                        .HasForeignKey("FkParamId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserCompanyRoleID_UserParamedic"),
                    j =>
                    {
                        j.HasKey("FkParamId", "FkRoleId");
                        j.ToTable("UserCompanyRoleID");
                        j.IndexerProperty<int>("FkParamId").HasColumnName("FK_PARAM_ID");
                        j.IndexerProperty<int>("FkRoleId").HasColumnName("FK_ROLE_ID");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
