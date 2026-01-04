import { useMemo, useState } from 'react';
import './card-payment-form.css';

interface Props {
    amount: number;
    currency: string;
    onSubmit: (data: CardFormData) => void;
    submitting: boolean;
    error: string | null;
}

export interface CardFormData {
    cardNumber: string;
    expiry: string;
    cvv: string;
    cardHolder: string;
}

export type CardBrand = 'VISA' | 'MASTERCARD' | 'UNKNOWN';

export function CardPaymentForm({
    amount,
    currency,
    onSubmit,
    submitting,
    error
}: Readonly<Props>) {
    const [form, setForm] = useState<CardFormData>({
        cardNumber: '',
        expiry: '',
        cvv: '',
        cardHolder: '',
    });
    const [hasAttempted, setHasAttempted] = useState(false);

    const brand: CardBrand = useMemo(
        () => detectCardBrand(form.cardNumber),
        [form.cardNumber]
    );

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

    console.log(form)
    console.log(isValidLuhn(form.cardNumber))
    console.log(isExpiryValid(form.expiry))


    const isFormValid =
        form.cardHolder.trim().length > 2 &&
        isValidLuhn(form.cardNumber) &&
        brand !== 'UNKNOWN' &&
        isExpiryValid(form.expiry) &&
        /^\d{3}$/.test(form.cvv);

    const handleSubmit = () => {
        if (!isFormValid || hasAttempted) return;

        setHasAttempted(true);
        onSubmit(form);
    };

    function detectCardBrand(cardNumber: string): CardBrand {
        const digits = cardNumber.replace(/\s+/g, '');

        if (/^4\d{0,}$/.test(digits)) return 'VISA';
        if (/^(5[1-5]|2[2-7])\d{0,}$/.test(digits)) return 'MASTERCARD';

        return 'UNKNOWN';
    }

    console.log(!isFormValid || submitting || hasAttempted)

    return (
        <div className="payment-container">
            <div className="payment-card">
                <h2 className="bank-title">Secure Card Payment</h2>

                <div className="amount-box">
                    <span>Total amount</span>
                    <strong>
                        {amount.toFixed(2)} {currency}
                    </strong>
                </div>

                <div className="form-group">
                    <label>Cardholder name</label>
                    <input
                        name="cardHolder"
                        placeholder="Enter card holder name..."
                        value={form.cardHolder}
                        onChange={handleChange}
                    />
                </div>

                <div className="card-info-group">
                    <div className="form-group">
                        <label>Card number</label>
                        <input
                            name="cardNumber"
                            placeholder="Enter card number..."
                            value={form.cardNumber}
                            onChange={handleChange}
                        />
                    </div>

                    <div className="card-logos">
                        {brand === 'VISA' && <img
                            src="/visa.png"
                            alt="Visa"
                        />}
                        {brand === 'MASTERCARD' && <img
                            src="/mastercard.png"
                            alt="MasterCard"
                        />}
                    </div>
                </div>

                <div className="form-row">
                    <div className="form-group">
                        <label>Expiry date</label>
                        <input
                            name="expiry"
                            placeholder="MM/YY"
                            value={form.expiry}
                            onChange={handleChange}
                        />
                    </div>

                    <div className="form-group">
                        <label>CVV</label>
                        <input
                            name="cvv"
                            placeholder="Enter security code..."
                            type="password"
                            value={form.cvv}
                            onChange={handleChange}
                        />
                    </div>
                </div>

                <button
                    className="pay-button"
                    disabled={!isFormValid || submitting || hasAttempted}
                    onClick={handleSubmit}
                >
                    {submitting ? 'Processing...' : 'Pay securely'}
                </button>

                {error && <div className="error-text">{error}</div>}

                <div className="secure-note">
                    🔒 Payments are encrypted and secured by PSP
                </div>
            </div>
        </div>
    );
}

    function isExpiryValid(expiry: string): boolean {
        if (!/^\d{2}\/\d{2}$/.test(expiry)) return false;

        const [month, year] = expiry.split('/').map(Number);
        if (month < 1 || month > 12) return false;

        const now = new Date();
        const expiryDate = new Date(2000 + year, month);

        return expiryDate > now;
    }

    function isValidLuhn(cardNumber: string): boolean {
        const digits = cardNumber.replace(/\s+/g, '');
        let sum = 0;
        let alternate = false;

        for (let i = digits.length - 1; i >= 0; i--) {
            let n = parseInt(digits[i], 10);

            if (alternate) {
                n *= 2;
                if (n > 9) n -= 9;
            }

            sum += n;
            alternate = !alternate;
        }

        return sum % 10 === 0;
    }
