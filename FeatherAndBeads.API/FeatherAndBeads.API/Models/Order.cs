namespace FeatherAndBeads.API.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public double Cost { get; set; }

        public bool Paid { get; set; }

        public DateTime DeliveredDate { get; set; }

        public int UserId { get; set; }
    }
}
