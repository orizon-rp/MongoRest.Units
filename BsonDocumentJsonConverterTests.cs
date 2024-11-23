using System.Text.Json;
using MongoDB.Bson;
using MongoRest.Converters;

namespace MongoRest.Units;

[TestFixture]
public class BsonDocumentJsonConverterTests
{
    private BsonDocumentJsonConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new BsonDocumentJsonConverter();
    }

    [Test]
    public void CanDeserializeJsonToBsonDocument()
    {
        const string json = "{\"Name\":\"John Doe\",\"Age\":30,\"Occupation\":\"Developer\"}";

        var bsonDocument = JsonSerializer.Deserialize<BsonDocument>(json, new JsonSerializerOptions
        {
            Converters = { _converter }
        });

        Assert.That(bsonDocument, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(bsonDocument["Name"].AsString, Is.EqualTo("John Doe"));
            Assert.That(bsonDocument["Age"].AsInt32, Is.EqualTo(30));
            Assert.That(bsonDocument["Occupation"].AsString, Is.EqualTo("Developer"));
        });
    }

    [Test]
    public void CanSerializeBsonDocumentToJson()
    {
        var bsonDocument = new BsonDocument
        {
            { "Name", "Alice" },
            { "Age", 28 },
            { "Country", "Wonderland" }
        };

        var json = JsonSerializer.Serialize(bsonDocument, new JsonSerializerOptions
        {
            Converters = { _converter }
        });

        Assert.That(json, Is.Not.Null);
        Assert.That(json, Does.Contain("\"Name\":\"Alice\""));
        Assert.That(json, Does.Contain("\"Age\":28"));
        Assert.That(json, Does.Contain("\"Country\":\"Wonderland\""));
    }
}