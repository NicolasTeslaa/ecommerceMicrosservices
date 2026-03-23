import { useEffect } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import Header from "@/components/layout/Header";
import Footer from "@/components/layout/Footer";
import Home from "@/pages/Home";
import Catalog from "@/pages/Catalog";
import ProductDetail from "@/pages/ProductDetail";
import Categories from "@/pages/Categories";
import CategoryProducts from "@/pages/CategoryProducts";
import Cart from "@/pages/Cart";
import Login from "@/pages/Login";
import Register from "@/pages/Register";
import Checkout from "@/pages/Checkout";
import Payment from "@/pages/Payment";
import Confirmation from "@/pages/Confirmation";
import Orders from "@/pages/Orders";
import NotFound from "@/pages/NotFound";
import { useAuth } from "@/store/useAuth";
import { useCart } from "@/store/useCart";

const queryClient = new QueryClient();

const CartBootstrap = () => {
  const initializeCart = useCart((state) => state.initializeCart);
  const cartOwnerKey = useAuth((state) =>
    state.isAuthenticated && state.user?.id ? `user:${state.user.id}` : "guest"
  );

  useEffect(() => {
    void initializeCart().catch(() => undefined);
  }, [cartOwnerKey, initializeCart]);

  return null;
};

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Sonner />
      <BrowserRouter>
        <CartBootstrap />
        <Header />
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/catalog" element={<Catalog />} />
          <Route path="/product/:id" element={<ProductDetail />} />
          <Route path="/categories" element={<Categories />} />
          <Route path="/categories/:id" element={<CategoryProducts />} />
          <Route path="/cart" element={<Cart />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/checkout" element={<Checkout />} />
          <Route path="/payment/:orderId" element={<Payment />} />
          <Route path="/confirmation" element={<Confirmation />} />
          <Route path="/orders" element={<Orders />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
        <Footer />
      </BrowserRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
