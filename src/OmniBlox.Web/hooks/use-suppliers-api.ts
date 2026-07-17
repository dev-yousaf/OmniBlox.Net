"use client";

import { useAuthenticatedApi } from "./use-authenticated-api";
import { useCallback, useMemo } from "react";

export interface Supplier {
  id: string;
  name: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  status: string;
  createdAt: string;
  updatedAt?: string | null;
  creditLimit?: number;
  balance?: number;
}

export interface SuppliersListResponse {
  suppliers: Supplier[];
  total: number;
  pages: number;
}

export interface CreateSupplierData {
  name: string;
  email?: string;
  phone?: string;
  address?: string;
}

export interface UpdateSupplierData {
  name?: string;
  email?: string;
  phone?: string;
  address?: string;
}

export type CreateSupplierPayload = CreateSupplierData;
export type UpdateSupplierPayload = UpdateSupplierData;

export function useSuppliersApi() {
  const { get, post, put, delete: del } = useAuthenticatedApi();

  const getSuppliers = useCallback(async (filters?: { search?: string; page?: number; limit?: number }): Promise<SuppliersListResponse> => {
    const params = new URLSearchParams();
    if (filters?.page) params.set("page", String(filters.page));
    if (filters?.limit) params.set("limit", String(filters.limit));
    if (filters?.search) params.set("search", filters.search);
    const query = params.toString();
    return get(`/suppliers${query ? `?${query}` : ""}`) as Promise<SuppliersListResponse>;
  }, [get]);

  const getSupplier = useCallback(async (id: string): Promise<Supplier> => {
    return get(`/suppliers/${id}`) as Promise<Supplier>;
  }, [get]);

  const createSupplier = useCallback(async (data: CreateSupplierData): Promise<Supplier> => {
    return post("/suppliers", data) as Promise<Supplier>;
  }, [post]);

  const updateSupplier = useCallback(async (id: string, data: UpdateSupplierData): Promise<Supplier> => {
    return put(`/suppliers/${id}`, data) as Promise<Supplier>;
  }, [put]);

  const deleteSupplier = useCallback(async (id: string): Promise<void> => {
    await del(`/suppliers/${id}`);
  }, [del]);

  return useMemo(() => ({ getSuppliers, getSupplier, createSupplier, updateSupplier, deleteSupplier }), [getSuppliers, getSupplier, createSupplier, updateSupplier, deleteSupplier]);
}
