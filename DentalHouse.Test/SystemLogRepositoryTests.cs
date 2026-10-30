using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Repositories;

namespace DentalHouse.Test
{
    [TestFixture]
    public class SystemLogRepositoryTests
    {
        private SystemLogRepository _repository = null!;
        private IConfiguration _configuration = null!;
        private static int _createdLogId;

        [SetUp]
        public void Setup()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _repository = new SystemLogRepository(_configuration);
        }

        [Test]
        [Order(1)]
        public async Task SystemLogRepository_InsertAsync_DebeInsertarLogYRetornarId()
        {
            // Arrange
            var log = new SystemLog
            {
                Timestamp = DateTime.Now,
                UserId = 1,
                UserName = "TestUser",
                UserRole = "Administrador",
                Action = "TEST_ACTION",
                Module = "TestModule",
                EntityId = "999",
                Description = "Prueba unitaria de inserción de log del sistema",
                Details = "{\"test\": true}",
                IpAddress = "127.0.0.1",
                IsSuccess = true
            };

            // Act
            var response = await _repository.InsertAsync(log);

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.OperationStatusCode, Is.EqualTo(0));
            Assert.That(response.Data, Is.GreaterThan(0));

            _createdLogId = response.Data;
            Console.WriteLine($"Log insertado con ID: {_createdLogId}");
        }

        [Test]
        [Order(2)]
        public async Task SystemLogRepository_GetByIdAsync_DebeRetornarLogCreado()
        {
            // Act
            var response = await _repository.GetByIdAsync(_createdLogId);

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.OperationStatusCode, Is.EqualTo(0));
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data!.LogId, Is.EqualTo(_createdLogId));
            Assert.That(response.Data.Module, Is.EqualTo("TestModule"));
            Assert.That(response.Data.Action, Is.EqualTo("TEST_ACTION"));
            Assert.That(response.Data.UserName, Is.EqualTo("TestUser"));
        }

        [Test]
        [Order(3)]
        public async Task SystemLogRepository_GetPagedAsync_DebeRetornarListaPaginada()
        {
            // Act
            var response = await _repository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 10,
                module: null,
                action: null,
                userName: null,
                startDate: null,
                endDate: null,
                searchTerm: null,
                isSuccess: null);

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.OperationStatusCode, Is.EqualTo(0));
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data!.TotalRecords, Is.GreaterThanOrEqualTo(1));
            Assert.That(response.Data.Data, Is.Not.Null);
            Console.WriteLine($"Total logs encontrados: {response.Data.TotalRecords}");
        }

        [Test]
        [Order(4)]
        public async Task SystemLogRepository_GetPagedAsync_FiltroPorModulo_DebeRetornarCoincidencias()
        {
            // Act
            var response = await _repository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 10,
                module: "TestModule",
                action: null,
                userName: null,
                startDate: null,
                endDate: null,
                searchTerm: null,
                isSuccess: null);

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.OperationStatusCode, Is.EqualTo(0));
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data!.Data.All(l => l.Module == "TestModule"), Is.True);
        }

        [Test]
        [Order(5)]
        public async Task SystemLogRepository_GetModulesAsync_DebeRetornarModulos()
        {
            // Act
            var response = await _repository.GetModulesAsync();

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.OperationStatusCode, Is.EqualTo(0));
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data!.Contains("TestModule"), Is.True);
            Console.WriteLine($"Módulos encontrados: {string.Join(", ", response.Data!)}");
        }

        [Test]
        [Order(6)]
        public async Task SystemLogRepository_GetActionsAsync_DebeRetornarAcciones()
        {
            // Act
            var response = await _repository.GetActionsAsync();

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.OperationStatusCode, Is.EqualTo(0));
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data!.Contains("TEST_ACTION"), Is.True);
            Console.WriteLine($"Acciones encontradas: {string.Join(", ", response.Data!)}");
        }
    }
}
