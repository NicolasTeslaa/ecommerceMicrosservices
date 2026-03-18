import { Canvas } from '@react-three/fiber';
import { Float, MeshDistortMaterial } from '@react-three/drei';
import { useMemo, Suspense } from 'react';
import { hashString } from '@/utils/format';

interface ProductVisualProps {
  seed: string;
  size?: 'sm' | 'md' | 'lg';
}

const VisualMesh = ({ seed }: { seed: string }) => {
  const params = useMemo(() => {
    const h = hashString(seed);
    return {
      colorA: `hsl(${h % 360}, 70%, 60%)`,
      colorB: `hsl(${(h + 80) % 360}, 60%, 45%)`,
      distort: 0.3 + (h % 30) / 100,
      speed: 1.5 + (h % 20) / 20,
      geometry: h % 3,
    };
  }, [seed]);

  return (
    <Float speed={2} rotationIntensity={1.2} floatIntensity={1.5}>
      <mesh>
        {params.geometry === 0 && <icosahedronGeometry args={[1, 12]} />}
        {params.geometry === 1 && <torusKnotGeometry args={[0.7, 0.25, 128, 32]} />}
        {params.geometry === 2 && <octahedronGeometry args={[1, 4]} />}
        <MeshDistortMaterial
          color={params.colorA}
          emissive={params.colorB}
          emissiveIntensity={0.3}
          distort={params.distort}
          speed={params.speed}
          roughness={0.2}
          metalness={0.8}
        />
      </mesh>
    </Float>
  );
};

const ProductVisual = ({ seed, size = 'md' }: ProductVisualProps) => {
  const sizeClasses = {
    sm: 'h-40',
    md: 'h-64',
    lg: 'h-[50vh] lg:h-[70vh]',
  };

  return (
    <div className={`w-full relative overflow-hidden rounded-2xl bg-card ${sizeClasses[size]}`}>
      <Suspense fallback={<div className="w-full h-full bg-card animate-pulse_glow" />}>
        <Canvas camera={{ position: [0, 0, 3.5], fov: 45 }}>
          <ambientLight intensity={0.4} />
          <spotLight position={[10, 10, 10]} angle={0.15} penumbra={1} intensity={0.8} />
          <pointLight position={[-5, -5, -5]} intensity={0.3} color="#8b5cf6" />
          <VisualMesh seed={seed} />
        </Canvas>
      </Suspense>
      <div className="absolute inset-0 bg-gradient-to-t from-background/60 via-transparent to-transparent pointer-events-none" />
    </div>
  );
};

export default ProductVisual;
