import { useState } from 'react';
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

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

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
                        onChange={handleChange}
                    />
                </div>

                <div className="form-group">
                    <label>Card number</label>
                    <input
                        name="cardNumber"
                        placeholder="Enter card number..."
                        onChange={handleChange}
                    />
                </div>

                <div className="form-row">
                    <div className="form-group">
                        <label>Expiry date</label>
                        <input
                            name="expiry"
                            placeholder="MM/YY"
                            onChange={handleChange}
                        />
                    </div>

                    <div className="form-group">
                        <label>CVV</label>
                        <input
                            name="cvv"
                            placeholder="Enter security code..."
                            type="password"
                            onChange={handleChange}
                        />
                    </div>
                </div>

                <button
                    className="pay-button"
                    disabled={submitting}
                    onClick={() => onSubmit(form)}
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
