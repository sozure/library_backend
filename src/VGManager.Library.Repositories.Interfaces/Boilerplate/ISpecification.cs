using System.Linq.Expressions;

namespace VGManager.Library.Repositories.Interfaces.Boilerplate;

public interface ISpecification<TEntity>
{
    Expression<Func<TEntity, bool>> Criteria { get; }

    ICollection<Expression<Func<TEntity, object>>> Includes { get; }

    ICollection<string> IncludeStrings { get; }
}
