import { useCallback } from "react";
import { api } from "../lib/api";

export interface Product {
  id: string;
  name: string;
  sku: string;
  description?: string;
  price: number;
  costPrice?: number;
  unit?: string;
  stockQuantity: number;
  category?: string;
  brand?: string;
  barcode?: string;
  status: string;
  type: string;
  imageUrl?: string;
  notes?: string;
  reorderLevel?: number;
  reorderQuantity?: number;
  taxRate?: number;
  weight?: number;
  weightUnit?: string;
  createdBy?: string;
  createdAt: string;
  updatedAt: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateProductData {
  name: string;
  sku: string;
  description?: string;
  price: number;
  costPrice?: number;
  unit?: string;
  stockQuantity: number;
  category?: string;
  brand?: string;
  barcode?: string;
  status: string;
  type: string;
  notes?: string;
  reorderLevel?: number;
  reorderQuantity?: number;
  taxRate?: number;
  weight?: number;
  weightUnit?: string;
}

export function useProductApi() {
  const getProducts = useCallback(async (params?: {
    search?: string;
    page?: number;
    pageSize?: number;
  }): Promise<PaginatedResponse<Product>> => {
    const query = new URLSearchParams();
    if (params?.search) query.set("search", params.search);
    if (params?.page) query.set("page", params.page.toString());
    if (params?.pageSize) query.set("pageSize", params.pageSize.toString());
    const qs = query.toString();
    return api.get<PaginatedResponse<Product>>(`/products${qs ? `?${qs}` : ""}`);
  }, []);

  const getProduct = useCallback(async (id: string): Promise<Product> => {
    return api.get<Product>(`/products/${id}`);
  }, []);

  const createProduct = useCallback(async (data: CreateProductData): Promise<Product> => {
    return api.post<Product>("/products", data);
  }, []);

  const updateProduct = useCallback(async (id: string, data: Partial<CreateProductData>): Promise<Product> => {
    return api.put<Product>(`/products/${id}`, data);
  }, []);

  const deleteProduct = useCallback(async (id: string): Promise<void> => {
    return api.delete<void>(`/products/${id}`);
  }, []);

  return { getProducts, getProduct, createProduct, updateProduct, deleteProduct };
}
