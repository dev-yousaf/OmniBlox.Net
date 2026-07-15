"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Loader2 } from "lucide-react";
import { useUnitsApi } from "@/hooks/use-units-api";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreated?: (unit: { shortName: string; name: string }) => void;
}

export function CreateUnitDialog({ open, onOpenChange, onCreated }: Props) {
  const { createUnit } = useUnitsApi();
  const [name, setName] = useState("");
  const [shortName, setShortName] = useState("");
  const [creating, setCreating] = useState(false);

  const handleCreate = async () => {
    if (!name.trim() || !shortName.trim()) return;
    setCreating(true);
    try {
      const created = await createUnit({ name: name.trim(), shortName: shortName.trim() });
      onCreated?.({ shortName: created.shortName, name: created.name });
      setName("");
      setShortName("");
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
          <DialogTitle>New Unit</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>Unit Name</Label>
            <Input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g. Kilogram"
            />
          </div>
          <div className="space-y-2">
            <Label>Short Name</Label>
            <Input
              value={shortName}
              onChange={(e) => setShortName(e.target.value)}
              placeholder="e.g. kg"
            />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleCreate}
            disabled={creating || !name.trim() || !shortName.trim()}
          >
            {creating ? <Loader2 className="h-4 w-4 animate-spin" /> : null}
            Create
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
