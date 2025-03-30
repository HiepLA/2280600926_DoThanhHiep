using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using _2280600926_DoThanhHiep.Models;
using _2280600926_DoThanhHiep.Repository;

namespace _2280600926_DoThanhHiep.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // GET: Hiển thị form chỉnh sửa sản phẩm
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Lưu thay đổi sản phẩm
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageUrl, List<IFormFile> imageUrls)
        {
            if (id != product.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
                return View(product);
            }

            if (imageUrl != null)
            {
                product.ImageUrl = await SaveImage(imageUrl);
            }

            if (imageUrls != null && imageUrls.Any())
            {
                product.Images = new List<ProductImage>();
                foreach (var file in imageUrls)
                {
                    product.Images.Add(new ProductImage
                    {
                        Url = await SaveImage(file),
                        ProductId = product.Id
                    });
                }
            }

            await _productRepository.UpdateAsync(product);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            if (image == null || image.Length == 0) return string.Empty;

            var fileName = Path.GetFileName(image.FileName);
            var savePath = Path.Combine("wwwroot/images", fileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + fileName;
        }
    }
}
