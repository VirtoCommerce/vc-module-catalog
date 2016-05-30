set V=2.10.3
nuget push VirtoCommerce.CatalogModule.Data.%V%.nupkg -Source nuget.org -ApiKey %1
nuget push VirtoCommerce.CatalogModule.Web.Core.%V%.nupkg -Source nuget.org -ApiKey %1
pause
