using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoAdminUI.Models.PolicyModels
{
    public class PolicyModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? PolicyId { get; set; }

        public string Name { get; set; }

        public List<string>? Roles { get; set; } // Now a list of strings
    }

}
