"use client";

import { useAuthenticatedApi } from "./use-authenticated-api";
import { useCallback, useMemo } from "react";

export type QuotationStatus = "DRAFT" | "PENDING" | "COMPLETED" | "CANCELLED";

export interface QuotationItem {
  id: string;
  productId: string;
  productName: string;
  productSku: string | null;
  quantity: number;
  unitPrice: number;
  total: number;
}

export interface QuotationSummary {
  id: string;
  referenceNumber: string;
  customerId: string;
  customerName: string;
  quoteDate: string;
  expiryDate: string | null;
  status: QuotationStatus;
  totalAmount: number;
  createdAt: string;
}

export interface QuotationDetail extends QuotationSummary {
  notes?: string | null;
  items: QuotationItem[];
}

export interface QuotationsListResponse {
  quotations: QuotationSummary[];
  total: number;
  pages: number;
}

export interface CreateQuotationPayload {
  customerId: string;
  quoteDate: string;
  expiryDate?: string | null;
  status?: QuotationStatus;
  notes?: string;
  items: Array<{
    productId: string;
    quantity: number;
    unitPrice: number;
  }>;
}

export interface UpdateQuotationPayload extends Partial<CreateQuotationPayload> {}

export interface ConvertQuotationPayload {
  warehouseId: string;
  saleDate: string;
  dueDate: string;
  status?: string;
  paymentStatus?: string;
  paymentMethod?: string | null;
  notes?: string;
  shippingAddress?: string;
}

export interface QuotationSaleResult {
  saleId: string;
  invoiceNumber: string;
  totalAmount: number;
  message: string;
}

export function useQuotationsApi() {
  const { get, post, put, delete: del } = useAuthenticatedApi();

  const list = useCallback(async (filters: Record<string, any> = {}): Promise<QuotationsListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.set("page", String(filters.page));
    if (filters.limit) params.set("limit", String(filters.limit));
    if (filters.status) params.set("status", filters.status);
    if (filters.customerId) params.set("customerId", filters.customerId);
    if (filters.dateFrom) params.set("dateFrom", filters.dateFrom);
    if (filters.dateTo) params.set("dateTo", filters.dateTo);
    const qs = params.toString();
    return get(`/quotations${qs ? `?${qs}` : ""}`) as Promise<QuotationsListResponse>;
  }, [get]);

  const getById = useCallback(async (id: string): Promise<QuotationDetail> => {
    return get(`/quotations/${id}`) as Promise<QuotationDetail>;
  }, [get]);

  const create = useCallback(async (data: CreateQuotationPayload): Promise<QuotationDetail> => {
    return post("/quotations", data) as Promise<QuotationDetail>;
  }, [post]);

  const update = useCallback(async (id: string, data: UpdateQuotationPayload): Promise<QuotationDetail> => {
    return put(`/quotations/${id}`, data) as Promise<QuotationDetail>;
  }, [put]);

  const remove = useCallback(async (id: string): Promise<void> => {
    await del(`/quotations/${id}`);
  }, [del]);

  const convertToSale = useCallback(async (id: string, data: ConvertQuotationPayload): Promise<QuotationSaleResult> => {
    return post(`/quotations/${id}/convert-to-sale`, data) as Promise<QuotationSaleResult>;
  }, [post]);

  return useMemo(() => ({ list, getById, create, update, remove, convertToSale }), [list, getById, create, update, remove, convertToSale]);
}
