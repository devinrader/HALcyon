using System.ComponentModel.DataAnnotations;

namespace HALcyon.Tests
{
    [Link("self", "/orders/{id}")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public double Price { get; set; }

    }
}
