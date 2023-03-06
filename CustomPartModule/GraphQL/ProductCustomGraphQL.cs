using CustomPartModule.Models;
using GraphQL.Types;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Indexes;

namespace CustomPartModule.GraphQL
{
    public class ProductObjectGraphType : ObjectGraphType<Product>
    {
        public ProductObjectGraphType()
        {
            Name = "Product";

            // Map the fields you want to expose
            Field("productName", x => x.ProductName, true);
            Field("price", x => x.Price, true);
        }
    }
    
    ///Adds Filteration with ProductName in GraphQL Where clause.
    public class ProductInputObjectType : WhereInputObjectGraphType<Product> 
    {
        public ProductInputObjectType()
        {
            Name = "ProductInput";

            //AddScalarFilterFields<StringGraphType>(nameof(ProductIndex.ProductName), "the product name of the content item to filter");
            AddScalarFilterFields<StringGraphType>("ProductName", "the product name of the content item to filter");

            // Using the enumeration graph type turns the filter into a dropdown of the valid options in GraphiQL.
            //AddScalarFilterFields<ProductObjectGraphType>(nameof(ProductIndex.Price), "the price of the content item to filter");

        }
    }

    /// ProductIndexAliasProvider Adds the Index table in which data filteration will be done to GraphQL Indexes in order to do the filteration
    public class ProductIndexAliasProvider : IIndexAliasProvider 
    {
        private static readonly IndexAlias[] _aliases = new[]
        {
            new IndexAlias
            {
                Alias = "product",
                Index = nameof(ProductIndex),
                IndexType = typeof(ProductIndex)
            },
            new IndexAlias
            {
                Alias = "spaceBlogs",
                Index = nameof(SpaceBlogsIndex),
                IndexType = typeof(SpaceBlogsIndex)
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }
    }

    /// Used to add row in the created Index table in DB related to the original Content in Document table
    public class ProductIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<ProductIndex>().Map(contentItem =>
            {
                if (!contentItem.Latest && !contentItem.Published)
                {
                    return null;
                }
                var productPartContent = contentItem.As<Product>(); //Same name like part object

                return productPartContent == null ? null : new ProductIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    ProductName = productPartContent.ProductName.Text,
                    Price = (decimal)productPartContent.Price.Value,
                    Latest = contentItem.Latest,
                    Published = contentItem.Published,
                };
            });
        }
    }

    public class SpaceBlogsInputObjectType : WhereInputObjectGraphType<SpaceBlogs>
    {
        public SpaceBlogsInputObjectType()
        {
            Name = "SpaceBlogsInput";

            //AddScalarFilterFields<StringGraphType>(nameof(ProductIndex.ProductName), "the product name of the content item to filter");
            AddScalarFilterFields<DateTimeGraphType>("Date", "the date of the content item to filter");

            // Using the enumeration graph type turns the filter into a dropdown of the valid options in GraphiQL.
            //AddScalarFilterFields<ProductObjectGraphType>(nameof(ProductIndex.Price), "the price of the content item to filter");

        }
    }

    //public class SpaceBlogsIndexAliasProvider : IIndexAliasProvider
    //{
    //    private static readonly IndexAlias[] _aliases = new[]
    //    {
    //        new IndexAlias
    //        {
    //            Alias = "spaceBlogs",
    //            Index = nameof(SpaceBlogsIndex),
    //            IndexType = typeof(SpaceBlogsIndex)
    //        }
    //    };

    //    public IEnumerable<IndexAlias> GetAliases()
    //    {
    //        return _aliases;
    //    }
    //}

    public class SpaceBlogsIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<SpaceBlogsIndex>().Map(contentItem =>
            {
                if (!contentItem.Latest && !contentItem.Published)
                {
                    return null;
                }
                var productPartContent = contentItem.As<SpaceBlogs>(); //Same name like part object

                return productPartContent == null ? null : new SpaceBlogsIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    AbstractDescription = productPartContent.AbstractDescription.Text,
                    Date = (DateTime)productPartContent.Date.Value,
                    IsShowInHome = productPartContent.IsShowInHome.Value,
                    Latest = contentItem.Latest,
                    Published = contentItem.Published,
                };
            });
        }
    }

}
