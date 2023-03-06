//using GraphQL;
using Microsoft.AspNetCore.Mvc;
using SpaceAPI.Models;
using OrchardCore;
using System.Text.Json;
using OrchardCore.ContentManagement;

namespace SpaceAPI.Controllers
{
    public class ProductController : Controller
    {
        //private readonly IOrchardHelper _orchardHelper;
        private readonly IContentManager _contentManager;

        public ProductController(/*IOrchardHelper orchardHelper, */IContentManager contentManager)
        {
            //_orchardHelper = orchardHelper;
            _contentManager = contentManager;
        }

        [HttpGet("/api/product/{productId}")]
        public async Task<ObjectResult> GetProductAsync(string productId)
        {
            var product = await _contentManager.GetAsync(productId, VersionOptions.Latest);

            //var product = await _orchardHelper.GetContentItemByIdAsync(productId);

            if (product == null)
            {
                return new ObjectResult(null);
            }
            var productPart = product.As<Product>();
            //var productPart0 = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)product.Content).First).Value.ToString();
            //var productPart = JsonSerializer.Deserialize<Product>(productPart0);

            // you'll get exceptions if any of these Fields are null
            // the null-conditional operator (?) should be used for any fields which aren't required
            return new ObjectResult(new
            {
                ProductName = productPart.ProductName.Text,
                Price = productPart.Price.Value,
                Image = productPart.Image.Paths.FirstOrDefault(),
            });
        }
    }
}
