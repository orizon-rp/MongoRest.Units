using System.Net;
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

    private const string TestCollectionName = "tests";
    private const string DocumentId = "6741b5d0f12b1561701b9688";
    private const string CollectionUrl = $"{Constants.APIRootPath}/collections/{TestCollectionName}";

    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new BsonDocumentJsonConverter() }
    };

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

    private static void StatusCode_Ok(HttpStatusCode statusCodes)
    {
        Assert.That(statusCodes, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test, Order(0)]
    public async Task DeleteDocumentById_ReturnsSuccess()
    {
        const string deleteUrl = $"{CollectionUrl}/delete/{DocumentId}";

        var deleteResponse = await _client.DeleteAsync(deleteUrl);

        try
        {
            deleteResponse.EnsureSuccessStatusCode();

            StatusCode_Ok(deleteResponse.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }

    [Test, Order(1)]
    public async Task CreateDocument_ReturnsSuccess()
    {
        const string url = $"{CollectionUrl}/create";

        var bsonDocument = new BsonDocument
        {
            { "_id", DocumentId },
            { "Name", "Alice" },
            { "Age", 28 },
            { "Country", "Wonderland" }
        };

        var json = JsonSerializer.Serialize(bsonDocument, _options);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            StatusCode_Ok(response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test, Order(2)]
    public async Task GetDocument_ReturnsDocument()
    {
        const string url = $"{CollectionUrl}/get?id={DocumentId}";

        try
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            StatusCode_Ok(response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();

            Assert.That(responseString, Does.Contain("Name"));
            Assert.That(responseString, Does.Contain("Alice"));
        }
        catch (HttpRequestException ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test, Order(3)]
    public async Task DeleteDocumentByFilter_ReturnsDocument()
    {
        const string url = $"{CollectionUrl}/delete";

        try
        {
            var bsonDocument = new BsonDocument
            {
                { "Name", "Alice" },
                { "Country", "Wonderland" }
            };

            var json = JsonSerializer.Serialize(bsonDocument, _options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            StatusCode_Ok(response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}