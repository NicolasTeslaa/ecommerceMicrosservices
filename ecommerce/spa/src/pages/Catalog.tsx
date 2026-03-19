import { Fragment, useMemo, useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { Search, SlidersHorizontal } from 'lucide-react';
import { usePagedProducts, useCategories } from '@/hooks/useData';
import ProductCard from '@/components/product/ProductCard';
import ProductSkeleton from '@/components/product/ProductSkeleton';
import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from '@/components/ui/pagination';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0, transition: { duration: 0.5 } },
};

const stagger = {
  hidden: {},
  show: { transition: { staggerChildren: 0.06 } },
};

const PRODUCTS_PER_PAGE = 12;

const buildVisiblePages = (currentPage: number, totalPages: number) => {
  if (totalPages <= 5) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  if (currentPage <= 3) {
    return [1, 2, 3, 4, totalPages];
  }

  if (currentPage >= totalPages - 2) {
    return [1, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
  }

  return [1, currentPage - 1, currentPage, currentPage + 1, totalPages];
};

const Catalog = () => {
  const { data: categories } = useCategories();
  const [search, setSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const trimmedSearch = useMemo(() => search.trim(), [search]);
  const { data: pagedProducts, isLoading, isFetching } = usePagedProducts(currentPage, PRODUCTS_PER_PAGE, {
    searchTerm: trimmedSearch || undefined,
    categoryId: selectedCategory,
  });

  useEffect(() => {
    setCurrentPage(1);
  }, [search, selectedCategory]);

  const products = pagedProducts?.items ?? [];
  const totalItems = pagedProducts?.pagination?.totalItems ?? 0;
  const totalPages = Math.max(1, pagedProducts?.pagination?.totalPages ?? 1);

  const visiblePages = useMemo(
    () => buildVisiblePages(currentPage, totalPages),
    [currentPage, totalPages]
  );

  return (
    <div className="min-h-screen pt-24 lg:pt-28 pb-12 px-4">
      <div className="max-w-7xl mx-auto">
        <motion.div initial="hidden" animate="show" variants={stagger}>
          {/* Header */}
          <motion.div variants={fadeUp} className="mb-8">
            <p className="text-primary font-mono text-xs tracking-widest uppercase mb-2">Catálogo</p>
            <h1 className="text-4xl sm:text-5xl font-display font-bold tracking-tight">Todos os Produtos</h1>
          </motion.div>

          {/* Filters */}
          <motion.div variants={fadeUp} className="flex flex-col sm:flex-row gap-4 mb-8">
            <div className="relative flex-1">
              <Search size={16} className="absolute left-4 top-1/2 -translate-y-1/2 text-muted-foreground" />
              <input
                type="text"
                value={search}
                onChange={e => setSearch(e.target.value)}
                placeholder="Buscar produtos..."
                className="w-full pl-11 pr-4 py-3 rounded-xl bg-card border border-border text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 font-body text-sm transition-all"
              />
            </div>
            <div className="flex items-center gap-2 overflow-x-auto pb-1">
              <SlidersHorizontal size={14} className="text-muted-foreground flex-shrink-0" />
              <button
                onClick={() => setSelectedCategory(null)}
                className={`px-4 py-2 rounded-lg text-xs font-mono whitespace-nowrap transition-all ${
                  !selectedCategory ? 'bg-primary text-primary-foreground' : 'bg-secondary text-secondary-foreground hover:bg-muted'
                }`}
              >
                Todos
              </button>
              {categories?.map(cat => (
                <button
                  key={cat.id}
                  onClick={() => setSelectedCategory(cat.id)}
                  className={`px-4 py-2 rounded-lg text-xs font-mono whitespace-nowrap transition-all ${
                    selectedCategory === cat.id ? 'bg-primary text-primary-foreground' : 'bg-secondary text-secondary-foreground hover:bg-muted'
                  }`}
                >
                  {cat.name}
                </button>
              ))}
            </div>
          </motion.div>

          {/* Results count */}
          <motion.p variants={fadeUp} className="text-xs text-muted-foreground font-mono mb-6">
            {totalItems} produto{totalItems !== 1 ? 's' : ''} encontrado{totalItems !== 1 ? 's' : ''}
          </motion.p>

          {/* Grid */}
          {isLoading ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {Array.from({ length: 8 }).map((_, i) => (
                <ProductSkeleton key={i} />
              ))}
            </div>
          ) : products.length === 0 ? (
            <motion.div variants={fadeUp} className="flex flex-col items-center justify-center py-24 text-center">
              <Search size={48} className="text-muted-foreground/30 mb-4" />
              <p className="text-muted-foreground">Nenhum produto encontrado</p>
              <button onClick={() => { setSearch(''); setSelectedCategory(null); }} className="mt-4 text-primary text-sm hover:underline">
                Limpar filtros
              </button>
            </motion.div>
          ) : (
            <>
              {isFetching && (
                <motion.p variants={fadeUp} className="mb-4 text-xs font-mono text-muted-foreground">
                  Atualizando resultados...
                </motion.p>
              )}
              <motion.div variants={stagger} className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {products.map(product => (
                  <motion.div key={product.id} variants={fadeUp}>
                    <ProductCard product={product} />
                  </motion.div>
                ))}
              </motion.div>

              {totalPages > 1 && (
                <motion.div variants={fadeUp} className="mt-10 space-y-3">
                  <p className="text-center text-xs font-mono text-muted-foreground">
                    Página {currentPage} de {totalPages}
                  </p>
                  <Pagination>
                    <PaginationContent>
                      <PaginationItem>
                        <PaginationPrevious
                          href="#"
                          onClick={(event) => {
                            event.preventDefault();
                            setCurrentPage((page) => Math.max(1, page - 1));
                          }}
                          className={currentPage === 1 ? 'pointer-events-none opacity-50' : ''}
                        />
                      </PaginationItem>

                      {visiblePages.map((page, index) => {
                        const previousPage = visiblePages[index - 1];
                        const shouldShowEllipsis = previousPage && page - previousPage > 1;

                        return (
                          <Fragment key={page}>
                            {shouldShowEllipsis && (
                              <PaginationItem>
                                <PaginationEllipsis />
                              </PaginationItem>
                            )}
                            <PaginationItem>
                              <PaginationLink
                                href="#"
                                isActive={page === currentPage}
                                onClick={(event) => {
                                  event.preventDefault();
                                  setCurrentPage(page);
                                }}
                              >
                                {page}
                              </PaginationLink>
                            </PaginationItem>
                          </Fragment>
                        );
                      })}

                      <PaginationItem>
                        <PaginationNext
                          href="#"
                          onClick={(event) => {
                            event.preventDefault();
                            setCurrentPage((page) => Math.min(totalPages, page + 1));
                          }}
                          className={currentPage === totalPages ? 'pointer-events-none opacity-50' : ''}
                        />
                      </PaginationItem>
                    </PaginationContent>
                  </Pagination>
                </motion.div>
              )}
            </>
          )}
        </motion.div>
      </div>
    </div>
  );
};

export default Catalog;
