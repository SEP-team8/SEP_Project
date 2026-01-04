import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { PaymentPage } from './components/payment-page';

function App() {
  return (
    <BrowserRouter>
          <Routes>
              <Route path="/pay/:paymentRequestId" element={<PaymentPage />} />
          </Routes>
      </BrowserRouter>
  );
}

export default App;
