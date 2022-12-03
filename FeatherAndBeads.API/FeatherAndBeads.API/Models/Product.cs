using System.ComponentModel.DataAnnotations.Schema;

namespace FeatherAndBeads.API.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        [NotMapped]
        public Photo? MainPhoto { get; set; }

        public double PriceWithoutTax { get; set; }

        public double Tax { get; set; }

        public int Quantity { get; set; }

        public bool Removed { get; set; }

        public IEnumerable<Photo>? Photos { get; set; }
    }
}
