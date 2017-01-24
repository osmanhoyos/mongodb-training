using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using M101DotNet.Poco;
using System.Collections.Generic;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using System.Linq;

namespace M101DotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            Updates().GetAwaiter().GetResult();
            Console.WriteLine();
            Console.WriteLine("Press Enter");
            Console.ReadLine();
        }        

        static void ConnectionSetUp() {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);

            var db = client.GetDatabase("test");
            var col = db.GetCollection<BsonDocument>("people"); //Dymanic Object that represents the schema
        }

        static void DocumentRepresentation() {

            //Creating a Document
            var doc = new BsonDocument
            {
                { "name", "Jones"}
            };

            //Adding a new field - Opt 1
            doc.Add("age", 30);

            //Adding a new field - Opt 2
            doc["profession"] = "hacker";

            //Adding a Nested Array
            var nestedArray = new BsonArray();
            nestedArray.Add(new BsonDocument("color", "red"));

            doc.Add("array", nestedArray);

            //Accesing to the array's item
            Console.WriteLine(doc["array"][0]["color"]);

            //Shows the full document
            Console.WriteLine(doc);
        }

        /// <summary>
        /// 
        /// 1. Using attibutes on the POCO properties
        /// 2. Configuring by code using the BsonClassMap (Global Config)
        /// 3. Creating a Convention Pack
        /// </summary>
        static void PocoRepresentation() {

            //Opt 3
            var conventionPack = new ConventionPack();
            conventionPack.Add(new CamelCaseElementNameConvention());
            ConventionRegistry.Register("camelCase", conventionPack, t => true);

            //Opt 2
            /*BsonClassMap.RegisterClassMap<Person>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(x => x.Name).SetElementName("name");
            });*/

            var person = new Person
            {
                Name = "Jones",
                Age = 30,
                Colors = new List<string> { "red", "blue" },
                Pets = new List<Pet> { new Pet {Name = "Fluffy", Type = "Pig" } },
                ExtraElements = new BsonDocument ("anotherName", "anotherValue" )                
            };

            //Opt 1
            var personAttr = new PersonAttr
            {
                Name = "Osman",
                Age = 27,
                Colors = new List<string> { "white", "yellow" },
                Pets = new List<Pet> { new Pet { Name = "falcon", Type = "eagle" } },
                ExtraElements = new BsonDocument("anotherName", "anotherValue")
            };

            using (var writer = new JsonWriter(Console.Out))
            {
                BsonSerializer.Serialize(writer, person);
                BsonSerializer.Serialize(writer, personAttr);
            }
        }

        static void InsertDocuments()
        {  
            var client = new MongoClient();
            var db = client.GetDatabase("test");

            InsertPocoDocuments(db).GetAwaiter().GetResult();
            InsertDynamicDocuments(db).GetAwaiter().GetResult();
        }

        static async Task InsertPocoDocuments(IMongoDatabase db)
        {
            var col = db.GetCollection<BsonDocument>("people");
            var doc1 = new BsonDocument
            {
                {"Name", "Smith"},
                {"Age", 30},
                {"Profession","Hacker"},
            };

            //Insert one
            //await col.InsertOneAsync(doc1);
            //doc1.Remove("_id");
            //await col.InsertOneAsync(doc1);

            var doc2 = new BsonDocument
            {
                {"SomethingElse", true}
            };           

            //Insert many
            await col.InsertManyAsync(new[] { doc1, doc2 });
        }

        static async Task InsertDynamicDocuments(IMongoDatabase db)
        {
            var col = db.GetCollection<Person>("people");

            var person = new Person
            {
                Name = "Jones",
                Age = 30,
                Colors = new List<string> { "red", "blue" },
                Pets = new List<Pet> { new Pet { Name = "Fluffy", Type = "Pig" } },
                ExtraElements = new BsonDocument("anotherName", "anotherValue")
            };

            Console.WriteLine(person.Id);
            await col.InsertOneAsync(person);            

            //MongoDb Driver assign automatically the _id
            Console.WriteLine(person.Id);
        }

        static async Task Find()
        {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);

            var db = client.GetDatabase("test");
            var col = db.GetCollection<BsonDocument>("people");


            Console.WriteLine("Opt 1 - More granularity\n");            
            using (var cursor = await col.Find(new BsonDocument()).ToCursorAsync())
            {
                while (await cursor.MoveNextAsync())
                {
                    foreach (var doc in cursor.Current)
                    {
                        Console.WriteLine(doc);
                    }
                }
            }

            Console.WriteLine("\nOpt 2 - It's more cleaner, much quicker, code-wise, but it does force all documents in memory\n");
            var list = await col.Find(new BsonDocument()).ToListAsync();
            foreach (var doc in list)
            {
                Console.WriteLine(doc);
            }

            Console.WriteLine("\nOpt 3 - Foreach Method\n");
            await col.Find(new BsonDocument())
                     .ForEachAsync( doc => Console.WriteLine(doc)                
                );
        }

        static async Task FindWithFilters()
        {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);

            var db = client.GetDatabase("test");

            await FilterDynamicSchema(db);
            await FilterStronglyObjectSchema(db);
        }

        private static async Task FilterDynamicSchema(IMongoDatabase db)
        {
            Console.WriteLine("\nDynamic Schema\n");
            
            var col = db.GetCollection<BsonDocument>("people");

            //Opt 1 - Not Great API to query
            //var filter = new BsonDocument("Name", "Smith");
            //var filter = new BsonDocument("Age", new BsonDocument("$lt", 40));
            //var filter = new BsonDocument("$and", new BsonArray {
            //    new BsonDocument("Age", new BsonDocument("$gt", 10)),
            //    new BsonDocument("Name", "Smith")
            //});

            //Opt 2 - Builder
            var builder = Builders<BsonDocument>.Filter;
            //var filter = builder.Eq("Name", "Smith");
            //var filter = builder.Lt("Age", 40);
            //var filter = builder.And(builder.Gt("Age", 10), builder.Eq("Name", "Smith"));
            var filter = builder.Gt("Age", 10) & builder.Eq("Name", "Smith") | !builder.Eq("Name", "Yang");//Operators & | !


            var list = await col.Find(filter).ToListAsync(); //Opt 1
            //var list = await col.Find("{Name : 'Smith'}").ToListAsync(); //Opt 2 - it's necessary to parse, therefore more overhead 

            foreach (var doc in list)
            {
                Console.WriteLine(doc);
            }
        }

        private static async Task FilterStronglyObjectSchema(IMongoDatabase db)
        {
            Console.WriteLine("\nStrongly Object Schema\n");
            
            var col = db.GetCollection<Person>("people");
            var builder = Builders<Person>.Filter;

            //Opt 1
            //var filter = builder.Gt("Age", 10) & builder.Eq("Name", "Smith") | !builder.Eq("Name", "Yang"); 

            //Opt 2
            //var filter = builder.Gt(x => x.Age, 10) & builder.Eq(x => x.Name, "Smith") | !builder.Eq("Name", "Yang");
            //var list = await col.Find(filter).ToListAsync(); 

            //Opt 3
            var list = await col.Find(x => x.Age < 40 && x.Name != "Yang")
                .ToListAsync();

            foreach (var doc in list)
            {
                Console.WriteLine(doc);
            }
        }

        private static async Task Sort() {

            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);

            var db = client.GetDatabase("test");

            await SortDynamicSchema(db);
            await StronglyObjectSchema(db);
        }

        private static async Task SortDynamicSchema(IMongoDatabase db) {
            Console.WriteLine("\n Sort Dynamic Schema\n");

            var col = db.GetCollection<BsonDocument>("people");
            var list = await col.Find(new BsonDocument())
                //.Skip(1) 
                //.Limit(1)
                //.Sort("{Age: 1}") //Descending Opt 1
                //.Sort(new BsonDocument("Age", 1)) //Descending Opt 2
                //.Sort(new SortDefinitionBuilder ()) //Descending Opt 3
                .Sort(Builders<BsonDocument>.Sort.Ascending("Age").Descending("Name")) //Opt 4
                .ToListAsync();

            foreach (var doc in list)
            {
                Console.WriteLine(doc);
            }
        }

        private static async Task StronglyObjectSchema(IMongoDatabase db)
        {
            Console.WriteLine("\nStrongly Object Schema\n");

            var col = db.GetCollection<Person>("people");
            var list = await col.Find(new BsonDocument())
                //.Sort(Builders<Person>.Sort.Ascending("Age").Descending("Name")) //Opt 1
                //.Sort(Builders<Person>.Sort.Ascending(x => x.Age)) //Opt 2
                .SortBy(x => x.Age)
                .SortByDescending(x => x.Name)//Opt 2
                .ToListAsync();

            foreach (var doc in list)
            {
                Console.WriteLine(doc);
            }
        }

        private static async Task Projection() {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase("test");

            var col = db.GetCollection<Person>("people");
            var list = await col.Find(new BsonDocument())
                //.Project("{ Name : 1, _id : 0}") // Op 1
                //.Project(new BsonDocument("Name", 1).Add("_id", 0)) // Op 2
                //.Project(Builders<Person>.Projection.Include("Name").Exclude("_id")) // Op 3
                //.Project(Builders<Person>.Projection.Include(x => x.Name).Exclude(x => x.Id)) // Op 4
                //.Project(x => x.Name) // Op 5
                .Project(x => new { x.Name, CalcAge = x.Age + 20}) // Op 6
                .ToListAsync();

            foreach (var doc in list)
            {
                Console.WriteLine(doc);
            }
        }

        private static async Task Updates() {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase("test");

            var col = db.GetCollection<BsonDocument>("widgets");

            await db.DropCollectionAsync("widgets"); //Cleaning the collection

            var docs = Enumerable.Range(0, 10).Select(i => new BsonDocument("_id", i).Add("x", i));
            await col.InsertManyAsync(docs);

            //var result = await col.ReplaceOneAsync(
            //    new BsonDocument("_id", 5),
            //    new BsonDocument("_id", 5).Add("x", 30));

            //When the document doesn't exist, don't do anything
            var result = await col.ReplaceOneAsync(
                new BsonDocument("x", 10),
                new BsonDocument("x", 30));

            //When the document doesn't exist, but the upsert is true, the document is created
            var result2 = await col.ReplaceOneAsync(
                new BsonDocument("x", 11),
                new BsonDocument("x", 40),
                new UpdateOptions {IsUpsert = true });

            var result3 = await col.ReplaceOneAsync(
                Builders<BsonDocument>.Filter.Eq("x", 5),
                new BsonDocument("x", 50),
                new UpdateOptions { IsUpsert = true });

            var result4 = await col.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Gt("x", 1),
                //new BsonDocument("$inc", new BsonDocument("x", 10)));
                Builders<BsonDocument>.Update.Inc("x", 10));

            await col.Find(new BsonDocument())
                .ForEachAsync(x => Console.WriteLine(x));
        }
    }
}
