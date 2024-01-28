using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Text.RegularExpressions;

namespace VGManager.Library.Services.Interfaces;

public interface IVariableFilterService
{
    IEnumerable<KeyValuePair<string, VariableValue>> Filter(IDictionary<string, VariableValue> variables, Regex regex);

    IEnumerable<KeyValuePair<string, VariableValue>> Filter(IDictionary<string, VariableValue> variables, string filter);
}
