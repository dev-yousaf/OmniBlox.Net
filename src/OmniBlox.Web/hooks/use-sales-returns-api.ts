"use client";

import { useAuthenticatedApi } from "./use-authenticated-api";
import { useCallback, useMemo } from "react";

export type ReturnStatus = "PENDING" | "PROCESSING" | "COMPLETED" | "CANCELLED";

export interface SalesReturnItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  total: number;
}

export interface SalesReturnSummary {
  id: string;
  referenceNumber: string;
  totalAmount: number;
  reason: string | null;
  status: ReturnStatus;
  returnDate: string;
  warehouseId: string;
  warehouseName: string;
  saleId: string | null;
  saleInvoiceNumber: string | null;
  createdAt: string;
}

export interface SalesReturnDetail extends SalesReturnSummary {
  items: SalesReturnItem[];
}

export interface SalesReturnsListResponse {
  returns: SalesReturnSummary[];
  total: number;
  pages: number;
}

export interface CreateSalesReturnPayload {
  saleId?: string | null;
  warehouseId: string;
  returnDate: string;
  reason?: string;
  items: Array<{
    productId: string;
    quantity: number;
    unitPrice: number;
    saleItemId?: string | null;
  }>;
}

export function useSalesReturnsApi() {
  const { get, post, patch, delete: del } = useAuthenticatedApi();

  const list = useCallback(async (filters: Record<string, any> = {}): Promise<SalesReturnsListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.set("page", String(filters.page));
    if (filters.limit) params.set("pageSize", String(filters.limit));
    if (filters.search) params.set("search", filters.search);
    if (filters.status) params.set("status", filters.status);
    if (filters.warehouseId) params.set("warehouseId", filters.warehouseId);
    if (filters.dateFrom) params.set("dateFrom", filters.dateFrom);
    if (filters.dateTo) params.set("dateTo", filters.dateTo);
    const qs = params.toString();
    return get(`/sales-returns${qs ? `?${qs}` : ""}`) as Promise<SalesReturnsListResponse>;
  }, [get]);

  const getById = useCallback(async (id: string): Promise<SalesReturnDetail> => {
    return get(`/sales-returns/${id}`) as Promise<SalesReturnDetail>;
  }, [get]);

  const create = useCallback(async (data: CreateSalesReturnPayload): Promise<SalesReturnDetail> => {
    return post("/sales-returns", data) as Promise<SalesReturnDetail>;
  }, [post]);

  const updateStatus = useCallback(async (id: string, status: ReturnStatus): Promise<SalesReturnDetail> => {
    return patch(`/sales-returns/${id}`, { status }) as Promise<SalesReturnDetail>;
  }, [patch]);

  const remove = useCallback(async (id: string): Promise<void> => {
    await del(`/sales-returns/${id}`);
  }, [del]);

  return useMemo(() => ({ list, getById, create, updateStatus, remove }), [list, getById, create, updateStatus, remove]);
}
