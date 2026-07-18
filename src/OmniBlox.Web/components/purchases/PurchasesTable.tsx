"use client";

import * as React from "react";
import { WorkspaceLink as Link } from "@/components/workspace-link";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Eye, RotateCcw } from "lucide-react";
import type { PurchaseOrder } from "@/hooks/use-purchases-api";

function formatCurrency(n: number | string | undefined) {
  const value = typeof n === "string" ? Number(n) : n ?? 0;
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 2,
  }).format(value);
}

function formatDate(s: string | Date | undefined) {
  if (!s) return "-";
  try {
    const d = typeof s === "string" ? new Date(s) : s;
    return d.toLocaleDateString();
  } catch {
    return String(s);
  }
}

function statusBadgeVariant(
  status: string
): React.ComponentProps<typeof Badge>["variant"] {
  switch (status) {
    case "PENDING":
      return "secondary";
    case "COMPLETED":
      return "default";
    case "CANCELLED":
      return "outline";
    default:
      return "secondary";
  }
}

export interface PurchasesTableProps {
  purchases: PurchaseOrder[];
  canManage: boolean; // OWNER | ADMIN | MANAGER
  onReceive?: (purchase: PurchaseOrder) => Promise<void> | void;
}

export function PurchasesTable({
  purchases,
  canManage,
  onReceive,
}: PurchasesTableProps) {
  return (
    <div className="w-full overflow-x-auto">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Reference #</TableHead>
            <TableHead>Supplier</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Total Amount</TableHead>
            <TableHead>Order Date</TableHead>
            <TableHead>Returns</TableHead>
            {canManage ? (
              <TableHead className="text-right">Actions</TableHead>
            ) : null}
          </TableRow>
        </TableHeader>
        <TableBody>
          {purchases?.length ? (
            purchases.map((po) => (
              <TableRow key={po.id}>
                <TableCell className="font-mono text-xs">
                  <Link
                    href={`/purchases/${po.id}`}
                    className="text-primary hover:underline"
                  >
                    {po.referenceNumber}
                  </Link>
                </TableCell>
                <TableCell>{po.supplier?.name ?? "-"}</TableCell>
                <TableCell>
                  <Badge variant={statusBadgeVariant(po.status)}>
                    {po.status}
                  </Badge>
                </TableCell>
                <TableCell className="font-semibold">
                  {formatCurrency(po.netTotal ?? po.totalAmount)}
                </TableCell>
                <TableCell>{formatDate(po.orderDate)}</TableCell>
                <TableCell>
                  {po.returnStatus === "ALL" ? (
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className="text-purple-600 border-purple-600">
                        <RotateCcw className="mr-1 h-3 w-3" /> All Returned
                      </Badge>
                    </div>
                  ) : (po.processingReturnCount ?? 0) > 0 ? (
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className="text-blue-600 border-blue-600">
                        <RotateCcw className="mr-1 h-3 w-3" /> Processing
                      </Badge>
                    </div>
                  ) : (po.pendingReturnCount ?? 0) > 0 ? (
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className="text-amber-600 border-amber-600">
                        <RotateCcw className="mr-1 h-3 w-3" /> Pending
                      </Badge>
                    </div>
                  ) : po.hasReturns ? (
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className="text-emerald-600 border-emerald-600">
                        <RotateCcw className="mr-1 h-3 w-3" /> Returned
                      </Badge>
                    </div>
                  ) : null}
                </TableCell>
                {canManage ? (
                  <TableCell className="text-right">
                    <div className="flex items-center justify-end gap-1">
                      <Link href={`/purchases/${po.id}`}>
                        <Button variant="ghost" size="icon" className="h-7 w-7" title="View">
                          <Eye className="h-3.5 w-3.5" />
                        </Button>
                      </Link>
                      {po.status === "PENDING" && (
                        <Button variant="outline" size="sm" className="h-7 text-[12px]" onClick={async () => { if (onReceive) await onReceive(po); }}>
                          <RotateCcw className="mr-1 h-3 w-3" /> Receive
                        </Button>
                      )}
                    </div>
                  </TableCell>
                ) : null}
              </TableRow>
            ))
          ) : (
            <TableRow>
              <TableCell
                colSpan={7}
                className="text-center text-sm text-muted-foreground py-10"
              >
                No purchase orders found.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </div>
  );
}

export default PurchasesTable;
