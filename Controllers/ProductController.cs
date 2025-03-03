using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using B2B.Models;

namespace B2B.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        private DBContext db = new DBContext();

        public JsonResult GetProducts(string search = "", int? category = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            // Ürün sorgusunu başlat
            var query = db.Products.AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || p.ProductCode.Contains(search));
            }

            // Kategori filtresi
            if (category.HasValue)
            {
                query = query.Where(p => p.Category.CategoryId == category);
            }

            // Minimum fiyat filtresi
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            // Maksimum fiyat filtresi
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Sonuçları projekte et
            var products = query.Select(p => new
            {
                p.ProductID,
                p.ProductCode,
                p.ProductName,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.UnitsPerBox,
                CategoryName = p.Category.CategoryName,
                ImageUrl = p.Images
                             .Where(i => i.IsMainImage)
                             .Select(i => i.ImageUrl)
                             .FirstOrDefault() ?? "https://via.placeholder.com/300x300.png?text=No+Image"
            }).ToList();

            return Json(products, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetProductById(int id)
        {
            var product = db.Products
                .Where(p => p.ProductID == id)
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Description,
                    p.Price,
                    MainImage = p.Images
                                .Where(i => i.IsMainImage)
                                .Select(i => i.ImageUrl)
                                .FirstOrDefault() ?? "https://via.placeholder.com/500x500.png?text=No+Image",
                    Images = p.Images.Select(i => i.ImageUrl).ToList()
                })
                .FirstOrDefault();

            if (product == null)
                return Json(new { success = false, message = "Ürün bulunamadı." }, JsonRequestBehavior.AllowGet);

            return Json(product, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(string id)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductCode == id);
            var images = db.ProductImages.Where(img => img.ProductID == product.ProductID).ToList();

            dynamic model = new System.Dynamic.ExpandoObject();
            model.Product = product;
            model.Images = images;

            return View(model);
        }




        public ActionResult ProductDetails()
        {
            return View(); // Razor View olan ProductList.cshtml dosyasını çağırır
        }
   
    }
}