using WebDiscrete.Models;

namespace WebDiscrete.Data.Entity
{
    public class ObjectAccessRights
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public int ObjectId { get; set; }

        public AccessRightType AccessType { get; set; }
    }
}