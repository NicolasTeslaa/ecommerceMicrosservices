import axios from 'axios';
import type { ApiResponse, Product, Category } from '@/types';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://api.example.com',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

const mockCategories: Category[] = [
  { id: 'cat-1', name: 'Processadores', description: 'CPUs de alta performance' },
  { id: 'cat-2', name: 'Placas de Video', description: 'GPUs para gaming e criacao' },
  { id: 'cat-3', name: 'Memoria RAM', description: 'Modulos DDR5 e DDR4' },
  { id: 'cat-4', name: 'Armazenamento', description: 'SSDs NVMe e HDDs' },
  { id: 'cat-5', name: 'Perifericos', description: 'Teclados, mouses e headsets' },
  { id: 'cat-6', name: 'Monitores', description: 'Displays de alta resolucao' },
];

const logistics = {
  heightCm: 12,
  widthCm: 18,
  cubageM3: 0.02,
  weightKg: 1.5,
  originZipCode: '01001-000',
};

const mockProducts: Product[] = [
  { id: 'prod-1', name: 'Quantum Core X9', description: 'Processador 16 nucleos, 5.8GHz.', price: 4299.9, stockQuantity: 12, active: true, categoryId: 'cat-1', categoryName: 'Processadores', ...logistics },
  { id: 'prod-2', name: 'Nebula RTX 5090', description: 'GPU flagship com 24GB.', price: 12999.9, stockQuantity: 5, active: true, categoryId: 'cat-2', categoryName: 'Placas de Video', ...logistics },
  { id: 'prod-3', name: 'Horizon DDR5-7200', description: 'Kit 32GB DDR5.', price: 1599.9, stockQuantity: 28, active: true, categoryId: 'cat-3', categoryName: 'Memoria RAM', ...logistics },
  { id: 'prod-4', name: 'Velocity NVMe Pro', description: 'SSD 2TB NVMe Gen5.', price: 2199.9, stockQuantity: 0, active: true, categoryId: 'cat-4', categoryName: 'Armazenamento', ...logistics },
  { id: 'prod-5', name: 'Phantom Mech 75%', description: 'Teclado mecanico hot-swap.', price: 899.9, stockQuantity: 45, active: true, categoryId: 'cat-5', categoryName: 'Perifericos', ...logistics },
  { id: 'prod-6', name: 'Eclipse 4K UltraWide', description: 'Monitor 34 OLED curvo.', price: 7499.9, stockQuantity: 8, active: true, categoryId: 'cat-6', categoryName: 'Monitores', ...logistics },
  { id: 'prod-7', name: 'Apex Ryzen Ultra', description: 'CPU 24 nucleos para workstation.', price: 5899.9, stockQuantity: 3, active: true, categoryId: 'cat-1', categoryName: 'Processadores', ...logistics },
  { id: 'prod-8', name: 'Titan RX 9800 XT', description: 'GPU enthusiast com 32GB.', price: 9799.9, stockQuantity: 7, active: true, categoryId: 'cat-2', categoryName: 'Placas de Video', ...logistics },
  { id: 'prod-9', name: 'Precision Mouse X1', description: 'Mouse ergonomico 30K DPI.', price: 649.9, stockQuantity: 60, active: true, categoryId: 'cat-5', categoryName: 'Perifericos', ...logistics },
  { id: 'prod-10', name: 'Flux SSD Portable', description: 'SSD externo 4TB USB4.', price: 1899.9, stockQuantity: 15, active: true, categoryId: 'cat-4', categoryName: 'Armazenamento', ...logistics },
  { id: 'prod-11', name: 'Spectra DDR5-8000', description: 'Kit 64GB DDR5 para entusiastas.', price: 3299.9, stockQuantity: 10, active: true, categoryId: 'cat-3', categoryName: 'Memoria RAM', ...logistics },
  { id: 'prod-12', name: 'Zenith 8K ProDisplay', description: 'Monitor 32 Mini-LED 8K.', price: 14999.9, stockQuantity: 2, active: true, categoryId: 'cat-6', categoryName: 'Monitores', ...logistics },
];

const USE_MOCK = !import.meta.env.VITE_API_BASE_URL || import.meta.env.VITE_API_BASE_URL === 'https://api.example.com';

export const productService = {
  async getAll(): Promise<Product[]> {
    if (USE_MOCK) return mockProducts;
    const { data } = await api.get<ApiResponse<Product[]>>('/api/catalog/products');
    return data.data;
  },

  async getById(id: string): Promise<Product | undefined> {
    if (USE_MOCK) return mockProducts.find((p) => p.id === id);
    const { data } = await api.get<ApiResponse<Product>>(`/api/catalog/products/${id}`);
    return data.data;
  },
};

export const categoryService = {
  async getAll(): Promise<Category[]> {
    if (USE_MOCK) return mockCategories;
    const { data } = await api.get<ApiResponse<Category[]>>('/api/catalog/categories');
    return data.data;
  },

  async getById(id: string): Promise<Category | undefined> {
    if (USE_MOCK) return mockCategories.find((c) => c.id === id);
    const { data } = await api.get<ApiResponse<Category>>(`/api/catalog/categories/${id}`);
    return data.data;
  },
};

export default api;
