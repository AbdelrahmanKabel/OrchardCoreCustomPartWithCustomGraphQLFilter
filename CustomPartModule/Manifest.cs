using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "ProductModule",
    Author = "Abdo",
    Website = "https://naseej.com",
    Version = "1.0.0.0",
    Description = "Product Module",
    Category = "Content Management",
    Dependencies = new[]{ "OrchardCore.Contents", "OrchardCore.ContentTypes", "OrchardCore.ContentFields", "OrchardCore.Media"}
)]