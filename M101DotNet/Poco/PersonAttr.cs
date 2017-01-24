using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace M101DotNet.Poco
{
    public class PersonAttr
    {
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonRepresentation(BsonType.String)]
        public int Age { get; set; }

        public List<string> Colors { get; set; }

        public List<Pet> Pets { get; set; }

        public BsonDocument ExtraElements { get; set; }
    }
}
