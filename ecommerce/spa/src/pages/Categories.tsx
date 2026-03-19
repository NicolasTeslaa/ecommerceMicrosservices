import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import { ArrowRight } from 'lucide-react';
import { useQueries } from '@tanstack/react-query';
import { useCategories } from '@/hooks/useData';
import { productService } from '@/services/backendApi';
import { hashString } from '@/utils/format';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0, transition: { duration: 0.5 } },
};
const stagger = { hidden: {}, show: { transition: { staggerChildren: 0.08 } } };

const Categories = () => {
  const { data: categories, isLoading } = useCategories();
  const categoryCountQueries = useQueries({
    queries: (categories ?? []).map((category) => ({
      queryKey: ['category-product-count', category.id],
      queryFn: async () => {
        const result = await productService.getPage(1, 1, { categoryId: category.id });
        return result.pagination?.totalItems ?? 0;
      },
      enabled: !!categories?.length,
      staleTime: 60_000,
    })),
  });

  const countByCategory = (index: number) => categoryCountQueries[index]?.data ?? 0;
  const countsAreLoading = categoryCountQueries.some((query) => query.isLoading);

  return (
    <div className="min-h-screen pt-24 lg:pt-28 pb-12 px-4">
      <div className="max-w-7xl mx-auto">
        <motion.div initial="hidden" animate="show" variants={stagger}>
          <motion.div variants={fadeUp} className="mb-12">
            <p className="text-primary font-mono text-xs tracking-widest uppercase mb-2">Explorar</p>
            <h1 className="text-4xl sm:text-5xl font-display font-bold tracking-tight">Categorias</h1>
          </motion.div>

          {isLoading ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="h-48 bg-card border-glow rounded-2xl animate-pulse" />
              ))}
            </div>
          ) : (
            <motion.div variants={stagger} className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
              {categories?.map((cat, i) => {
                const h = hashString(cat.id);
                const hue = h % 360;
                return (
                  <motion.div key={cat.id} variants={fadeUp}>
                    <Link
                      to={`/categories/${cat.id}`}
                      className="group block p-8 rounded-2xl bg-card border-glow hover:glow-primary transition-all duration-500 relative overflow-hidden"
                    >
                      <div
                        className="absolute top-0 right-0 w-40 h-40 rounded-full blur-3xl opacity-10 group-hover:opacity-20 transition-opacity"
                        style={{ background: `hsl(${hue}, 70%, 50%)` }}
                      />
                      <div className="relative z-10">
                        <span className="text-xs font-mono text-muted-foreground">0{i + 1}</span>
                        <h2 className="text-2xl font-display font-bold mt-2 group-hover:text-primary transition-colors">
                          {cat.name}
                        </h2>
                        {cat.description && (
                          <p className="text-sm text-muted-foreground mt-2 leading-relaxed">{cat.description}</p>
                        )}
                        <div className="flex items-center justify-between mt-6">
                          <span className="text-xs font-mono text-muted-foreground">
                            {countsAreLoading ? 'Carregando...' : `${countByCategory(i)} produto${countByCategory(i) !== 1 ? 's' : ''}`}
                          </span>
                          <span className="flex items-center gap-1 text-xs text-primary opacity-0 group-hover:opacity-100 transition-opacity">
                            Explorar <ArrowRight size={12} />
                          </span>
                        </div>
                      </div>
                    </Link>
                  </motion.div>
                );
              })}
            </motion.div>
          )}
        </motion.div>
      </div>
    </div>
  );
};

export default Categories;
