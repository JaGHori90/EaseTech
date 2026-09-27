namespace Api.Enums
{
    public enum OrderStatus
    {
        Angerichtet,        // OrderPlaced
        TerminGeplant,               // AppointmentScheduled
        InBearbeitung,     // OrderInProgress
        Abgeschlossen,               // Completed
        Storniert,                   // Canceled
        Verzögert,                   // Delayed
    }
}
