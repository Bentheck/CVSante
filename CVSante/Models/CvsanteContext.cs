using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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

    public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }

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

            entity.HasMany(e => e.AspNetUserRoles)
                  .WithOne(e => e.Role)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AspNetRoles_AspNetUserRoles_RoleId");

            entity.HasMany(e => e.AspNetRoleClaims)
                  .WithOne(e => e.Role)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AspNetRoles_AspNetRoleClaims_RoleId");
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetUs__3214EC07C0010FD5");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(e => e.AspNetUserRoles)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AspNetUsers_AspNetUserRoles_UserId");

            entity.HasMany(e => e.AspNetUserClaims)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AspNetUsers_AspNetUserClaims_UserId");

            entity.HasMany(e => e.AspNetUserLogins)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AspNetUsers_AspNetUserLogins_UserId");

            entity.HasMany(e => e.AspNetUserTokens)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AspNetUsers_AspNetUserTokens_UserId");
        });

        modelBuilder.Entity<AspNetUserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__AspNetUs__AF2760ADC25C17C5");
            entity.ToTable("AspNetUserRoles");
            entity.HasIndex(e => e.RoleId, "IX_AspNetUserRoles_RoleId");
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetRo__3214EC07EFA270EA");
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");
            entity.HasOne(d => d.Role)
                  .WithMany(p => p.AspNetRoleClaims)
                  .HasForeignKey(d => d.RoleId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AspNetRoleClaims_AspNetRoles_RoleId");
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetUs__3214EC070A5BA62B");
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");
            entity.HasOne(d => d.User)
                  .WithMany(p => p.AspNetUserClaims)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AspNetUserClaims_AspNetUsers_UserId");
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey }).HasName("PK__AspNetUs__2B2C5B5278600BD3");
            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");
            entity.HasOne(d => d.User)
                  .WithMany(p => p.AspNetUserLogins)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AspNetUserLogins_AspNetUsers_UserId");
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name }).HasName("PK__AspNetUs__8CC49841C5BE8CB4");
            entity.HasOne(d => d.User)
                  .WithMany(p => p.AspNetUserTokens)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AspNetUserTokens_AspNetUsers_UserId");
        });

        modelBuilder.Entity<Commentaire>(entity =>
        {
            entity.HasIndex(e => e.FkUserparamedic, "IX_Commentaires_FK_USERPARAMEDIC");

            entity.HasIndex(e => e.FkUserId, "IX_Commentaires_FK_USER_ID");

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

            entity.HasIndex(e => e.FkCompany, "IX_Company_Roles_FK_COMPANY");

            entity.Property(e => e.IdRole).HasColumnName("ID_Role");
            entity.Property(e => e.CreateParamedic).HasColumnName("Create_Paramedic");
            entity.Property(e => e.EditCompany).HasColumnName("Edit_Company");
            entity.Property(e => e.EditParamedic).HasColumnName("Edit_Paramedic");
            entity.Property(e => e.EditRole).HasColumnName("Edit_Role");
            entity.Property(e => e.FkCompany).HasColumnName("FK_COMPANY");
            entity.Property(e => e.GetCitoyen).HasColumnName("Get_Citoyen");
            entity.Property(e => e.GetHistorique).HasColumnName("Get_Historique");

            entity.HasOne(d => d.FkCompanyNavigation).WithMany(p => p.CompanyRoles)
                .HasForeignKey(d => d.FkCompany)
                .HasConstraintName("FK_Company_Roles_Company1");
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
            // Define a single primary key, or use composite keys with non-nullable parts
            entity.HasKey(e => e.HistId); // Assuming you have an Id property or use another key strategy

            entity.ToTable("HistoriqueParam");

            // Configure properties
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.FkParamId).HasColumnName("FK_PARAM_ID");
            entity.Property(e => e.Action).HasMaxLength(450);
            entity.Property(e => e.Date).HasColumnType("datetime");

            // Configure relationships
            entity.HasOne(d => d.FkParam)
                .WithMany(p => p.HistoriqueParams)
                .HasForeignKey(d => d.FkParamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistoriqueParam_UserParamedic");

            entity.HasOne(d => d.FkUser)
                .WithMany(p => p.HistoriqueParams)
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.SetNull) // Handle null values properly
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
            entity.HasKey(e => e.AdId).HasName("PK_UserAdresse_1");

            entity.ToTable("UserAdresse");

            entity.HasIndex(e => e.FkUserId, "IX_UserAdresse_FK_USER_ID");

            entity.Property(e => e.AdId).HasColumnName("AdID");
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

            entity.HasOne(d => d.FkUser).WithMany(p => p.UserAdresses)
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAdresse_UserCitoyen1");
        });

        modelBuilder.Entity<UserAllergy>(entity =>
        {
            entity.HasKey(e => e.AlId).HasName("PK_UserAllergies_1");

            entity.HasIndex(e => e.FkUserId, "IX_UserAllergies_FK_USER_ID");

            entity.Property(e => e.AlId).HasColumnName("AlID");
            entity.Property(e => e.AllergieIntolerance)
                .HasMaxLength(450)
                .HasColumnName("Allergie_Intolerance");
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.Gravite).HasMaxLength(450);
            entity.Property(e => e.Produit).HasMaxLength(450);

            entity.HasOne(d => d.FkUser).WithMany(p => p.UserAllergies)
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAllergies_UserCitoyen");
        });

        modelBuilder.Entity<UserAntecedent>(entity =>
        {
            entity.HasKey(e => e.AnId);

            entity.HasIndex(e => e.FkUserId, "IX_UserAntecedents_FK_USER_ID");

            entity.Property(e => e.AnId).HasColumnName("AnID");
            entity.Property(e => e.Antecedent).HasMaxLength(2500);
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");

            entity.HasOne(d => d.FkUser).WithMany(p => p.UserAntecedents)
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAntecedents_UserCitoyen");
        });

        modelBuilder.Entity<UserCitoyen>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("UserCitoyen");

            entity.HasIndex(e => e.FkIdentityUser, "IX_UserCitoyen_FK_IDENTITY_USER");

            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.FkIdentityUser).HasColumnName("FK_IDENTITY_USER");

            entity.HasOne(d => d.FkIdentityUserNavigation).WithMany(p => p.UserCitoyens)
                .HasForeignKey(d => d.FkIdentityUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserCitoyen_AspNetUsers");
        });

        modelBuilder.Entity<UserFamily>(entity =>
        {
            entity.HasKey(e => new { e.FkFamilyId, e.FkUserId });

            entity.ToTable("UserFamily");

            entity.HasIndex(e => e.FkUserId, "IX_UserFamily_FK_USER_ID");

            entity.Property(e => e.FkFamilyId).HasColumnName("FK_FAMILY_ID");
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.FamilyRole)
                .HasMaxLength(450)
                .HasColumnName("Family_Role");
            entity.Property(e => e.IsFamilyAccount).HasColumnName("Is_family_account");

            entity.HasOne(d => d.FkFamily).WithMany(p => p.UserFamilies)
                .HasForeignKey(d => d.FkFamilyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFamily_FamilyList");

            entity.HasOne(d => d.FkUser).WithMany(p => p.UserFamilies)
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFamily_UserCitoyen");
        });

        modelBuilder.Entity<UserHandicap>(entity =>
        {
            entity.HasKey(e => e.HanId);

            entity.ToTable("UserHandicap");

            entity.HasIndex(e => e.FkUserId, "IX_UserHandicap_FK_USER_ID");

            entity.Property(e => e.HanId).HasColumnName("HanID");
            entity.Property(e => e.Definition).HasMaxLength(2500);
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.FkUser).WithMany(p => p.UserHandicaps)
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserHandicap_UserCitoyen");
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.FkUserId);

            entity.Property(e => e.FkUserId)
                .ValueGeneratedNever()
                .HasColumnName("FK_USER_ID");
            entity.Property(e => e.DateNaissance)
                .HasMaxLength(450)
                .HasColumnName("Date_Naissance");
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

            entity.HasOne(d => d.FkUser).WithOne(p => p.UserInfo)
                .HasForeignKey<UserInfo>(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserInfos_UserCitoyen");
        });

        modelBuilder.Entity<UserMedication>(entity =>
        {
            entity.HasKey(e => e.MedId);

            entity.ToTable("UserMedication");

            entity.HasIndex(e => e.FkUserId, "IX_UserMedication_FK_USER_ID");

            entity.Property(e => e.MedId).HasColumnName("MedID");
            entity.Property(e => e.FkUserId).HasColumnName("FK_USER_ID");
            entity.Property(e => e.MedicamentProduitNat)
                .HasMaxLength(50)
                .HasColumnName("Medicament_ProduitNat");
            entity.Property(e => e.Nom).HasMaxLength(450);
            entity.Property(e => e.Posologie).HasMaxLength(450);
            entity.Property(e => e.Raison).HasMaxLength(450);

            entity.HasOne(d => d.FkUser).WithMany(p => p.UserMedications)
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserMedicaiton_UserCitoyen");
        });

        modelBuilder.Entity<UserParamedic>(entity =>
        {
            entity.HasKey(e => e.ParamId);

            entity.ToTable("UserParamedic");

            entity.HasIndex(e => e.FkCompany, "IX_UserParamedic_FK_COMPANY");

            entity.HasIndex(e => e.FkIdentityUser, "IX_UserParamedic_FK_IDENTITY_USER");

            entity.HasIndex(e => e.FkRole, "IX_UserParamedic_FK_ROLE");

            entity.Property(e => e.ParamId).HasColumnName("Param_ID");
            entity.Property(e => e.FkCompany).HasColumnName("FK_COMPANY");
            entity.Property(e => e.FkIdentityUser).HasColumnName("FK_IDENTITY_USER");
            entity.Property(e => e.FkRole).HasColumnName("FK_ROLE");
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
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserParamedic_AspNetUsers");

            entity.HasOne(d => d.FkRoleNavigation).WithMany(p => p.UserParamedics)
                .HasForeignKey(d => d.FkRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserParamedic_Company_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
