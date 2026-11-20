namespace VideoGallery.Library.Parsing;

public sealed record ImpliesExpression(BooleanExpression Left, BooleanExpression Right) : BooleanExpression;
