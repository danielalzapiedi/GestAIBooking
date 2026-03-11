namespace GestAI.Web.Service
{
    public class AppState
    {
        public int? AccountId { get; private set; }
        public int? PropertyId { get; private set; }
        public int? UnitId { get; private set; }

        public event Action? OnChange;

        public void SetDefaults(int? accountId, int? propertyId, int? unitId)
        {
            AccountId = accountId;
            PropertyId = propertyId;
            UnitId = unitId;
            OnChange?.Invoke();
        }

        public void SetProperty(int propertyId)
        {
            PropertyId = propertyId;
            OnChange?.Invoke();
        }
    }
}
