import { useParams, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Minus, Plus, ShoppingBag, ArrowLeft, Package } from 'lucide-react';
import { useState } from 'react';
import { useProduct, useProducts } from '@/hooks/useData';
import { useCart } from '@/store/useCart';
import { formatCurrency } from '@/utils/format';
import ProductVisual from '@/components/canvas/ProductVisual';
import ProductCard from '@/components/product/ProductCard';
import { toast } from 'sonner';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0, transition: { duration: 0.5, ease: [0.2, 0, 0, 1] } },
};

const ProductDetail = () => {
  const { id } = useParams<{ id: string }>();
  const { data: product, isLoading } = useProduct(id || '');
  const { data: allProducts } = useProducts();
  const addItem = useCart(s => s.addItem);
  const [qty, setQty] = useState(1);

  const related = allProducts
    ?.filter(p => p.categoryId === product?.categoryId && p.id !== product?.id)
    .slice(0, 4);

  if (isLoading) {
    return (
      <div className="min-h-screen pt-24 flex items-center justify-center">
        <div className="w-8 h-8 rounded-full border-2 border-primary border-t-transparent animate-spin" />
      </div>
    );
  }

  if (!product) {
    return (
      <div className="min-h-screen pt-24 flex flex-col items-center justify-center gap-4">
        <Package size={48} className="text-muted-foreground/30" />
        <p className="text-muted-foreground">Produto não encontrado</p>
        <Link to="/catalog" className="text-primary text-sm hover:underline">Voltar ao catálogo</Link>
      </div>
    );
  }

  const handleAdd = async () => {
    try {
      await addItem(product, qty);
      toast.success(`${qty}x ${product.name} adicionado ao carrinho`);
      setQty(1);
    } catch {
      toast.error('Nao foi possivel adicionar o produto ao carrinho.');
    }
  };

  const isOutOfStock = product.stockQuantity === 0;
  const isLowStock = product.stockQuantity > 0 && product.stockQuantity <= 5;

  return (
    <motion.div initial="hidden" animate="show" className="min-h-screen pt-24 lg:pt-28 pb-12 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Breadcrumb */}
        <motion.div variants={fadeUp} className="mb-8">
          <Link to="/catalog" className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
            <ArrowLeft size={14} /> Voltar ao catálogo
          </Link>
        </motion.div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-16">
          {/* Visual */}
          <motion.div variants={fadeUp} className="lg:sticky lg:top-28 lg:self-start">
            <ProductVisual seed={product.id} size="lg" />
          </motion.div>

          {/* Info */}
          <motion.div variants={fadeUp} className="flex flex-col justify-center space-y-8">
            <div>
              {product.categoryName && (
                <p className="text-primary font-mono text-sm mb-2">
                  {product.categoryName} // {product.id.slice(0, 6).toUpperCase()}
                </p>
              )}
              <h1 className="text-4xl sm:text-5xl lg:text-6xl font-display font-bold tracking-tighter leading-[0.95]">
                {product.name}
              </h1>
            </div>

            <p className="text-lg text-muted-foreground leading-relaxed max-w-prose font-body">
              {product.description}
            </p>

            <div className="flex items-baseline gap-4">
              <span className="text-4xl font-display font-light tracking-tight">
                {formatCurrency(product.price)}
              </span>
            </div>

            {/* Stock status */}
            <div className="flex items-center gap-3">
              <div className={`w-2 h-2 rounded-full ${isOutOfStock ? 'bg-destructive' : isLowStock ? 'bg-yellow-500' : 'bg-emerald-500'}`} />
              <span className="text-sm font-mono text-muted-foreground">
                {isOutOfStock ? 'ESGOTADO' : isLowStock ? `${product.stockQuantity} UNIDADES RESTANTES` : `${product.stockQuantity} EM ESTOQUE`}
              </span>
            </div>

            {/* Quantity + Add */}
            {!isOutOfStock && (
              <div className="flex items-center gap-4 pt-4">
                <div className="flex items-center gap-3 px-4 py-2 rounded-xl bg-secondary">
                  <button onClick={() => setQty(Math.max(1, qty - 1))} className="p-1 text-muted-foreground hover:text-foreground transition-colors">
                    <Minus size={16} />
                  </button>
                  <span className="font-mono text-sm w-8 text-center">{qty}</span>
                  <button onClick={() => setQty(Math.min(product.stockQuantity, qty + 1))} className="p-1 text-muted-foreground hover:text-foreground transition-colors">
                    <Plus size={16} />
                  </button>
                </div>
                <button
                  onClick={handleAdd}
                  className="flex-1 flex items-center justify-center gap-2 px-8 py-4 rounded-xl bg-primary text-primary-foreground font-medium btn-physical hover:opacity-90 transition-all active:scale-[0.98]"
                >
                  <ShoppingBag size={18} />
                  Adicionar ao Carrinho
                </button>
              </div>
            )}

            {/* Specs */}
            <div className="pt-8 border-t border-border space-y-4">
              <h3 className="text-sm font-mono text-muted-foreground uppercase tracking-widest">Especificações</h3>
              <div className="grid grid-cols-2 gap-4">
                {[
                  { label: 'ID', value: product.id.slice(0, 8) },
                  { label: 'Categoria', value: product.categoryName || '—' },
                  { label: 'Estoque', value: `${product.stockQuantity} un.` },
                  { label: 'Status', value: product.active ? 'Ativo' : 'Inativo' },
                ].map(spec => (
                  <div key={spec.label} className="p-3 rounded-lg bg-secondary/50">
                    <p className="text-xs text-muted-foreground font-mono">{spec.label}</p>
                    <p className="text-sm font-medium mt-0.5">{spec.value}</p>
                  </div>
                ))}
              </div>
            </div>
          </motion.div>
        </div>

        {/* Related */}
        {related && related.length > 0 && (
          <motion.div initial={{ opacity: 0 }} whileInView={{ opacity: 1 }} viewport={{ once: true }} className="mt-24">
            <h2 className="text-2xl font-display font-bold tracking-tight mb-8">Relacionados</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
              {related.map(p => (
                <ProductCard key={p.id} product={p} />
              ))}
            </div>
          </motion.div>
        )}
      </div>
    </motion.div>
  );
};

export default ProductDetail;
