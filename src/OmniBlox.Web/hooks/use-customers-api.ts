"use client";

import { useAuthenticatedApi } from "./use-authenticated-api";
import { useCallback, useMemo } from "react";

export interface Customer {
  id: string;
  name: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  status: string;
  createdAt: string;
  creditLimit?: number;
  balance?: number;
}

export interface CustomersListResponse {
  customers: Customer[];
  total: number;
  pages: number;
}

export interface CreateCustomerData {
  name: string;
  email?: string;
  phone?: string;
  address?: string;
}

export interface UpdateCustomerData {
  name?: string;
  email?: string;
  phone?: string;
  address?: string;
}

export type CreateCustomerPayload = CreateCustomerData;
export type UpdateCustomerPayload = UpdateCustomerData;

export function useCustomersApi() {
  const { get, post, put, delete: del } = useAuthenticatedApi();

  const getCustomers = useCallback(async (filters?: { search?: string; page?: number; limit?: number }): Promise<CustomersListResponse> => {
    const params = new URLSearchParams();
    if (filters?.page) params.set("page", String(filters.page));
    if (filters?.limit) params.set("limit", String(filters.limit));
    if (filters?.search) params.set("search", filters.search);
    const query = params.toString();
    return get(`/customers${query ? `?${query}` : ""}`) as Promise<CustomersListResponse>;
  }, [get]);

  const getCustomer = useCallback(async (id: string): Promise<Customer> => {
    return get(`/customers/${id}`) as Promise<Customer>;
  }, [get]);

  const createCustomer = useCallback(async (data: CreateCustomerData): Promise<Customer> => {
    return post("/customers", data) as Promise<Customer>;
  }, [post]);

  const updateCustomer = useCallback(async (id: string, data: UpdateCustomerData): Promise<Customer> => {
    return put(`/customers/${id}`, data) as Promise<Customer>;
  }, [put]);

  const deleteCustomer = useCallback(async (id: string): Promise<void> => {
    await del(`/customers/${id}`);
  }, [del]);

  return useMemo(() => ({ getCustomers, getCustomer, createCustomer, updateCustomer, deleteCustomer }), [getCustomers, getCustomer, createCustomer, updateCustomer, deleteCustomer]);
}
