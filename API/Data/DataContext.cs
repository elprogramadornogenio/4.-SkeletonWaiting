using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    /*
        IdentityDbContext<
        AppUser, // mediante IdentityDbContext esta clase representa los usuarios por TUser
        AppRole, // mediante IdentityDbContext esta clase representa los roles por TRole
        int, // representa el tipo de dato de las claves primarias de AppRole y AppUser
        IdentityUserClaim<int> Permite crear datos y cosas personalizadas en los usuarios
        AppUserRole, // relaciona los usuarios con los roles
        IdentityUserLogin<int>, // permite registro de usuarios por lugares externos como
        Google, Twiiter, Facebook
        IdentityRoleClaim<int> Permite crear datos y cosas personalizadas en los roles
        IdentityUserToken<int> La utilidad de esta funcionalidad representa el token asociado
        al usuario
        // con esta configuración se va a crear las tablas AppUser, AppRole, AppUserRole y otras
    */
    public class DataContext: IdentityDbContext<AppUser, AppRole, int, 
        IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, 
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        // DbContextOptions son las opciones de configuración del contexto de la base de datos
        public DataContext(DbContextOptions options): base(options){}
        public DbSet<UserLike> Likes { get; set; } // Crear la tabla Likes
        public DbSet<Message> Messages { get; set; } // Crear la tabla Messages
        public DbSet<Group> Groups { get; set; } // Crear la tabla Groups
        public DbSet<Connection> Connections { get; set; } // Crear la tabla Connections
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // con este paso se asegura que las configuraciones
            // hechas por IdentityDbContext se apliquen y luego se va a crear las configuraciones
            // en los siguientes pasos

            /*
                La entidad AppUser se relaciona
                por medio de HasMany (muchos)
                Roles representado por
                ICollection<AppUserRole> UserRoles
                despues con WithOne (con un) hace referencia
                al dato User que esta en la tabla AppUserRole
                para generar una relación de uno a muchos
                (AppUser y AppUserRole) respectivamente declara
                la clave foranea UserId de AppUserRole es requerido
            */
            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
            /*
                La entidad AppRole se relaciona
                por medio de HasMany (muchos)
                Roles representado por
                ICollection<AppUserRole> UserRoles
                despues con WithOne (con un) hace referencia
                al dato Role que esta en la tabla AppUserRole
                para generar una relación de uno a muchos
                (AppRole y AppUserRole) respectivamente declara
                la clave foranea RoleId de AppUserRole es requerido
            */
            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            /*
                dice que va a configurar la tabla UserLike para definir
                SourceUserId y TargetUserId como llaves primarias en
                consecuencia forman una llave compuesta es decir que
                la clave que resulta de la
                combinación de las dos columnas no puede ser igual
            */
            builder.Entity<UserLike>()
                .HasKey(k => new {k.SourceUserId, k.TargetUserId});

            /*
                La tabla UserLike se relaciona de uno a muchos con
                la tabla AppUser mediante el objeto la AppUser SourceUser
                y poder acceder a los datos de AppUser especificamente
                la lista List<UserLike> LikedUsers y va a generar como
                Clave foranea SourcedUserId de la tabla AppUserRole
                y los datos se eliminan en cascada
            */

            builder.Entity<UserLike>()
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            /*
                La tabla UserLike se relaciona de uno a muchos con
                la tabla AppUser mediante el objeto la AppUser TargetUser
                y poder acceder a los datos de AppUser especificamente
                la lista List<UserLike> LikedByUsers y va a generar como
                Clave foranea TargetUserId de la tabla AppUserRole
                y los datos se eliminan en cascada
            */

            builder.Entity<UserLike>()
                .HasOne(s => s.TargetUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.Cascade);

            /*
                se relaciona la tabla messages con AppUser de uno a muchos
                mediante el campo Recipient de la tabla messages que va de uno
                al campo List<Message> MessageReceived y se va a eliminar
                los mensajes de forma restringida
            */

            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);
            
            /*
                se relaciona la tabla messages con AppUser de uno a muchos
                mediante el campo Sender de la tabla messages que va de uno
                al campo List<Message> MessagesSent y se va a eliminar
                los mensajes de forma restringida
            */

            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}