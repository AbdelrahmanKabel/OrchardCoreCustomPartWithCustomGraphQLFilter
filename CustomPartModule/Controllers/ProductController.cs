//using GraphQL;
using Microsoft.AspNetCore.Mvc;
using CustomPartModule.Models;
using OrchardCore;
using System.Text.Json;
using OrchardCore.ContentManagement;
using OrchardCore.Settings;
using GraphQL;
using OrchardCore.Entities;

namespace CustomPartModule.Controllers
{
    public class ProductController : Controller
    {
        private readonly IOrchardHelper _orchardHelper;
        private readonly ISiteService _siteService;

        public ProductController(IOrchardHelper orchardHelper, ISiteService siteService)
        {
            _orchardHelper = orchardHelper;
            _siteService = siteService;
        }

        [HttpGet("/api/product/{productId}")]
        public async Task<ObjectResult> GetProductAsync(string productId)
        {
            var survey = await _orchardHelper.GetContentItemByIdAsync("471x9qwqh2eg4yvh00gn4vexc6");
            //var qs = survey.Get("List");

            var product = await _orchardHelper.GetContentItemByIdAsync(productId);

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

        [HttpGet("/api/GetFooterData")]
        public async Task<ObjectResult> GetFooterData()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var footerSettings = siteSettings.As<ContentItem>("FooterSettings");
            var footer = footerSettings.As<FooterSettings>();

            if (footer == null)
            {
                return new ObjectResult(null);
            }
            
            return new ObjectResult(footer);
        }
    }
}
