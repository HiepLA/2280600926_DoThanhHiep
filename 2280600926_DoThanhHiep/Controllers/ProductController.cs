using _2280600926_DoThanhHiep.Models;
using _2280600926_DoThanhHiep.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

[Authorize] // Yêu cầu người dùng phải đăng nhập mới có thể truy cập
public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ApplicationDbContext _context;

    public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, ApplicationDbContext context)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _context = context;
    }

    // ✅ Người dùng có thể xem danh sách sản phẩm
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllAsync();
        return View(products);
    }

    // ✅ Chỉ Admin mới có thể thêm sản phẩm
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add()
    {
        ViewBag.Categories = await GetCategorySelectList();
        return View(new Product());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add(Product product, List<IFormFile> ImageFiles)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await GetCategorySelectList();
            return View(product);
        }

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
        Directory.CreateDirectory(uploadsFolder);

        if (ImageFiles != null && ImageFiles.Count > 0)
        {
            var firstImage = ImageFiles[0];
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + firstImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await firstImage.CopyToAsync(fileStream);
            }

            product.ImageUrl = "/images/" + uniqueFileName;
        }

        await _productRepository.AddAsync(product);
        await _context.SaveChangesAsync();

        if (ImageFiles != null && ImageFiles.Count > 1)
        {
            foreach (var file in ImageFiles.Skip(1))
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var productImage = new ProductImage
                {
                    ProductId = product.Id,
                    Url = "/images/" + uniqueFileName
                };

                _context.ProductImages.Add(productImage);
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    // ✅ Chỉ Admin mới có thể chỉnh sửa sản phẩm
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();

        ViewBag.Categories = await GetCategorySelectList();
        return View(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Product product, List<IFormFile> ImageFiles)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await GetCategorySelectList();
            return View(product);
        }

        var existingProduct = await _productRepository.GetByIdAsync(product.Id);
        if (existingProduct == null) return NotFound();

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.Description = product.Description;
        existingProduct.CategoryId = product.CategoryId;

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
        Directory.CreateDirectory(uploadsFolder);

        if (ImageFiles != null && ImageFiles.Count > 0)
        {
            var firstImage = ImageFiles[0];
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + firstImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await firstImage.CopyToAsync(fileStream);
            }

            existingProduct.ImageUrl = "/images/" + uniqueFileName;
        }

        await _productRepository.UpdateAsync(existingProduct);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // ✅ Chỉ Admin mới có thể xóa sản phẩm
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();

        try
        {
            await _productRepository.DeleteAsync(product.Id);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            return BadRequest("Có lỗi khi xóa sản phẩm: " + ex.Message);
        }
    }

    // ✅ Lấy danh sách danh mục
    private async Task<IEnumerable<SelectListItem>> GetCategorySelectList()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        });
    }
}
