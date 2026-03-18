import { motion, AnimatePresence } from 'framer-motion';
import { X, Minus, Plus, ShoppingBag, ArrowRight } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useCart } from '@/store/useCart';
import { formatCurrency } from '@/utils/format';
import { toast } from 'sonner';

interface CartDrawerProps {
  open: boolean;
  onClose: () => void;
}

const CartDrawer = ({ open, onClose }: CartDrawerProps) => {
  const { items, removeItem, updateQuantity, subtotal } = useCart();
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

  return (
    <AnimatePresence>
      {open && (
        <>
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
            className="fixed inset-0 bg-background/60 backdrop-blur-sm z-50"
          />
          <motion.div
            initial={{ x: '100%' }}
            animate={{ x: 0 }}
            exit={{ x: '100%' }}
            transition={{ type: 'spring', damping: 30, stiffness: 300 }}
            className="fixed top-0 right-0 bottom-0 w-full max-w-md z-50 glass-panel flex flex-col"
          >
            <div className="flex items-center justify-between p-6 border-b border-border">
              <div className="flex items-center gap-3">
                <ShoppingBag size={20} className="text-primary" />
                <h2 className="text-lg font-display font-semibold">Carrinho</h2>
                <span className="text-xs font-mono text-muted-foreground">
                  {items.length} {items.length === 1 ? 'item' : 'itens'}
                </span>
              </div>
              <button onClick={onClose} className="p-2 rounded-lg hover:bg-secondary transition-colors text-muted-foreground hover:text-foreground">
                <X size={18} />
              </button>
            </div>

            <div className="flex-1 overflow-y-auto p-6 space-y-4">
              {items.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full text-center">
                  <ShoppingBag size={48} className="text-muted-foreground/30 mb-4" />
                  <p className="text-muted-foreground text-sm">Seu carrinho está vazio</p>
                  <Link
                    to="/catalog"
                    onClick={onClose}
                    className="mt-4 text-primary text-sm flex items-center gap-1 hover:gap-2 transition-all"
                  >
                    Explorar catálogo <ArrowRight size={14} />
                  </Link>
                </div>
              ) : (
                <AnimatePresence>
                  {items.map(item => (
                    <motion.div
                      key={item.product.id}
                      layout
                      initial={{ opacity: 0, x: 20 }}
                      animate={{ opacity: 1, x: 0 }}
                      exit={{ opacity: 0, x: -20 }}
                      className="flex gap-4 p-4 rounded-xl bg-secondary/50 border-glow"
                    >
                      <div className="w-16 h-16 rounded-lg bg-card flex items-center justify-center flex-shrink-0">
                        <span className="text-2xl font-display font-bold text-gradient-primary">
                          {item.product.name.charAt(0)}
                        </span>
                      </div>
                      <div className="flex-1 min-w-0">
                        <h3 className="text-sm font-medium truncate">{item.product.name}</h3>
                        <p className="text-sm text-primary font-mono mt-0.5">
                          {formatCurrency(item.product.price)}
                        </p>
                        <div className="flex items-center gap-2 mt-2">
                          <button
                            onClick={() => { void handleQuantityChange(item.product.id, item.quantity - 1); }}
                            className="w-7 h-7 rounded-md bg-card flex items-center justify-center hover:bg-muted transition-colors"
                          >
                            <Minus size={12} />
                          </button>
                          <span className="text-sm font-mono w-6 text-center">{item.quantity}</span>
                          <button
                            onClick={() => { void handleQuantityChange(item.product.id, item.quantity + 1); }}
                            className="w-7 h-7 rounded-md bg-card flex items-center justify-center hover:bg-muted transition-colors"
                          >
                            <Plus size={12} />
                          </button>
                          <button
                            onClick={() => { void handleRemove(item.product.id); }}
                            className="ml-auto text-muted-foreground hover:text-destructive transition-colors"
                          >
                            <X size={14} />
                          </button>
                        </div>
                      </div>
                    </motion.div>
                  ))}
                </AnimatePresence>
              )}
            </div>

            {items.length > 0 && (
              <div className="p-6 border-t border-border space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-muted-foreground text-sm">Total</span>
                  <span className="text-xl font-display font-semibold">{formatCurrency(total)}</span>
                </div>
                <Link
                  to="/checkout"
                  onClick={onClose}
                  className="w-full flex items-center justify-center gap-2 px-6 py-3.5 rounded-xl bg-primary text-primary-foreground font-medium btn-physical hover:opacity-90 transition-all active:scale-[0.98]"
                >
                  Finalizar Compra <ArrowRight size={16} />
                </Link>
              </div>
            )}
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
};

export default CartDrawer;
