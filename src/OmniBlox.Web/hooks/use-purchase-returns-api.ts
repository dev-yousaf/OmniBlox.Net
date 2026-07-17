"use client";

import { useAuthenticatedApi } from "./use-authenticated-api";
import { useCallback, useMemo } from "react";

export type ReturnStatus = "PENDING" | "PROCESSING" | "COMPLETED" | "CANCELLED";

export interface PurchaseReturnItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  unitCost: number;
  total: number;
}

export interface PurchaseReturnSummary {
  id: string;
  referenceNumber: string;
  totalAmount: number;
  reason: string | null;
  status: ReturnStatus;
  returnDate: string;
  warehouseId: string;
  warehouseName: string;
  supplierId: string;
  supplierName: string;
  purchaseOrderId: string | null;
  purchaseOrderReference: string | null;
  createdAt: string;
}

export interface PurchaseReturnDetail extends PurchaseReturnSummary {
  items: PurchaseReturnItem[];
}

export interface PurchaseReturnsListResponse {
  returns: PurchaseReturnSummary[];
  total: number;
  pages: number;
}

export interface CreatePurchaseReturnPayload {
  purchaseOrderId?: string | null;
  supplierId: string;
  warehouseId: string;
  returnDate: string;
  reason?: string;
  items: Array<{
    productId: string;
    quantity: number;
    unitCost: number;
    purchaseOrderItemId?: string | null;
  }>;
}

export function usePurchaseReturnsApi() {
  const { get, post, patch, delete: del } = useAuthenticatedApi();

  const list = useCallback(async (filters: Record<string, any> = {}): Promise<PurchaseReturnsListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.set("page", String(filters.page));
    if (filters.limit) params.set("limit", String(filters.limit));
    if (filters.search) params.set("search", filters.search);
    if (filters.status) params.set("status", filters.status);
    if (filters.supplierId) params.set("supplierId", filters.supplierId);
    if (filters.warehouseId) params.set("warehouseId", filters.warehouseId);
    if (filters.dateFrom) params.set("fromDate", filters.dateFrom);
    if (filters.dateTo) params.set("toDate", filters.dateTo);
    const qs = params.toString();
    return get(`/purchase-returns${qs ? `?${qs}` : ""}`) as Promise<PurchaseReturnsListResponse>;
  }, [get]);

  const getById = useCallback(async (id: string): Promise<PurchaseReturnDetail> => {
    return get(`/purchase-returns/${id}`) as Promise<PurchaseReturnDetail>;
  }, [get]);

  const create = useCallback(async (data: CreatePurchaseReturnPayload): Promise<PurchaseReturnDetail> => {
    return post("/purchase-returns", data) as Promise<PurchaseReturnDetail>;
  }, [post]);

  const updateStatus = useCallback(async (id: string, status: ReturnStatus): Promise<PurchaseReturnDetail> => {
    return patch(`/purchase-returns/${id}`, { status }) as Promise<PurchaseReturnDetail>;
  }, [patch]);

  const remove = useCallback(async (id: string): Promise<void> => {
    await del(`/purchase-returns/${id}`);
  }, [del]);

  return useMemo(() => ({ list, getById, create, updateStatus, remove }), [list, getById, create, updateStatus, remove]);
}
