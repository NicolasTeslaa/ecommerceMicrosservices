import type { CheckoutData } from '@/types';

const normalizeDigits = (value: string) => value.replace(/\D/g, '');

const toHex = (buffer: ArrayBuffer) =>
  Array.from(new Uint8Array(buffer))
    .map((value) => value.toString(16).padStart(2, '0'))
    .join('');

export const detectCardBrand = (cardNumber: string) => {
  const normalized = normalizeDigits(cardNumber);

  if (/^4\d{12}(\d{3})?(\d{3})?$/.test(normalized)) return 'Visa';
  if (/^(5[1-5]\d{14}|2(2[2-9]\d{12}|[3-6]\d{13}|7[01]\d{12}|720\d{12}))$/.test(normalized)) return 'Mastercard';
  if (/^3[47]\d{13}$/.test(normalized)) return 'Amex';
  if (/^(4011(78|79)|431274|438935|451416|457393|45763(1|2)|504175|627780|636297|636368)\d*$/.test(normalized)) return 'Elo';
  if (/^(606282\d{10}(\d{3})?|3841\d{15})$/.test(normalized)) return 'Hipercard';

  return 'Cartao';
};

const validateCardPayload = (form: CheckoutData) => {
  const cardNumber = normalizeDigits(form.cardNumber ?? '');
  const cardExpiry = (form.cardExpiry ?? '').replace(/\s/g, '');
  const cardCvv = normalizeDigits(form.cardCvv ?? '');

  if (cardNumber.length < 13 || cardNumber.length > 19) {
    throw new Error('Informe um numero de cartao valido.');
  }

  if (!/^\d{2}\/\d{2}$/.test(cardExpiry)) {
    throw new Error('Informe a validade do cartao no formato MM/AA.');
  }

  if (cardCvv.length < 3 || cardCvv.length > 4) {
    throw new Error('Informe um CVV valido.');
  }

  return {
    cardNumber,
    cardExpiry,
    cardCvv,
  };
};

export const tokenizeCardPayment = async (form: CheckoutData) => {
  const { cardNumber, cardExpiry, cardCvv } = validateCardPayload(form);
  const cardBrand = detectCardBrand(cardNumber);
  const cardLast4 = cardNumber.slice(-4);
  const fingerprint = [
    form.paymentMethod,
    cardNumber,
    cardExpiry,
    cardCvv,
    crypto.randomUUID(),
  ].join('|');

  const hash = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(fingerprint));
  const paymentToken = `tok_${toHex(hash).slice(0, 32)}`;

  return {
    paymentToken,
    paymentCardBrand: cardBrand,
    paymentCardLast4: cardLast4,
  };
};
