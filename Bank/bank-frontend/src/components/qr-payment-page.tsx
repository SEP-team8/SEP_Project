import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import axios from 'axios';
import './qr-payment-page.css';

export interface QrPaymentResponseDto {
    paymentRequestId: string;
    qrCodeBase64: string;
}

export function QrPaymentPage() {
    const { paymentRequestId } = useParams<{ paymentRequestId: string }>();

    const [qr, setQr] = useState<QrPaymentResponseDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!paymentRequestId) return;

        axios
            .post<QrPaymentResponseDto>(
                `https://localhost:7278/api/payments/${paymentRequestId}/qr`
            )
            .then(res => setQr(res.data))
            .catch(() => setError('Unable to generate QR code'))
            .finally(() => setLoading(false));
    }, [paymentRequestId]);

    if (loading) return <div>Loading QR code...</div>;
    if (error) return <div>{error}</div>;
    if (!qr) return <div>QR code not available</div>;

    return (
        <div className="payment-container">
            <div className="payment-card">
                <h2>Scan QR code to pay</h2>

                <img
                    src={qr.qrCodeBase64}
                    alt="QR code for payment"
                    style={{ width: 260, height: 260 }}
                />

                <p style={{ marginTop: 16 }}>
                    Open your mobile banking app and scan the QR code to complete the payment.
                </p>
            </div>
        </div>
    );
}