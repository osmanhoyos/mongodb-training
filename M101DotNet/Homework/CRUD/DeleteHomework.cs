using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace M101DotNet.Homework.CRUD
{
    public class DeleteHomework
    {  
        public void Execute()
        {
            var client = new MongoClient();
            var db = client.GetDatabase("m101");
            var collection = db.GetCollection<Grade>("hw22");

            OptionFromMongoDB(collection);
        }

        private void OptionFromMongoDB(IMongoCollection<Grade> grades)
        {
            // no student has a negative id, so we'll use that as a safe starting
            // point
            int currentStudentId = -1;          

            // Find all the homeworks, sort by StudentId and then Score.
            grades
                .Find(x => x.Type == GradeType.homework)
                .SortBy(x => x.StudentId).ThenBy(x => x.Score)
                .ForEachAsync(async grade =>
                {
                    if (grade.StudentId != currentStudentId)
                    {
                        currentStudentId = grade.StudentId;

                        // The first grade for each student will always be their lowest,
                        // so delete it...
                        await grades.DeleteOneAsync(x => x.Id == grade.Id);
                    }
                });

            // We haven't gotten to this part in the class yet, but it's the
            // translation of the aggregation query from the instructions into .NET.
            var result = grades.Aggregate()
                .Group(x => x.StudentId, g => new { StudentId = g.Key, Average = g.Average(x => x.Score) })
                .SortByDescending(x => x.Average)
                .FirstAsync();

            Console.WriteLine(result);
        }

        private class Grade
        {
            public ObjectId Id { get; set; }

            [BsonElement("student_id")]
            public int StudentId { get; set; }

            [BsonElement("type")]
            [BsonRepresentation(BsonType.String)]
            public GradeType Type { get; set; }

            [BsonElement("score")]
            public double Score { get; set; }
        }

        public enum GradeType
        {
            // I don't like needing to use lowercase here, but we don't have a built-in solution
            // for changing the case of enum values.
            homework,
            exam,
            quiz
        }
    }    
}

