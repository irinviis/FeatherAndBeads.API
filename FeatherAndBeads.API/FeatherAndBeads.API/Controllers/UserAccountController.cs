using FeatherAndBeads.API.Interfaces;
using FeatherAndBeads.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FeatherAndBeads.API.Controllers
{
    public class UserAccountController : BaseApiController
    {
        private readonly Database _database;
        private readonly ITokenService _tokenService;
        private SignInManager<IdentityUser> _signInManager;

        public UserAccountController(Database database, ITokenService tokenService)
        {
            _database = database;
            _tokenService = tokenService;
            //_signInManager = signInManager;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult> GetUser(int userId)
        {
            var user = await _database.User.FirstOrDefaultAsync(
                u => u.Id == userId && u.Removed != true);

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User userModel)
        {
            if(await _database.User.AnyAsync(x => x.Email == userModel.Email))
            {
                return BadRequest("Email is taken.");
            }

            using var hmac = new HMACSHA512();
            userModel.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userModel.Password));
            userModel.PasswordSalt = hmac.Key;

            _database.Add(userModel);
            await _database.SaveChangesAsync();

            return Ok(new User
            {
                Email = userModel.Email,
                Token = _tokenService.CreateToken(userModel)
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(User userModel)
        {
            var user = await _database.User.FirstOrDefaultAsync(
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

                //await _signInManager.SignInAsync(user.GetIdentity(), true);


                return new User
                {
                    Id = user.Id,
                    Email = user.Email,
                    Token = _tokenService.CreateToken(user)
                };
            }
            else
            {
                return Unauthorized("Invalid username or password");
            }
        }


        [Authorize]
        [HttpGet("getLoggedInUser")]
        public ActionResult GetLoggedInUser()
        {
            var r = Request;
            var p = _signInManager.UserManager;
            return Ok("Na derzhi");
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost("update-user")]
        public async Task UpdateUser(User updatedUser)
        {
            var user = await _database.User.FirstOrDefaultAsync(
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
                _database.SaveChanges();
            }
        }

        [HttpPost("remove-user")]
        public async Task<ActionResult> RemoveUser(User user)
        {
            var userToRemove = await _database.User.FirstOrDefaultAsync(u => u.Id == user.Id);

            if(userToRemove != null)
            {
                userToRemove.Removed = true;
                _database.SaveChanges();
            }
            return Ok();
        }
    }
}
