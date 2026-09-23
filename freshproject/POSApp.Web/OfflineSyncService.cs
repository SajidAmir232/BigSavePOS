using Microsoft.JSInterop;
using POSApp.Data.Services;
using POSApp.Data.Models;
using System.Text.Json;

namespace POSApp.Web;

public class OfflineSyncService
{
    private readonly IJSRuntime _js;
    private bool _isOnline = true;

    public bool IsOnline => _isOnline;
    public event Action? OnSyncStateChanged;

    public OfflineSyncService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _isOnline = await _js.InvokeAsync<bool>("offlineSync.isOnline");
            OnSyncStateChanged?.Invoke();
        }
        catch { _isOnline = true; }
    }

    public async Task<bool> SaveInvoiceOffline(Invoice invoice)
    {
        try
        {
            var json = JsonSerializer.Serialize(invoice);
            await _js.InvokeVoidAsync("offlineSync.saveInvoice", json);
            return true;
        }
        catch { return false; }
    }

    public async Task<List<Invoice>> GetPendingInvoices()
    {
        try
        {
            var json = await _js.InvokeAsync<string>("offlineSync.getPendingInvoices");
            return JsonSerializer.Deserialize<List<Invoice>>(json) ?? new();
        }
        catch { return new(); }
    }

    public async Task<int> SyncPendingInvoices(InvoiceService invoiceService)
    {
        var pending = await GetPendingInvoices();
        int synced = 0;
        foreach (var inv in pending)
        {
            try
            {
                invoiceService.Create(inv, "web");
                await _js.InvokeVoidAsync("offlineSync.removePendingInvoice", inv.Guid.ToString());
                synced++;
            }
            catch { }
        }
        return synced;
    }

    public async Task<int> GetPendingCount()
    {
        try { return await _js.InvokeAsync<int>("offlineSync.getPendingCount"); }
        catch { return 0; }
    }
}
