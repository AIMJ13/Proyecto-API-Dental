using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Repositories;

namespace DentalHouse.Test
{
    [TestFixture]
    public class DoctorRepositoryTests
    {
        private DoctorRepository _repository;
        private IConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            _repository = new DoctorRepository(_configuration);
        }

        [Test]
        [Order(1)]
        public async Task DoctorRepository_GetAll_DebeRetornarLista()
        {
            // Act
            var response = await _repository.GetAllAsync();

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.OperationStatusCode, Is.EqualTo(0).Or.EqualTo(70023));
            Console.WriteLine($"Doctores encontrados: {response.Data?.Count()}");
        }

        [Test]
        [Order(2)]
        public async Task DoctorRepository_Create_DebeCrearDoctor()
        {

            var nuevoDoctor = new Doctor
            {
                FirstName = "Marcos",
                LastName = "sanchez",
                Phone = "5467832121",
                SpecialtyIds = new List<int> { 1, 2 },
                State = true,
                IsActive = true
            };

            // Act
            var response = await _repository.CreateAsync(nuevoDoctor);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Data, "El doctor debería haberse creado");
            Assert.That(response.OperationStatusCode, Is.EqualTo(0));
            Assert.That(response.Data.Specialties.Count, Is.GreaterThanOrEqualTo(2));
            Console.WriteLine($"Doctor creado: {response.Data.FirstName} {response.Data.LastName} con especialidades: {response.Data.SpecialtyName}");
        }

        [Test]
        [Order(3)]
        public async Task DoctorRepository_GetById_DebeRetornarDoctor()
        {
            // Arrange
            int idABuscar = 8;

            // Act
            var response = await _repository.GetByIdAsync(idABuscar);

            // Assert
            Assert.IsNotNull(response);
            Console.WriteLine($"Doctor encontrado: {response.Data?.FirstName} {response.Data?.LastName}");
        }
    }
}