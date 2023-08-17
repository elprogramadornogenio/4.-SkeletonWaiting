namespace API.Helpers
{
    // Esta clase se utiliza para representar la respuesta del encabezado en HTTP
    public class PaginationHeader
    {
        public PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            TotalItems = totalItems;
            TotalPages = totalPages;
        }

        public int CurrentPage { get; set; } // Almacena el número de la página actual
        public int ItemsPerPage { get; set; } 
        // Almacena la cantidad de elementos que muestra por página 
        public int TotalItems { get; set; } // almacena el número total de elementos en la
        // colección no solo en la página actual
        public int TotalPages { get; set; } // número total de páginas con todos los elementos
    }
}