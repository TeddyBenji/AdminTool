using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MongoAdminUI.Models
{
    [BsonIgnoreExtraElements]
    public class RoleModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.Binary)]
        public Guid Id { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<ClaimModel>? Claims { get; set; }
    }

}


