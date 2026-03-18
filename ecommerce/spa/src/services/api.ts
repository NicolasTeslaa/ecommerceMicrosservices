import axios from 'axios';
import type { ApiResponse, Product, Category } from '@/types';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://api.example.com',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Mock data for development
const mockCategories: Category[] = [
  { id: 'cat-1', name: 'Processadores', description: 'CPUs de alta performance' },
  { id: 'cat-2', name: 'Placas de Vídeo', description: 'GPUs para gaming e criação' },
  { id: 'cat-3', name: 'Memória RAM', description: 'Módulos DDR5 e DDR4' },
  { id: 'cat-4', name: 'Armazenamento', description: 'SSDs NVMe e HDDs' },
  { id: 'cat-5', name: 'Periféricos', description: 'Teclados, mouses e headsets' },
  { id: 'cat-6', name: 'Monitores', description: 'Displays de alta resolução' },
];

const mockProducts: Product[] = [
  { id: 'prod-1', name: 'Quantum Core X9', description: 'Processador 16 núcleos, 5.8GHz, arquitetura de última geração com eficiência energética revolucionária.', price: 4299.90, stockQuantity: 12, active: true, categoryId: 'cat-1', categoryName: 'Processadores' },
  { id: 'prod-2', name: 'Nebula RTX 5090', description: 'GPU flagship com 24GB GDDR7, ray tracing de 4ª geração e DLSS 5.0 para experiências visuais sem precedentes.', price: 12999.90, stockQuantity: 5, active: true, categoryId: 'cat-2', categoryName: 'Placas de Vídeo' },
  { id: 'prod-3', name: 'Horizon DDR5-7200', description: 'Kit 32GB (2x16GB) DDR5, latência ultra-baixa, RGB sincronizado e heat spreader em alumínio.', price: 1599.90, stockQuantity: 28, active: true, categoryId: 'cat-3', categoryName: 'Memória RAM' },
  { id: 'prod-4', name: 'Velocity NVMe Pro', description: 'SSD 2TB NVMe Gen5, leitura 14.000 MB/s, gravação 12.000 MB/s, controlador dedicado.', price: 2199.90, stockQuantity: 0, active: true, categoryId: 'cat-4', categoryName: 'Armazenamento' },
  { id: 'prod-5', name: 'Phantom Mech 75%', description: 'Teclado mecânico hot-swap, switches magnéticos, chassis de alumínio CNC, iluminação por tecla.', price: 899.90, stockQuantity: 45, active: true, categoryId: 'cat-5', categoryName: 'Periféricos' },
  { id: 'prod-6', name: 'Eclipse 4K UltraWide', description: 'Monitor 34" OLED curvo, 4K, 240Hz, tempo de resposta 0.03ms, HDR 1000, calibração de fábrica.', price: 7499.90, stockQuantity: 8, active: true, categoryId: 'cat-6', categoryName: 'Monitores' },
  { id: 'prod-7', name: 'Apex Ryzen Ultra', description: 'CPU 24 núcleos para workstation, base 4.2GHz, boost 6.0GHz, 80MB cache L3.', price: 5899.90, stockQuantity: 3, active: true, categoryId: 'cat-1', categoryName: 'Processadores' },
  { id: 'prod-8', name: 'Titan RX 9800 XT', description: 'GPU enthusiast com 32GB GDDR7, refrigeração vapor chamber tripla, overclock de fábrica.', price: 9799.90, stockQuantity: 7, active: true, categoryId: 'cat-2', categoryName: 'Placas de Vídeo' },
  { id: 'prod-9', name: 'Precision Mouse X1', description: 'Mouse ergonômico 30K DPI, sensor óptico de precisão, chassis em magnésio, 58g.', price: 649.90, stockQuantity: 60, active: true, categoryId: 'cat-5', categoryName: 'Periféricos' },
  { id: 'prod-10', name: 'Flux SSD Portable', description: 'SSD externo 4TB USB4, 4000 MB/s, criptografia por hardware, resistente a impacto.', price: 1899.90, stockQuantity: 15, active: true, categoryId: 'cat-4', categoryName: 'Armazenamento' },
  { id: 'prod-11', name: 'Spectra DDR5-8000', description: 'Kit 64GB (2x32GB) DDR5 para entusiastas, overclock certificado, perfil XMP 3.0.', price: 3299.90, stockQuantity: 10, active: true, categoryId: 'cat-3', categoryName: 'Memória RAM' },
  { id: 'prod-12', name: 'Zenith 8K ProDisplay', description: 'Monitor 32" Mini-LED 8K, P3 wide gamut, Thunderbolt 4, suporte ergonômico integrado.', price: 14999.90, stockQuantity: 2, active: true, categoryId: 'cat-6', categoryName: 'Monitores' },
];

const USE_MOCK = !import.meta.env.VITE_API_BASE_URL || import.meta.env.VITE_API_BASE_URL === 'https://api.example.com';

export const productService = {
  async getAll(): Promise<Product[]> {
    if (USE_MOCK) return mockProducts;
    const { data } = await api.get<ApiResponse<Product[]>>('/api/catalog/products');
    return data.data;
  },

  async getById(id: string): Promise<Product | undefined> {
    if (USE_MOCK) return mockProducts.find(p => p.id === id);
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
    if (USE_MOCK) return mockCategories.find(c => c.id === id);
    const { data } = await api.get<ApiResponse<Category>>(`/api/catalog/categories/${id}`);
    return data.data;
  },
};

export default api;
