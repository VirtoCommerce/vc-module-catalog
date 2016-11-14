# VirtoCommerce.Catalog
VirtoCommerce.Catalog module represents PIM (product information management) system.
Key features:
* categories taxonomy
* virtual catalogs
* properties inheritance
* product variations 
* robust content localization

![image](https://cloud.githubusercontent.com/assets/7566324/15540050/edd41b2e-2285-11e6-8962-a173e002ace7.png)

# Documentation
User guide: <a href="https://virtocommerce.com/docs/vc2userguide/merchandise-management" target="_blank"> Products catalog</a>

Developer guide: <a href="https://virtocommerce.com/docs/vc2devguide/extending-commerce/creating-catalog-custom-import-export-extensions" target="_blank"> Creating catalog custom import-export extensions</a>

# Installation
Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Catalog module -> Install
* Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-catalog/releases. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

# Settings
* **Catalog.AssociationGroups** - catalog product association groups for making different product relations (Related, Accessories, Cross sails, etc.);
* **Catalog.EditorialReviewTypes** - types that item Descriptions can have (QuickReview, FullReview, etc.).

# Available resources
* Module related service implementations as a <a href="https://www.nuget.org/packages/VirtoCommerce.CatalogModule.Data" target="_blank">NuGet package</a>
* Catalog web model, converters and security as a <a href="https://www.nuget.org/packages/VirtoCommerce.CatalogModule.Web.Core" target="_blank">NuGet package</a>
* API client as a <a href="https://www.nuget.org/packages/VirtoCommerce.CatalogModule.Client" target="_blank">NuGet package</a>
* API client documentation http://demo.virtocommerce.com/admin/docs/ui/index#!/Catalog_module

# License
Copyright (c) Virtosoftware Ltd.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
