using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Domain.ValueObjects;

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object obj)
    {
        if (obj is not ValueObject other)
            return false;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + obj.GetHashCode();
                }
            });
    }
}
