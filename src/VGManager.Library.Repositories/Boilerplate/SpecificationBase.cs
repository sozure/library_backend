using System.Linq.Expressions;
using VGManager.Library.Repositories.Interfaces.Boilerplate;

namespace VGManager.Library.Repositories.Boilerplate;

public abstract class SpecificationBase<TEntity> : ISpecification<TEntity>
{
    public Expression<Func<TEntity, bool>> Criteria { get; }

    public ICollection<Expression<Func<TEntity, object>>> Includes { get; } = [];


    public ICollection<string> IncludeStrings { get; } = [];


    protected SpecificationBase(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
    }

    protected virtual void AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }
}
