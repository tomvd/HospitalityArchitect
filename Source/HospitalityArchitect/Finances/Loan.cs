using System;
using Verse;

namespace HospitalityArchitect;

public class Loan : IExposable
{ 
    private string _loanType;
    private float _balance;
    private float _interest;
    
    public Loan()
    {
    }

    public Loan(LoanType loanType)
    {
        _loanType = loanType.GetUniqueLoadID();
        this._balance = loanType.Amount;
        this._interest = loanType.Interest;
    }

    public Loan(string loanType, float balance, float interest)
    {
        _loanType = loanType;
        _balance = balance;
        _interest = interest;
    }

    public string LoanType => _loanType;

    public float Balance => _balance;
    
    public float Interest => _interest;

    public float Repay(float amount)
    {
        // cant repay more than balance
        float actuallyRepaid = Math.Min(amount, _balance);
        _balance -= actuallyRepaid;
        return actuallyRepaid;
    }
    
    public void ExposeData()
    {
        Scribe_Values.Look(ref this._balance, "balance");
        Scribe_Values.Look(ref this._interest, "interest");
        Scribe_Values.Look(ref this._loanType, "loantype");
    }
}