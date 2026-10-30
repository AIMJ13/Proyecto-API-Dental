using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using WebAPI.DataAccess.Repositories;

namespace DentalHouse.Test
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private UserRepository _repository;
        private IConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            _repository = new UserRepository(_configuration);
        }

        [Test]
        [Order(1)]
        public async Task UserRepository_GetAll_DebeRetornarLista()
        {
            var response = await _repository.GetAllAsync();

            Assert.IsNotNull(response);
            Assert.That(response.OperationStatusCode, Is.EqualTo(0).Or.EqualTo(5053));
            Console.WriteLine($"Usuarios encontrados: {response.Data?.Count()}");
        }

        [Test]
        [Order(2)]
        public async Task UserRepository_GetPaged_DebeRetornarPaginado()
        {
            var response = await _repository.GetPagedAsync(pageNumber: 1, pageSize: 5);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data.PageNumber, Is.EqualTo(1));
            Assert.That(response.Data.PageSize, Is.EqualTo(5));
            Assert.That(response.Data.TotalRecords, Is.GreaterThan(0));
            Assert.That(response.Data.Data.Count(), Is.LessThanOrEqualTo(5));
            Console.WriteLine($"Paginación usuarios: {response.Data.Data.Count()} de {response.Data.TotalRecords} totales en {response.Data.TotalPages} páginas");
        }

        [Test]
        [Order(3)]
        public async Task UserRepository_GetPaged_ConFiltro_DebeFiltrar()
        {
            var response = await _repository.GetPagedAsync(pageNumber: 1, pageSize: 10, searchTerm: "admin");

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data.TotalRecords, Is.GreaterThan(0));
            Console.WriteLine($"Filtro usuarios 'admin': {response.Data.TotalRecords} encontrados");
        }
    }
}
