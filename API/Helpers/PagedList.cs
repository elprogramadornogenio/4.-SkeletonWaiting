using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class PagedList<T>: List<T>
    {
        public PagedList(
            IEnumerable<T> items, // secuencia de tipo generico (recibe cualquier tipo de clase)
            int count, // recuento total de elementos
            int pageNumber, // numero actual de la pagina
            int pageSize // tamaño de datos en una página
            )
        {
            CurrentPage = pageNumber; // guarda el numero actual de la página
            TotalPages = (int) Math.Ceiling(count/(double) pageSize); // Total de paginas
            /*
                Divide count/ pageSize es decir count(total de elementos)/ pageSize (tamaño de la pagina)
                Math.Ceiling redondea el resultado anterior hacia arriba y lo castea a entero
            */
            PageSize = pageSize; // numero total de elementos en una pagina
            TotalCount = count; // total de elementos
            AddRange(items); // agrega los elementos de la colección a una lista existente
        }

        public int CurrentPage { get; set; } // guarda la pagina actual
        public int TotalPages { get; set; } // guarda el total de las paginas calculadas
        public int PageSize { get; set; } // guarda el numero de elementos por pagina
        public int TotalCount { get; set; } // total de elementos

        public static async Task<PagedList<T>> CreateAsync(
            IQueryable<T> source, // resultado de una consulta de bases de datos
            int pageNumber, // número de paginas 
            int pageSize // numero de elementos por pagina
            ) 
        {
            var count = await source.CountAsync(); // obtiene el total de elementos del
            // resultado de la consulta IQueryable
            var items = await source.Skip((pageNumber -1)* pageSize)
                .Take(pageSize).ToListAsync();
                /*
                    source.Skip es el numero de elementos a omitir
                    con la formula (pageNumber-1)* pageSize 
                    por ejemplo
                    en estos momentos me encuentro quiero los elementos de la página 3
                    debo omitir los elementos de las paginas 1 y 2 
                    es por esta razón (pageNumber-1)
                    ahora se toma el numero de elementos de la página y se multiplica por
                    (pageNumber-1) digamos que cada pagina tiene 5 elementos
                    por lo tanto (3-1)*5 = (2)*5 = 10 es decir que 10 elementos deben ser
                    omitidos
                    con take se toma el numero de elementos especificados despues de los
                    elementos omitidos
                    despues el método ToListAsync consulta la base de datos y lista los
                    elementos

                */ 
            
            return new PagedList<T>(items, count, pageNumber, pageSize); // se insertan
            // los elementos al constructor de la misma clase.
            
        }
    }
}