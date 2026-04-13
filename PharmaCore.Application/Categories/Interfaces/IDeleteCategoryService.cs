using PharmaCore.Application.Categories.Requests;

namespace PharmaCore.Application.Categories.Interfaces;

public interface IDeleteCategoryService
{
    Task ExecuteAsync(int categoryId, CancellationToken cancellationToken = default);
}
