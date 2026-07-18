import { useAuthenticatedApi } from "./use-authenticated-api";
import { useCallback } from "react";

export interface TeamUser {
  id: string;
  email: string;
  name: string;
  role: "OWNER" | "ADMIN" | "MANAGER" | "OBSERVER";
  createdAt: string;
  lastLogin?: string;
  status: "active" | "inactive";
  inviteToken?: string;
}

export interface CreateUserData {
  email: string;
  name: string;
  role?: "ADMIN" | "MANAGER" | "OBSERVER";
}

export interface UpdateUserData {
  email?: string;
  name?: string;
  role?: "ADMIN" | "MANAGER" | "OBSERVER";
}

export interface ChangePasswordData {
  currentPassword: string;
  newPassword: string;
}

export interface TeamListResponse {
  users: TeamUser[];
  total: number;
  pages: number;
}

export interface TeamStats {
  totalUsers: number;
  adminCount: number;
  managerCount: number;
  staffCount: number;
  activeUsers: number;
  inactiveUsers: number;
  invitedUsers?: number;
}

interface TeamFilters {
  page?: number;
  limit?: number;
  search?: string;
  role?: string;
}

export interface UserProduct {
  id: string;
  name: string;
  sku: string;
  category: string;
  salePrice: number;
  stock: number;
  createdAt: string;
}

export interface AuditLogEntry {
  id: string;
  userId: string;
  userName: string;
  userRole: string;
  action: string;
  entity: string;
  entityId?: string;
  details?: string;
  createdAt: string;
}

export interface AuditLogListResponse {
  logs: AuditLogEntry[];
  total: number;
  pages: number;
  page: number;
  limit: number;
}

export function useTeamApi() {
  const { post, get, put, delete: del } = useAuthenticatedApi();

  const createUser = useCallback(
    async (data: CreateUserData): Promise<TeamUser> => {
      console.log("Team API: Creating user with data:", data);
      console.log("Team API: Endpoint /team");

      try {
        const result = (await post("/team", data)) as Promise<TeamUser>;
        console.log("Team API: User creation successful");
        return result;
      } catch (error: any) {
        // Log minimal, stringified details to avoid noisy 'Object' logs
        const details = {
          message: error?.message,
          statusCode: error?.statusCode || error?.status,
        };
        console.error(
          "Team API: User creation failed:",
          JSON.stringify(details)
        );
        throw error;
      }
    },
    [post]
  );

  const getUsers = useCallback(
    async (
      filters: TeamFilters = {}
    ): Promise<TeamListResponse | TeamUser[]> => {
      const params = new URLSearchParams();
      if (filters.page) params.set("page", filters.page.toString());
      if (filters.limit) params.set("limit", filters.limit.toString());
      if (filters.search) params.set("search", filters.search);
      if (filters.role) params.set("role", filters.role);

      const query = params.toString();
      return get(`/team${query ? `?${query}` : ""}`) as Promise<
        TeamListResponse | TeamUser[]
      >;
    },
    [get]
  );

  const getAllUsers = useCallback(async (): Promise<TeamUser[]> => {
    return get("/team") as Promise<TeamUser[]>;
  }, [get]);

  const getUser = useCallback(
    async (id: string): Promise<TeamUser> => {
      return get(`/team/${id}`) as Promise<TeamUser>;
    },
    [get]
  );

  const updateUser = useCallback(
    async (id: string, data: UpdateUserData): Promise<TeamUser> => {
      return put(`/team/${id}`, data) as Promise<TeamUser>;
    },
    [put]
  );

  const changePassword = useCallback(
    async (
      id: string,
      data: ChangePasswordData
    ): Promise<{ message: string }> => {
      return put(`/team/${id}/password`, data) as Promise<{ message: string }>;
    },
    [put]
  );

  const deleteUser = useCallback(
    async (id: string): Promise<{ message: string }> => {
      return del(`/team/${id}`) as Promise<{ message: string }>;
    },
    [del]
  );

  const getTeamStats = useCallback(async (): Promise<TeamStats> => {
    return get("/team/stats") as Promise<TeamStats>;
  }, [get]);

  const acceptInvitation = useCallback(
    async (token: string, password: string): Promise<{ message: string }> => {
      return post("/auth/accept-invitation", { token, password }) as Promise<{
        message: string;
      }>;
    },
    [post]
  );

  const getUserProducts = useCallback(
    async (id: string): Promise<UserProduct[]> => {
      return get(`/team/${id}/products`) as Promise<UserProduct[]>;
    },
    [get]
  );

  const getUserAuditLogs = useCallback(
    async (id: string, page = 1, limit = 20): Promise<AuditLogListResponse> => {
      return get(`/team/${id}/audit-logs?page=${page}&limit=${limit}`) as Promise<AuditLogListResponse>;
    },
    [get]
  );

  return {
    createUser,
    getUsers,
    getAllUsers,
    getUser,
    updateUser,
    changePassword,
    deleteUser,
    getTeamStats,
    acceptInvitation,
    getUserProducts,
    getUserAuditLogs,
  };
}
