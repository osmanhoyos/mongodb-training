using MongoDB.Bson;
using System.Collections.Generic;

namespace M101DotNet.Training.Poco
{
    public class Person
    {
        public ObjectId Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public List<string> Colors { get; set; }

        public List<Pet> Pets { get; set; }

        public BsonDocument ExtraElements { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, Name: \" {1}\", Age: {2}", Id, Name, Age);
        }
    }
}
