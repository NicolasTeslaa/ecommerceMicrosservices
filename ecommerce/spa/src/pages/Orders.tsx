import axios from 'axios';
import { Fragment, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ChevronDown, Download, Loader2, Package, ShoppingBag } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import { usePagedOrders } from '@/hooks/useData';
import { useAuth } from '@/store/useAuth';
import { invoiceService, orderService } from '@/services/backendApi';
import { formatCurrency } from '@/utils/format';
import { toast } from 'sonner';
import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from '@/components/ui/pagination';

const ORDERS_PER_PAGE = 6;

const buildVisiblePages = (currentPage: number, totalPages: number) => {
  if (totalPages <= 5) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  if (currentPage <= 3) {
    return [1, 2, 3, 4, totalPages];
  }

  if (currentPage >= totalPages - 2) {
    return [1, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
  }

  return [1, currentPage - 1, currentPage, currentPage + 1, totalPages];
};

const formatOrderStatus = (status: string | number) => {
  if (status === 1 || status === 'PendingPayment' || status === 'Pending') return 'Aguardando pagamento';
  if (status === 2 || status === 'Confirmed') return 'Confirmado';
  if (status === 3 || status === 'Cancelled') return 'Cancelado';
  if (status === 4 || status === 'PaymentRejected') return 'Pagamento recusado';

  return typeof status === 'string' ? status : 'Em processamento';
};

const formatRejectionReason = (reason?: string | number | null) => {
  if (reason === 1 || reason === 'ProductUnavailable') return 'Produto indisponivel';
  if (reason === 2 || reason === 'InsufficientStock') return 'Estoque insuficiente';
  if (reason === 3 || reason === 'InvalidCustomerAddress') return 'Endereco invalido';
  if (reason === 4 || reason === 'ValidationFailed') return 'Falha de validacao';
  return null;
};

const formatPaymentLabel = (method: string, brand?: string, last4?: string) => {
  const normalized = method.toLowerCase();

  if (normalized === 'pix') return 'PIX';
  if (brand && last4) return `${brand} final ${last4}`;
  if (normalized === 'debit') return 'Cartao de debito';
  return 'Cartao de credito';
};

const Orders = () => {
  const customerId = useAuth((state) => state.user?.customerId ?? '');
  const isAuthenticated = useAuth((state) => state.isAuthenticated);
  const [currentPage, setCurrentPage] = useState(1);
  const [cancellingOrderId, setCancellingOrderId] = useState<string | null>(null);
  const [downloadingInvoiceOrderId, setDownloadingInvoiceOrderId] = useState<string | null>(null);
  const queryClient = useQueryClient();
  const { data, isLoading, isFetching } = usePagedOrders(customerId, currentPage, ORDERS_PER_PAGE);

  const orders = data?.items ?? [];
  const totalItems = data?.pagination?.totalItems ?? 0;
  const totalPages = Math.max(1, data?.pagination?.totalPages ?? 1);
  const visiblePages = useMemo(
    () => buildVisiblePages(currentPage, totalPages),
    [currentPage, totalPages]
  );

  const handleCancelOrder = async (orderId: string) => {
    if (!customerId || cancellingOrderId) return;

    setCancellingOrderId(orderId);

    try {
      const result = await orderService.cancel(orderId, customerId);
      toast.success(result.message);
      await queryClient.invalidateQueries({ queryKey: ['orders', customerId] });
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Nao foi possivel cancelar o pedido.');
    } finally {
      setCancellingOrderId(null);
    }
  };

  const handleDownloadInvoice = async (orderId: string) => {
    if (downloadingInvoiceOrderId) return;

    setDownloadingInvoiceOrderId(orderId);

    try {
      const invoice = await invoiceService.getByOrderId(orderId);
      const blob = new Blob([invoice.xmlContent], { type: 'application/xml;charset=utf-8' });
      const downloadUrl = window.URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = downloadUrl;
      anchor.download = `nota-fiscal-${invoice.number}-pedido-${invoice.orderId}.xml`;
      document.body.appendChild(anchor);
      anchor.click();
      anchor.remove();
      window.URL.revokeObjectURL(downloadUrl);
      toast.success('Nota fiscal baixada com sucesso.');
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        toast.error('A nota fiscal ainda nao foi emitida para este pedido.');
      } else {
        toast.error(error instanceof Error ? error.message : 'Nao foi possivel baixar a nota fiscal.');
      }
    } finally {
      setDownloadingInvoiceOrderId(null);
    }
  };

  if (!isAuthenticated || !customerId) {
    return (
      <div className="min-h-screen pt-24 lg:pt-28 pb-12 px-4">
        <div className="max-w-3xl mx-auto rounded-3xl glass-panel p-8 sm:p-10 text-center space-y-4">
          <Package size={44} className="mx-auto text-muted-foreground/40" />
          <h1 className="text-3xl font-display font-bold tracking-tight">Entre para ver seus pedidos</h1>
          <p className="text-muted-foreground">
            Faca login para acompanhar seus pedidos, revisar itens e consultar o endereco de entrega.
          </p>
          <div className="flex justify-center gap-3">
            <Link
              to="/login"
              className="inline-flex items-center justify-center rounded-xl bg-primary px-5 py-3 text-sm font-medium text-primary-foreground"
            >
              Entrar
            </Link>
            <Link
              to="/catalog"
              className="inline-flex items-center justify-center rounded-xl bg-secondary px-5 py-3 text-sm font-medium text-secondary-foreground"
            >
              Ir para o catalogo
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen pt-24 lg:pt-28 pb-12 px-4 bg-gradient-hero relative">
      <div className="absolute inset-0 bg-background/80" />
      <div className="relative z-10 max-w-5xl mx-auto">
        <motion.div initial={{ opacity: 0, y: 18 }} animate={{ opacity: 1, y: 0 }} className="mb-8">
          <p className="text-primary font-mono text-xs tracking-widest uppercase mb-2">Minha conta</p>
          <h1 className="text-4xl sm:text-5xl font-display font-bold tracking-tight">Meus Pedidos</h1>
          <p className="text-muted-foreground mt-3">
            Acompanhe o historico de compras e consulte os detalhes de cada pedido.
          </p>
        </motion.div>

        {isLoading ? (
          <div className="space-y-4">
            {Array.from({ length: 3 }).map((_, index) => (
              <div key={index} className="rounded-2xl glass-panel p-6 animate-pulse">
                <div className="h-5 w-32 bg-muted rounded mb-4" />
                <div className="h-4 w-48 bg-muted rounded mb-2" />
                <div className="h-4 w-56 bg-muted rounded" />
              </div>
            ))}
          </div>
        ) : orders.length === 0 ? (
          <div className="rounded-3xl glass-panel p-10 text-center space-y-4">
            <ShoppingBag size={48} className="mx-auto text-muted-foreground/35" />
            <h2 className="text-2xl font-display font-semibold">Nenhum pedido encontrado</h2>
            <p className="text-muted-foreground">
              Assim que voce finalizar uma compra, seus pedidos aparecerao aqui.
            </p>
            <Link
              to="/catalog"
              className="inline-flex items-center justify-center rounded-xl bg-primary px-5 py-3 text-sm font-medium text-primary-foreground"
            >
              Explorar produtos
            </Link>
          </div>
        ) : (
          <>
            <div className="flex items-center justify-between mb-5">
              <p className="text-xs font-mono text-muted-foreground">
                {totalItems} pedido{totalItems !== 1 ? 's' : ''} encontrado{totalItems !== 1 ? 's' : ''}
              </p>
              {isFetching && (
                <p className="text-xs font-mono text-muted-foreground">Atualizando pedidos...</p>
              )}
            </div>

            <div className="space-y-4">
              {orders.map((order) => (
                <motion.div
                  key={order.id}
                  initial={{ opacity: 0, y: 18 }}
                  animate={{ opacity: 1, y: 0 }}
                >
                  {(() => {
                    const normalizedStatus = typeof order.status === 'string' ? order.status : String(order.status);
                    const canPayNow = normalizedStatus === 'PendingPayment' || normalizedStatus === 'Pending' || normalizedStatus === '1';
                    const canCancel = canPayNow;
                    const canDownloadInvoice = normalizedStatus === 'Confirmed' || normalizedStatus === '2';

                    return (
                  <details className="group rounded-3xl glass-panel overflow-hidden">
                  <summary className="list-none cursor-pointer p-6 sm:p-7">
                    <div className="flex flex-col gap-5 lg:flex-row lg:items-center lg:justify-between">
                      <div>
                        <p className="text-xs font-mono text-muted-foreground uppercase mb-2">Pedido</p>
                        <p className="text-sm text-muted-foreground mt-2">
                          {new Date(order.createdAtUtc).toLocaleString('pt-BR')}
                        </p>
                      </div>

                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 lg:min-w-[460px]">
                        <div>
                          <p className="text-xs font-mono text-muted-foreground uppercase mb-1">Status</p>
                          <p className="font-medium">{formatOrderStatus(order.status)}</p>
                        </div>
                        <div>
                          <p className="text-xs font-mono text-muted-foreground uppercase mb-1">Pagamento</p>
                          <p className="font-medium">{formatPaymentLabel(order.paymentMethod, order.paymentCardBrand, order.paymentCardLast4)}</p>
                        </div>
                        <div>
                          <p className="text-xs font-mono text-muted-foreground uppercase mb-1">Itens</p>
                          <p className="font-medium">{order.items.length}</p>
                        </div>
                        <div>
                          <p className="text-xs font-mono text-muted-foreground uppercase mb-1">Total</p>
                          <p className="font-semibold text-primary">{formatCurrency(order.totalAmount)}</p>
                        </div>
                    </div>

                    {(canPayNow || canCancel || canDownloadInvoice) && (
                      <div className="flex flex-wrap items-center gap-2">
                        {canPayNow && (
                          <Link
                            to={`/payment/${order.id}`}
                            className="inline-flex items-center justify-center rounded-xl bg-primary px-4 py-2 text-sm font-medium text-primary-foreground"
                            onClick={(event) => event.stopPropagation()}
                          >
                            Pagar agora
                          </Link>
                        )}
                        {canCancel && (
                          <button
                            type="button"
                            onClick={(event) => {
                              event.preventDefault();
                              event.stopPropagation();
                              void handleCancelOrder(order.id);
                            }}
                            disabled={cancellingOrderId === order.id}
                            className="inline-flex items-center justify-center gap-2 rounded-xl bg-secondary px-4 py-2 text-sm font-medium text-secondary-foreground disabled:opacity-60"
                          >
                            {cancellingOrderId === order.id ? <Loader2 size={14} className="animate-spin" /> : null}
                            Cancelar pedido
                          </button>
                        )}
                        {canDownloadInvoice && (
                          <button
                            type="button"
                            onClick={(event) => {
                              event.preventDefault();
                              event.stopPropagation();
                              void handleDownloadInvoice(order.id);
                            }}
                            disabled={downloadingInvoiceOrderId === order.id}
                            className="inline-flex items-center justify-center gap-2 rounded-xl border border-border bg-background/70 px-4 py-2 text-sm font-medium text-foreground disabled:opacity-60"
                          >
                            {downloadingInvoiceOrderId === order.id ? (
                              <Loader2 size={14} className="animate-spin" />
                            ) : (
                              <Download size={14} />
                            )}
                            Baixar nota fiscal
                          </button>
                        )}
                      </div>
                    )}

                      <div className="flex items-center gap-2 text-muted-foreground group-open:text-foreground transition-colors">
                        <span className="text-xs font-mono uppercase">Detalhes</span>
                        <ChevronDown size={18} className="transition-transform group-open:rotate-180" />
                      </div>
                    </div>
                  </summary>

                  <div className="border-t border-border px-6 sm:px-7 py-6 grid gap-6 lg:grid-cols-[1.4fr_0.8fr]">
                    <div>
                      <h3 className="text-sm font-mono text-muted-foreground uppercase mb-4">Itens do pedido</h3>
                      <div className="space-y-3">
                        {order.items.map((item) => (
                          <div
                            key={item.id}
                            className="flex flex-col gap-2 rounded-2xl bg-background/50 px-4 py-4 sm:flex-row sm:items-center sm:justify-between"
                          >
                            <div>
                              <p className="font-medium">{item.productName}</p>
                              <p className="text-sm text-muted-foreground">
                                {item.quantity} x {formatCurrency(item.unitPrice)}
                              </p>
                            </div>
                            <p className="font-semibold">{formatCurrency(item.totalPrice)}</p>
                          </div>
                        ))}
                      </div>
                    </div>

                    <div className="space-y-4">
                      <div className="rounded-2xl bg-background/50 p-5">
                        <h3 className="text-sm font-mono text-muted-foreground uppercase mb-3">Entrega</h3>
                        <p className="text-sm leading-6 text-muted-foreground">{order.shippingAddress}</p>
                      </div>

                      {(order.rejectionReason || order.rejectionDetail) && (
                        <div className="rounded-2xl border border-destructive/30 bg-destructive/10 p-5 space-y-2">
                          <h3 className="text-sm font-mono uppercase text-destructive">Motivo da rejeicao</h3>
                          {formatRejectionReason(order.rejectionReason) && (
                            <p className="font-medium text-sm">{formatRejectionReason(order.rejectionReason)}</p>
                          )}
                          {order.rejectionDetail && (
                            <p className="text-sm text-muted-foreground leading-6">{order.rejectionDetail}</p>
                          )}
                        </div>
                      )}

                      <div className="rounded-2xl bg-background/50 p-5 space-y-3">
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-muted-foreground">Frete</span>
                          <span>{formatCurrency(order.shippingAmount)}</span>
                        </div>
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-muted-foreground">Email</span>
                          <span className="text-right break-all">{order.customerEmail}</span>
                        </div>
                        <div className="border-t border-border pt-3 flex items-center justify-between">
                          <span className="font-medium">Total</span>
                          <span className="text-lg font-semibold text-primary">
                            {formatCurrency(order.totalAmount)}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                  </details>
                    );
                  })()}
                </motion.div>
              ))}
            </div>

            {totalPages > 1 && (
              <div className="mt-10 space-y-3">
                <p className="text-center text-xs font-mono text-muted-foreground">
                  Pagina {currentPage} de {totalPages}
                </p>
                <Pagination>
                  <PaginationContent>
                    <PaginationItem>
                      <PaginationPrevious
                        href="#"
                        onClick={(event) => {
                          event.preventDefault();
                          setCurrentPage((page) => Math.max(1, page - 1));
                        }}
                        className={currentPage === 1 ? 'pointer-events-none opacity-50' : ''}
                      />
                    </PaginationItem>

                    {visiblePages.map((page, index) => {
                      const previousPage = visiblePages[index - 1];
                      const shouldShowEllipsis = previousPage && page - previousPage > 1;

                      return (
                        <Fragment key={page}>
                          {shouldShowEllipsis && (
                            <PaginationItem>
                              <PaginationEllipsis />
                            </PaginationItem>
                          )}
                          <PaginationItem>
                            <PaginationLink
                              href="#"
                              isActive={page === currentPage}
                              onClick={(event) => {
                                event.preventDefault();
                                setCurrentPage(page);
                              }}
                            >
                              {page}
                            </PaginationLink>
                          </PaginationItem>
                        </Fragment>
                      );
                    })}

                    <PaginationItem>
                      <PaginationNext
                        href="#"
                        onClick={(event) => {
                          event.preventDefault();
                          setCurrentPage((page) => Math.min(totalPages, page + 1));
                        }}
                        className={currentPage === totalPages ? 'pointer-events-none opacity-50' : ''}
                      />
                    </PaginationItem>
                  </PaginationContent>
                </Pagination>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default Orders;
