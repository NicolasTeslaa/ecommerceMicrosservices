const ProductSkeleton = () => {
  return (
    <div className="bg-card border-glow p-4 rounded-3xl animate-pulse">
      <div className="aspect-square rounded-2xl mb-4 bg-secondary" />
      <div className="space-y-2 px-1">
        <div className="h-4 bg-secondary rounded w-3/4" />
        <div className="h-3 bg-secondary rounded w-1/3" />
        <div className="h-6 bg-secondary rounded w-1/2" />
      </div>
    </div>
  );
};

export default ProductSkeleton;
