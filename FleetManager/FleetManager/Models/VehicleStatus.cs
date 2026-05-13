namespace FleetManager.Models
{
    public enum VehicleStatus
    {
        Idle = 0,       // Pojazd stoi na parkingu
        InTransit = 1,  // Pojazd w ruchu (aktywny symulator)
        Maintenance = 2 // Pojazd w warsztacie (zablokowany dla tras)
    }
}
