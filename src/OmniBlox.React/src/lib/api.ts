export interface User {
  id: string;
  email: string;
  name: string | null;
  role: string;
  companyId: string;
  company?: {
    id: string;
    name: string;
    workspaceUrl: string;
    industry?: string;
    country?: string;
  };
}

export interface AuthResponse {
  userId?: string;
  user: User;
  company: {
    id: string;
    name: string;
    workspaceUrl: string;
    industry?: string;
    country?: string;
  };
}

export interface ApiError {
  message: string;
  statusCode: number;
  details?: any;
}

class ApiClient {
  private baseUrl: string;

  constructor() {
    this.baseUrl = import.meta.env.VITE_API_URL || "http://localhost:5005";
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;

    const headers: Record<string, string> = {
      "Content-Type": "application/json",
      ...(options.headers as Record<string, string>),
    };

    try {
      const response = await fetch(url, {
        ...options,
        headers,
        credentials: "include",
      });

      if (!response.ok) {
        const rawText = await response.text().catch(() => "");
        let errorData: any = {};
        try {
          errorData = rawText ? JSON.parse(rawText) : {};
        } catch {
          errorData = { raw: rawText };
        }

        const error = {
          message: errorData?.message || `HTTP ${response.status}: ${response.statusText}`,
          statusCode: response.status,
          details: errorData,
        } as ApiError;

        console.warn(`API Error: ${error.message} (${options.method || "GET"} ${url}) [${response.status}]`);
        throw error;
      }

      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        return response.json();
      }

      return response.text() as any;
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch")) {
        throw {
          message: "Network error. Please check your connection.",
          statusCode: 0,
        } as ApiError;
      }
      throw error;
    }
  }

  async signup(data: {
    email: string;
    password: string;
    name: string;
    companyName: string;
    workspaceUrl: string;
    industry: string;
    country: string;
  }): Promise<AuthResponse> {
    return this.request<AuthResponse>("/auth/signup", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async login(email: string, password: string): Promise<{ userId?: string }> {
    return this.request("/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
  }

  async logout(): Promise<void> {
    try {
      await this.request("/auth/logout", { method: "POST" });
    } catch { }
  }

  async getProfile(): Promise<User> {
    return this.request<User>("/auth/me");
  }

  async get<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: "GET" });
  }

  async post<T>(endpoint: string, data?: any): Promise<T> {
    return this.request<T>(endpoint, {
      method: "POST",
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async put<T>(endpoint: string, data?: any): Promise<T> {
    return this.request<T>(endpoint, {
      method: "PUT",
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async patch<T>(endpoint: string, data?: any): Promise<T> {
    return this.request<T>(endpoint, {
      method: "PATCH",
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async delete<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: "DELETE" });
  }
}

export const api = new ApiClient();
