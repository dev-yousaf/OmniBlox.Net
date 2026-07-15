"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Loader2 } from "lucide-react";
import { useWarrantiesApi } from "@/hooks/use-warranties-api";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreated?: (warranty: { id: string; name: string; duration: number; durationType: string }) => void;
}

export function CreateWarrantyDialog({ open, onOpenChange, onCreated }: Props) {
  const { createWarranty } = useWarrantiesApi();
  const [name, setName] = useState("");
  const [duration, setDuration] = useState("1");
  const [durationType, setDurationType] = useState("Year");
  const [creating, setCreating] = useState(false);

  const handleCreate = async () => {
    if (!name.trim() || !duration) return;
    setCreating(true);
    try {
      const created = await createWarranty({
        name: name.trim(),
        duration: parseInt(duration) || 1,
        durationType,
      });
      onCreated?.(created);
      setName("");
      setDuration("1");
      onOpenChange(false);
    } catch (err: any) {
      console.error(err);
    } finally {
      setCreating(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>New Warranty</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>Warranty Name</Label>
            <Input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g. Standard Warranty"
            />
          </div>
          <div className="flex gap-4">
            <div className="space-y-2 flex-1">
              <Label>Duration</Label>
              <Input
                type="number"
                min="1"
                value={duration}
                onChange={(e) => setDuration(e.target.value)}
              />
            </div>
            <div className="space-y-2 flex-1">
              <Label>Period</Label>
              <Select value={durationType} onValueChange={setDurationType}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Day">Day</SelectItem>
                  <SelectItem value="Month">Month</SelectItem>
                  <SelectItem value="Year">Year</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleCreate}
            disabled={creating || !name.trim() || !duration}
          >
            {creating ? <Loader2 className="h-4 w-4 animate-spin" /> : null}
            Create
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
