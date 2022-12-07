using FeatherAndBeads.API.Interfaces;
using FeatherAndBeads.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeatherAndBeads.API.Controllers
{
    public class ProductController : BaseApiController
    {
        private readonly Database database;
        private readonly IPhotoService photoService;

        public ProductController(Database dBase, IPhotoService pService)
        {
            database = dBase;
            photoService = pService;
        }


        [HttpGet("GetProductsForCategory")]
        public async Task<ActionResult> GetProductsForCategory(int categoryId)
        {
            var productIdsOfCategory = await database.ProductCategory.Where(pc => pc.CategoryId == categoryId).Select(pc =>pc.ProductId).ToListAsync();
            var products = await database.Product.Where(p => productIdsOfCategory.Contains(p.Id)).ToListAsync();

            return Ok(products);
        }


        [HttpGet("GetCategories")]
        public async Task<ActionResult> GetCategories()
        {
            var categories = await database.Category.Where(c => c.Removed != true && c.Disabled != true).ToListAsync();
            return Ok(categories);
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult> GetProduct(int productId)
        {
            var product = await database.Product.FirstOrDefaultAsync(
                p => p.Id == productId && p.Removed != true);

            if(product != null)
            {
                product.Photos = await database.Photo.Where(p => p.ProductId == product.Id).ToListAsync();
                product.SelectMainPhoto();
            }
            return Ok(product);
        }

        [HttpPost("add-product")]
        public async Task<ActionResult> AddProduct(Product product)
        {
            database.Add(product);
            await database.SaveChangesAsync();

            if (product.CategoryId > 0)
            {
                var category = await database.Category.FirstOrDefaultAsync(c => c.Id == product.CategoryId);
                if (category != null)
                {
                    var productCategory = new ProductCategory
                    {
                        CategoryId = category.Id,
                        ProductId = product.Id
                    };

                    database.Add(productCategory);
                    await database.SaveChangesAsync();
                }
            }
            return Ok();
        }

        [HttpPost("update-product")]
        public async Task UpdateProduct(Product updatedProduct)
        {
            var product = await database.Product.FirstOrDefaultAsync(
                p => p.Id == updatedProduct.Id);

            if (product != null)
            {
                product.Name = updatedProduct.Name;
                product.Description = updatedProduct.Description;
                product.PriceWithoutTax = updatedProduct.PriceWithoutTax;
                product.Tax = updatedProduct.Tax;
                product.Quantity = updatedProduct.Quantity;
                database.SaveChanges();
            }
        }

        [HttpPost("remove-product")]
        public async Task<ActionResult> RemoveProduct(Product product)
        {
            var productToRemove =  await database.Product.FirstOrDefaultAsync(p => p.Id == product.Id);

            if (productToRemove != null)
            {
                productToRemove.Removed = true;
                database.SaveChanges();
            }
            return Ok();
        }

        //Private Function SetProductMainPhoto
        private async Task SetProductMainPhoto(int productId, int photoId = 0)
        {
            var productPhotos = await database.Photo.Where(
                p => p.ProductId == productId).ToListAsync();
            if (productPhotos.Any())
            {
                if (photoId == 0)
                {
                    photoId = productPhotos.First().Id;
                }

                foreach (var photo in productPhotos)
                {
                    photo.IsMain = false;
                    if (photo.Id == photoId)
                    {
                        photo.IsMain = true;
                    }
                }
                database.SaveChanges();
            }
        }


        [HttpPost("add-photo")]
        public async Task<ActionResult> AddPhoto(IFormFile file, int productId)
        {
            var result = await photoService.AddPhotoAsync(file);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }

            var productPhoto = new Photo()
            {
                ProductId = productId,
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                IsMain = false
            };

            database.Photo.Add(productPhoto);
            await database.SaveChangesAsync();

            var productHasMainPhoto = await database.Photo.Where(
                p => p.ProductId == productId && p.IsMain == true).AnyAsync();
            if (!productHasMainPhoto)
            {
                await SetProductMainPhoto(productId);
            }
            return Ok();
        }

        [HttpPost("set-main-photo")]
        public async Task<ActionResult> SetMainPhoto(int photoId, int productId)
        {
            await SetProductMainPhoto(photoId, productId);
            return Ok();
        }

        [HttpPost("remove-photo")]
        public async Task<ActionResult> RemovePhoto(int photoId)
        {
            var photoToRemove = await database.Photo.FirstOrDefaultAsync(
                p => p.Id == photoId);
            if (photoToRemove != null)
            {
                database.Photo.Remove(photoToRemove);
                database.SaveChanges();

                SetProductMainPhoto(photoToRemove.ProductId);
            }
            return Ok();
        }
    }
}
