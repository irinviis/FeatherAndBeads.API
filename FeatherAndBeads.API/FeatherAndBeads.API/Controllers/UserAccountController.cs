using FeatherAndBeads.API.Interfaces;
using FeatherAndBeads.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FeatherAndBeads.API.Controllers
{
    public class UserAccountController : BaseApiController
    {
        private readonly Database database;
        private readonly ITokenService tokenService;

        public UserAccountController(Database dBase, ITokenService tokenService)
        {
            database = dBase;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User userModel)
        {
            if(await database.User.AnyAsync(x => x.Email == userModel.Email))
            {
                return BadRequest("Email is taken.");
            }

            using var hmac = new HMACSHA512();
            userModel.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userModel.Password));
            userModel.PasswordSalt = hmac.Key;

            database.Add(userModel);
            await database.SaveChangesAsync();

            return Ok(new User
            {
                Email = userModel.Email,
                Token = tokenService.CreateToken(userModel)
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(User userModel)
        {
            var user = await database.User.FirstOrDefaultAsync(
                u => u.Email == userModel.Email && u.Removed != true);

            if(user != null)
            {
                using var hmac = new HMACSHA512(user.PasswordSalt);
                var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userModel.Password));

                for(int i = 0; i < passwordHash.Length; i++)
                {
                    if (user.PasswordHash[i] != passwordHash[i])
                    {
                        return Unauthorized("Invalid username or password");
                    }
                }
                return new User
                {
                    Id = user.Id,
                    Email = user.Email,
                    Token = tokenService.CreateToken(user)
                };
            }
            else
            {
                return Unauthorized("Invalid username or password");
            }
        }

        [HttpPost("update-user")]
        public async Task UpdateUser(User updatedUser)
        {
            var user = await database.User.FirstOrDefaultAsync(
                u => u.Id == updatedUser.Id);
            if (user != null)
            {
                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Email = updatedUser.Email;
                user.Mobile = updatedUser.Mobile;
                user.StreetAddress = updatedUser.StreetAddress;
                user.PostCode = updatedUser.PostCode;
                user.City = updatedUser.City;
                user.Country = updatedUser.Country;
                database.SaveChanges();
            }
        }

        [HttpPost("remove-user")]
        public async Task<ActionResult> RemoveUser(User user)
        {
            var userToRemove = await database.User.FirstOrDefaultAsync(u => u.Id == user.Id);

            if(userToRemove != null)
            {
                userToRemove.Removed = true;
                database.SaveChanges();
            }
            return Ok();
        }
    }
}
