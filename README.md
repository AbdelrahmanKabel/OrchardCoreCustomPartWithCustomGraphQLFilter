Here a sample for creating custom filter in Orchard Core GraphQL queries to your Content Type

In a migration:
-Add your cutom part to your type
-Create MapIndexTable in order to store your custom part in a separate table which will be used for custom filtration

For GraphQL:
-Add your custom GraphQL input type which will be take form GraphQL in Orchard 
-Add your IndexAliasProvider to register your new custom part to IndexAliases to be used in search while filtration (Important point)
