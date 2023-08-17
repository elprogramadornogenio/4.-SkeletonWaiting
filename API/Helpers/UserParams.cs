namespace API.Helpers
{
    // esta clase se encarga de declarar los parámetros y filtrar los datos
    public class UserParams: PaginationParams
    {
        public string CurrentUsername { get; set; } // usuario actual
        public string Gender { get; set; } // sexo
        public int MinAge { get; set; } = 18; // edad mínima
        public int MaxAge { get; set; } = 100; // edad máxima
        public string OrderBy { get; set; } = "lastActive"; // se ordena dependiendo al tiempo
        // de actividad.
    }
}