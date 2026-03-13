using GestAI.Web.Dtos;

namespace GestAI.Web.Service;

public sealed class PropertyFeatureState
{
    private readonly ApiClient _api;
    private readonly Dictionary<int, PropertyFeatureSettingsDto> _cache = new();

    public PropertyFeatureState(ApiClient api)
    {
        _api = api;
    }

    public event Action? FeaturesChanged;

    public int? CurrentPropertyId { get; private set; }
    public PropertyFeatureSettingsDto? Current { get; private set; }

    public async Task<PropertyFeatureSettingsDto?> EnsureAsync(int propertyId, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(propertyId, out var cached))
        {
            SetCurrent(propertyId, cached, notify: false);
            return cached;
        }

        return await RefreshAsync(propertyId, ct);
    }

    public async Task<PropertyFeatureSettingsDto?> RefreshAsync(int propertyId, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<AppResult<PropertyFeatureSettingsDto>>($"api/properties/{propertyId}/feature-settings", ct);
        var settings = res?.Success == true ? res.Data : null;

        if (settings is null)
            return null;

        _cache[propertyId] = settings;
        SetCurrent(propertyId, settings, notify: true);
        return settings;
    }

    public void Invalidate(int propertyId)
    {
        _cache.Remove(propertyId);
    }

    public bool IsEnabled(int propertyId, Func<PropertyFeatureSettingsDto, bool> selector)
    {
        if (_cache.TryGetValue(propertyId, out var settings))
            return selector(settings);

        if (CurrentPropertyId == propertyId && Current is not null)
            return selector(Current);

        return true;
    }

    private void SetCurrent(int propertyId, PropertyFeatureSettingsDto settings, bool notify)
    {
        CurrentPropertyId = propertyId;
        Current = settings;
        if (notify)
            FeaturesChanged?.Invoke();
    }
}
