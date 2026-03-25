import { useEffect, useMemo, useRef, useState } from 'react';
import axios from 'axios';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { Elements, PaymentElement, useElements, useStripe } from '@stripe/react-stripe-js';
import { loadStripe } from '@stripe/stripe-js';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { ArrowLeft, CreditCard, Loader2, ShieldCheck } from 'lucide-react';
import { motion } from 'framer-motion';
import { API_BASE_URL, AUTH_TOKEN_STORAGE_KEY, paymentService } from '@/services/backendApi';
import { useOrder } from '@/store/useOrder';
import { formatCurrency } from '@/utils/format';
import type { Payment as PaymentDto } from '@/types';
import { toast } from 'sonner';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0, transition: { duration: 0.45 } },
};

const translateStripeError = (message?: string) => {
  switch (message) {
    case 'Your card has insufficient funds.':
      return 'Seu cartao nao tem saldo suficiente.';
    case 'Your card was declined.':
      return 'O pagamento foi recusado pela operadora do cartao.';
    case 'Your card has expired.':
      return 'Seu cartao esta vencido.';
    case "Your card's security code is incorrect.":
      return 'O codigo de seguranca do cartao esta incorreto.';
    default:
      return message ?? 'Nao foi possivel confirmar o pagamento.';
  }
};

const PaymentForm = ({
  orderId,
  amount,
  onSuccess,
}: {
  orderId: string;
  amount: number;
  onSuccess: () => void;
}) => {
  const stripe = useStripe();
  const elements = useElements();
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!stripe || !elements) {
      toast.error('A interface de pagamento ainda nao foi carregada.');
      return;
    }

    setSubmitting(true);

    const { error, paymentIntent } = await stripe.confirmPayment({
      elements,
      redirect: 'if_required',
    });

    setSubmitting(false);

    if (error) {
      toast.error(translateStripeError(error.message));
      return;
    }

    if (paymentIntent?.status === 'succeeded' || paymentIntent?.status === 'processing') {
      toast.success('Pagamento confirmado com sucesso.');
      onSuccess();
      return;
    }

    if (paymentIntent?.status === 'requires_action') {
      toast.message('Sua instituicao solicitou uma etapa adicional de autenticacao.');
      return;
    }

    toast.message(`Pagamento do pedido ${orderId} em processamento.`);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      <div className="rounded-2xl border border-border bg-card p-5">
        <PaymentElement />
      </div>
      <button
        type="submit"
        disabled={!stripe || !elements || submitting}
        className="w-full flex items-center justify-center gap-2 px-6 py-3.5 rounded-xl bg-primary text-primary-foreground font-medium btn-physical hover:opacity-90 transition-all active:scale-[0.98] disabled:opacity-50"
      >
        {submitting ? <Loader2 size={16} className="animate-spin" /> : <CreditCard size={16} />}
        Pagar {formatCurrency(amount)}
      </button>
    </form>
  );
};

