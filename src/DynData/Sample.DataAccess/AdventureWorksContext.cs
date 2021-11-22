using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Sample.DataAccess
{
    public partial class AdventureWorksContext
    {
        public AdventureWorksContext()
        {
        }

        public AdventureWorksContext(DbContextOptions<AdventureWorksContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Address> Addresses { get; set; } = null!;
        public virtual DbSet<AddressType> AddressTypes { get; set; } = null!;
        public virtual DbSet<AwbuildVersion> AwbuildVersions { get; set; } = null!;
        public virtual DbSet<BillOfMaterial> BillOfMaterials { get; set; } = null!;
        public virtual DbSet<BusinessEntity> BusinessEntities { get; set; } = null!;
        public virtual DbSet<BusinessEntityAddress> BusinessEntityAddresses { get; set; } = null!;
        public virtual DbSet<BusinessEntityContact> BusinessEntityContacts { get; set; } = null!;
        public virtual DbSet<ContactType> ContactTypes { get; set; } = null!;
        public virtual DbSet<CountryRegion> CountryRegions { get; set; } = null!;
        public virtual DbSet<CountryRegionCurrency> CountryRegionCurrencies { get; set; } = null!;
        public virtual DbSet<CreditCard> CreditCards { get; set; } = null!;
        public virtual DbSet<Culture> Cultures { get; set; } = null!;
        public virtual DbSet<Currency> Currencies { get; set; } = null!;
        public virtual DbSet<CurrencyRate> CurrencyRates { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<DatabaseLog> DatabaseLogs { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<EmailAddress> EmailAddresses { get; set; } = null!;
        public virtual DbSet<Employee> Employees { get; set; } = null!;
        public virtual DbSet<EmployeeDepartmentHistory> EmployeeDepartmentHistories { get; set; } = null!;
        public virtual DbSet<EmployeePayHistory> EmployeePayHistories { get; set; } = null!;
        public virtual DbSet<ErrorLog> ErrorLogs { get; set; } = null!;
        public virtual DbSet<Illustration> Illustrations { get; set; } = null!;
        public virtual DbSet<JobCandidate> JobCandidates { get; set; } = null!;
        public virtual DbSet<Location> Locations { get; set; } = null!;
        public virtual DbSet<Password> Passwords { get; set; } = null!;
        public virtual DbSet<Person> People { get; set; } = null!;
        public virtual DbSet<PersonCreditCard> PersonCreditCards { get; set; } = null!;
        public virtual DbSet<PersonPhone> PersonPhones { get; set; } = null!;
        public virtual DbSet<PhoneNumberType> PhoneNumberTypes { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public virtual DbSet<ProductCostHistory> ProductCostHistories { get; set; } = null!;
        public virtual DbSet<ProductDescription> ProductDescriptions { get; set; } = null!;
        public virtual DbSet<ProductInventory> ProductInventories { get; set; } = null!;
        public virtual DbSet<ProductListPriceHistory> ProductListPriceHistories { get; set; } = null!;
        public virtual DbSet<ProductModel> ProductModels { get; set; } = null!;
        public virtual DbSet<ProductModelIllustration> ProductModelIllustrations { get; set; } = null!;
        public virtual DbSet<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCultures { get; set; } = null!;
        public virtual DbSet<ProductPhoto> ProductPhotos { get; set; } = null!;
        public virtual DbSet<ProductProductPhoto> ProductProductPhotos { get; set; } = null!;
        public virtual DbSet<ProductReview> ProductReviews { get; set; } = null!;
        public virtual DbSet<ProductSubcategory> ProductSubcategories { get; set; } = null!;
        public virtual DbSet<ProductVendor> ProductVendors { get; set; } = null!;
        public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = null!;
        public virtual DbSet<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; } = null!;
        public virtual DbSet<SalesOrderDetail> SalesOrderDetails { get; set; } = null!;
        public virtual DbSet<SalesOrderHeader> SalesOrderHeaders { get; set; } = null!;
        public virtual DbSet<SalesOrderHeaderSalesReason> SalesOrderHeaderSalesReasons { get; set; } = null!;
        public virtual DbSet<SalesPerson> SalesPeople { get; set; } = null!;
        public virtual DbSet<SalesPersonQuotaHistory> SalesPersonQuotaHistories { get; set; } = null!;
        public virtual DbSet<SalesReason> SalesReasons { get; set; } = null!;
        public virtual DbSet<SalesTaxRate> SalesTaxRates { get; set; } = null!;
        public virtual DbSet<SalesTerritory> SalesTerritories { get; set; } = null!;
        public virtual DbSet<SalesTerritoryHistory> SalesTerritoryHistories { get; set; } = null!;
        public virtual DbSet<ScrapReason> ScrapReasons { get; set; } = null!;
        public virtual DbSet<Shift> Shifts { get; set; } = null!;
        public virtual DbSet<ShipMethod> ShipMethods { get; set; } = null!;
        public virtual DbSet<ShoppingCartItem> ShoppingCartItems { get; set; } = null!;
        public virtual DbSet<SpecialOffer> SpecialOffers { get; set; } = null!;
        public virtual DbSet<SpecialOfferProduct> SpecialOfferProducts { get; set; } = null!;
        public virtual DbSet<StateProvince> StateProvinces { get; set; } = null!;
        public virtual DbSet<Store> Stores { get; set; } = null!;
        public virtual DbSet<TransactionHistory> TransactionHistories { get; set; } = null!;
        public virtual DbSet<TransactionHistoryArchive> TransactionHistoryArchives { get; set; } = null!;
        public virtual DbSet<UnitMeasure> UnitMeasures { get; set; } = null!;
        public virtual DbSet<VAdditionalContactInfo> VAdditionalContactInfos { get; set; } = null!;
        public virtual DbSet<VEmployee> VEmployees { get; set; } = null!;
        public virtual DbSet<VEmployeeDepartment> VEmployeeDepartments { get; set; } = null!;
        public virtual DbSet<VEmployeeDepartmentHistory> VEmployeeDepartmentHistories { get; set; } = null!;
        public virtual DbSet<VIndividualCustomer> VIndividualCustomers { get; set; } = null!;
        public virtual DbSet<VJobCandidate> VJobCandidates { get; set; } = null!;
        public virtual DbSet<VJobCandidateEducation> VJobCandidateEducations { get; set; } = null!;
        public virtual DbSet<VJobCandidateEmployment> VJobCandidateEmployments { get; set; } = null!;
        public virtual DbSet<VPersonDemographic> VPersonDemographics { get; set; } = null!;
        public virtual DbSet<VProductAndDescription> VProductAndDescriptions { get; set; } = null!;
        public virtual DbSet<VProductModelCatalogDescription> VProductModelCatalogDescriptions { get; set; } = null!;
        public virtual DbSet<VProductModelInstruction> VProductModelInstructions { get; set; } = null!;
        public virtual DbSet<VSalesPerson> VSalesPeople { get; set; } = null!;
        public virtual DbSet<VSalesPersonSalesByFiscalYear> VSalesPersonSalesByFiscalYears { get; set; } = null!;
        public virtual DbSet<VStateProvinceCountryRegion> VStateProvinceCountryRegions { get; set; } = null!;
        public virtual DbSet<VStoreWithAddress> VStoreWithAddresses { get; set; } = null!;
        public virtual DbSet<VStoreWithContact> VStoreWithContacts { get; set; } = null!;
        public virtual DbSet<VStoreWithDemographic> VStoreWithDemographics { get; set; } = null!;
        public virtual DbSet<VVendorWithAddress> VVendorWithAddresses { get; set; } = null!;
        public virtual DbSet<VVendorWithContact> VVendorWithContacts { get; set; } = null!;
        public virtual DbSet<Vendor> Vendors { get; set; } = null!;
        public virtual DbSet<WorkOrder> WorkOrders { get; set; } = null!;
        public virtual DbSet<WorkOrderRouting> WorkOrderRoutings { get; set; } = null!;

    }
}
