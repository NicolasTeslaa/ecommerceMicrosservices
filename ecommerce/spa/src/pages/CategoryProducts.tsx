import { useParams, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ArrowLeft } from 'lucide-react';
import { useCategory, useProducts } from '@/hooks/useData';
import ProductCard from '@/components/product/ProductCard';
import ProductSkeleton from '@/components/product/ProductSkeleton';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0, transition: { duration: 0.5 } },
};
const stagger = { hidden: {}, show: { transition: { staggerChildren: 0.06 } } };

const CategoryProducts = () => {
  const { id } = useParams<{ id: string }>();
  const { data: category } = useCategory(id || '');
  const { data: products, isLoading } = useProducts();

  const filtered = products?.filter(p => p.categoryId === id) || [];

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
          </motion.div>

          {isLoading ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {Array.from({ length: 4 }).map((_, i) => <ProductSkeleton key={i} />)}
            </div>
          ) : filtered.length === 0 ? (
            <motion.div variants={fadeUp} className="text-center py-24">
              <p className="text-muted-foreground">Nenhum produto nesta categoria</p>
            </motion.div>
          ) : (
            <motion.div variants={stagger} className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {filtered.map(p => (
                <motion.div key={p.id} variants={fadeUp}>
                  <ProductCard product={p} />
                </motion.div>
              ))}
            </motion.div>
          )}
        </motion.div>
      </div>
    </div>
  );
};

export default CategoryProducts;
