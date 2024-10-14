namespace Entities.Exceptions;

public class MaxAgeRangeBadRequestException() : BadRequestException("Max age cannot be less than min age.");