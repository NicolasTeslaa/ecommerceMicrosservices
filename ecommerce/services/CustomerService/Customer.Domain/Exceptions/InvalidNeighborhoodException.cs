using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidNeighborhoodException : CustomerException
{
    public InvalidNeighborhoodException()
        : base(CustomerErrorCode.InvalidNeighborhood, "Neighborhood is required.")
    {
    }
}
