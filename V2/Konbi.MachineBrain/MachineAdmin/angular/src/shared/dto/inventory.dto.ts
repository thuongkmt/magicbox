class InventoryDto {
  TagId: string;
  TrayLevel: number;
  Id: number;
  Product: ProductDto;
  Found: boolean;
}
class ProductDto {
  ProductName: string;
  SKU: string;
  Price: string;
}
class UnstableInventoryDto {
  Inventory: InventoryDto;
  NumberOfChanges: number;
  LastChange: Date;
}