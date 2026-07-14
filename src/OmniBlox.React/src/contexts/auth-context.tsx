import React, { createContext, useContext, useState, useEffect, type ReactNode } from "react";
import { api } from "../lib/api";
import type { User } from "../lib/api";

interface SignupData {
  email: string;
  password: string;
  name: string;
  companyName: string;
  workspaceUrl: string;
  industry: string;
  country: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  signup: (data: SignupData) => Promise<{ userId: string }>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const validateSession = async () => {
      try {
        setIsLoading(true);
        const userData = await api.getProfile();
        setUser(userData);
      } catch {
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };
    validateSession();
  }, []);

  const login = async (email: string, password: string) => {
    try {
      const response = await api.login(email, password);
      setUser(response.user);
    } catch (error: any) {
      throw error;
    }
  };

  const signup = async (data: SignupData) => {
    try {
      const response = await api.signup(data) as any;
      return { userId: response.userId as string };
    } catch (error: any) {
      throw error;
    }
  };

  const logout = async () => {
    try {
      setIsLoading(true);
      await api.logout();
      setUser(null);
    } catch {
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  };

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    signup,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
