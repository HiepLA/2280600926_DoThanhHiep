using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace _2280600926_DoThanhHiep.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        [Required]
        public string Description { get; set; }

        public string? ImageUrl { get; set; } // Lưu ảnh chính

        [NotMapped] // Không lưu vào DB
        public List<IFormFile>? ImageFiles { get; set; } // Upload nhiều ảnh

        public List<ProductImage> Images { get; set; } = new List<ProductImage>(); // Danh sách ảnh

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
