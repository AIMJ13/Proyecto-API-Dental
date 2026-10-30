using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Repositories;

namespace DentalHouse.Test
{
    [TestFixture]
    public class PatientRepositoryTests
    {
        private PatientRepository _repository;
        private IConfiguration _configuration;

        [SetUp]
        public void Setup()
        {

            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            _repository = new PatientRepository(_configuration);
        }

        [Test]
        [Order(1)]
        public async Task PatientRepository_GetAll_DebeRetornarLista()
        {
            // Act
            var response = await _repository.GetAllAsync();

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.OperationStatusCode, Is.EqualTo(0).Or.EqualTo(70015));
            // 0 = hay datos, 70015 = lista vacía
            Console.WriteLine($"Pacientes encontrados: {response.Data?.Count()}");
        }

        [Test]
        [Order(2)]
        public async Task PatientRepository_Create_DebeCrearPaciente()
        {
            // Arrange
            var nuevoPaciente = new Patient
            {
                FirstName = "meylling",
                LastName = "Aleman",
                Phone = "82737829",
                Address = "managua, Nicaragua",
                BirthDate = "2000-04-12",
                State = true
            };

            // Act
            var response = await _repository.CreateAsync(nuevoPaciente);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Data, "El paciente debería haberse creado");
            Assert.That(response.OperationStatusCode, Is.EqualTo(0));
            Console.WriteLine($"Paciente creado: {response.Data.FirstName} {response.Data.LastName}");
        }

        [Test]
        [Order(3)]
        public async Task PatientRepository_GetById_DebeRetornarPaciente()
        {
            // Arrange - usamos el ID 1, 
            int idABuscar = 6;

            // Act
            var response = await _repository.GetByIdAsync(idABuscar);

            // Assert
            Assert.IsNotNull(response);
            Console.WriteLine($"Paciente encontrado: {response.Data?.FirstName} {response.Data?.LastName}");
        }

        [Test]
        [Order(4)]
        public async Task PatientRepository_GetByName_DebeRetornarPaciente()
        {

            var response = await _repository.GetByNameAsync("Maritza");

            // Assert
            Assert.IsNotNull(response);
            Console.WriteLine($"Búsqueda por nombre resultado: {response.Data?.FirstName}");
        }
    }
}