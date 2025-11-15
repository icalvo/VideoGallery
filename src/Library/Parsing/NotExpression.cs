namespace VideoGallery.Library.Parsing;

public sealed record NotExpression(BooleanExpression Expression) : BooleanExpression;