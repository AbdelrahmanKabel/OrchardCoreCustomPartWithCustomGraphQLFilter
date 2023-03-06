using CustomPartModule.GraphQL;
using CustomPartModule.Migrations;
using CustomPartModule.Models;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace CustomPartModule
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : OrchardCore.Modules.StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // I have omitted the registering of the AutoroutePart, as we expect that to already be registered
            services.AddContentPart<Product>();
            services.AddContentPart<SpaceBlogs>();
            services.AddContentPart<Survey>();
            services.AddContentPart<SurveyAnswer>();
            //services.AddContentPart<ProductPart>();
            services.AddScoped<IDataMigration, ProductMigrations>();


            //services.AddSingleton<ContentPart, Product>();
            //services.AddSingleton<ContentPart, ProductPart>();
            services.AddSingleton<IIndexProvider, ProductIndexProvider>();
            services.AddSingleton<IIndexProvider, SpaceBlogsIndexProvider>();

            //services.AddObjectGraphType<Product, ProductObjectGraphType>();
            services.AddInputObjectGraphType<Product, ProductInputObjectType>();
            services.AddWhereInputIndexPropertyProvider<ProductIndex>();

            services.AddInputObjectGraphType<SpaceBlogs, SpaceBlogsInputObjectType>();
            services.AddWhereInputIndexPropertyProvider<SpaceBlogsIndex>();

            services.AddTransient<IIndexAliasProvider, ProductIndexAliasProvider>();
            //services.AddGraphQLFilterType<ContentItem, ProductGraphQLFilter>();
        }
    }
}
