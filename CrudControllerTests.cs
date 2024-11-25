﻿using System.Net;
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

    private async Task InsertDocument(HttpClient client, BsonDocument bsonDocument)
    {
        const string url = $"{CollectionUrl}/insert";

        var array = new[] { bsonDocument };
        var json = JsonSerializer.Serialize(array, _options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            StatusCode_Ok(response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            Assert.Fail(ex.Message);
        }
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
        var bsonDocument1 = new BsonDocument
        {
            { "_id", DocumentId },
            { "Name", "Alice" },
            { "Age", 28 },
            { "Country", "Wonderland" }
        };

        await InsertDocument(_client, bsonDocument1);
        
        var bsonDocument2 = new BsonDocument
        {
            { "_id", "6741b5d0f12b1561701b9689" },
            { "Name", "John" },
            { "Age", 32 },
            { "Country", "Wonderland" }
        };

        await InsertDocument(_client,bsonDocument2);
        
        var bsonDocument3 = new BsonDocument
        {
            { "_id", "6741b5d0f12b1561701b968a" },
            { "Name", "Marry" },
            { "Age", 14 },
            { "Country", "Mongoland" }
        };

        await InsertDocument(_client,bsonDocument3);
    }

    [Test, Order(5)]
    public async Task DeleteAllDocumentsByFilter_ReturnsDocument()
    {
        const string url = $"{CollectionUrl}/delete";

        try
        {
            var bsonDocument = new BsonDocument
            {
                { "Name", "John" },
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
    
    [Test, Order(6)]
    public async Task CountAllDocumentsByFilter_ReturnsDocument()
    {
        const string url = $"{CollectionUrl}/count";

        try
        {
            var bsonDocument = new BsonDocument
            {
                { "Name", "Marry" }
            };
            
            var json = JsonSerializer.Serialize(bsonDocument, _options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            StatusCode_Ok(response.StatusCode);
            
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.That(responseString, Does.Contain("1"));
        }
        catch (HttpRequestException ex)
        {
            Assert.Fail(ex.Message);
        }
    }
    
    [Test, Order(7)]
    public async Task DeleteAllDocuments_ReturnsDocument()
    {
        const string url = $"{CollectionUrl}/delete";

        try
        {
            var bsonDocument = new BsonDocument();

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