using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace M101DotNet.Homework.Schema_Design
{
    public class DeleteHomeworkArray
    {
        public void Execute()
        {

            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);

            var db = client.GetDatabase("school");
            var col = db.GetCollection<Students>("students"); //Dymanic Object that represents the schema

            int studentId = -1;

            var list = col.Find(new BsonDocument())
                .ForEachAsync(x =>
                {
                    if (studentId != x.Id)
                    {
                        studentId = x.Id;

                        if (x.scores.Any())
                        {
                            var lessHomeworkScore = -1D;
                            
                            foreach (var item in x.scores)
                            {
                                if (item.Type.Equals("homework"))
                                {
                                    if (lessHomeworkScore.Equals(-1))
                                    {
                                        lessHomeworkScore = item.Score;
                                    }
                                    else if (lessHomeworkScore < item.Score)
                                    {
                                        var update = Builders<Students>.Update.PullFilter("scores",
                                                        Builders<Scores>.Filter.Eq("score", lessHomeworkScore)
                                                     );
                                        var result = col.UpdateOne(Builders<Students>.Filter.Eq("_id", studentId), update);
                                    }
                                    else
                                    {
                                        var update = Builders<Students>.Update.PullFilter("scores",
                                                        Builders<Scores>.Filter.Eq("score", item.Score)
                                                     );
                                        var result = col.UpdateOne(Builders<Students>.Filter.Eq("_id", studentId), update);
                                    }
                                }
                            }
                        }
                    }
                });
        }
    }

    public class Students
    {

        [BsonElement("_id")]
        public int Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("scores")]
        public List<Scores> scores;
    }


    public class Scores
    {

        [BsonElement("type")]
        [BsonRepresentation(BsonType.String)]
        public string Type { get; set; }

        [BsonElement("score")]
        public double Score { get; set; }
    }
}
