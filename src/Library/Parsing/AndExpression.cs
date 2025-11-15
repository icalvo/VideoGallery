namespace VideoGallery.Library.Parsing;

public sealed record AndExpression(BooleanExpression Left, BooleanExpression Right) : BooleanExpression;