import { Fragment, useEffect, useMemo, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ArrowLeft } from 'lucide-react';
import { useCategory, usePagedProducts } from '@/hooks/useData';
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
const stagger = { hidden: {}, show: { transition: { staggerChildren: 0.06 } } };
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

const CategoryProducts = () => {
  const { id } = useParams<{ id: string }>();
  const { data: category } = useCategory(id || '');
  const [currentPage, setCurrentPage] = useState(1);
  const { data: pagedProducts, isLoading, isFetching } = usePagedProducts(currentPage, PRODUCTS_PER_PAGE, {
    categoryId: id || null,
  });
  const products = pagedProducts?.items ?? [];
  const totalItems = pagedProducts?.pagination?.totalItems ?? 0;
  const totalPages = Math.max(1, pagedProducts?.pagination?.totalPages ?? 1);
  const visiblePages = useMemo(
    () => buildVisiblePages(currentPage, totalPages),
    [currentPage, totalPages]
  );

  useEffect(() => {
    setCurrentPage(1);
  }, [id]);

  return (
    <div className="min-h-screen pt-24 lg:pt-28 pb-12 px-4">
      <div className="max-w-7xl mx-auto">
        <motion.div initial="hidden" animate="show" variants={stagger}>
          <motion.div variants={fadeUp} className="mb-4">
            <Link to="/categories" className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
              <ArrowLeft size={14} /> Todas as categorias
            </Link>
          </motion.div>

          <motion.div variants={fadeUp} className="mb-12">
            <p className="text-primary font-mono text-xs tracking-widest uppercase mb-2">Categoria</p>
            <h1 className="text-4xl sm:text-5xl font-display font-bold tracking-tight">
              {category?.name || 'Carregando...'}
            </h1>
            {category?.description && (
              <p className="text-lg text-muted-foreground mt-3">{category.description}</p>
            )}
            <p className="text-xs text-muted-foreground font-mono mt-4">
              {totalItems} produto{totalItems !== 1 ? 's' : ''}
            </p>
          </motion.div>

          {isLoading ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {Array.from({ length: 4 }).map((_, i) => <ProductSkeleton key={i} />)}
            </div>
          ) : products.length === 0 ? (
            <motion.div variants={fadeUp} className="text-center py-24">
              <p className="text-muted-foreground">Nenhum produto nesta categoria</p>
            </motion.div>
          ) : (
            <>
              {isFetching && (
                <motion.p variants={fadeUp} className="mb-4 text-xs font-mono text-muted-foreground">
                  Atualizando resultados...
                </motion.p>
              )}
              <motion.div variants={stagger} className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {products.map(p => (
                  <motion.div key={p.id} variants={fadeUp}>
                    <ProductCard product={p} />
                  </motion.div>
                ))}
              </motion.div>

              {totalPages > 1 && (
                <motion.div variants={fadeUp} className="mt-10 space-y-3">
                  <p className="text-center text-xs font-mono text-muted-foreground">
                    Pagina {currentPage} de {totalPages}
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

export default CategoryProducts;
