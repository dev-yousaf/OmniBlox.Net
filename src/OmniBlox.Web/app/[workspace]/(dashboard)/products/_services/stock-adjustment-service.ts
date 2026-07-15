import { useCallback } from "react";
import { useAuthenticatedApi } from "@/hooks/use-authenticated-api";

export interface StockAdjustmentItemInput {
  productId: string;
  warehouseId: string;
  previousQuantity: number;
  newQuantity: number;
}

export interface CreateStockAdjustmentPayload {
  notes?: string;
  type?: "ADDITION" | "REMOVAL";
  items: StockAdjustmentItemInput[];
}

export interface StockAdjustmentItemResponse {
  id: string;
  previousQuantity: number;
  newQuantity: number;
  difference: number;
  productId: string;
  productName: string;
  productSku: string;
  productImage: string | null;
  warehouseId: string;
  warehouseName: string;
}

export interface StockAdjustmentResponse {
  id: string;
  referenceNumber: string;
  adjustmentDate: string;
  notes?: string;
  type: string;
  totalItems: number;
  netChange: number;
  createdAt: string;
  updatedAt: string;
  userId: string;
  userName: string;
  items: StockAdjustmentItemResponse[];
}

export function useStockAdjustmentService() {
  const { post, get } = useAuthenticatedApi();

  const createStockAdjustment = useCallback(async (
    payload: CreateStockAdjustmentPayload
  ): Promise<StockAdjustmentResponse> => {
    const response: any = await post("/inventory/adjustments", payload);
    // Map backend StockAdjustmentDto -> frontend StockAdjustmentResponse
    return {
      id: response.id,
      referenceNumber: response.referenceNumber,
      adjustmentDate: response.adjustmentDate,
      notes: response.notes,
      type: response.type,
      totalItems: response.totalItems,
      netChange: response.netChange,
      createdAt: response.createdAt,
      updatedAt: response.createdAt,
      userId: response.user?.id || "",
      userName: response.user?.name || "",
      items: (response.items || []).map((i: any) => ({
        id: i.id,
        previousQuantity: i.previousQuantity,
        newQuantity: i.newQuantity,
        difference: i.difference,
        productId: i.productId,
        productName: i.productName,
        productSku: i.productSku,
        productImage: i.productImage,
        warehouseId: i.warehouseId,
        warehouseName: i.warehouseName,
      })),
    } as StockAdjustmentResponse;
  }, [post]);

  const getStockAdjustments = useCallback(async (): Promise<StockAdjustmentResponse[]> => {
    const response: any = await get("/inventory/adjustments?limit=20");
    // Backend returns { adjustments: [...], total, pages }
    const list = response.adjustments || response || [];
    return (Array.isArray(list) ? list : []).map((adj: any) => ({
      id: adj.id,
      referenceNumber: adj.referenceNumber,
      adjustmentDate: adj.adjustmentDate,
      notes: adj.notes,
      type: adj.type,
      totalItems: adj.totalItems,
      netChange: adj.netChange,
      createdAt: adj.createdAt,
      updatedAt: adj.createdAt,
      userId: adj.user?.id || "",
      userName: adj.user?.name || "",
      items: (adj.items || []).map((i: any) => ({
        id: i.id,
        previousQuantity: i.previousQuantity,
        newQuantity: i.newQuantity,
        difference: i.difference,
        productId: i.productId,
        productName: i.productName,
        productSku: i.productSku,
        productImage: i.productImage,
        warehouseId: i.warehouseId,
        warehouseName: i.warehouseName,
      })),
    }));
  }, [get]);

  const getStockAdjustment = useCallback(async (
    id: string
  ): Promise<StockAdjustmentResponse> => {
    const response: any = await get(`/inventory/adjustments/${id}`);
    return {
      id: response.id,
      referenceNumber: response.referenceNumber,
      adjustmentDate: response.adjustmentDate,
      notes: response.notes,
      type: response.type,
      totalItems: response.totalItems,
      netChange: response.netChange,
      createdAt: response.createdAt,
      updatedAt: response.createdAt,
      userId: response.user?.id || "",
      userName: response.user?.name || "",
      items: (response.items || []).map((i: any) => ({
        id: i.id,
        previousQuantity: i.previousQuantity,
        newQuantity: i.newQuantity,
        difference: i.difference,
        productId: i.productId,
        productName: i.productName,
        productSku: i.productSku,
        productImage: i.productImage,
        warehouseId: i.warehouseId,
        warehouseName: i.warehouseName,
      })),
    } as StockAdjustmentResponse;
  }, [get]);

  return {
    createStockAdjustment,
    getStockAdjustments,
    getStockAdjustment,
  };
}
