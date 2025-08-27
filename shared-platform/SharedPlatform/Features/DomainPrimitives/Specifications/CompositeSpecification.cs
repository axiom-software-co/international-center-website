using System.Linq.Expressions;

namespace SharedPlatform.Features.DomainPrimitives.Specifications;

public enum CompositeOperator
{
    And,
    Or,
    Not
}

public class CompositeSpecification<T> : BaseSpecification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T>? _right;
    private readonly CompositeOperator _operator;
    
    public CompositeSpecification(ISpecification<T> left, ISpecification<T>? right, CompositeOperator @operator)
    {
        _left = left;
        _right = right;
        _operator = @operator;
    }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        
        return _operator switch
        {
            CompositeOperator.And => CombineExpressions(leftExpression, _right!.ToExpression(), Expression.AndAlso),
            CompositeOperator.Or => CombineExpressions(leftExpression, _right!.ToExpression(), Expression.OrElse),
            CompositeOperator.Not => Expression.Lambda<Func<T, bool>>(
                Expression.Not(leftExpression.Body), 
                leftExpression.Parameters),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private static Expression<Func<T, bool>> CombineExpressions(
        Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second,
        Func<Expression, Expression, Expression> merge)
    {
        var parameter = Expression.Parameter(typeof(T));
        var firstBody = ReplaceParameterVisitor.ReplaceParameters(first.Body, first.Parameters[0], parameter);
        var secondBody = ReplaceParameterVisitor.ReplaceParameters(second.Body, second.Parameters[0], parameter);
        
        return Expression.Lambda<Func<T, bool>>(merge(firstBody, secondBody), parameter);
    }
}

internal class ReplaceParameterVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;
    
    private ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }
    
    public static Expression ReplaceParameters(Expression expression, ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        return new ReplaceParameterVisitor(oldParameter, newParameter).Visit(expression);
    }
    
    protected override Expression VisitParameter(ParameterExpression node)
    {
        return ReferenceEquals(node, _oldParameter) ? _newParameter : base.VisitParameter(node);
    }
}