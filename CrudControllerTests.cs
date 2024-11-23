using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoRest.Converters;

namespace MongoRest.Units;

[TestFixture]
public class CrudControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IMongoClient _mongoClient;
    private IMongoDatabase _database;

    private const string ConnectionString = "mongodb://localhost:27017";
    private const string TestCollectionName = "tests";
    private const string DocumentId = "6741b5d0f12b1561701b9688";

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();

        _mongoClient = new MongoClient(ConnectionString);
        _database = _mongoClient.GetDatabase(TestCollectionName);
    }

    [TearDown]
    public void TearDown()
    {
        _database.DropCollection(TestCollectionName);

        _client.Dispose();
        _factory.Dispose();
        _mongoClient.Dispose();
    }

    [Test, Order(0)]
    public async Task CreateDocument_ReturnsSuccess()
    {
        const string url = $"{Constants.APIRootPath}/collections/{TestCollectionName}/create";

        var bsonDocument = new BsonDocument
        {
            { "_id", DocumentId },
            { "Name", "Alice" },
            { "Age", 28 },
            { "Country", "Wonderland" }
        };

        var json = JsonSerializer.Serialize(bsonDocument, new JsonSerializerOptions
        {
            Converters = { new BsonDocumentJsonConverter() }
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        Assert.That(responseString, Does.Contain("Document created successfully."));
    }

    [Test, Order(1)]
    public async Task GetDocument_ReturnsDocument()
    {
        const string url = $"{Constants.APIRootPath}/collections/{TestCollectionName}/get/{DocumentId}";

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.That(responseString, Does.Contain("Name"));
        Assert.That(responseString, Does.Contain("Alice"));
    }
}