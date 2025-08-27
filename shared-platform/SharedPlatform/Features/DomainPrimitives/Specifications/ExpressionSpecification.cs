using System.Linq.Expressions;

namespace SharedPlatform.Features.DomainPrimitives.Specifications;

public class ExpressionSpecification<T> : BaseSpecification<T>
{
    private readonly Expression<Func<T, bool>> _expression;
    
    public ExpressionSpecification(Expression<Func<T, bool>> expression)
    {
        _expression = expression;
    }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        return _expression;
    }
}