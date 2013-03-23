using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HALcyon.Tests
{
    [Link("self", "/customers/{id}")]
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Order> Orders { get; set; }
    }
}
