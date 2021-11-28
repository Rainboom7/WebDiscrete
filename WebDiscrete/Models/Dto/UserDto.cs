using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebDiscrete.Models;

namespace WebDiscrete.Data.Entity
{
    public class UserDto
    {
        public int Id { get; set; }
        [Required] public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<AccessRightType> AccessRights { get; set; }
    }
}