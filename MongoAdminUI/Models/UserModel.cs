using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MongoAdminUI.Models
{
    [BsonIgnoreExtraElements]
    public class UserModel
    {

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        public string? UserName { get; set; }

        public string? Name { get; set; }

        [BsonElement("Roles")]
        public List<Guid>? Roles { get; set; }

        public string? Email { get; set; }

    }
}
