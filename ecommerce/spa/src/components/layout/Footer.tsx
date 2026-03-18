import { Link } from 'react-router-dom';

const Footer = () => {
  return (
    <footer className="border-t border-border mt-24">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          <div className="md:col-span-2">
            <div className="flex items-center gap-2 mb-4">
              <div className="w-8 h-8 rounded-lg bg-primary flex items-center justify-center">
                <span className="text-primary-foreground font-bold text-sm">A</span>
              </div>
              <span className="text-xl font-display font-semibold">Aura</span>
            </div>
            <p className="text-muted-foreground text-sm max-w-md leading-relaxed">
              Artefatos digitais para o colecionador moderno. Hardware premium selecionado com curadoria e entregue com excelência.
            </p>
          </div>
          <div>
            <h4 className="font-medium text-sm mb-4">Navegação</h4>
            <div className="space-y-2">
              <Link to="/catalog" className="block text-sm text-muted-foreground hover:text-foreground transition-colors">Catálogo</Link>
              <Link to="/categories" className="block text-sm text-muted-foreground hover:text-foreground transition-colors">Categorias</Link>
              <Link to="/cart" className="block text-sm text-muted-foreground hover:text-foreground transition-colors">Carrinho</Link>
            </div>
          </div>
          <div>
            <h4 className="font-medium text-sm mb-4">Conta</h4>
            <div className="space-y-2">
              <Link to="/login" className="block text-sm text-muted-foreground hover:text-foreground transition-colors">Entrar</Link>
              <Link to="/register" className="block text-sm text-muted-foreground hover:text-foreground transition-colors">Criar Conta</Link>
            </div>
          </div>
        </div>
        <div className="mt-12 pt-8 border-t border-border flex flex-col sm:flex-row justify-between items-center gap-4">
          <p className="text-xs text-muted-foreground font-mono">© 2026 Aura. Todos os direitos reservados.</p>
          <p className="text-xs text-muted-foreground font-mono">Batch 04.2 — Curadoria digital</p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
