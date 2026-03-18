import { Canvas } from '@react-three/fiber';
import { Float, MeshDistortMaterial } from '@react-three/drei';
import { Suspense } from 'react';

const HeroShape = () => {
  return (
    <>
      <Float speed={1.5} rotationIntensity={0.8} floatIntensity={1.2}>
        <mesh position={[0, 0, 0]}>
          <torusKnotGeometry args={[1.2, 0.4, 256, 64]} />
          <MeshDistortMaterial
            color="#38bdf8"
            emissive="#7c3aed"
            emissiveIntensity={0.4}
            distort={0.25}
            speed={2}
            roughness={0.15}
            metalness={0.9}
          />
        </mesh>
      </Float>
      <Float speed={2.5} rotationIntensity={1.5} floatIntensity={0.5}>
        <mesh position={[2.5, -1, -2]} scale={0.4}>
          <icosahedronGeometry args={[1, 8]} />
          <MeshDistortMaterial
            color="#a78bfa"
            emissive="#38bdf8"
            emissiveIntensity={0.3}
            distort={0.5}
            speed={3}
            roughness={0.2}
            metalness={0.8}
          />
        </mesh>
      </Float>
      <Float speed={1.8} rotationIntensity={2} floatIntensity={0.8}>
        <mesh position={[-2.5, 1, -1.5]} scale={0.3}>
          <octahedronGeometry args={[1, 3]} />
          <MeshDistortMaterial
            color="#06b6d4"
            emissive="#8b5cf6"
            emissiveIntensity={0.5}
            distort={0.35}
            speed={2.5}
            roughness={0.1}
            metalness={0.95}
          />
        </mesh>
      </Float>
    </>
  );
};

const HeroScene = () => {
  return (
    <div className="absolute inset-0 z-0">
      <Suspense fallback={null}>
        <Canvas camera={{ position: [0, 0, 5], fov: 50 }}>
          <ambientLight intensity={0.3} />
          <spotLight position={[10, 10, 10]} angle={0.2} penumbra={1} intensity={0.6} />
          <pointLight position={[-10, -10, -10]} intensity={0.3} color="#7c3aed" />
          <pointLight position={[5, 5, 5]} intensity={0.2} color="#38bdf8" />
          <HeroScene3D />
        </Canvas>
      </Suspense>
    </div>
  );
};

const HeroScene3D = () => <HeroShape />;

export default HeroScene;
