using System.Linq.Expressions;

namespace SharedPlatform.Features.DomainPrimitives.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();
    
    public bool IsSatisfiedBy(T entity)
    {
        return ToExpression().Compile()(entity);
    }
    
    public ISpecification<T> And(ISpecification<T> other)
    {
        return new CompositeSpecification<T>(this, other, CompositeOperator.And);
    }
    
    public ISpecification<T> Or(ISpecification<T> other)
    {
        return new CompositeSpecification<T>(this, other, CompositeOperator.Or);
    }
    
    public ISpecification<T> Not()
    {
        return new CompositeSpecification<T>(this, null, CompositeOperator.Not);
    }
}