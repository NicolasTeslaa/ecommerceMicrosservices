import { useEffect, useMemo, useRef, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, Link } from 'react-router-dom';
import { CreditCard, QrCode, ArrowLeft, ArrowRight, Loader2, MapPin, Truck } from 'lucide-react';
import { useCart } from '@/store/useCart';
import { useAuth } from '@/store/useAuth';
import { useOrder } from '@/store/useOrder';
import { customerService, orderService, shippingService, viaCepService } from '@/services/backendApi';
import { formatCurrency } from '@/utils/format';
import type { CheckoutData, CustomerAddress, OrderConfirmation, ShippingQuote } from '@/types';
import { toast } from 'sonner';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0, transition: { duration: 0.5 } },
};

const normalizeZipCode = (zipCode: string) => zipCode.replace(/\D/g, '');
const formatZipCode = (zipCode: string) => {
  const normalized = normalizeZipCode(zipCode).slice(0, 8);
  if (normalized.length <= 5) return normalized;
  return `${normalized.slice(0, 5)}-${normalized.slice(5)}`;
};

const paymentMethodLabel = (paymentMethod: CheckoutData['paymentMethod']) => {
  if (paymentMethod === 'pix') return 'PIX';
  if (paymentMethod === 'debit') return 'Debito';
  return 'Credito';
};

