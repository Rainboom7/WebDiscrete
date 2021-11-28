using System.Collections.Generic;
using System.Security.AccessControl;
using WebDiscrete.Models;

namespace WebDiscrete.Data.Entity
{
    public class SystemObjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<AccessRightType> AccessRights { get; set; }
    }
}