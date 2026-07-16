"use client";

import { useAuthenticatedApi } from "./use-authenticated-api";
import { useCallback, useMemo } from "react";

export type SaleStatus = "DRAFT" | "PENDING" | "COMPLETED" | "CANCELLED";
export type SalePaymentStatus = "PAID" | "PENDING" | "PARTIAL";
export type SalePaymentMethod = "CASH" | "CREDIT_CARD" | "BANK_TRANSFER" | "CHECK";

export interface SaleItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  returnedQuantity: number;
  unitPrice: number;
  total: number;
}

export interface SaleSummary {
  id: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  customerEmail: string | null;
  saleDate: string;
  dueDate: string;
  status: SaleStatus;
  paymentStatus: SalePaymentStatus;
  paymentMethod: SalePaymentMethod | null;
  warehouseId: string;
  warehouseName: string | null;
  hasReturns: boolean;
  subtotal: number;
  tax: number;
  discount: number;
  totalAmount: number;
  createdAt: string;
}

export interface SaleDetail extends SaleSummary {
  notes?: string | null;
  items: SaleItem[];
}

export interface SalesListResponse {
  sales: SaleSummary[];
  total: number;
  pages: number;
}

export interface SalesStats {
  totalSales: number;
  totalRevenue: number;
  pendingAmount: number;
  overdueAmount: number;
  paidInvoices: number;
  pendingInvoices: number;
  overdueInvoices: number;
}

export interface CreateSalePayload {
  customerId: string;
  warehouseId: string;
  saleDate: string;
  dueDate: string;
  status?: SaleStatus;
  paymentStatus?: SalePaymentStatus;
  paymentMethod?: SalePaymentMethod | null;
  taxRate?: number;
  discount?: number;
  notes?: string;
  shippingAddress?: string;
  items: Array<{
    productId: string;
    quantity: number;
    unitPrice: number;
  }>;
}

export interface UpdateSalePayload extends Partial<CreateSalePayload> {}

export interface SalesFilters {
  page?: number;
  limit?: number;
  search?: string;
  status?: SaleStatus | "ALL";
  paymentStatus?: SalePaymentStatus | "ALL";
  warehouseId?: string;
  dateFrom?: string;
  dateTo?: string;
  productId?: string;
}

export function useSalesApi() {
  const { get, post, put, patch, delete: del } = useAuthenticatedApi();

  const list = useCallback(async (filters: SalesFilters = {}): Promise<SalesListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.set("page", String(filters.page));
    if (filters.limit) params.set("pageSize", String(filters.limit));
    if (filters.search) params.set("search", filters.search);
    if (filters.status && filters.status !== "ALL") params.set("status", filters.status);
    if (filters.paymentStatus && filters.paymentStatus !== "ALL") params.set("paymentStatus", filters.paymentStatus);
    if (filters.warehouseId) params.set("warehouseId", filters.warehouseId);
    if (filters.dateFrom) params.set("dateFrom", filters.dateFrom);
    if (filters.dateTo) params.set("dateTo", filters.dateTo);
    if (filters.productId) params.set("productId", filters.productId);
    const qs = params.toString();
    return get(`/sales${qs ? `?${qs}` : ""}`) as Promise<SalesListResponse>;
  }, [get]);

  const getById = useCallback(async (id: string): Promise<SaleDetail> => {
    return get(`/sales/${id}`) as Promise<SaleDetail>;
  }, [get]);

  const getStats = useCallback(async (): Promise<SalesStats> => {
    return get(`/sales/stats`) as Promise<SalesStats>;
  }, [get]);

  const create = useCallback(async (data: CreateSalePayload): Promise<SaleDetail> => {
    return post("/sales", data) as Promise<SaleDetail>;
  }, [post]);

  const update = useCallback(async (id: string, data: UpdateSalePayload): Promise<SaleDetail> => {
    return put(`/sales/${id}`, data) as Promise<SaleDetail>;
  }, [put]);

  const markPaid = useCallback(async (id: string): Promise<SaleDetail> => {
    return patch(`/sales/${id}/mark-paid`) as Promise<SaleDetail>;
  }, [patch]);

  const remove = useCallback(async (id: string): Promise<void> => {
    await del(`/sales/${id}`);
  }, [del]);

  return useMemo(() => ({
    list,
    getById,
    getStats,
    create,
    update,
    markPaid,
    remove,
    getSales: list,
    getSale: getById,
  }), [list, getById, getStats, create, update, markPaid, remove]);
}
