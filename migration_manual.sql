-- Migration: Product_AddSubCategoryAndRenameStock
ALTER TABLE "Products" RENAME COLUMN "StockQuantity" TO "Stock";
ALTER TABLE "Products" ADD COLUMN "SubCategory" character varying(100);

-- Record migration
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260714130000_Product_AddSubCategoryAndRenameStock', '10.0.9');
