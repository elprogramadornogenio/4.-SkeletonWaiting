using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Group
    {

        public Group()
        {
            
        }
        public Group(string name)
        {
            Name = name;
        }

        [Key]
        public string Name { get; set; } // nombre de la conexión entre dos usuarios
        public ICollection<Connection> Connections { get; set; } = new List<Connection>(); 
        // relacion de un grupo contiene muchos usuarios (relación de uno a muchos)
    }
}