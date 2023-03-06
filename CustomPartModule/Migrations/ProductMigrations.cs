using CustomPartModule.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Media.Fields;
using YesSql.Sql;

namespace CustomPartModule.Migrations
{
    public class ProductMigrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public ProductMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {

            // This code will be run when the feature is enabled
            ///Creating Content part with its fields
            _contentDefinitionManager.AlterPartDefinition(nameof(Product), part => part
                .WithField(nameof(Product.ProductName), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("ProductName")
                )
                .WithField(nameof(Product.Price), field => field
                    .OfType(nameof(NumericField))
                    .WithDisplayName("Price")
                )
                .WithField("Image", field => field
                    .OfType(nameof(MediaField))
                    .WithDisplayName("Main image")
                )
            );
            ///Creating Content Type based on the Content Part
            _contentDefinitionManager.AlterTypeDefinition("Product", type => type
                // content items of this type can have drafts
                .Draftable()
                // content items versions of this type have saved
                .Versionable()
                // this content type appears in the New menu section
                .Creatable()
                .Listable()
                // permissions can be applied specifically to instances of this type
                .Securable()
                .WithPart("Product")
                .WithPart("TitlePart")
            );

            //SchemaBuilder.CreateMapIndexTable(typeof(ProductIndex), table =>
            //{
            //    table.Column<string>("ContentItemId"); // Default column for all index tables
            //    table.Column<string>("ProductName");
            //    table.Column<decimal>("Price");
            //    table.Column<bool>("Latest", c => c.WithDefault(false));
            //    table.Column<bool>("Published", c => c.WithDefault(true));

            //}, "");

            //SchemaBuilder.CreateMapIndexTable<ProductIndex>(table =>
            //{
            //    table.Column<string>("ContentItemId"); // Default column for all index tables
            //    table.Column<string>("ProductName");
            //    table.Column<decimal>("Price");
            //    table.Column<bool>("Latest", c => c.WithDefault(false));
            //    table.Column<bool>("Published", c => c.WithDefault(true));

            //}, "");

            ///Creating Map Index table in the DB for this Content Type
            SchemaBuilder.CreateMapIndexTable<ProductIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26)) // Default column for all index tables
                .Column<string>("ProductName", column => column.WithLength(100))
                .Column<decimal>("Price")
                .Column<bool>("Latest")
                .Column<bool>("Published")

            );
            SchemaBuilder.AlterIndexTable<ProductIndex>(table => table
                .CreateIndex("IDX_AliasPartIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ProductName",
                    "Price",
                    "Published",
                    "Latest")
            );

            return 1;
        }
        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTable<SpaceBlogsIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26)) // Default column for all index tables
                .Column<string>("AbstractDescription")
                .Column<DateTime>("Date")
                .Column<bool>("IsShowInHome")
                .Column<bool>("Latest")
                .Column<bool>("Published")

            );
            SchemaBuilder.AlterIndexTable<SpaceBlogsIndex>(table => table
                .CreateIndex("IDX_SpaceBlogsIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "AbstractDescription",
                    "Date",
                    "IsShowInHome",
                    "Published",
                    "Latest")
            );

            return 2;
        }
    }
}
