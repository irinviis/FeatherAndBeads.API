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


        [HttpGet("GetCategories")]
        public async Task<ActionResult> GetCategories()
        {
            var categories = await database.Category.Where(
                c => c.Removed != true && c.Disabled != true).ToListAsync();

            foreach (var category in categories)
            {
                category.Photo = database.Photo.FirstOrDefault(
                    p => p.CategoryId == category.Id);
            }

            return Ok(categories);
        }

        [HttpGet("GetCategoryByLinkName")]
        public async Task<ActionResult> GetCategoryByLinkName(string linkName)
        {
            var category = await database.Category.Where(
                c => c.Removed != true && c.Disabled != true &&
                c.Link == linkName).FirstOrDefaultAsync();

            return Ok(category);
        }


        [HttpGet("GetCategory")]
        public async Task<ActionResult> GetCategory(int categoryId)
        {
            var category = await database.Category.FirstOrDefaultAsync(
                c => c.Id == categoryId && c.Removed != true && c.Disabled != true);

            if (category != null)
            {
                category.Photo = await database.Photo.FirstOrDefaultAsync(
                    p => p.CategoryId == categoryId);
            }
            return Ok(category);
        }


        [HttpPost("add-category")]
        public async Task<ActionResult> AddCategory(Category category)
        {
            database.Add(category);
            await database.SaveChangesAsync();

            return Ok(category);
        }

        [HttpPost("update-category")]
        public async Task UpdateCategory(Category updatedCategory)
        {
            var category = await database.Category.FirstOrDefaultAsync(c => c.Id == updatedCategory.Id);

            if (category != null)
            {
                category.CategoryName = updatedCategory.CategoryName;
                category.Link = updatedCategory.Link;

                database.SaveChanges();
            }
        }


        [HttpPost("upload-category-photo")]
        public async Task<ActionResult> UploadCategoryPhoto(int categoryId, IFormFile photo)
        {
            var result = await photoService.AddPhotoAsync(photo);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }


            //Removing old photo
            var oldPhoto = await database.Photo.Where(p => p.CategoryId == categoryId).FirstOrDefaultAsync();
            if (oldPhoto != null)
            {
                database.Photo.Remove(oldPhoto);
                if (!string.IsNullOrWhiteSpace(oldPhoto.PublicId))
                {
                    await photoService.DeletePhotoAsync(oldPhoto.PublicId);
                }
            }


            //Adding new photo
            var categoryPhoto = new Photo()
            {
                CategoryId = categoryId,
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            database.Photo.Add(categoryPhoto);
            await database.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("remove-category-photo")]
        public async Task<ActionResult> RemoveCategoryPhoto(int photoId)
        {
            var photoToRemove = await database.Photo.FirstOrDefaultAsync(p => p.Id == photoId);
            if (photoToRemove != null && photoToRemove.CategoryId > 0)
            {
                if (!string.IsNullOrWhiteSpace(photoToRemove.PublicId))
                {
                    await photoService.DeletePhotoAsync(photoToRemove.PublicId);
                }

                database.Photo.Remove(photoToRemove);
                await database.SaveChangesAsync();
            }
            return Ok();
        }


        [HttpPost("remove-category")]
        public async Task<ActionResult> RemoveCategory(Category category)
        {
            var categoryToRemove = await database.Category.FirstOrDefaultAsync(c => c.Id == category.Id);

            if (categoryToRemove != null)
            {
                categoryToRemove.Removed = true;
                database.SaveChanges();
            }
            return Ok();
        }



        [HttpGet("GetProductsForCategory")]
        public async Task<ActionResult> GetProductsForCategory(int categoryId)
        {
            var productIdsOfCategory = await database.ProductCategory.Where(
                pc => pc.CategoryId == categoryId).Select(pc => pc.ProductId).ToListAsync();

            var products = await database.Product.Where(
                p => productIdsOfCategory.Contains(p.Id) && p.Quantity > 0 && p.Removed != true).ToListAsync();


            var productMainPhotos = await database.Photo.Where(
                f => f.IsMain == true && f.ProductId > 0 && productIdsOfCategory.Contains((int)f.ProductId)).ToListAsync();

            foreach (var product in products)
            {
                product.MainPhoto = productMainPhotos.Where(f => f.ProductId == product.Id).FirstOrDefault(p => p.IsMain == true);
            }

            return Ok(products);
        }


        [HttpGet("GetProducts")]
        public async Task<ActionResult> GetProducts()
        {
            var products = await database.Product.ToListAsync();

            var productIds = products.Select(p => p.Id).ToList();


            var productCategories = await database.ProductCategory.ToListAsync();
            var photos = await database.Photo.Where(f => f.ProductId > 0 && productIds.Contains((int)f.ProductId)).ToListAsync();

            foreach (var product in products)
            {
                product.ProductCategories = productCategories.Where(
                    c => c.ProductId == product.Id).Select(c => c.CategoryId).ToList();

                product.Photos = photos.Where(f => f.ProductId == product.Id).ToList();
                product.SelectMainPhoto();
            }
            return Ok(products);
        }


        [HttpGet("{productId}")]
        public async Task<ActionResult> GetProduct(int productId, bool includeRemoved = false)
        {
            var product = await database.Product.FirstOrDefaultAsync(
                p => p.Id == productId && p.Removed == includeRemoved);

            if (product != null)
            {
                product.ProductCategories = await database.ProductCategory.Where(
                    pc => pc.ProductId == product.Id).Select(pc => pc.CategoryId).ToListAsync();

                product.Photos = await database.Photo.Where(
                    p => p.ProductId == product.Id).ToListAsync();
                product.SelectMainPhoto();
            }
            return Ok(product);
        }


        [HttpGet("getProductSaldo")]
        public async Task<ActionResult> GetProductSaldo(int productId)
        {
            var product = await database.Product.FirstOrDefaultAsync(
                p => p.Id == productId && p.Removed != true);

            if (product != null)
            {
                return Ok(product.Quantity);
            }
            return Ok(0);
        }


        [HttpPost("add-product")]
        public async Task<ActionResult> AddProduct(Product product)
        {
            database.Add(product);
            await database.SaveChangesAsync();

            if (product.ProductCategories != null && product.ProductCategories.Any())
            {
                foreach (var categoryId in product.ProductCategories)
                {
                    var productCategory = new ProductCategory
                    {
                        CategoryId = categoryId,
                        ProductId = product.Id
                    };

                    database.Add(productCategory);
                }
                await database.SaveChangesAsync();
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
                product.ShortDescription = updatedProduct.ShortDescription;
                product.LongDescription = updatedProduct.LongDescription;
                product.PriceWithTax = updatedProduct.PriceWithTax;
                product.PriceWithoutTax = updatedProduct.PriceWithoutTax;
                product.Tax = updatedProduct.Tax;
                product.Quantity = updatedProduct.Quantity;

                var existingProductCategories = database.ProductCategory.Where(p => p.ProductId == product.Id).ToList();
                database.ProductCategory.RemoveRange(existingProductCategories);

                foreach (var selectedCategory in updatedProduct.ProductCategories)
                {
                    var category = new ProductCategory()
                    {
                        ProductId = product.Id,
                        CategoryId = selectedCategory
                    };
                    database.ProductCategory.Add(category);
                }
                database.SaveChanges();
            }
        }

        [HttpPost("remove-product")]
        public async Task<ActionResult> RemoveProduct(Product product)
        {
            var productToRemove = await database.Product.FirstOrDefaultAsync(p => p.Id == product.Id);

            if (productToRemove != null)
            {
                productToRemove.Removed = true;
                database.SaveChanges();
            }
            return Ok();
        }

        [HttpPost("return-product")]
        public async Task<ActionResult> ReturnProduct(Product product)
        {
            var productToReturn = await database.Product.FirstOrDefaultAsync(p => p.Id == product.Id);

            if (productToReturn != null)
            {
                productToReturn.Removed = false;
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


        [HttpPost("upload-product-photos")]
        public async Task<ActionResult> UploadProductPhotos(int productId, List<IFormFile> photos)
        {
            var productPhotos = new List<Photo>();
            foreach (var photoFile in photos)
            {
                var result = await photoService.AddPhotoAsync(photoFile);
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
                productPhotos.Add(productPhoto);
            }

            database.Photo.AddRange(productPhotos);
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
            await SetProductMainPhoto(productId, photoId);
            return Ok();
        }

        [HttpPost("remove-photo")]
        public async Task<ActionResult> RemovePhoto(int photoId)
        {
            var photoToRemove = await database.Photo.FirstOrDefaultAsync(p => p.Id == photoId);
            if (photoToRemove != null && photoToRemove.ProductId > 0)
            {
                if (!string.IsNullOrWhiteSpace(photoToRemove.PublicId))
                {
                    await photoService.DeletePhotoAsync(photoToRemove.PublicId);
                }

                database.Photo.Remove(photoToRemove);
                await database.SaveChangesAsync();

                await SetProductMainPhoto((int)photoToRemove.ProductId);
            }
            return Ok();
        }


    }
}
