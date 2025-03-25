using System.Diagnostics;
using _2280600926_DoThanhHiep.Models;
using _2280600926_DoThanhHiep.Repository;
using Microsoft.AspNetCore.Mvc;


namespace _2280600926_DoThanhHiep.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IProductRepository productRepository, ILogger<HomeController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllAsync();
        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}