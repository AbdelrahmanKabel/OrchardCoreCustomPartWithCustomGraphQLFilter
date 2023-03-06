using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Media.Fields;
using YesSql.Indexes;

namespace CustomPartModule.Models
{
    public class ProductIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public bool Latest { get; set; }
        public bool Published { get; set; }
    }

    public class Product : ContentPart
    {
        public TextField ProductName { get; set; }
        public NumericField Price { get; set; }
        public MediaField Image { get; set; }
    }

    //public class ProductPart : ContentPart
    //{
    //    public string ProductName { get; set; }
    //    public decimal Price { get; set; }
    //    //public MediaField Image { get; set; }
    //}

    public class SpaceBlogsIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string AbstractDescription { get; set; }
        public DateTime Date { get; set; }
        public bool IsShowInHome { get; set; }
        public bool Latest { get; set; }
        public bool Published { get; set; }
    }

    public class SpaceBlogs : ContentPart
    {
        public TextField AbstractDescription { get; set; }
        public BooleanField IsShowInHome { get; set; }
        public DateTimeField Date { get; set; }
    }

    public class FooterSettings : ContentPart
    {
        public TextField CopyRights { get; set; }
        public TextField CopyRightsYear { get; set; }
    }
    
}
