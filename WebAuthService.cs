using POSApp.Data.Models;

namespace POSApp.Web;

public class WebAuthService
{
    private User? _currentUser;
    private readonly object _lock = new();

    public User? CurrentUser
    {
        get { lock (_lock) return _currentUser; }
    }

    public bool IsLoggedIn => CurrentUser != null;

    public event Action? OnAuthStateChanged;

    public (bool success, string message) Login(string username, string password, Data.Services.AuthService authService)
    {
        var (user, message) = authService.Login(username, password);
        if (user != null)
        {
            lock (_lock)
            {
                _currentUser = user;
            }
            OnAuthStateChanged?.Invoke();
            return (true, message);
        }
        return (false, message);
    }

    public void Logout()
    {
        lock (_lock)
        {
            _currentUser = null;
        }
        OnAuthStateChanged?.Invoke();
    }
}
