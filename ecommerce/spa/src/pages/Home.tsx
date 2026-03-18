import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import { ArrowRight, Zap, Shield, Cpu } from 'lucide-react';
import { useProducts, useCategories } from '@/hooks/useData';
import ProductCard from '@/components/product/ProductCard';
import HeroScene from '@/components/canvas/HeroScene';

const stagger = {
  hidden: {},
  show: { transition: { staggerChildren: 0.1 } },
};

const fadeUp = {
  hidden: { opacity: 0, y: 30 },
  show: { opacity: 1, y: 0, transition: { duration: 0.6, ease: [0.2, 0, 0, 1] } },
};

const Home = () => {
  const { data: products } = useProducts();
  const { data: categories } = useCategories();
  const featured = products?.slice(0, 4);

  return (
    <div className="min-h-screen">
      {/* Hero */}
      <section className="relative min-h-screen flex items-center justify-center overflow-hidden bg-gradient-hero">
        <HeroScene />
        <div className="relative z-10 max-w-5xl mx-auto px-4 text-center">
          <motion.div
            initial="hidden"
            animate="show"
            variants={stagger}
            className="space-y-6"
          >
            <motion.p variants={fadeUp} className="text-primary font-mono text-sm tracking-widest uppercase">
              Batch 04.2 — Curadoria Digital
            </motion.p>
            <motion.h1 variants={fadeUp} className="text-5xl sm:text-7xl lg:text-8xl font-display font-bold tracking-tighter leading-[0.9]">
              Artefatos digitais
              <br />
              <span className="text-gradient-primary">para o colecionador</span>
              <br />
              moderno
            </motion.h1>
            <motion.p variants={fadeUp} className="text-lg sm:text-xl text-muted-foreground max-w-2xl mx-auto leading-relaxed">
              Hardware premium selecionado com curadoria. Cada peça é um statement de performance e design.
            </motion.p>
            <motion.div variants={fadeUp} className="flex flex-col sm:flex-row gap-4 justify-center pt-4">
              <Link
                to="/catalog"
                className="inline-flex items-center justify-center gap-2 px-8 py-4 rounded-xl bg-primary text-primary-foreground font-medium btn-physical hover:opacity-90 transition-all active:scale-[0.98] text-base"
              >
                Explorar Catálogo <ArrowRight size={18} />
              </Link>
              <Link
                to="/categories"
                className="inline-flex items-center justify-center gap-2 px-8 py-4 rounded-xl bg-secondary text-secondary-foreground font-medium btn-physical hover:bg-muted transition-all active:scale-[0.98] text-base"
              >
                Ver Categorias
              </Link>
            </motion.div>
          </motion.div>
        </div>
        <div className="absolute bottom-0 left-0 right-0 h-32 bg-gradient-to-t from-background to-transparent" />
      </section>

      {/* Features */}
      <section className="py-24 px-4">
        <div className="max-w-7xl mx-auto">
          <motion.div
            initial="hidden"
            whileInView="show"
            viewport={{ once: true, margin: "-100px" }}
            variants={stagger}
            className="grid grid-cols-1 md:grid-cols-3 gap-6"
          >
            {[
              { icon: Zap, title: 'Performance', desc: 'Componentes selecionados para máxima performance.' },
              { icon: Shield, title: 'Garantia Premium', desc: 'Cada produto com garantia estendida e suporte dedicado.' },
              { icon: Cpu, title: 'Curadoria Tech', desc: 'Apenas os melhores componentes do mercado global.' },
            ].map((f, i) => (
              <motion.div key={i} variants={fadeUp} className="p-8 rounded-2xl bg-card border-glow group hover:glow-primary transition-shadow duration-500">
                <f.icon size={28} className="text-primary mb-4" />
                <h3 className="text-lg font-display font-semibold mb-2">{f.title}</h3>
                <p className="text-sm text-muted-foreground leading-relaxed">{f.desc}</p>
              </motion.div>
            ))}
          </motion.div>
        </div>
      </section>

      {/* Featured Products */}
      {featured && featured.length > 0 && (
        <section className="py-24 px-4">
          <div className="max-w-7xl mx-auto">
            <motion.div initial="hidden" whileInView="show" viewport={{ once: true }} variants={stagger}>
              <motion.div variants={fadeUp} className="flex items-end justify-between mb-12">
                <div>
                  <p className="text-primary font-mono text-xs tracking-widest uppercase mb-2">Seleção</p>
                  <h2 className="text-3xl sm:text-4xl font-display font-bold tracking-tight">Em Destaque</h2>
                </div>
                <Link to="/catalog" className="text-sm text-muted-foreground hover:text-primary flex items-center gap-1 transition-colors">
                  Ver todos <ArrowRight size={14} />
                </Link>
              </motion.div>
              <motion.div variants={stagger} className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                {featured.map(product => (
                  <motion.div key={product.id} variants={fadeUp}>
                    <ProductCard product={product} />
                  </motion.div>
                ))}
              </motion.div>
            </motion.div>
          </div>
        </section>
      )}

      {/* Categories */}
      {categories && categories.length > 0 && (
        <section className="py-24 px-4">
          <div className="max-w-7xl mx-auto">
            <motion.div initial="hidden" whileInView="show" viewport={{ once: true }} variants={stagger}>
              <motion.div variants={fadeUp} className="mb-12">
                <p className="text-primary font-mono text-xs tracking-widest uppercase mb-2">Explorar</p>
                <h2 className="text-3xl sm:text-4xl font-display font-bold tracking-tight">Categorias</h2>
              </motion.div>
              <motion.div variants={stagger} className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                {categories.map((cat, i) => (
                  <motion.div key={cat.id} variants={fadeUp}>
                    <Link
                      to={`/categories/${cat.id}`}
                      className="group block p-6 rounded-2xl bg-card border-glow hover:glow-primary transition-all duration-500"
                    >
                      <span className="text-xs font-mono text-muted-foreground">0{i + 1}</span>
                      <h3 className="text-xl font-display font-semibold mt-2 group-hover:text-primary transition-colors">{cat.name}</h3>
                      {cat.description && <p className="text-sm text-muted-foreground mt-1">{cat.description}</p>}
                      <div className="mt-4 flex items-center gap-1 text-xs text-primary opacity-0 group-hover:opacity-100 transition-opacity">
                        Explorar <ArrowRight size={12} />
                      </div>
                    </Link>
                  </motion.div>
                ))}
              </motion.div>
            </motion.div>
          </div>
        </section>
      )}

      {/* CTA */}
      <section className="py-24 px-4">
        <div className="max-w-4xl mx-auto text-center">
          <motion.div initial="hidden" whileInView="show" viewport={{ once: true }} variants={stagger} className="space-y-6">
            <motion.h2 variants={fadeUp} className="text-4xl sm:text-5xl font-display font-bold tracking-tight">
              Pronto para elevar
              <br />
              <span className="text-gradient-primary">seu setup?</span>
            </motion.h2>
            <motion.div variants={fadeUp}>
              <Link
                to="/catalog"
                className="inline-flex items-center justify-center gap-2 px-8 py-4 rounded-xl bg-primary text-primary-foreground font-medium btn-physical hover:opacity-90 transition-all active:scale-[0.98]"
              >
                Começar Agora <ArrowRight size={18} />
              </Link>
            </motion.div>
          </motion.div>
        </div>
      </section>
    </div>
  );
};

export default Home;
