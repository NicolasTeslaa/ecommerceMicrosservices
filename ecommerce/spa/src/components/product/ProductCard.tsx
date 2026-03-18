import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { useCart } from '@/store/useCart';
import { formatCurrency, hashString } from '@/utils/format';
import type { Product } from '@/types';
import { toast } from 'sonner';
import { useState } from 'react';

const ProductCard = ({ product }: { product: Product }) => {
  const addItem = useCart(s => s.addItem);
  const [isAdding, setIsAdding] = useState(false);
  const h = hashString(product.id);
  const gradientHue = h % 360;
  const isLowStock = product.stockQuantity > 0 && product.stockQuantity <= 5;
  const isOutOfStock = product.stockQuantity === 0;

  const handleAdd = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (isOutOfStock || isAdding) return;

    setIsAdding(true);

    try {
      await addItem(product, 1);
      toast.success(`${product.name} adicionado ao carrinho`);
    } catch {
      toast.error('Nao foi possivel adicionar o produto ao carrinho.');
    } finally {
      setIsAdding(false);
    }
  };

  return (
    <Link to={`/product/${product.id}`}>
      <motion.div
        whileHover={{ y: -6 }}
        transition={{ type: 'spring', stiffness: 300, damping: 25 }}
        className="group relative bg-card border-glow p-4 rounded-3xl overflow-hidden"
      >
        {/* Visual */}
        <div
          className="aspect-square rounded-2xl mb-4 flex items-center justify-center relative overflow-hidden"
          style={{
            background: `linear-gradient(135deg, hsl(${gradientHue}, 60%, 15%), hsl(${(gradientHue + 60) % 360}, 50%, 10%))`,
          }}
        >
          <span
            className="text-7xl font-display font-bold opacity-20 select-none"
            style={{ color: `hsl(${gradientHue}, 70%, 60%)` }}
          >
            {product.name.charAt(0)}
          </span>
          <div
            className="absolute w-32 h-32 rounded-full blur-3xl opacity-30 animate-pulse_glow"
            style={{ background: `hsl(${gradientHue}, 70%, 50%)` }}
          />

          {/* Badges */}
          {isLowStock && (
            <span className="absolute top-3 left-3 px-2 py-1 rounded-md bg-destructive/20 text-destructive text-xs font-mono">
              {product.stockQuantity} restantes
            </span>
          )}
          {isOutOfStock && (
            <span className="absolute top-3 left-3 px-2 py-1 rounded-md bg-muted text-muted-foreground text-xs font-mono">
              Esgotado
            </span>
          )}

          {/* Add button */}
          {!isOutOfStock && (
            <button
              onClick={handleAdd}
              disabled={isAdding}
              className="absolute bottom-3 right-3 opacity-0 group-hover:opacity-100 translate-y-2 group-hover:translate-y-0 transition-all p-3 rounded-full bg-primary text-primary-foreground btn-physical hover:scale-105 active:scale-95"
            >
              <Plus size={18} />
            </button>
          )}
        </div>

        {/* Info */}
        <div className="space-y-1.5 px-1">
          <div className="flex justify-between items-start gap-2">
            <h3 className="text-sm font-medium group-hover:text-primary transition-colors line-clamp-1">
              {product.name}
            </h3>
            <span className="font-mono text-[10px] text-muted-foreground flex-shrink-0 mt-0.5">
              {isOutOfStock ? 'ESGOTADO' : 'DISPONÍVEL'}
            </span>
          </div>
          {product.categoryName && (
            <p className="text-xs text-muted-foreground font-mono">{product.categoryName}</p>
          )}
          <p className="text-xl font-display font-light tracking-tight">
            {formatCurrency(product.price)}
          </p>
        </div>
      </motion.div>
    </Link>
  );
};

export default ProductCard;
