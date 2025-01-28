using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model;

public class LocalizedString : ValueObject
{
    private readonly Dictionary<string, string> _values = [];

    public Dictionary<string, string> Values => _values;

    public void Set(string languageCode, string value)
    {
        _values[languageCode] = value;
    }

    public string Get(string languageCode)
    {
        return _values.TryGetValue(languageCode, out var value) ? value : null;
    }

    public void Remove(string languageCode)
    {
        _values.Remove(languageCode);
    }

    public bool Validate(IList<string> allowedLanguages, out IList<string> invalidLanguages)
    {
        invalidLanguages = _values.Keys.Where(key => !allowedLanguages.Contains(key)).ToList();
        if (invalidLanguages.Count > 0)
        {
            return false;
        }
        return true;
    }

    public void Clean(IList<string> allowedLanguages)
    {
        var invalidKeys = _values.Keys.Where(key => !allowedLanguages.Contains(key));
        foreach (var key in invalidKeys)
        {
            _values.Remove(key);
        }
    }

    public virtual object GetCopy()
    {
        var result = Clone() as LocalizedString;
        return result;
    }

    public IDictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>(_values);
    }
}
