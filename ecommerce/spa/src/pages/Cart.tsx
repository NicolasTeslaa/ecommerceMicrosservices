import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import { Minus, Plus, X, ShoppingBag, ArrowRight, ArrowLeft } from 'lucide-react';
import { useCart } from '@/store/useCart';
import { formatCurrency } from '@/utils/format';
import { toast } from 'sonner';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0, transition: { duration: 0.5 } },
};
const stagger = { hidden: {}, show: { transition: { staggerChildren: 0.06 } } };

const Cart = () => {
  const { items, removeItem, updateQuantity, subtotal, clearCart } = useCart();
  const total = subtotal();

  const handleQuantityChange = async (productId: string, quantity: number) => {
    try {
      await updateQuantity(productId, quantity);
    } catch {
      toast.error('Nao foi possivel atualizar a quantidade do item.');
    }
  };

  const handleRemove = async (productId: string) => {
    try {
      await removeItem(productId);
    } catch {
      toast.error('Nao foi possivel remover o item do carrinho.');
    }
  };

  const handleClear = async () => {
    try {
      await clearCart();
    } catch {
      toast.error('Nao foi possivel limpar o carrinho.');
    }
  };

  return (
    <div className="min-h-screen pt-24 lg:pt-28 pb-12 px-4">
      <div className="max-w-6xl mx-auto">
        <motion.div initial="hidden" animate="show" variants={stagger}>
          <motion.div variants={fadeUp} className="mb-4">
            <Link to="/catalog" className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
              <ArrowLeft size={14} /> Continuar comprando
            </Link>
          </motion.div>

          <motion.div variants={fadeUp} className="flex items-end justify-between mb-12">
            <div>
              <p className="text-primary font-mono text-xs tracking-widest uppercase mb-2">Seu Carrinho</p>
              <h1 className="text-4xl sm:text-5xl font-display font-bold tracking-tight">Carrinho</h1>
            </div>
            {items.length > 0 && (
              <button onClick={() => { void handleClear(); }} className="text-xs text-muted-foreground hover:text-destructive transition-colors font-mono">
                Limpar tudo
              </button>
            )}
          </motion.div>

          {items.length === 0 ? (
            <motion.div variants={fadeUp} className="flex flex-col items-center justify-center py-24 text-center">
              <ShoppingBag size={64} className="text-muted-foreground/20 mb-6" />
              <h2 className="text-xl font-display font-semibold mb-2">Carrinho vazio</h2>
              <p className="text-muted-foreground text-sm mb-6">Adicione produtos para começar</p>
              <Link
                to="/catalog"
                className="inline-flex items-center gap-2 px-6 py-3 rounded-xl bg-primary text-primary-foreground font-medium btn-physical hover:opacity-90 transition-all"
              >
                Explorar Catálogo <ArrowRight size={16} />
              </Link>
            </motion.div>
          ) : (
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
              {/* Items */}
              <motion.div variants={stagger} className="lg:col-span-2 space-y-4">
                {items.map(item => (
                  <motion.div
                    key={item.product.id}
                    variants={fadeUp}
                    layout
                    className="flex gap-4 sm:gap-6 p-5 rounded-2xl bg-card border-glow"
                  >
                    <div className="w-20 h-20 sm:w-24 sm:h-24 rounded-xl bg-secondary flex items-center justify-center flex-shrink-0">
                      <span className="text-3xl font-display font-bold text-gradient-primary">
                        {item.product.name.charAt(0)}
                      </span>
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex justify-between items-start">
                        <div>
                          <Link to={`/product/${item.product.id}`} className="font-medium hover:text-primary transition-colors line-clamp-1">
                            {item.product.name}
                          </Link>
                          <p className="text-xs text-muted-foreground font-mono mt-0.5">{item.product.categoryName}</p>
                        </div>
                        <button
                          onClick={() => { void handleRemove(item.product.id); }}
                          className="p-1 text-muted-foreground hover:text-destructive transition-colors"
                        >
                          <X size={16} />
                        </button>
                      </div>
                      <div className="flex items-center justify-between mt-4">
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => { void handleQuantityChange(item.product.id, item.quantity - 1); }}
                            className="w-8 h-8 rounded-lg bg-secondary flex items-center justify-center hover:bg-muted transition-colors"
                          >
                            <Minus size={14} />
                          </button>
                          <span className="font-mono text-sm w-8 text-center">{item.quantity}</span>
                          <button
                            onClick={() => { void handleQuantityChange(item.product.id, item.quantity + 1); }}
                            className="w-8 h-8 rounded-lg bg-secondary flex items-center justify-center hover:bg-muted transition-colors"
                          >
                            <Plus size={14} />
                          </button>
                        </div>
                        <span className="font-display text-lg font-light">
                          {formatCurrency(item.product.price * item.quantity)}
                        </span>
                      </div>
                    </div>
                  </motion.div>
                ))}
              </motion.div>

              {/* Summary */}
              <motion.div variants={fadeUp} className="lg:sticky lg:top-28 lg:self-start">
                <div className="p-6 rounded-2xl bg-card border-glow space-y-6">
                  <h3 className="font-display font-semibold">Resumo do Pedido</h3>
                  <div className="space-y-3">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Subtotal</span>
                      <span>{formatCurrency(total)}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Frete</span>
                      <span className="text-primary font-mono text-xs">GRÁTIS</span>
                    </div>
                    <div className="border-t border-border pt-3 flex justify-between">
                      <span className="font-medium">Total</span>
                      <span className="text-xl font-display font-semibold">{formatCurrency(total)}</span>
                    </div>
                  </div>
                  <Link
                    to="/checkout"
                    className="w-full flex items-center justify-center gap-2 px-6 py-3.5 rounded-xl bg-primary text-primary-foreground font-medium btn-physical hover:opacity-90 transition-all active:scale-[0.98]"
                  >
                    Finalizar Compra <ArrowRight size={16} />
                  </Link>
                </div>
              </motion.div>
            </div>
          )}
        </motion.div>
      </div>
    </div>
  );
};

export default Cart;
