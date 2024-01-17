using System.ComponentModel.DataAnnotations;
using VGManager.Library.Models.StatusEnums;

namespace VGManager.Library.Models.Models;

public class RepositoryResponseModel<T>
{
    [Required]
    public RepositoryStatus Status { get; set; }

    [Required]
    public IEnumerable<T> Data { get; set; } = null!;
}
