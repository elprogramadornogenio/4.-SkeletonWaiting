namespace API.Helpers
{
    // representa los parametros de paginación que se van a solicitar
    public class PaginationParams
    {
        private const int MaxPageSize = 50; 
        // la cantidad máxima de valores por pagina
        public int PageNumber { get; set; } = 1; 
        // representa el numero de páginas por defecto es 1
        private int _pageSize = 10;
        // tamaño de la página que se va a comparar con MaxPageSize
        public int PageSize 
        { 
            get => _pageSize; 
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize: value; 
        }
        // representa el tamaño de la página PageSize como get 
        // el set compara si el tamaño de la página supera los 50 devuelve MaxPageSize
    }
}