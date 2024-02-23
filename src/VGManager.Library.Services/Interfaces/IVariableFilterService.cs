using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Text.RegularExpressions;

namespace VGManager.Library.Services.Interfaces;

public interface IVariableFilterService
{
    IEnumerable<KeyValuePair<string, string>> Filter(IDictionary<string, string> variables, Regex regex);

    IEnumerable<KeyValuePair<string, string>> Filter(IDictionary<string, string> variables, string filter);
}