const Checkout = () => {
  const navigate = useNavigate();
  const { items, subtotal, clearCart } = useCart();
  const { isAuthenticated, user } = useAuth();
  const { setLastOrder } = useOrder();
  const customerId = user?.customerId ?? '';
  const itemsSubtotal = subtotal();

  const [loading, setLoading] = useState(false);
  const [addressLoading, setAddressLoading] = useState(false);
  const [viaCepLoading, setViaCepLoading] = useState(false);
  const [shippingLoading, setShippingLoading] = useState(false);
  const [addresses, setAddresses] = useState<CustomerAddress[]>([]);
  const [addressMode, setAddressMode] = useState<'saved' | 'new'>('new');
  const [selectedAddressId, setSelectedAddressId] = useState('');
  const [shippingQuote, setShippingQuote] = useState<ShippingQuote | null>(null);
  const [form, setForm] = useState<CheckoutData>({
    zipCode: '',
    street: '',
    number: '',
    complement: '',
    neighborhood: '',
    city: '',
    state: '',
    country: 'Brasil',
    reference: '',
    label: 'Casa',
    recipientName: user?.name ?? '',
    paymentMethod: 'credit',
    cardNumber: '',
    cardExpiry: '',
    cardCvv: '',
  });
  const lastLookupZipRef = useRef('');

  const selectedSavedAddress = useMemo(
    () => addresses.find((address) => address.id === selectedAddressId) ?? null,
    [addresses, selectedAddressId]
  );

  const destinationZipCode = addressMode === 'saved'
    ? selectedSavedAddress?.zipCode ?? ''
    : form.zipCode;

  const total = itemsSubtotal + (shippingQuote?.amount ?? 0);

  useEffect(() => {
    if (!customerId) return;

    let active = true;
    setAddressLoading(true);

    customerService
      .getAddresses(customerId)
      .then((result) => {
        if (!active) return;

        setAddresses(result);

        if (result.length > 0) {
          const defaultAddress = result.find((address) => address.isDefault) ?? result[0];
          setSelectedAddressId(defaultAddress.id);
          setAddressMode('saved');
        } else {
          setAddressMode('new');
        }
      })
      .catch(() => {
        if (!active) return;
        toast.error('Nao foi possivel carregar os enderecos salvos.');
      })
      .finally(() => {
        if (active) setAddressLoading(false);
      });

    return () => {
      active = false;
    };
  }, [customerId]);

  useEffect(() => {
    if (user?.name && !form.recipientName) {
      setForm((prev) => ({ ...prev, recipientName: user.name }));
    }
  }, [form.recipientName, user?.name]);

  useEffect(() => {
    setShippingQuote(null);
  }, [addressMode, selectedAddressId, form.zipCode, form.number, items]);

  useEffect(() => {
    if (addressMode !== 'new') return;

    const normalizedZip = normalizeZipCode(form.zipCode);
    if (normalizedZip.length !== 8 || lastLookupZipRef.current === normalizedZip) return;

    lastLookupZipRef.current = normalizedZip;
    setViaCepLoading(true);

    void viaCepService.lookup(normalizedZip)
      .then((data) => {
        if (data.erro) {
          toast.error('CEP nao encontrado.');
          return;
        }

        setForm((prev) => ({
          ...prev,
          zipCode: formatZipCode(data.cep ?? normalizedZip),
          street: data.logradouro ?? prev.street,
          complement: prev.complement || data.complemento || '',
          neighborhood: data.bairro ?? prev.neighborhood,
          city: data.localidade ?? prev.city,
          state: data.uf ?? prev.state,
          country: prev.country || 'Brasil',
        }));
      })
      .catch(() => {
        toast.error('Nao foi possivel consultar o CEP.');
      })
      .finally(() => {
        setViaCepLoading(false);
      });
  }, [addressMode, form.zipCode]);

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen pt-24 flex flex-col items-center justify-center gap-4 px-4">
        <h2 className="text-2xl font-display font-bold">Faca login para continuar</h2>
        <p className="text-muted-foreground text-sm">Voce precisa estar logado para finalizar a compra.</p>
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
        <Link to="/catalog" className="text-primary text-sm hover:underline">Voltar ao catalogo</Link>
      </div>
    );
  }

  const update = (field: keyof CheckoutData, value: string) =>
    setForm((prev) => ({ ...prev, [field]: value }));

  const validateNewAddress = () => {
    if (!form.zipCode || !form.street || !form.number || !form.neighborhood || !form.city || !form.state || !form.country || !form.label || !form.recipientName) {
      toast.error('Preencha todos os campos obrigatorios do endereco.');
      return false;
    }

    if (normalizeZipCode(form.zipCode).length !== 8) {
      toast.error('Informe um CEP valido.');
      return false;
    }

    return true;
  };

  const buildShippingPayload = () => {
    const originZipCode = items.find((item) => item.product.originZipCode)?.product.originZipCode ?? '';

    if (!originZipCode) {
      throw new Error('Os produtos do carrinho nao possuem CEP de origem configurado.');
    }

    return {
      heightCm: Math.max(...items.map((item) => item.product.heightCm || 0)),
      widthCm: Math.max(...items.map((item) => item.product.widthCm || 0)),
      cubageM3: items.reduce((sum, item) => sum + ((item.product.cubageM3 || 0) * item.quantity), 0),
      weightKg: items.reduce((sum, item) => sum + ((item.product.weightKg || 0) * item.quantity), 0),
      originZipCode,
      destinationZipCode,
      provider: 'mock',
    };
  };

  const handleCalculateShipping = async () => {
    if (addressMode === 'new' && !validateNewAddress()) {
      return;
    }

    if (!destinationZipCode || normalizeZipCode(destinationZipCode).length !== 8) {
      toast.error('Informe um CEP valido para calcular o frete.');
      return;
    }

    setShippingLoading(true);
    try {
      const quote = await shippingService.calculateQuote(buildShippingPayload());
      setShippingQuote(quote);
      toast.success('Frete calculado com sucesso.');
    } catch (error) {
      setShippingQuote(null);
      toast.error(error instanceof Error ? error.message : 'Nao foi possivel calcular o frete.');
    } finally {
      setShippingLoading(false);
    }
  };

  const persistAddressIfNeeded = async () => {
    if (addressMode === 'saved' && selectedSavedAddress) {
      return selectedSavedAddress;
    }

    if (!customerId) {
      throw new Error('Cliente nao identificado.');
    }

    if (!validateNewAddress()) {
      throw new Error('Endereco invalido.');
    }

    const createdAddress = await customerService.createAddress(customerId, {
      label: form.label,
      recipientName: form.recipientName,
      street: form.street,
      number: form.number,
      complement: form.complement,
      neighborhood: form.neighborhood,
      city: form.city,
      state: form.state,
      zipCode: form.zipCode,
      country: form.country,
      reference: form.reference,
      isDefault: addresses.length === 0,
    });

    setAddresses((prev) => [...prev, createdAddress]);
    setSelectedAddressId(createdAddress.id);
    setAddressMode('saved');
    return createdAddress;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!customerId) {
      toast.error('Cliente nao identificado.');
      return;
    }

    if (!shippingQuote) {
      toast.error('Calcule o frete antes de confirmar a compra.');
      return;
    }

    if (form.paymentMethod !== 'pix' && (!form.cardNumber || !form.cardExpiry || !form.cardCvv)) {
      toast.error('Preencha os dados do cartao.');
      return;
    }

    setLoading(true);

    try {
      const address = await persistAddressIfNeeded();
      const order = await orderService.create({
        customerId,
        customerAddressId: address.id,
        shippingAmount: shippingQuote.amount,
        paymentMethod: form.paymentMethod,
        items: items.map((item) => ({
          productId: item.product.id,
          productName: item.product.name,
          unitPrice: item.product.price,
          quantity: item.quantity,
        })),
      });

      const confirmation: OrderConfirmation = {
        orderId: order.orderId,
        items: [...items],
        total,
        paymentMethod: paymentMethodLabel(form.paymentMethod),
        status: 'pending',
        date: order.requestedAtUtc,
        shippingAmount: shippingQuote.amount,
        shippingAddress: addressMode === 'saved'
          ? `${address.street}, ${address.number} - ${address.neighborhood}, ${address.city}/${address.state}`
          : `${form.street}, ${form.number} - ${form.neighborhood}, ${form.city}/${form.state}`,
      };

      setLastOrder(confirmation);
      toast.success(order.message);

      try {
        await clearCart();
      } catch {
        toast.error('Nao foi possivel sincronizar a limpeza do carrinho.');
      }

      navigate('/confirmation');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Nao foi possivel finalizar a compra.');
    } finally {
      setLoading(false);
    }
  };

  const inputClass = 'w-full px-4 py-3 rounded-xl bg-card border border-border text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/30 text-sm transition-all';

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
                <h2 className="text-xl font-display font-semibold mb-2">Finalizando pedido...</h2>
                <p className="text-muted-foreground text-sm">Aguarde enquanto registramos sua solicitacao de compra</p>
              </div>
            </motion.div>
          ) : (
            <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-3 gap-8">
              <motion.div variants={fadeUp} className="lg:col-span-2 space-y-8">
                <div className="p-6 rounded-2xl bg-card border-glow space-y-5">
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <h3 className="font-display font-semibold">Entrega</h3>
                      <p className="text-sm text-muted-foreground">Escolha um endereco salvo ou cadastre um novo.</p>
                    </div>
                    {addresses.length > 0 && (
                      <div className="flex gap-2">
                        <button
                          type="button"
                          onClick={() => setAddressMode('saved')}
                          className={`px-3 py-2 rounded-lg text-sm ${addressMode === 'saved' ? 'bg-primary text-primary-foreground' : 'bg-secondary text-foreground'}`}
                        >
                          Endereco salvo
                        </button>
                        <button
                          type="button"
                          onClick={() => setAddressMode('new')}
                          className={`px-3 py-2 rounded-lg text-sm ${addressMode === 'new' ? 'bg-primary text-primary-foreground' : 'bg-secondary text-foreground'}`}
                        >
                          Novo endereco
                        </button>
                      </div>
                    )}
                  </div>

                  {addressLoading ? (
                    <div className="flex items-center gap-3 text-sm text-muted-foreground">
                      <Loader2 size={16} className="animate-spin" /> Carregando enderecos...
                    </div>
                  ) : addressMode === 'saved' && addresses.length > 0 ? (
                    <div className="space-y-3">
                      {addresses.map((address) => (
                        <button
                          key={address.id}
                          type="button"
                          onClick={() => setSelectedAddressId(address.id)}
                          className={`w-full text-left p-4 rounded-xl border transition-all ${selectedAddressId === address.id ? 'border-primary bg-primary/10' : 'border-border hover:border-primary/40'}`}
                        >
                          <div className="flex items-center justify-between gap-3">
                            <div>
                              <div className="font-medium flex items-center gap-2">
                                <MapPin size={16} /> {address.label}
                              </div>
                              <p className="text-sm text-muted-foreground mt-1">
                                {address.recipientName} - {address.street}, {address.number}
                              </p>
                              <p className="text-sm text-muted-foreground">
                                {address.neighborhood}, {address.city}/{address.state} - {address.zipCode}
                              </p>
                            </div>
                            {address.isDefault && <span className="text-xs font-mono text-primary">PADRAO</span>}
                          </div>
                        </button>
                      ))}
                    </div>
                  ) : (
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">CEP</label>
                        <input
                          value={form.zipCode}
                          onChange={(e) => update('zipCode', formatZipCode(e.target.value))}
                          className={inputClass}
                          placeholder="00000-000"
                        />
                        {viaCepLoading && <p className="text-xs text-muted-foreground mt-2">Consultando CEP...</p>}
                      </div>
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Numero</label>
                        <input value={form.number} onChange={(e) => update('number', e.target.value)} className={inputClass} placeholder="123" />
                      </div>
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Destinatario</label>
                        <input value={form.recipientName} onChange={(e) => update('recipientName', e.target.value)} className={inputClass} placeholder="Nome de quem recebe" />
                      </div>
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Rotulo</label>
                        <input value={form.label} onChange={(e) => update('label', e.target.value)} className={inputClass} placeholder="Casa, Trabalho..." />
                      </div>
                      <div className="sm:col-span-2">
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Rua</label>
                        <input value={form.street} onChange={(e) => update('street', e.target.value)} className={inputClass} placeholder="Rua" />
                      </div>
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Complemento</label>
                        <input value={form.complement} onChange={(e) => update('complement', e.target.value)} className={inputClass} placeholder="Apto, bloco..." />
                      </div>
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Referencia</label>
                        <input value={form.reference} onChange={(e) => update('reference', e.target.value)} className={inputClass} placeholder="Ponto de referencia" />
                      </div>
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Bairro</label>
                        <input value={form.neighborhood} onChange={(e) => update('neighborhood', e.target.value)} className={inputClass} placeholder="Bairro" />
                      </div>
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Cidade</label>
                        <input value={form.city} onChange={(e) => update('city', e.target.value)} className={inputClass} placeholder="Cidade" />
                      </div>
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Estado</label>
                        <input value={form.state} onChange={(e) => update('state', e.target.value)} className={inputClass} placeholder="UF" />
                      </div>
                      <div>
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Pais</label>
                        <input value={form.country} onChange={(e) => update('country', e.target.value)} className={inputClass} placeholder="Brasil" />
                      </div>
                    </div>
                  )}

                  <div className="pt-2">
                    <button
                      type="button"
                      onClick={() => void handleCalculateShipping()}
                      disabled={shippingLoading}
                      className="inline-flex items-center gap-2 px-5 py-3 rounded-xl bg-secondary text-foreground hover:bg-secondary/80 transition-all disabled:opacity-50"
                    >
                      {shippingLoading ? <Loader2 size={16} className="animate-spin" /> : <Truck size={16} />}
                      Calcular frete
                    </button>
                  </div>
                </div>

                <div className="p-6 rounded-2xl bg-card border-glow space-y-4">
                  <h3 className="font-display font-semibold">Forma de Pagamento</h3>
                  <div className="grid grid-cols-3 gap-3">
                    {[
                      { val: 'credit' as const, label: 'Credito', icon: CreditCard },
                      { val: 'debit' as const, label: 'Debito', icon: CreditCard },
                      { val: 'pix' as const, label: 'PIX', icon: QrCode },
                    ].map((pm) => (
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
                        <label className="text-xs font-mono text-muted-foreground block mb-1.5">Numero do cartao</label>
                        <input value={form.cardNumber} onChange={(e) => update('cardNumber', e.target.value)} className={inputClass} placeholder="0000 0000 0000 0000" />
                      </div>
                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <label className="text-xs font-mono text-muted-foreground block mb-1.5">Validade</label>
                          <input value={form.cardExpiry} onChange={(e) => update('cardExpiry', e.target.value)} className={inputClass} placeholder="MM/AA" />
                        </div>
                        <div>
                          <label className="text-xs font-mono text-muted-foreground block mb-1.5">CVV</label>
                          <input value={form.cardCvv} onChange={(e) => update('cardCvv', e.target.value)} className={inputClass} placeholder="123" />
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              </motion.div>

              <motion.div variants={fadeUp} className="lg:sticky lg:top-28 lg:self-start">
                <div className="p-6 rounded-2xl bg-card border-glow space-y-4">
                  <h3 className="font-display font-semibold">Resumo</h3>
                  <div className="space-y-3 max-h-64 overflow-y-auto">
                    {items.map((item) => (
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
                      <span>{formatCurrency(itemsSubtotal)}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Frete</span>
                      <span>
                        {shippingQuote ? `${formatCurrency(shippingQuote.amount)} (${shippingQuote.estimatedDeliveryDescription})` : 'Calcule antes de confirmar'}
                      </span>
                    </div>
                    <div className="flex justify-between pt-2 border-t border-border">
                      <span className="font-medium">Total</span>
                      <span className="text-xl font-display font-semibold">{formatCurrency(total)}</span>
                    </div>
                  </div>
                  <button
                    type="submit"
                    disabled={!shippingQuote}
                    className="w-full flex items-center justify-center gap-2 px-6 py-3.5 rounded-xl bg-primary text-primary-foreground font-medium btn-physical hover:opacity-90 transition-all active:scale-[0.98] disabled:opacity-50"
                  >
                    Confirmar Pedido <ArrowRight size={16} />
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
