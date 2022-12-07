using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FeatherAndBeads.API.Models
{
    public class User
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        [JsonIgnore]
        public byte[]? PasswordHash { get; set; }

        [JsonIgnore]
        public byte[]? PasswordSalt { get; set; }

        [NotMapped]
        public string? Password { get; set; }

        [NotMapped]
        public string? Token { get; set; }

        public string? StreetAddress { get; set; }

        public string? PostCode { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public string? Mobile { get; set; }

        public bool Removed { get; set; }

        public IdentityUser GetIdentity()
        {
            return new IdentityUser() {
                UserName = $"{FirstName} {LastName}",
                Email = Email
            };
        }
    }
}
