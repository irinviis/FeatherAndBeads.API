using System.ComponentModel.DataAnnotations.Schema;

namespace FeatherAndBeads.API.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? ShortDescription { get; set; }

        public string? LongDescription { get; set; }

        [NotMapped]
        public Photo? MainPhoto { get; set; }

        public double PriceWithTax { get; set; }

        public double PriceWithoutTax { get; set; }

        public double Tax { get; set; }

        [NotMapped]
        public int CartQuantity { get; set; } = 1;

        public int Quantity { get; set; }

        public bool Removed { get; set; }

        [NotMapped]
        public List<int> ProductCategories { get; set; } = new List<int>();

        public IEnumerable<Photo>? Photos { get; set; }


        public void SelectMainPhoto()
        {
            if (Photos != null && Photos.Count() > 0)
            {
                var userMainPhoto = Photos.Where(p => p.IsMain == true).FirstOrDefault();
                if (userMainPhoto != null)
                {
                    MainPhoto = userMainPhoto;
                }
                else
                {
                    MainPhoto = Photos.FirstOrDefault();
                }
            }
        }
    }
}
