using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using _2280600926_DoThanhHiep.Models;
using _2280600926_DoThanhHiep.Repository;
using _2280600926_DoThanhHiep.Extensions; // Giả sử namespace của các model

[Authorize] // Yêu cầu đăng nhập cho toàn bộ controller
public class ShoppingCartController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // Constructor tiêm các dependency
    public ShoppingCartController(
        IProductRepository productRepository,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _productRepository = productRepository;
        _context = context;
        _userManager = userManager;
    }

    // Hiển thị giỏ hàng (Index)
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
        return View(cart);
    }

    // Thêm sản phẩm vào giỏ hàng
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var product = await GetProductFromDatabase(productId);
        if (product == null)
        {
            return NotFound("Sản phẩm không tồn tại.");
        }

        var cartItem = new CartItem
        {
            ProductId = productId,
            Name = product.Name,
            Price = product.Price,
            Quantity = quantity
        };

        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
        cart.AddItem(cartItem);
        HttpContext.Session.SetObjectAsJson("Cart", cart);

        return RedirectToAction("Index");
    }

    // Xóa sản phẩm khỏi giỏ hàng
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart != null)
        {
            cart.RemoveItem(productId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
        }
        return RedirectToAction("Index");
    }

    // Hiển thị trang thanh toán
    public IActionResult Checkout()
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart == null || !cart.Items.Any())
        {
            return RedirectToAction("Index");
        }
        return View(new Order());
    }

    // Xử lý thanh toán (POST)
    [HttpPost]
    public async Task<IActionResult> Checkout(Order order)
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart == null || !cart.Items.Any())
        {
            ModelState.AddModelError("", "Giỏ hàng trống.");
            return View(order);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("Vui lòng đăng nhập để thanh toán.");
        }

        order.UserId = user.Id;
        order.OrderDate = DateTime.UtcNow;
        order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
        order.OrderDetails = cart.Items.Select(i => new OrderDetail
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Price = i.Price
        }).ToList();

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        HttpContext.Session.Remove("Cart");

        return View("OrderCompleted", order.Id); // Trả về view xác nhận với OrderId
    }

    // Phương thức hỗ trợ: Lấy sản phẩm từ DB
    private async Task<Product> GetProductFromDatabase(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product;
    }
}