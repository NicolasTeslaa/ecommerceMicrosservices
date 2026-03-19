import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import { CheckCircle, ArrowRight, Package } from 'lucide-react';
import { useOrder } from '@/store/useOrder';
import { formatCurrency } from '@/utils/format';

const Confirmation = () => {
  const { lastOrder } = useOrder();

  if (!lastOrder) {
    return (
      <div className="min-h-screen pt-24 flex flex-col items-center justify-center gap-4 px-4">
        <Package size={48} className="text-muted-foreground/30" />
        <p className="text-muted-foreground">Nenhum pedido encontrado</p>
        <Link to="/" className="text-primary text-sm hover:underline">Voltar ao início</Link>
      </div>
    );
  }

  return (
    <div className="min-h-screen pt-24 lg:pt-28 pb-12 px-4 bg-gradient-hero relative">
      <div className="absolute inset-0 bg-background/80" />
      <div className="relative z-10 max-w-2xl mx-auto">
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.6, ease: [0.2, 0, 0, 1] }}
          className="text-center mb-10"
        >
          <motion.div
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            transition={{ delay: 0.2, type: 'spring', stiffness: 200, damping: 15 }}
            className="w-20 h-20 rounded-full bg-emerald-500/20 flex items-center justify-center mx-auto mb-6"
          >
            <CheckCircle size={40} className="text-emerald-500" />
          </motion.div>
          <h1 className="text-3xl sm:text-4xl font-display font-bold tracking-tight mb-2">
            Pedido Recebido!
          </h1>
          <p className="text-muted-foreground">
            Seu pedido sera processado em instantes e voce sera notificado apos a conclusao
          </p>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="p-8 rounded-2xl glass-panel space-y-6"
        >
          {/* Order info */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-xs font-mono text-muted-foreground uppercase">Pedido</p>
              <p className="font-mono font-medium mt-1">{lastOrder.orderId}</p>
            </div>
            <div>
              <p className="text-xs font-mono text-muted-foreground uppercase">Status</p>
              <p className="text-emerald-500 font-medium mt-1 flex items-center gap-1">
                <span className="w-2 h-2 rounded-full bg-emerald-500" />
                {lastOrder.status === 'approved' ? 'Aprovado' : 'Pendente'}
              </p>
            </div>
            <div>
              <p className="text-xs font-mono text-muted-foreground uppercase">Pagamento</p>
              <p className="font-medium mt-1">{lastOrder.paymentMethod}</p>
            </div>
            <div>
              <p className="text-xs font-mono text-muted-foreground uppercase">Data</p>
              <p className="font-medium mt-1">
                {new Date(lastOrder.date).toLocaleDateString('pt-BR')}
              </p>
            </div>
          </div>

          <div className="border-t border-border pt-4">
            <h3 className="text-sm font-mono text-muted-foreground uppercase mb-3">Itens</h3>
            <div className="space-y-3">
              {lastOrder.items.map(item => (
                <div key={item.product.id} className="flex justify-between text-sm">
                  <span className="text-muted-foreground">
                    {item.quantity}x {item.product.name}
                  </span>
                  <span>{formatCurrency(item.product.price * item.quantity)}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="border-t border-border pt-4 flex justify-between items-center">
            <span className="font-medium">Total do Pedido</span>
            <span className="text-2xl font-display font-bold text-gradient-primary">
              {formatCurrency(lastOrder.total)}
            </span>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.5 }}
          className="flex flex-col sm:flex-row gap-4 justify-center mt-8"
        >
          <Link
            to="/"
            className="inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl bg-primary text-primary-foreground font-medium btn-physical hover:opacity-90 transition-all"
          >
            Voltar ao Início <ArrowRight size={16} />
          </Link>
          <Link
            to="/catalog"
            className="inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl bg-secondary text-secondary-foreground font-medium btn-physical hover:bg-muted transition-all"
          >
            Continuar Comprando
          </Link>
        </motion.div>
      </div>
    </div>
  );
};

export default Confirmation;
