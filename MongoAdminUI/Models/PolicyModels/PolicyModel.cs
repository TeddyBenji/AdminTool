using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoAdminUI.Models.PolicyModels
{
    public class PolicyModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? PolicyId { get; set; }

        public string Name { get; set; }

        public List<string>? Roles { get; set; }

        public List<string>? RolesToAdd { get; set; }

        public List<string>? RolesToRemove { get; set; }
    }
}
