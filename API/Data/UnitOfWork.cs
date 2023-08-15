using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    // la unidad de trabajo es un patrón de diseño que agrupa operaciones de la base de datos
    // en una sola transacción agrupa multiples operaciones
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UnitOfWork(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        // agrupa las funciones de UserRepository 
        public IUserRepository UserRepository => new UserRepository(context, mapper);

        // se agrupa las funciones de MessageRepository
        public IMessageRepository MessageRepository => new MessageRepository(context, mapper);

        // se agrupa las funciones de LikesRepository
        public ILikesRepository LikesRepository => new LikesRepository(context);

        // se
        public async Task<bool> Complete()
        {
            return await context.SaveChangesAsync() > 0; // guardar información en la base de datos
            // devolver un valor bool verdadero si guardó al menos un valor en la base de datos
        }

        public bool HasChanges()
        {
            return context.ChangeTracker.HasChanges(); // verifica si hay cambios pendientes
            // en el contexto, si hay cambios devuelve true de lo contrario es un false
        }
    }
}