using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await context.Users // obtener los datos de la tabla users
                .Where(x => x.UserName == username) // filtrar los datos del usuario
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider) // transformar los datos
                // de AppUser a MemberDto
                .SingleOrDefaultAsync();
                /*
                    Como tal SingleOrDefaultAsync funciona de la siguiente manera:
                    1. Este método se utiliza cuando se pretende devolver un elemento o ninguno
                    2. Si la consulta Where devuelve un elemento, el método SingleOrDefaultAsync
                    Lo devolverá.
                    3. Si la consulta where no devulve elementos, el método SingleOrDefaultAsync
                    devolverá un null
                    4. Si la consulta where devuelve más de un elemento, el método
                    SingleOrDefaultAsync devolverá un error (InvalidOperationException) el objetivo
                    de esta consulta es buscar elementos que sean únicos.
                    5. La diferencia entre SingleOrDefaultAsync y FirstOrDefaultAsync es que 
                    SingleOrDefaultAsync: Exige devolver un elemento si encuentra dos devolverá un error
                    FirstOrDefaultAsync: Si encuentra dos devolverá el primero de todos.
                */
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = context.Users.AsQueryable(); // convertir la variable query en una consulta
            // devuelve un tipo de dato IQueryable<AppUser>

            query = query.Where(u => u.UserName != userParams.CurrentUsername); // aplica el filtro
            // para exluir el usuario actual

            query = query.Where(u => u.Gender == userParams.Gender); // aplica el filtro para consultar
            // los usuarios

            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge-1));
            /*
                DateTime.Today = obtiene la fecha de hoy
                DateTime.Today.AddYears = se suman los años a la fecha de hoy
                (-userParam.MaxAge-1) (-100-1) = (-101) en este caso resta años a la fecha actual
                DateOnly.FromDateTime = Castea la fecha a DateOnly para quitarle la hora y 
                dejar solo  la fecha sin la hora, porque se le resta -1 porque queremos tener 
                encuenta las personas que tienen 100 años y no han cumplido 101 años
            */
            
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));
            /*
                DateTime.Today = obtiene la fecha de hoy
                DateTime.Today.AddYears = se suman los años a la fecha de hoy
                (-userParam.MinAge) (-18) en este caso resta años a la fecha actual
                DateOnly.FromDateTime = Castea la fecha a DateOnly para quitarle la hora y 
                dejar solo  la fecha sin la hora, porque no se le resta -1 porque queremos tener 
                encuenta las personas que tienen 18 años en vez de tener presente
                los que han cumplido 19 años
            */

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            /*
                Filtra las personas de acuerdo a la fecha minima y la fecha máxima
            */

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive),
            };
            /*
                si el parámetro OrderBy por defecto es lastActive entonces va a ordenar
                por query.OrderByDescending(u => u.LastActive), por el ultimo activo

                sie le parámetro OrderBy es created ordena por dato creado
                "created" => query.OrderByDescending(u => u.Created)
            */
            // retorna una lista con los elementos páginados
            return await PagedList<MemberDto>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDto>(mapper.ConfigurationProvider), 
                userParams.PageNumber,
                userParams.PageSize);
            /*
                query.AsNoTracking() // cuando utilizamos Entity Framework por defecto se
                realizan seguimiento a los cambios que se realizan en las entidades 
                que estan en memoria y se guarda un registro, por ejemplo cuando 
                se llama el método SaveChanges() se guardan los cambios de ese registro
                pero con AsNoTracking() No necesitamos hacer ningun cambio y por lo tanto
                NO NECESITAMOS HACER SEGUIMIENTO A NINGUNA ENTIDAD porque solo se va 
                a hacer lectura AsNoTracking se encarga de no hacer este seguimiento y no
                se enviará ningún cambio a la base de datos.
                ProjectTo<MemberDto>(mapper.ConfigurationProvider) se encarga de transformar
                los datos a MemberDto y retorna una lista de MemberDto.
                userParams.PageNumber = Enviar el numero actual de la página
                userParams.PageSize = Envia la cantidad de registros por página
            */
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await context.Users // ingresa a los datos de la tabla Users
                .Include(p => p.Photos)
                /*
                    Utiliza el método Include para realizar una carga anticipada de los datos
                    (eager loading) se utiliza para cargar datos relacionados en este caso
                    Users y Photos estan relacionados por lo tanto cuando se hace 
                    SingleOrDefaultAsync(x => x.UserName == username); trae las fotos relacionadas
                    con este usuario y no tocaría hacer una consulta adicional
                */
                .SingleOrDefaultAsync(x => x.UserName == username);
                /*
                    SingleOrDefaultAsync(x => x.UserName == username) se realiza un filtro por usuario
                    y devuelve el valor único o null
                */
        }

        public async Task<string> GetUserGender(string username)
        {
            return await context.Users // ingresa a los datos de la tabla Users
                .Where(x => x.UserName == username) // filtra por UserName
                .Select(x => x.Gender) // con select devuelve el valor del genero del usuario
                .FirstOrDefaultAsync(); // devuelve el primer elemento que encuentra
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await context.Users // traer todos los users
                .Include(p =>p.Photos) // incluir la consulta de fotos de cada usuario
                .ToListAsync(); // traer una lista
        }

        public void Update(AppUser user)
        {
            context
            .Entry(user). // se utiliza para obtener una entidad rastreada por el contexto
            State = EntityState.Modified; // esto se refiere a que el estado de la entidad
            // ha sido modificada y cuando se llame SaveChanges debe cambiar
        }
    }
}