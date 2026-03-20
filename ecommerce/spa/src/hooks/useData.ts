import { useQuery } from '@tanstack/react-query';
import { categoryService, orderService, productService } from '@/services/backendApi';

export const useProducts = () => {
  return useQuery({
    queryKey: ['products'],
    queryFn: productService.getAll,
  });
};

export const usePagedProducts = (
  pageNumber: number,
  pageSize: number,
  filters?: {
    searchTerm?: string;
    categoryId?: string | null;
  }
) => {
  return useQuery({
    queryKey: ['products', 'paged', pageNumber, pageSize, filters?.searchTerm ?? '', filters?.categoryId ?? ''],
    queryFn: () => productService.getPage(pageNumber, pageSize, filters),
  });
};

export const useProduct = (id: string) => {
  return useQuery({
    queryKey: ['product', id],
    queryFn: () => productService.getById(id),
    enabled: !!id,
  });
};

export const useCategories = () => {
  return useQuery({
    queryKey: ['categories'],
    queryFn: categoryService.getAll,
  });
};

export const useCategory = (id: string) => {
  return useQuery({
    queryKey: ['category', id],
    queryFn: () => categoryService.getById(id),
    enabled: !!id,
  });
};

export const usePagedOrders = (customerId: string, pageNumber: number, pageSize: number) => {
  return useQuery({
    queryKey: ['orders', customerId, pageNumber, pageSize],
    queryFn: () => orderService.getByCustomer(customerId, pageNumber, pageSize),
    enabled: !!customerId,
  });
};
