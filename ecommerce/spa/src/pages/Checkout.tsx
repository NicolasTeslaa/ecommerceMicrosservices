import { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, Link } from 'react-router-dom';
import { CreditCard, QrCode, ArrowLeft, ArrowRight, Loader2 } from 'lucide-react';
import { useCart } from '@/store/useCart';
import { useAuth } from '@/store/useAuth';
import { useOrder } from '@/store/useOrder';
import { formatCurrency, generateOrderId } from '@/utils/format';
import type { CheckoutData, OrderConfirmation } from '@/types';
import { toast } from 'sonner';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0, transition: { duration: 0.5 } },
};

const Checkout = () => {
  const navigate = useNavigate();
  const { items, subtotal, clearCart } = useCart();
  const { isAuthenticated } = useAuth();
  const { setLastOrder } = useOrder();
  const total = subtotal();
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState<CheckoutData>({
    fullName: '', email: '', address: '', city: '', zipCode: '',
    paymentMethod: 'credit', cardNumber: '', cardExpiry: '', cardCvv: '',
  });

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen pt-24 flex flex-col items-center justify-center gap-4 px-4">
        <h2 className="text-2xl font-display font-bold">Faça login para continuar</h2>
        <p className="text-muted-foreground text-sm">Você precisa estar logado para finalizar a compra.</p>
        <Link to="/login" className="mt-4 inline-flex items-center gap-2 px-6 py-3 rounded-xl bg-primary text-primary-foreground font-medium btn-physical">
          Entrar <ArrowRight size={16} />
        </Link>
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className="min-h-screen pt-24 flex flex-col items-center justify-center gap-4 px-4">
        <h2 className="text-2xl font-display font-bold">Carrinho vazio</h2>
        <Link to="/catalog" className="text-primary text-sm hover:underline">Voltar ao catálogo</Link>
      </div>
    );
  }

  const update = (field: keyof CheckoutData, value: string) =>
    setForm(prev => ({ ...prev, [field]: value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.fullName || !form.email || !form.address || !form.city || !form.zipCode) {
      toast.error('Preencha todos os campos obrigatórios');
      return;
    }
    setLoading(true);
    // Simulate payment processing
    await new Promise(r => setTimeout(r, 3000));

    const order: OrderConfirmation = {
      orderId: generateOrderId(),
      items: [...items],
      total,
      paymentMethod: form.paymentMethod === 'pix' ? 'PIX' : form.paymentMethod === 'debit' ? 'Débito' : 'Crédito',
      status: 'approved',
      date: new Date().toISOString(),
    };
    setLastOrder(order);

    try {
      await clearCart();
    } catch {
      toast.error('Nao foi possivel sincronizar a limpeza do carrinho.');
    } finally {
      setLoading(false);
      navigate('/confirmation');
    }
  };

  const inputClass = "w-full px-4 py-3 rounded-xl bg-card border border-border text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 text-sm transition-all";

  return (
    <div className="min-h-screen pt-24 lg:pt-28 pb-12 px-4">
      <div className="max-w-5xl mx-auto">
        <motion.div initial="hidden" animate="show" variants={{ hidden: {}, show: { transition: { staggerChildren: 0.08 } } }}>
          <motion.div variants={fadeUp} className="mb-4">
            <Link to="/cart" className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
              <ArrowLeft size={14} /> Voltar ao carrinho
            </Link>
          </motion.div>

          <motion.div variants={fadeUp} className="mb-12">
            <p className="text-primary font-mono text-xs tracking-widest uppercase mb-2">Finalizar</p>
            <h1 className="text-4xl sm:text-5xl font-display font-bold tracking-tight">Checkout</h1>
          </motion.div>

          {loading ? (
            <motion.div variants={fadeUp} className="flex flex-col items-center justify-center py-32 gap-6">
              <Loader2 size={48} className="text-primary animate-spin" />
              <div className="text-center">
                <h2 className="text-xl font-display font-semibold mb-2">Processando pagamento...</h2>
                <p className="text-muted-foreground text-sm">Aguarde enquanto confirmamos sua compra</p>
              </div>
              <div className="w-64 h-1 rounded-full bg-secondary overflow-hidden">
                <motion.div
                  initial={{ width: '0%' }}
                  animate={{ width: '100%' }}
                  transition={{ duration: 3, ease: 'linear' }}
                  className="h-full bg-primary rounded-full"
                />
              </div>
            </motion.div>
          ) : (
            <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-3 gap-8">
              {/* Form */}
              <motion.div variants={fadeUp} className="lg:col-span-2 space-y-8">
                {/* Shipping */}
                <div className="p-6 rounded-2xl bg-card border-glow space-y-4">
                  <h3 className="font-display font-semibold">Dados de Entrega</h3>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="sm:col-span-2">
                      <label className="text-xs font-mono text-muted-foreground block mb-1.5">Nome completo</label>
                      <input value={form.fullName} onChange={e => update('fullName', e.target.value)} className={inputClass} placeholder="Seu nome completo" />
                    </div>
                    <div className="sm:col-span-2">
                      <label className="text-xs font-mono text-muted-foreground block mb-1.5">Email</label>
                      <input type="email" value={form.email} onChange={e => update('email', e.target.value)} className={inputClass} placeholder="seu@email.com" />
                    </div>
                    <div className="sm:col-span-2">
                      <label className="text-xs font-mono text-muted-foreground block mb-1.5">Endereço</label>
                      <input value={form.address} onChange={e => update('address', e.target.value)} className={inputClass} placeholder="Rua, número, complemento" />
                    </div>
                    <div>
                      <label className="text-xs font-mono text-muted-foreground block mb-1.5">Cidade</label>
                      <input value={form.city} onChange={e => update('city', e.target.value)} className={inputClass} placeholder="Sua cidade" />
                    </div>
                    <div>
                      <label className="text-xs font-mono text-muted-foreground block mb-1.5">CEP</label>
                      <input value={form.zipCode} onChange={e => update('zipCode', e.target.value)} className={inputClass} placeholder="00000-000" />
                    </div>
                  </div>
                </div>

                {/* Payment */}
                <div className="p-6 rounded-2xl bg-card border-glow space-y-4">
                  <h3 className="font-display font-semibold">Forma de Pagamento</h3>
                  <div className="grid grid-cols-3 gap-3">
                    {[
                      { val: 'credit' as const, label: 'Crédito', icon: CreditCard },
                      { val: 'debit' as const, label: 'Débito', icon: CreditCard },
                      { val: 'pix' as const, label: 'PIX', icon: QrCode },
                    ].map(pm => (
                      <button
                        key={pm.val}
                        type="button"
                        onClick={() => update('paymentMethod', pm.val)}
                        className={`p-4 rounded-xl border text-sm font-medium flex flex-col items-center gap-2 transition-all ${
                          form.paymentMethod === pm.val
                            ? 'border-primary bg-primary/10 text-primary'
                            : 'border-border bg-card text-muted-foreground hover:border-primary/30'
                        }`}
                      >
                        <pm.icon size={20} />
                        {pm.label}
                      </button>
                    ))}
                  </div>

                  {form.paymentMethod !== 'pix' && (
                    <div className="space-y-4 pt-2">
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Número do cartão</label>
                        <input value={form.cardNumber} onChange={e => update('cardNumber', e.target.value)} className={inputClass} placeholder="0000 0000 0000 0000" />
                      </div>
                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <label className="text-xs font-mono text-muted-foreground block mb-1.5">Validade</label>
                          <input value={form.cardExpiry} onChange={e => update('cardExpiry', e.target.value)} className={inputClass} placeholder="MM/AA" />
                        </div>
                        <div>
                          <label className="text-xs font-mono text-muted-foreground block mb-1.5">CVV</label>
                          <input value={form.cardCvv} onChange={e => update('cardCvv', e.target.value)} className={inputClass} placeholder="123" />
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              </motion.div>

              {/* Order Summary */}
              <motion.div variants={fadeUp} className="lg:sticky lg:top-28 lg:self-start">
                <div className="p-6 rounded-2xl bg-card border-glow space-y-4">
                  <h3 className="font-display font-semibold">Resumo</h3>
                  <div className="space-y-3 max-h-64 overflow-y-auto">
                    {items.map(item => (
                      <div key={item.product.id} className="flex justify-between text-sm">
                        <span className="text-muted-foreground truncate mr-2">
                          {item.quantity}x {item.product.name}
                        </span>
                        <span className="flex-shrink-0">{formatCurrency(item.product.price * item.quantity)}</span>
                      </div>
                    ))}
                  </div>
                  <div className="border-t border-border pt-3 space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Subtotal</span>
                      <span>{formatCurrency(total)}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Frete</span>
                      <span className="text-primary font-mono text-xs">GRÁTIS</span>
                    </div>
                    <div className="flex justify-between pt-2 border-t border-border">
                      <span className="font-medium">Total</span>
                      <span className="text-xl font-display font-semibold">{formatCurrency(total)}</span>
                    </div>
                  </div>
                  <button
                    type="submit"
                    className="w-full flex items-center justify-center gap-2 px-6 py-3.5 rounded-xl bg-primary text-primary-foreground font-medium btn-physical hover:opacity-90 transition-all active:scale-[0.98]"
                  >
                    Confirmar Pagamento <ArrowRight size={16} />
                  </button>
                </div>
              </motion.div>
            </form>
          )}
        </motion.div>
      </div>
    </div>
  );
};

export default Checkout;
