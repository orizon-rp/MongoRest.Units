using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Bson;
using MongoRest.Converters;

namespace MongoRest.Units;

[TestFixture]
public class CrudControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    private const string DocumentId = "6741b5d0f12b1561701b9688";

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test, Order(0)]
    public async Task CreateDocument_ReturnsSuccess()
    {
        const string collectionName = "users";
        const string url = $"{Constants.APIRootPath}/collections/{collectionName}/create";

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
}