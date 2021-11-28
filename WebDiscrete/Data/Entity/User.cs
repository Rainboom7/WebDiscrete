using System.ComponentModel.DataAnnotations;

namespace WebDiscrete.Data.Entity
{
    public class User
    {
        [Required] public int Id { get; set; }
        [Required] public string Username { get; set; }
        [Required] public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
    }
}