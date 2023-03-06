using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Media.Fields;
using YesSql.Indexes;

namespace SpaceAPI.Models
{

    public class Product : ContentPart
    {
        public TextField ProductName { get; set; }
        public NumericField Price { get; set; }
        public MediaField Image { get; set; }
    }

    public class SpaceBlogs : ContentPart
    {
        public TextField AbstractDescription { get; set; }
        public BooleanField IsShowInHome { get; set; }
        public DateTimeField Date { get; set; }
    }
}
