using FeatherAndBeads.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeatherAndBeads.API.Controllers
{
    public class ProductController : BaseApiController
    {
        private readonly Database dataBase;

        public ProductController(Database dBase)
        {
            dataBase = dBase;
        }

        [HttpGet]
        public async Task<ActionResult> GetProducts()
        {
            var products = await dataBase.Product.Where(
                p => p.Removed != true).ToListAsync();

            return Ok(products);
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult> GetProduct(int productId)
        {
            var product = await dataBase.Product.FirstOrDefaultAsync(
                p => p.Id == productId && p.Removed != true);

            if(product != null)
            {
                product.Photos = await dataBase.Photo.Where(p => p.ProductId == product.Id).ToListAsync();
                product.SelectMainPhoto();
            }
            return Ok(product);
        }

        [HttpPost("add-product")]
        public async Task<ActionResult> AddProduct(Product product)
        {
            dataBase.Add(product);
            await dataBase.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("update-product")]
        public async Task UpdateProduct(Product updatedProduct)
        {
            var product = await dataBase.Product.FirstOrDefaultAsync(
                p => p.Id == updatedProduct.Id);

            if (product != null)
            {
                product.Name = updatedProduct.Name;
                product.Description = updatedProduct.Description;
                product.PriceWithoutTax = updatedProduct.PriceWithoutTax;
                product.Tax = updatedProduct.Tax;
                product.Quantity = updatedProduct.Quantity;
                dataBase.SaveChanges();
            }
        }

        [HttpPost("remove-product")]
        public async Task<ActionResult> RemoveProduct(Product product)
        {
            var productToRemove =  await dataBase.Product.FirstOrDefaultAsync(p => p.Id == product.Id);

            if (productToRemove != null)
            {
                productToRemove.Removed = true;
                dataBase.SaveChanges();
            }
            return Ok();
        }

        //[HttpPost("add-photo")]
        //public async Task<ActionResult> AddPhoto(Photo photo)
        //{

        //}
    }
}
