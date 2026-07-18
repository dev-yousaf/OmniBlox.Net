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
import { useSubCategoriesApi } from "@/hooks/use-sub-categories-api";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  categoryId: string;
  categories?: Array<{ id: string; name: string }>;
  onCreated?: (subCategory: { id: string; name: string; categoryId: string }) => void;
}

export function CreateSubCategoryDialog({ open, onOpenChange, categoryId, categories, onCreated }: Props) {
  const { createSubCategory } = useSubCategoriesApi();
  const [name, setName] = useState("");
  const [selectedCategoryId, setSelectedCategoryId] = useState(categoryId);
  const [creating, setCreating] = useState(false);

  const effectiveCategoryId = categoryId || selectedCategoryId;

  const handleCreate = async () => {
    if (!name.trim() || !effectiveCategoryId) return;
    setCreating(true);
    try {
      const created = await createSubCategory({ name: name.trim(), categoryId: effectiveCategoryId });
      onCreated?.({ id: created.id, name: created.name, categoryId: created.categoryId });
      setName("");
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
          <DialogTitle>New Sub Category</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>Category</Label>
            {categoryId ? (
              <Input value={categories?.find(c => c.id === categoryId)?.name || ""} disabled />
            ) : categories && categories.length > 0 ? (
              <Select value={selectedCategoryId} onValueChange={setSelectedCategoryId}>
                <SelectTrigger className="h-[38px] rounded-[5px] px-3 py-[7px] text-[14px]">
                  <SelectValue placeholder="Select a category" />
                </SelectTrigger>
                <SelectContent>
                  {categories.map((cat) => (
                    <SelectItem key={cat.id} value={cat.id}>{cat.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <Input value="No categories available" disabled />
            )}
          </div>
          <div className="space-y-2">
            <Label>Sub Category Name</Label>
            <Input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter sub category name"
            />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleCreate}
            disabled={creating || !name.trim() || !effectiveCategoryId}
          >
            {creating ? <Loader2 className="h-4 w-4 animate-spin" /> : null}
            Create
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
