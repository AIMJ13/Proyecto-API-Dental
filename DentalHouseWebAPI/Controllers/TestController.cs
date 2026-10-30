using System;
using Microsoft.AspNetCore.Mvc;

namespace DentalHouseWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                status = "OK",
                message = "API DentalHouse activa.",
                serverTime = DateTime.UtcNow
            });
        }

        [HttpGet("fail")]
        public IActionResult Fail()
        {
            throw new InvalidOperationException("Falla simulada intencional para validacion de auditoria en MongoDB.");
        }
    }
}
