using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DentalHouseWebAPI.Configurations;
using DentalHouseWebAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using WebAPI.Core.Common;

namespace DentalHouseWebAPI.Services
{
    public class MongoLogService : IMongoLogService
    {
        private readonly IMongoCollection<MongoSystemLog>? _collection;

        public MongoLogService(IOptions<MongoLogSettings> options)
        {
            var settings = options.Value;
            if (string.IsNullOrWhiteSpace(settings.ConnectionString) || settings.ConnectionString.Contains("<DB_PASSWORD>"))
            {
                return;
            }

            try
            {
                var client = new MongoClient(settings.ConnectionString);
                var database = client.GetDatabase(settings.DatabaseName);
                _collection = database.GetCollection<MongoSystemLog>(settings.CollectionName);
                InicializarIndices(settings.RetentionDays);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] Error al conectar con MongoDB: {ex.Message}");
            }
        }

        private void InicializarIndices(int retentionDays)
        {
            if (_collection == null) return;
            try
            {
                var ttlKeys = Builders<MongoSystemLog>.IndexKeys.Ascending(x => x.Timestamp);
                var ttlOptions = new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.FromDays(retentionDays)
                };

                var traceKeys = Builders<MongoSystemLog>.IndexKeys.Ascending(x => x.TraceId);
                var levelKeys = Builders<MongoSystemLog>.IndexKeys.Ascending(x => x.Level);

                _collection.Indexes.CreateMany(new[]
                {
                    new CreateIndexModel<MongoSystemLog>(ttlKeys, ttlOptions),
                    new CreateIndexModel<MongoSystemLog>(traceKeys),
                    new CreateIndexModel<MongoSystemLog>(levelKeys)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] Fallo al inicializar indices en MongoDB: {ex.Message}");
            }
        }

        public async Task InsertLogAsync(MongoSystemLog log)
        {
            if (_collection == null) return;
            try
            {
                await _collection.InsertOneAsync(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] Fallo de persistencia en MongoDB: {ex.Message}");
            }
        }

        public async Task<PagedResponse<MongoSystemLog>> GetPagedLogsAsync(
            int pageNumber,
            int pageSize,
            string? module = null,
            string? action = null,
            string? userName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            bool? isSuccess = null)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            if (_collection == null)
            {
                return new PagedResponse<MongoSystemLog>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = 0,
                    TotalPages = 1,
                    Data = new List<MongoSystemLog>()
                };
            }

            try
            {
                var builder = Builders<MongoSystemLog>.Filter;
                var filter = builder.Empty;

                if (startDate.HasValue)
                {
                    filter &= builder.Gte(x => x.Timestamp, startDate.Value.ToUniversalTime());
                }

                if (endDate.HasValue)
                {
                    var end = endDate.Value.TimeOfDay == TimeSpan.Zero
                        ? endDate.Value.Date.AddDays(1).AddTicks(-1)
                        : endDate.Value;
                    filter &= builder.Lte(x => x.Timestamp, end.ToUniversalTime());
                }

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    var userRegex = new BsonRegularExpression(Regex.Escape(userName.Trim()), "i");
                    filter &= builder.Regex(x => x.UserId, userRegex);
                }

                if (!string.IsNullOrWhiteSpace(action))
                {
                    var actTrimmed = action.Trim();
                    var actRegex = new BsonRegularExpression($"^{Regex.Escape(actTrimmed)}$", "i");
                    filter &= builder.Regex("request.method", actRegex);
                }

                if (!string.IsNullOrWhiteSpace(module))
                {
                    var modulePattern = MapModuleToPathPattern(module.Trim());
                    var modRegex = new BsonRegularExpression(modulePattern, "i");
                    filter &= builder.Regex("request.path", modRegex);
                }

                if (isSuccess.HasValue)
                {
                    if (isSuccess.Value)
                    {
                        filter &= builder.Lt("response.statusCode", 400) & builder.Ne(x => x.Level, "Error");
                    }
                    else
                    {
                        filter &= builder.Or(
                            builder.Gte("response.statusCode", 400),
                            builder.Eq(x => x.Level, "Error")
                        );
                    }
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchRegex = new BsonRegularExpression(Regex.Escape(searchTerm.Trim()), "i");
                    var searchFilter = builder.Or(
                        builder.Regex("request.path", searchRegex),
                        builder.Regex(x => x.UserId, searchRegex),
                        builder.Regex(x => x.ClientIp, searchRegex),
                        builder.Regex(x => x.TraceId, searchRegex),
                        builder.Regex("exception.message", searchRegex)
                    );
                    filter &= searchFilter;
                }

                var totalRecords = (int)await _collection.CountDocumentsAsync(filter);
                var totalPages = totalRecords > 0 ? (int)Math.Ceiling((double)totalRecords / pageSize) : 1;
                var skip = (pageNumber - 1) * pageSize;

                var data = await _collection.Find(filter)
                    .SortByDescending(x => x.Timestamp)
                    .Skip(skip)
                    .Limit(pageSize)
                    .ToListAsync();

                return new PagedResponse<MongoSystemLog>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] Error al consultar logs en MongoDB: {ex.Message}");
                return new PagedResponse<MongoSystemLog>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = 0,
                    TotalPages = 1,
                    Data = new List<MongoSystemLog>()
                };
            }
        }

        public async Task<MongoSystemLog?> GetLogByIdAsync(string id)
        {
            if (_collection == null || string.IsNullOrWhiteSpace(id)) return null;
            try
            {
                var builder = Builders<MongoSystemLog>.Filter;
                FilterDefinition<MongoSystemLog> filter;
                if (ObjectId.TryParse(id, out var objectId))
                {
                    filter = builder.Or(
                        builder.Eq(x => x.Id, id),
                        builder.Eq(x => x.TraceId, id)
                    );
                }
                else
                {
                    filter = builder.Eq(x => x.TraceId, id);
                }

                return await _collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] Error al buscar log por Id en MongoDB: {ex.Message}");
                return null;
            }
        }

        public async Task<List<string>> GetModulesAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "Autenticación",
                "Pacientes",
                "Citas",
                "Doctores",
                "Facturación",
                "Usuarios",
                "Especialidades",
                "Servicios",
                "Analíticas",
                "Logs",
                "General"
            });
        }

        public async Task<List<string>> GetActionsAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "GET",
                "POST",
                "PUT",
                "DELETE"
            });
        }

        private static string MapModuleToPathPattern(string module)
        {
            var m = module.ToLowerInvariant();
            if (m.Contains("auth") || m.Contains("autentica") || m.Contains("login")) return "/auth";
            if (m.Contains("pacient") || m.Contains("patient")) return "/patient";
            if (m.Contains("cita") || m.Contains("appoint")) return "/appoint|/cita";
            if (m.Contains("doctor")) return "/doctor";
            if (m.Contains("factur") || m.Contains("invoice")) return "/invoice";
            if (m.Contains("venta") || m.Contains("sale")) return "/sale";
            if (m.Contains("usuari") || m.Contains("user")) return "/user";
            if (m.Contains("rol") || m.Contains("role")) return "/role";
            if (m.Contains("especial") || m.Contains("special")) return "/specialt";
            if (m.Contains("servic")) return "/service";
            if (m.Contains("analit") || m.Contains("analytic")) return "/analytic";
            if (m.Contains("log")) return "/log";
            return Regex.Escape(module);
        }
    }
}
