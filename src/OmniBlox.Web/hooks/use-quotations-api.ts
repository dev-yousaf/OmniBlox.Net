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

export type QuotationWithDetails = QuotationDetail;
export type Quotation = QuotationDetail;

export function useQuotationsApi() {
  const { get, post, put, patch, delete: del } = useAuthenticatedApi();

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

  const updateQuotationStatus = useCallback(async (id: string, data: { status: string }): Promise<QuotationDetail> => {
    return patch(`/quotations/${id}/status`, data) as Promise<QuotationDetail>;
  }, [patch]);

  interface WarehouseStockLevel {
    warehouseId: string;
    warehouseName: string;
    location?: string;
    canFulfill: boolean;
    products: Array<{
      productId: string;
      productName: string;
      sku?: string;
      required: number;
      available: number;
      sufficient: boolean;
    }>;
  }

  const getQuotationStockLevels = useCallback(async (id: string): Promise<{ warehouses: WarehouseStockLevel[] }> => {
    return get(`/quotations/${id}/stock-levels`) as Promise<{ warehouses: WarehouseStockLevel[] }>;
  }, [get]);

  // Aliases for page compatibility
  const getQuotations = useCallback(async (): Promise<QuotationWithDetails[]> => {
    const resp = await list({ page: 1, limit: 10000 });
    return resp.quotations.map((q: QuotationSummary) => ({
      ...q,
      customer: { id: q.customerId, name: q.customerName, email: "", phone: "" },
      items: [],
    })) as unknown as QuotationWithDetails[];
  }, [list]);

  const getQuotation = useCallback(async (id: string): Promise<QuotationWithDetails> => {
    return getById(id) as Promise<QuotationWithDetails>;
  }, [getById]);

  const deleteQuotation = useCallback(async (id: string): Promise<void> => {
    await remove(id);
  }, [remove]);

  const createQuotation = useCallback(async (data: CreateQuotationPayload): Promise<QuotationDetail> => {
    return create(data);
  }, [create]);

  return useMemo(() => ({
    list, getById, create, update, remove, convertToSale,
    updateQuotationStatus, getQuotationStockLevels,
    getQuotations, getQuotation, deleteQuotation,
    createQuotation,
  }), [list, getById, create, update, remove, convertToSale,
      updateQuotationStatus, getQuotationStockLevels,
      getQuotations, getQuotation, deleteQuotation,
      createQuotation]);
}
