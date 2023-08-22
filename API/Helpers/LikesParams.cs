namespace API.Helpers
{
    public class LikesParams: PaginationParams
    {
        public int UserId { get; set; } // usuario id
        public string Predicate { get; set; } // predicado
    }
}