namespace VideoGallery.Interfaces;

public record VideoFormat(
    string Id,
    string Ext,
    string Resolution,
    string Size,
    string Note,
    string VCodec,
    string ACodec);
