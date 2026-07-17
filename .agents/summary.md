# Anchored Summary — History

## Session Goal
Diagnose and fix inventory desync: `Product.Stock` column out of sync with summed `Inventory.Quantity`.

## Core Fixes Applied

### 1. Primary Bug: `SaveChangesAsync` called BEFORE inventory mutations
**Root cause** in `CreateSaleCommand.cs`: `SaveChangesAsync` was on line 108 (after adding the sale) — the sale was saved, but then `DecrementInventory` modified `Inventory` and `Product.Stock` in-memory with **no subsequent save**. Those changes were discarded.

**Fix**: Removed the premature `SaveChangesAsync` at line 108 and moved it to line 117 — **after** the `DecrementInventory` loop. This ensures the sale + inventory + stock ledger + product stock are all flushed in a single transaction.

### 2. Stale `SumAsync` bug (secondary)
All 12 handlers that recalculated `Product.Stock` used `SumAsync` after in-memory inventory changes but before `SaveChangesAsync`. Since `SumAsync` queries the DB (stale values), the computed stock was wrong. Replaced with direct arithmetic (`product.Stock -= quantity` / `product.Stock += quantity`).

### 3. Warehouse stock display in sale forms
Sale new/edit pages now show warehouse-specific stock (fetched via `getWarehouseInventory`), not total stock.

### 4. `ProductDto.WarehouseId`
Edit form can pre-select the correct warehouse.

### 5. `SaleSummaryDto` computed fields
Added `BalanceDue`, `NetTotal`, `IsOverdue` to prevent `NaN` display.

## Files Changed (inventory desync fixes)
- `src/OmniBlox.Application/Features/Sales/Commands/CreateSaleCommand.cs` — moved `SaveChangesAsync` after inventory mutations
- `src/OmniBlox.Application/Features/Sales/Commands/CreateSaleCommand.cs` — `SumAsync` → arithmetic in `DecrementInventory`
- `src/OmniBlox.Application/Features/Sales/Commands/UpdateSaleCommand.cs` — `SumAsync` → arithmetic
- `src/OmniBlox.Application/Features/Sales/Commands/MarkSalePaidCommand.cs` — `SumAsync` → arithmetic
- `src/OmniBlox.Application/Features/Sales/Commands/DeleteSaleCommand.cs` — `SumAsync` → arithmetic
- `src/OmniBlox.Application/Features/SalesReturns/Commands/UpdateSalesReturnStatusCommand.cs` — `SumAsync` → arithmetic
- `src/OmniBlox.Application/Features/SalesReturns/Commands/DeleteSalesReturnCommand.cs` — `SumAsync` → arithmetic
- `src/OmniBlox.Application/Features/PurchaseReturns/Commands/UpdatePurchaseReturnStatusCommand.cs` — `SumAsync` → arithmetic
- `src/OmniBlox.Application/Features/PurchaseReturns/Commands/DeletePurchaseReturnCommand.cs` — `SumAsync` → arithmetic
- `src/OmniBlox.Application/Features/Purchases/Commands/ReceivePurchaseOrderCommand.cs` — `SumAsync` → arithmetic
- `src/OmniBlox.Application/Features/Purchases/Commands/DeletePurchaseOrderCommand.cs` — `SumAsync` → arithmetic
- `src/OmniBlox.Application/Features/Products/Commands/AdjustStockCommand.cs` — `SumAsync` → arithmetic (capture old qty, compute delta)
- `src/OmniBlox.Application/Features/Products/Commands/UpdateStockCommand.cs` — `SumAsync` → arithmetic

## Verified Not Buggy
All other handlers (`MarkSalePaidCommand`, `UpdateSaleCommand`, `DeleteSaleCommand`, `ReceivePurchaseOrderCommand`, `DeletePurchaseOrderCommand`, `UpdateSalesReturnStatusCommand`, `DeleteSalesReturnCommand`, `UpdatePurchaseReturnStatusCommand`, `DeletePurchaseReturnCommand`, `AdjustStockCommand`, `UpdateStockCommand`, `TransferStockCommand`, `BulkTransferStockCommand`, `CreateStockAdjustmentCommand`, `UpdateInventoryCommand`, `ConvertQuotationToSaleCommand`) had `SaveChangesAsync` AFTER inventory mutations — confirmed correct.

## Observing the correct behavior
After these fixes:
1. **Create a COMPLETED sale** → `Product.Stock` decrements, `Inventory.Quantity` decrements, stock ledger entry created
2. **Products table** now shows correct `Stock` column (reads `product.Stock` from DB)
3. **Warehouse inventory pages** show correct per-warehouse quantities (reads `Inventory.Quantity` from DB)

## Data consistency check
Products table reads `Product.Stock` (cached column). Inventory views either sum `Inventory.Quantity` or read individual records. With the fixes, both are consistent after each transaction.
