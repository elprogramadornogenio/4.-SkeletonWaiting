namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateOnly date)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow); // inicializa el dia de hoy
            var age = today.Year - date.Year; // resta el año actual con el año de nacimiento

            if(date > today.AddYears(-age)) age--; // si la fecha de nacimiento es mayor
            // a la fecha de hoy restando los años quiere decir que ya cumplio años de lo contrario
            // le resta un año porque no lo ha cumplido

            return age;
        }
    }
}