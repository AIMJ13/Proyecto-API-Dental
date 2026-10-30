using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class CreatePatientDto
    {
        public required string FirstName {  get; set; }
        public required string LastName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public required string BirthDate { get; set; }  
        public bool State {  get; set; }
    }
}
