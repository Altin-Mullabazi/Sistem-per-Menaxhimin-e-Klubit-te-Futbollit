namespace FootballClubAPI.Models
{
    public enum TransferType
    {
        Permanent = 1,
        Loan = 2,
        FreeTransfer = 3,
        Youth = 4
    }

    public enum TransferStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Completed = 4,
        Cancelled = 5
    }

    public enum ContractStatus
    {
        Active = 1,
        Expired = 2,
        Terminated = 3,
        Suspended = 4,
        Pending = 5
    }

    public enum InjuryStatus
    {
        Active = 1,
        Recovering = 2,
        Recovered = 3
    }

    public enum TrainingType
    {
        Tactical = 1,
        Physical = 2,
        Friendly = 3,
        Recovery = 4,
        SetPieces = 5
    }
}