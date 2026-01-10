import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import { CardPaymentForm, CardFormData } from './card-payment-form';

export interface CardPaymentRequestDto{
    amount: number;
    currency: string;
}

export function CardPaymentPage() {
    const { paymentRequestId } = useParams<{ paymentRequestId: string }>();

    const [payment, setPayment] = useState<CardPaymentRequestDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);

    useEffect(() => {
        if (!paymentRequestId) return;

        axios
            .get<CardPaymentRequestDto>(
                `https://localhost:7278/api/payments/${paymentRequestId}`
            )
            .then(res => setPayment(res.data))
            .catch(() => setError('Payment request not found'))
            .finally(() => setLoading(false));
    }, [paymentRequestId]);

    const submitPayment = async (form: CardFormData) => {
        if (!paymentRequestId) return;

        setSubmitting(true);
        setError(null);

        try {
            const response = await axios.post(
                `https://localhost:7278/api/payments/${paymentRequestId}/pay`,
                form
            );
            console.log(response)
            const redirectUrl = response.data;

            if (!redirectUrl) {
                throw new Error('Missing redirect URL');
            }

            window.location.href = redirectUrl;
        } catch {
            setError('Payment failed. Please try again.');
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) return <div>Loading...</div>;
    if (!payment) return <div>Payment not found</div>;

    if (success) {
        return (
            <div className="payment-container">
                <div className="payment-card">
                    <h2>Payment successful ✅</h2>
                    <p>
                        You have successfully paid{' '}
                        <b>{payment.amount} {payment.currency}</b>
                    </p>
                </div>
            </div>
        );
    }

    return (
        <CardPaymentForm
            amount={payment.amount}
            currency={payment.currency}
            onSubmit={submitPayment}
            submitting={submitting}
            error={error}
        />
    );
}
