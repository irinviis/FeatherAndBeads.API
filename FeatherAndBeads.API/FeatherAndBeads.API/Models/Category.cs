namespace FeatherAndBeads.API.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public bool? Disabled { get; set; }
        public bool? Removed { get; set; }
        public string? Link { get; set; }
        public Photo? Photo { get; set; }
    }
}
