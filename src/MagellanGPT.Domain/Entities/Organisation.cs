using System;
namespace MagellanGPT.Domain.Entities;

public class Organisation : BaseEntity
{
    public string PartitionKey { get; set; } = "Organisation";
    public string CompanyName { get; set; } = "Diiage20224";
    public Quota Quota { get; set; }

    public Organisation()
    {
        Quota = new Quota();
    }
}

