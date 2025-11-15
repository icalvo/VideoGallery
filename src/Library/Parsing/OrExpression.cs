namespace VideoGallery.Library.Parsing;

public sealed record OrExpression(BooleanExpression Left, BooleanExpression Right) : BooleanExpression;