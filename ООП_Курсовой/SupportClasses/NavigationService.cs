using System.Windows.Controls;

namespace ООП_Курсовой.SupportClasses;

public class NavigationService
{
    private readonly Dictionary<string, UserControl> _pages = new();
    private readonly Dictionary<string, Func<UserControl>> _pageFactories = new();
    private ContentControl? _contentControl;
    private string _currentPage = "Home";

    public void SetContentControl(ContentControl contentControl)
    {
        _contentControl = contentControl;
    }

    public void RegisterPage(string pageKey, Func<UserControl> pageFactory)
    {
        _pageFactories[pageKey] = pageFactory;
    }

    public void RegisterPage(string pageKey, UserControl page)
    {
        _pages[pageKey] = page;
    }

    public void NavigateTo(string pageKey)
    {
        if (_contentControl == null)
            return;

        UserControl? page = null;

        if (_pages.TryGetValue(pageKey, out var existingPage))
        {
            page = existingPage;
        }
        else if (_pageFactories.TryGetValue(pageKey, out var factory))
        {
            page = factory();
            _pages[pageKey] = page;
        }

        if (page != null)
        {
            _currentPage = pageKey;
            _contentControl.Content = page;
        }
    }

    public string GetCurrentPage() => _currentPage;

    public UserControl? GetCurrentPageControl()
    {
        if (_pages.TryGetValue(_currentPage, out var page))
            return page;
        return null;
    }

    public void ClearCache(params string[] keepPages)
    {
        var keepSet = new HashSet<string>(keepPages);
        var keysToRemove = new List<string>();

        foreach (var key in _pages.Keys)
        {
            if (!keepSet.Contains(key))
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
        {
            _pages.Remove(key);
        }
    }
}

