namespace GestAI.Web.Models.Forms
{
    public class ExampleFormModel
    {
        public int PropertyId { get; set; }
        public int RoomTypeId { get; set; }
        public int UnitId { get; set; }
        public int GuestId { get; set; }
        public DateTime CheckIn { get; set; } = DateTime.Today;
        public DateTime CheckOut { get; set; } = DateTime.Today.AddDays(1);

    }
}