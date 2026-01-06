import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { CardPaymentPage } from './components/card-payment-page';
import { QrPaymentPage } from './components/qr-payment-page';

function App() {
  return (
    <BrowserRouter>
          <Routes>
              <Route path="/payCard/:paymentRequestId" element={<CardPaymentPage />} />
              <Route path="/payQr/:paymentRequestId" element={<QrPaymentPage />} />
          </Routes>
      </BrowserRouter>
  );
}

export default App;