const PaymentPage = () => {
  const { orderId = '' } = useParams();
  const navigate = useNavigate();
  const { lastOrder, setLastOrder } = useOrder();
  const [payment, setPayment] = useState<PaymentDto | null>(null);
  const [publishableKey, setPublishableKey] = useState('');
  const [loading, setLoading] = useState(true);
  const [waitingForPayment, setWaitingForPayment] = useState(false);
  const retryTimeoutRef = useRef<number | undefined>(undefined);
  const attemptsRef = useRef(0);
  const maxAttempts = 15;
  const retryDelayMs = 2000;

  useEffect(() => {
    let active = true;

    const loadConfig = async () => {
      try {
        const config = await paymentService.getConfig();

        if (!active) return;

        setPublishableKey(config.publishableKey);
      } catch (error) {
        if (!active) return;
        toast.error(error instanceof Error ? error.message : 'Nao foi possivel carregar a configuracao do pagamento.');
      }
    };

    void loadConfig();

    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    let active = true;

    const loadPayment = async (showFullScreenLoader: boolean) => {
      if (!orderId) {
        toast.error('Pedido invalido.');
        navigate('/orders');
        return;
      }

      if (showFullScreenLoader) {
        setLoading(true);
      }

      try {
        const currentPayment = await paymentService.getByOrderId(orderId);

        if (!active) return;

        if (retryTimeoutRef.current) {
          window.clearTimeout(retryTimeoutRef.current);
          retryTimeoutRef.current = undefined;
        }

        if (!currentPayment && attemptsRef.current < maxAttempts) {
          attemptsRef.current += 1;
          setWaitingForPayment(true);
          retryTimeoutRef.current = window.setTimeout(() => {
            void loadPayment(false);
          }, retryDelayMs);
          return;
        }

        setPayment(currentPayment);
        setWaitingForPayment(!currentPayment);
      } catch (error) {
        if (!active) return;

        if (axios.isAxiosError(error) && error.response?.status === 404 && attemptsRef.current < maxAttempts) {
          attemptsRef.current += 1;
          setWaitingForPayment(true);
          retryTimeoutRef.current = window.setTimeout(() => {
            void loadPayment(false);
          }, retryDelayMs);
          return;
        }

        toast.error(error instanceof Error ? error.message : 'Nao foi possivel carregar o pagamento.');
      } finally {
        if (active && showFullScreenLoader) setLoading(false);
      }
    };

    const hubConnection = window.localStorage.getItem(AUTH_TOKEN_STORAGE_KEY)
      ? new HubConnectionBuilder()
          .withUrl(`${API_BASE_URL}/hubs/payments`, {
            accessTokenFactory: () => window.localStorage.getItem(AUTH_TOKEN_STORAGE_KEY) ?? '',
          })
          .withAutomaticReconnect()
          .configureLogging(LogLevel.Warning)
          .build()
      : null;

    if (hubConnection) {
      hubConnection.on('payment-updated', (updatedOrderId: string) => {
        if (updatedOrderId === orderId) {
          attemptsRef.current = 0;
          void loadPayment(false);
        }
      });

      void hubConnection
        .start()
        .then(() => hubConnection.invoke('JoinOrderPayment', orderId))
        .catch(() => {
          // The fallback polling below still keeps the page functional if the realtime channel is unavailable.
        });
    }

    void loadPayment(true);

    return () => {
      active = false;
      if (retryTimeoutRef.current) window.clearTimeout(retryTimeoutRef.current);
      if (hubConnection) {
        void hubConnection.stop();
      }
    };
  }, [navigate, orderId]);

  const stripePromise = useMemo(() => {
    if (!publishableKey) return null;
    return loadStripe(publishableKey);
  }, [publishableKey]);

  const handleSuccess = () => {
    if (lastOrder?.orderId === orderId) {
      setLastOrder({
        ...lastOrder,
        status: 'approved',
      });
    }

    navigate('/confirmation');
  };

  if (loading) {
    return (
      <div className="min-h-screen pt-24 flex flex-col items-center justify-center gap-4 px-4">
        <Loader2 size={40} className="animate-spin text-primary" />
        <p className="text-muted-foreground text-sm">Carregando dados do pagamento...</p>
      </div>
    );
  }

  if (!payment) {
    return (
      <div className="min-h-screen pt-24 flex flex-col items-center justify-center gap-4 px-4">
        <h2 className="text-2xl font-display font-bold">
          {waitingForPayment ? 'Preparando pagamento' : 'Pagamento nao encontrado'}
        </h2>
        <p className="text-muted-foreground text-sm text-center max-w-md">
          {waitingForPayment
            ? 'Recebemos o pedido e estamos aguardando o PaymentService criar a sessao da Stripe. Isso costuma levar alguns segundos.'
            : 'Nao foi possivel localizar o pagamento para este pedido.'}
        </p>
        {!waitingForPayment && (
          <Link to="/orders" className="text-primary text-sm hover:underline">Ir para meus pedidos</Link>
        )}
      </div>
    );
  }

  if (!payment.stripeClientSecret || !stripePromise) {
    return (
      <div className="min-h-screen pt-24 flex flex-col items-center justify-center gap-4 px-4">
        <h2 className="text-2xl font-display font-bold">Pagamento ainda nao pronto</h2>
        <p className="text-muted-foreground text-sm text-center max-w-md">
          O PaymentService ainda esta preparando a sessao da Stripe para o pedido. Assim que estiver pronta, esta tela sera atualizada automaticamente.
        </p>
        <Loader2 size={28} className="animate-spin text-primary" />
      </div>
    );
  }

  if (payment.maxAttemptsReached) {
    return (
      <div className="min-h-screen pt-24 lg:pt-28 pb-12 px-4">
        <div className="max-w-3xl mx-auto rounded-3xl glass-panel p-8 sm:p-10 text-center space-y-4">
          <h1 className="text-3xl font-display font-bold tracking-tight">Limite de tentativas atingido</h1>
          <p className="text-muted-foreground">
            Este pedido atingiu o limite maximo de 3 tentativas de pagamento. Crie um novo pedido para tentar novamente.
          </p>
          {payment.failureDetail && (
            <p className="text-sm text-destructive">{payment.failureDetail}</p>
          )}
          <div className="flex justify-center gap-3">
            <Link
              to="/orders"
              className="inline-flex items-center justify-center rounded-xl bg-primary px-5 py-3 text-sm font-medium text-primary-foreground"
            >
              Ver meus pedidos
            </Link>
            <Link
              to="/catalog"
              className="inline-flex items-center justify-center rounded-xl bg-secondary px-5 py-3 text-sm font-medium text-secondary-foreground"
            >
              Fazer novo pedido
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen pt-24 lg:pt-28 pb-12 px-4">
      <div className="max-w-4xl mx-auto">
        <motion.div initial="hidden" animate="show" variants={{ hidden: {}, show: { transition: { staggerChildren: 0.08 } } }}>
          <motion.div variants={fadeUp} className="mb-4">
            <Link to="/checkout" className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
              <ArrowLeft size={14} /> Voltar ao checkout
            </Link>
          </motion.div>

          <motion.div variants={fadeUp} className="mb-10">
            <p className="text-primary font-mono text-xs tracking-widest uppercase mb-2">Pagamento</p>
            <h1 className="text-4xl sm:text-5xl font-display font-bold tracking-tight">Confirme seu cartao</h1>
          </motion.div>

          <div className="grid grid-cols-1 lg:grid-cols-[1.2fr_0.8fr] gap-8">
            <motion.div variants={fadeUp} className="space-y-5">
              <div className="rounded-2xl bg-card border-glow p-6 space-y-3">
                <div className="flex items-center gap-3">
                  <ShieldCheck className="text-primary" size={20} />
                  <h2 className="font-display font-semibold">Pagamento seguro com Stripe</h2>
                </div>
                <p className="text-sm text-muted-foreground">
                  Seus dados de cartao sao enviados diretamente para a Stripe. O nosso backend acompanha apenas o status do pagamento.
                </p>
              </div>

              <div className="rounded-2xl bg-card border-glow p-6">
                <Elements stripe={stripePromise} options={{ clientSecret: payment.stripeClientSecret }}>
                  <PaymentForm orderId={orderId} amount={payment.amount} onSuccess={handleSuccess} />
                </Elements>
              </div>
            </motion.div>

            <motion.div variants={fadeUp} className="lg:sticky lg:top-28 lg:self-start">
              <div className="rounded-2xl bg-card border-glow p-6 space-y-4">
                <h3 className="font-display font-semibold">Resumo do pagamento</h3>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Pedido</span>
                  <span className="font-mono text-xs">{payment.orderId}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Status</span>
                  <span>{payment.status}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Tentativas</span>
                  <span>{payment.attemptCount}/3</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Moeda</span>
                  <span>{payment.currency.toUpperCase()}</span>
                </div>
                <div className="flex justify-between pt-2 border-t border-border">
                  <span className="font-medium">Total</span>
                  <span className="text-xl font-display font-semibold">{formatCurrency(payment.amount)}</span>
                </div>
                {payment.failureDetail && (
                  <p className="text-sm text-destructive">{payment.failureDetail}</p>
                )}
              </div>
            </motion.div>
          </div>
        </motion.div>
      </div>
    </div>
  );
};

export default PaymentPage;
