using MongoDB.Bson.Serialization.Attributes;

namespace M101DotNet.Training.Poco
{
    [BsonIgnoreExtraElements]
    public class Widget
    {
        public int Id { get; set; }

        [BsonElement("x")]
        public int X { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, X: {1}", Id, X);
        }
    }
}
