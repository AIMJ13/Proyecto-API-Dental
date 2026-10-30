using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DentalHouseWebAPI.Models
{
    [BsonIgnoreExtraElements]
    public class MongoSystemLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("traceId")]
        public string TraceId { get; set; } = string.Empty;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [BsonElement("level")]
        public string Level { get; set; } = "Information";

        [BsonElement("environment")]
        public string Environment { get; set; } = "Development";

        [BsonElement("clientIp")]
        public string? ClientIp { get; set; }

        [BsonElement("userId")]
        public string? UserId { get; set; }

        [BsonElement("application")]
        public string Application { get; set; } = "DentalHouseBackend";

        [BsonElement("client_platform")]
        public string ClientPlatform { get; set; } = "Web";

        [BsonElement("device_id")]
        public string? DeviceId { get; set; }

        [BsonElement("user_role")]
        public string? UserRole { get; set; }

        [BsonElement("request")]
        public MongoRequestDetail Request { get; set; } = new();

        [BsonElement("response")]
        public MongoResponseDetail Response { get; set; } = new();

        [BsonElement("exception")]
        [BsonIgnoreIfNull]
        public MongoExceptionDetail? Exception { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class MongoRequestDetail
    {
        [BsonElement("method")]
        public string Method { get; set; } = string.Empty;

        [BsonElement("path")]
        public string Path { get; set; } = string.Empty;

        [BsonElement("queryString")]
        public string? QueryString { get; set; }

        [BsonElement("payload")]
        public string? Payload { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class MongoResponseDetail
    {
        [BsonElement("statusCode")]
        public int StatusCode { get; set; }

        [BsonElement("elapsedMilliseconds")]
        public long ElapsedMilliseconds { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class MongoExceptionDetail
    {
        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("stackTrace")]
        public string? StackTrace { get; set; }

        [BsonElement("source")]
        public string Source { get; set; } = string.Empty;
    }
}
