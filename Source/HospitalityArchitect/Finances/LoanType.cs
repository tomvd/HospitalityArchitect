using Verse;

namespace RT;

public class LoanType : ILoadReferenceable
{
    private float amount;
    private float interest;
    private string company;
    private string id;

    public LoanType(float amount, float interest, string company, string id)
    {
        this.amount = amount;
        this.interest = interest;
        this.company = company;
        this.id = id;
    }

    public float Amount => amount;

    public float Interest => interest;

    public string Company => company;
    
    public string GetUniqueLoadID() => id;
}