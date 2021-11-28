using System.Collections.Generic;

namespace WebDiscrete.Data.Entity
{
    public class GetObjectAccessTypesResponse
    {
        public List<UserDto> Users { get; set; }
        public SystemObjectDto ObjectDto { get; set; }
    }
}